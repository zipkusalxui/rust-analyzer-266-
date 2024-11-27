using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.FindSymbols;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.Concurrent;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;

namespace RustAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class BaseNetworkableAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "RUST0005";
        
        private static readonly string[] PossibleBaseNetworkableNames = new[]
        {
            "BaseNetworkable",
            "global::BaseNetworkable"
        };

        private static readonly string Title = "Invalid prefab name";
        private static readonly string MessageFormat = "String '{0}' does not exist in StringPool {1}";
        private static readonly string Description = "Prefab names must exist in StringPool when used with BaseNetworkable properties or StringPool.Get() method. This ensures runtime safety and prevents potential errors.";

        private const string Category = "Correctness";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            Title,
            MessageFormat,
            Category,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            Debug.WriteLine("Initializing analyzer");
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeBinaryExpression, SyntaxKind.EqualsExpression);
            context.RegisterSyntaxNodeAction(AnalyzeStringPoolGet, SyntaxKind.InvocationExpression);
        }

        // Кеш для быстрого поиска префабов
        private static readonly HashSet<string> PrefabPaths = new HashSet<string>(
            StringPool.toNumber.Keys
                .Where(p => p.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
                .Select(p => p.ToLowerInvariant())
        );

        private static readonly ConcurrentDictionary<string, bool> PrefabPathCache 
            = new ConcurrentDictionary<string, bool>();
        private static readonly ConcurrentDictionary<string, List<string>> PrefixCache 
            = new ConcurrentDictionary<string, List<string>>();

        // Кеш для методов
        private static readonly ConcurrentDictionary<(string AssemblyName, string TypeName, string MethodName), bool> MethodCache 
            = new ConcurrentDictionary<(string, string, string), bool>();

        // Известные методы, использующие StringPool.Get
        private static readonly HashSet<(string TypeName, string MethodName)> KnownStringPoolMethods 
            = new HashSet<(string, string)>
        {
            ("GameManager", "CreateEntity"),
            ("BaseEntity", "Spawn"),
            ("PrefabAttribute", "server"),
            ("PrefabAttribute", "client"),
            ("GameManifest", "PathToStringID"),
            ("StringPool", "Add"),
            ("GameManager", "FindPrefab"),
            ("ItemManager", "CreateByName"),
            ("ItemManager", "FindItemDefinition"),
            ("GameManager", "LoadPrefab"),
            ("PrefabAttribute", "Find")
        };

        static BaseNetworkableAnalyzer()
        {
            // Инициализация кеша префиксов
            foreach (var path in PrefabPaths)
            {
                var prefix = path.Length >= 3 ? path.Substring(0, 3) : path;
                PrefixCache.AddOrUpdate(
                    prefix,
                    key => new List<string> { path },
                    (key, list) => { list.Add(path); return list; }
                );
            }
        }

        private bool IsValidPrefabPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            return PrefabPathCache.GetOrAdd(path, p =>
            {
                p = p.ToLowerInvariant().Replace("\\", "/").Trim();
                return PrefabPaths.Contains(p);
            });
        }

        private string GetSuggestionMessage(string invalidPath)
        {
            invalidPath = invalidPath.ToLowerInvariant().Replace("\\", "/").Trim();
            var prefix = invalidPath.Length >= 3 ? invalidPath.Substring(0, 3) : invalidPath;

            var candidates = PrefixCache.GetOrAdd(prefix, key => new List<string>());
            if (!candidates.Any())
                candidates = PrefabPaths.ToList();

            var suggestions = candidates
                .Select(p => new { Path = p, Distance = StringDistance.GetLevenshteinDistance(p, invalidPath) })
                .Where(x => x.Distance <= 5)
                .OrderBy(x => x.Distance)
                .Take(3)
                .Select(x => x.Path)
                .ToList();

            if (suggestions.Any())
            {
                return $"Invalid prefab path. Did you mean one of these?\n" +
                       string.Join("\n", suggestions.Select(s => $"  - {s}"));
            }

            return "Invalid prefab path. Make sure it starts with 'assets/prefabs/' and ends with '.prefab'";
        }

        private void AnalyzeBinaryExpression(SyntaxNodeAnalysisContext context)
        {
            Debug.WriteLine("Analyzing binary expression");
            var binaryExpression = (BinaryExpressionSyntax)context.Node;
            
            var leftMemberAccess = binaryExpression.Left as MemberAccessExpressionSyntax;
            var rightMemberAccess = binaryExpression.Right as MemberAccessExpressionSyntax;

            var leftLiteral = binaryExpression.Left as LiteralExpressionSyntax;
            var rightLiteral = binaryExpression.Right as LiteralExpressionSyntax;

            if (!IsValidPrefabNameComparison(context, leftMemberAccess, leftLiteral, rightMemberAccess, rightLiteral))
            {
                Debug.WriteLine("Not a valid prefab name comparison");
                return;
            }

            var literalExpression = leftLiteral ?? rightLiteral;
            var memberAccess = leftMemberAccess ?? rightMemberAccess;
            
            if (literalExpression == null || memberAccess == null)
            {
                Debug.WriteLine("Missing literal or member access");
                return;
            }

            var stringValue = literalExpression.Token.ValueText;
            bool isShortPrefabName = memberAccess.Name.Identifier.Text == "ShortPrefabName";

            if (isShortPrefabName)
            {
                bool foundMatch = false;
                if (StringPool.toNumber != null)
                {
                    foreach (var prefabName in StringPool.toNumber.Keys)
                    {
                        var shortName = Path.GetFileNameWithoutExtension(prefabName);
                        if (shortName == stringValue)
                        {
                            foundMatch = true;
                            break;
                        }
                    }
                }

                if (!foundMatch)
                {
                    var suggestions = StringPool.toNumber != null 
                        ? StringDistance.FindSimilarShortNames(stringValue, StringPool.toNumber.Keys)
                        : Enumerable.Empty<string>();

                    string suggestionMessage = suggestions.Any()
                        ? $"Did you mean one of these: {string.Join(", ", suggestions)}?"
                        : "Make sure to use a valid prefab short name";

                    Debug.WriteLine($"Creating diagnostic for {stringValue}");
                    var diagnostic = Diagnostic.Create(
                        Rule,
                        literalExpression.GetLocation(),
                        stringValue,
                        suggestionMessage);
                    context.ReportDiagnostic(diagnostic);
                    Debug.WriteLine("Diagnostic reported");
                }
            }
            else
            {
                if (StringPool.toNumber == null || !StringPool.toNumber.ContainsKey(stringValue))
                {
                    var suggestions = StringPool.toNumber != null 
                        ? StringDistance.FindSimilarPrefabs(stringValue, StringPool.toNumber.Keys)
                        : Enumerable.Empty<string>();

                    string suggestionMessage = suggestions.Any()
                        ? $"Did you mean one of these: {string.Join(", ", suggestions)}?"
                        : "Make sure to use a valid prefab path";

                    Debug.WriteLine($"Creating diagnostic for {stringValue}");
                    var diagnostic = Diagnostic.Create(
                        Rule,
                        literalExpression.GetLocation(),
                        stringValue,
                        suggestionMessage);
                    context.ReportDiagnostic(diagnostic);
                    Debug.WriteLine("Diagnostic reported");
                }
            }
        }

        private void AnalyzeStringPoolGet(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;
            Debug.WriteLine($"Analyzing StringPool.Get call at {invocation.GetLocation()}");
            
            if (!IsStringPoolGetCall(invocation, context.SemanticModel))
            {
                Debug.WriteLine("Not a StringPool.Get call");
                return;
            }

            AnalyzeStringPoolGetCall(context, invocation);
        }

        private void AnalyzeStringPoolGetCall(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax invocation)
        {
            var semanticModel = context.SemanticModel;
            var arguments = invocation.ArgumentList.Arguments;

            if (arguments.Count == 0)
                return;

            var firstArg = arguments[0].Expression;
            string? stringValue = null;

            // Быстрая проверка для литералов
            if (firstArg is LiteralExpressionSyntax literal)
            {
                stringValue = literal.Token.ValueText;
                if (!IsValidPrefabPath(stringValue))
                {
                    ReportDiagnostic(
                        context,
                        firstArg.GetLocation(),
                        stringValue,
                        GetSuggestionMessage(stringValue)
                    );
                }
                return;
            }

            // Анализ для нелитеральных выражений
            var dataFlow = semanticModel.AnalyzeDataFlow(firstArg);
            if (dataFlow?.DataFlowsIn != null)
            {
                foreach (var symbol in dataFlow.DataFlowsIn)
                {
                    if (symbol == null) continue;

                    var literals = GetStringLiteralsFromSymbol(symbol, semanticModel);
                    foreach (var kvp in literals)
                    {
                        if (!IsValidPrefabPath(kvp.Key))
                        {
                            ReportDiagnostic(
                                context,
                                kvp.Value,
                                kvp.Key,
                                GetSuggestionMessage(kvp.Key)
                            );
                        }
                    }
                }
            }
        }

        private bool IsStringPoolGetCall(InvocationExpressionSyntax invocation, SemanticModel semanticModel, HashSet<IMethodSymbol>? visitedMethods = null)
        {
            visitedMethods ??= new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);

            var methodSymbol = semanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
            if (methodSymbol == null)
            {
                Debug.WriteLine("No method symbol found");
                return false;
            }

            // Check cache first
            var cacheKey = (
                methodSymbol.ContainingAssembly?.Name ?? "Unknown",
                methodSymbol.ContainingType.Name,
                methodSymbol.Name
            );

            if (MethodCache.TryGetValue(cacheKey, out bool cachedResult))
            {
                Debug.WriteLine($"Cache hit for {cacheKey.Item2}.{cacheKey.Item3}");
                return cachedResult;
            }

            // Prevent infinite recursion
            if (!visitedMethods.Add(methodSymbol))
            {
                Debug.WriteLine($"Already visited method {methodSymbol.Name}");
                return false;
            }

            Debug.WriteLine($"Analyzing method: {methodSymbol.ToDisplayString()} (Static: {methodSymbol.IsStatic})");

            bool result = false;

            // Direct StringPool.Get call
            if (methodSymbol.IsStatic && methodSymbol.Name == "Get" && methodSymbol.ContainingType.Name == "StringPool")
            {
                Debug.WriteLine("Found direct StringPool.Get call");
                result = true;
            }
            // Check known methods that use StringPool.Get internally
            else if (KnownStringPoolMethods.Contains((methodSymbol.ContainingType.Name, methodSymbol.Name)))
            {
                Debug.WriteLine($"Found known method that uses StringPool.Get: {methodSymbol.ContainingType.Name}.{methodSymbol.Name}");
                
                // For CreateEntity and similar methods, verify the first argument is a string literal or path
                if (methodSymbol.Parameters.Length > 0 && methodSymbol.Parameters[0].Type.SpecialType == SpecialType.System_String)
                {
                    var args = invocation.ArgumentList.Arguments;
                    if (args.Count > 0)
                    {
                        var firstArg = args[0].Expression;
                        if (firstArg is LiteralExpressionSyntax || 
                            (firstArg is IdentifierNameSyntax && semanticModel.GetTypeInfo(firstArg).Type?.SpecialType == SpecialType.System_String))
                        {
                            result = true;
                        }
                    }
                }
            }
            // Check if this method wraps a StringPool.Get call
            else if (methodSymbol.DeclaringSyntaxReferences.Length > 0)
            {
                var methodDeclaration = methodSymbol.DeclaringSyntaxReferences[0].GetSyntax() as MethodDeclarationSyntax;
                if (methodDeclaration != null)
                {
                    // Look for any method invocations within this method
                    var invocations = methodDeclaration.DescendantNodes()
                        .OfType<InvocationExpressionSyntax>();

                    foreach (var innerInvocation in invocations)
                    {
                        // Recursively check each invocation
                        if (IsStringPoolGetCall(innerInvocation, semanticModel, visitedMethods))
                        {
                            Debug.WriteLine($"Found wrapped StringPool.Get call in {methodSymbol.Name}");
                            result = true;
                            break;
                        }
                    }
                }
            }

            // Cache the result
            MethodCache.TryAdd(cacheKey, result);
            Debug.WriteLine($"Cached result for {cacheKey.Item2}.{cacheKey.Item3}: {result}");

            return result;
        }

        private Dictionary<string, Location> GetStringLiteralsFromSymbol(ISymbol symbol, SemanticModel semanticModel)
        {
            var literals = new Dictionary<string, Location>();
            
            // If it's a parameter, we need to find all the places it's called from
            if (symbol is IParameterSymbol parameter)
            {
                var method = parameter.ContainingSymbol as IMethodSymbol;
                if (method != null)
                {
                    // Find all references to this method
                    var methodSyntax = method.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as MethodDeclarationSyntax;
                    if (methodSyntax != null)
                    {
                        var callers = FindMethodCallers(methodSyntax, semanticModel);
                        foreach (var caller in callers)
                        {
                            if (caller.ArgumentList.Arguments.Count > parameter.Ordinal)
                            {
                                var arg = caller.ArgumentList.Arguments[parameter.Ordinal].Expression;
                                if (arg is LiteralExpressionSyntax literal)
                                {
                                    literals[literal.Token.ValueText] = literal.GetLocation();
                                }
                                else 
                                {
                                    // If the argument isn't a literal, analyze its data flow
                                    var argSymbol = semanticModel.GetSymbolInfo(arg).Symbol;
                                    if (argSymbol != null)
                                    {
                                        var argLiterals = GetStringLiteralsFromSymbol(argSymbol, semanticModel);
                                        foreach (var kvp in argLiterals)
                                        {
                                            literals[kvp.Key] = kvp.Value;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            // For variables, look for assignments
            else
            {
                foreach (var reference in symbol.DeclaringSyntaxReferences)
                {
                    var node = reference.GetSyntax();
                    var assignments = node.Ancestors()
                        .OfType<AssignmentExpressionSyntax>()
                        .Where(a => semanticModel.GetSymbolInfo(a.Left).Symbol?.Equals(symbol, SymbolEqualityComparer.Default) == true)
                        .ToList();

                    foreach (var assignment in assignments)
                    {
                        if (assignment.Right is LiteralExpressionSyntax literal)
                        {
                            literals[literal.Token.ValueText] = literal.GetLocation();
                        }
                        else
                        {
                            // If assigned from another variable, analyze its data flow
                            var rightSymbol = semanticModel.GetSymbolInfo(assignment.Right).Symbol;
                            if (rightSymbol != null)
                            {
                                var rightLiterals = GetStringLiteralsFromSymbol(rightSymbol, semanticModel);
                                foreach (var kvp in rightLiterals)
                                {
                                    literals[kvp.Key] = kvp.Value;
                                }
                            }
                        }
                    }
                }
            }

            return literals;
        }

        private void CollectStringLiteralsWithLocations(ISymbol symbol, SemanticModel semanticModel, Dictionary<string, Location> literals)
        {
            Debug.WriteLine($"Collecting literals for symbol: {symbol?.GetType().Name} {symbol?.Name}");

            if (symbol is IParameterSymbol parameter)
            {
                Debug.WriteLine($"Symbol is parameter: {parameter.Name}");
                var references = symbol.DeclaringSyntaxReferences;
                foreach (var reference in references)
                {
                    var node = reference.GetSyntax();
                    var method = node.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
                    if (method != null)
                    {
                        Debug.WriteLine($"Found method: {method.Identifier}");
                        var callers = FindMethodCallers(method, semanticModel).ToList();
                        Debug.WriteLine($"Found {callers.Count} callers");
                        
                        foreach (var caller in callers)
                        {
                            Debug.WriteLine($"Analyzing caller at {caller.GetLocation()}");
                            var paramIndex = parameter.Ordinal;
                            if (caller.ArgumentList.Arguments.Count > paramIndex)
                            {
                                var arg = caller.ArgumentList.Arguments[paramIndex].Expression;
                                if (arg is LiteralExpressionSyntax literal)
                                {
                                    Debug.WriteLine($"Found literal: {literal.Token.ValueText}");
                                    literals[literal.Token.ValueText] = literal.GetLocation();
                                }
                                else
                                {
                                    Debug.WriteLine($"Argument is not literal: {arg.GetType().Name}");
                                    var argSymbol = semanticModel.GetSymbolInfo(arg).Symbol;
                                    if (argSymbol != null)
                                    {
                                        Debug.WriteLine($"Recursing into argument symbol: {argSymbol.GetType().Name} {argSymbol.Name}");
                                        CollectStringLiteralsWithLocations(argSymbol, semanticModel, literals);
                                    }
                                    else
                                    {
                                        Debug.WriteLine("No symbol found for argument");
                                    }
                                }
                            }
                            else
                            {
                                Debug.WriteLine($"Parameter index {paramIndex} out of range for caller arguments");
                            }
                        }
                    }
                }
            }
            else if (symbol is ILocalSymbol local)
            {
                Debug.WriteLine($"Symbol is local variable: {local.Name}");
                AnalyzeAssignments(local, semanticModel, literals);
            }
            else if (symbol is IFieldSymbol field)
            {
                Debug.WriteLine($"Symbol is field: {field.Name}");
                AnalyzeAssignments(field, semanticModel, literals);
            }
            else if (symbol is IPropertySymbol property)
            {
                Debug.WriteLine($"Symbol is property: {property.Name}");
                AnalyzeAssignments(property, semanticModel, literals);
            }
            else
            {
                Debug.WriteLine($"Unhandled symbol type: {symbol?.GetType().Name}");
            }
        }

        private void AnalyzeAssignments(ISymbol symbol, SemanticModel semanticModel, Dictionary<string, Location> literals)
        {
            var references = symbol.DeclaringSyntaxReferences;
            foreach (var reference in references)
            {
                var node = reference.GetSyntax();
                var assignments = node.Ancestors()
                    .OfType<AssignmentExpressionSyntax>()
                    .Where(a => SymbolEqualityComparer.Default.Equals(
                        semanticModel.GetSymbolInfo(a.Left).Symbol, symbol))
                    .ToList();

                Debug.WriteLine($"Found {assignments.Count} assignments for {symbol.Name}");

                foreach (var assignment in assignments)
                {
                    Debug.WriteLine($"Analyzing assignment at {assignment.GetLocation()}");
                    if (assignment.Right is LiteralExpressionSyntax literal)
                    {
                        literals[literal.Token.ValueText] = literal.GetLocation();
                        Debug.WriteLine($"Found literal in assignment: {literal.Token.ValueText}");
                    }
                    else
                    {
                        Debug.WriteLine($"Right side is not literal: {assignment.Right.GetType().Name}");
                        var rightSymbol = semanticModel.GetSymbolInfo(assignment.Right).Symbol;
                        if (rightSymbol != null)
                        {
                            Debug.WriteLine($"Recursing into right symbol: {rightSymbol.GetType().Name} {rightSymbol.Name}");
                            CollectStringLiteralsWithLocations(rightSymbol, semanticModel, literals);
                        }
                        else
                        {
                            Debug.WriteLine("No symbol found for right side of assignment");
                        }
                    }
                }
            }
        }

        private static IEnumerable<InvocationExpressionSyntax> FindMethodCallers(
            MethodDeclarationSyntax method,
            SemanticModel semanticModel)
        {
            Debug.WriteLine($"Finding callers for method: {method.Identifier}");
            var methodSymbol = semanticModel.GetDeclaredSymbol(method);
            if (methodSymbol == null)
            {
                Debug.WriteLine("No method symbol found");
                return Enumerable.Empty<InvocationExpressionSyntax>();
            }

            var root = method.SyntaxTree.GetRoot();
            Debug.WriteLine($"Searching in syntax tree with {root.DescendantNodes().Count()} nodes");

            return root.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Where(inv => 
                {
                    var symbolInfo = semanticModel.GetSymbolInfo(inv);
                    var symbol = symbolInfo.Symbol;
                    var matches = symbol != null && SymbolEqualityComparer.Default.Equals(symbol, methodSymbol);
                    Debug.WriteLine($"Found invocation at {inv.GetLocation()}, matches: {matches}, symbol: {symbol?.Name ?? "null"}");
                    return matches;
                });
        }

        private bool IsValidPrefabNameComparison(
            SyntaxNodeAnalysisContext context,
            MemberAccessExpressionSyntax? leftMember,
            LiteralExpressionSyntax? leftLiteral,
            MemberAccessExpressionSyntax? rightMember,
            LiteralExpressionSyntax? rightLiteral)
        {
            Debug.WriteLine("Checking prefab name comparison");
            if ((leftMember == null && rightMember == null) || (leftLiteral == null && rightLiteral == null))
            {
                Debug.WriteLine("Not a valid prefab name comparison");
                return false;
            }

            if ((leftMember != null && rightMember != null) || (leftLiteral != null && rightLiteral != null))
            {
                Debug.WriteLine("Not a valid prefab name comparison");
                return false;
            }

            var memberAccess = leftMember ?? rightMember;
            
            if (memberAccess == null)
            {
                Debug.WriteLine("Missing member access");
                return false;
            }

            var propertyName = memberAccess.Name.Identifier.Text;
            
            if (!(propertyName == "PrefabName" || propertyName == "ShortPrefabName"))
            {
                Debug.WriteLine("Not a prefab name property");
                return false;
            }

            var typeInfo = context.SemanticModel.GetTypeInfo(memberAccess.Expression);
            var objectType = typeInfo.Type;
            if (objectType == null)
            {
                Debug.WriteLine("No object type found");
                return false;
            }

            var currentType = objectType;
            while (currentType != null)
            {
                var currentTypeName = currentType.ToDisplayString();
                
                if (PossibleBaseNetworkableNames.Contains(currentTypeName))
                {
                    Debug.WriteLine("Found base networkable type");
                    return true;
                }
                    
                currentType = currentType.BaseType;
            }

            Debug.WriteLine("No base networkable type found");
            return false;
        }

        private void CheckPrefabName(SyntaxNodeAnalysisContext context, string stringValue, Location location)
        {
            if (StringPool.toNumber == null || !StringPool.toNumber.ContainsKey(stringValue))
            {
                var suggestions = StringPool.toNumber != null 
                    ? StringDistance.FindSimilarPrefabs(stringValue, StringPool.toNumber.Keys)
                    : Enumerable.Empty<string>();

                string suggestionMessage = suggestions.Any()
                    ? $"Did you mean one of these: {string.Join(", ", suggestions)}?"
                    : "Make sure to use a valid prefab path";

                Debug.WriteLine($"Creating diagnostic for {stringValue} at {location}");

                var diagnostic = Diagnostic.Create(
                    Rule,
                    location,
                    stringValue,
                    suggestionMessage);
                context.ReportDiagnostic(diagnostic);
                Debug.WriteLine("Diagnostic reported");
            }
        }

        private void ReportDiagnostic(SyntaxNodeAnalysisContext context, Location location, string message, string? suggestion = null)
        {
            var properties = suggestion != null
                ? ImmutableDictionary.CreateRange(new[] { new KeyValuePair<string, string?>( "Suggestion", suggestion) })
                : null;

            var diagnostic = Diagnostic.Create(
                Rule,
                location,
                properties,
                message);
            context.ReportDiagnostic(diagnostic);
        }

        // Optional: Add method to pre-analyze common methods
        private void PreAnalyzeCommonMethods(Compilation compilation)
        {
            var stringPoolSymbol = compilation.GetTypeByMetadataName("StringPool");
            if (stringPoolSymbol != null)
            {
                foreach (var method in stringPoolSymbol.GetMembers().OfType<IMethodSymbol>())
                {
                    var key = (stringPoolSymbol.ContainingAssembly.Name, "StringPool", method.Name);
                    if (!MethodCache.ContainsKey(key))
                    {
                        // Pre-analyze and cache the result
                        // This would need actual syntax tree analysis
                        // For now, just cache known results
                        if (method.Name == "Get" && method.IsStatic)
                        {
                            MethodCache.TryAdd(key, true);
                        }
                    }
                }
            }
        }
    }
}
