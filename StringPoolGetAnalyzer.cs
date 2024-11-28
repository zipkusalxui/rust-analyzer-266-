using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace RustAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class StringPoolGetAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "RUST0005";

        private static readonly string[] PossibleBaseNetworkableNames = new[]
        {
            "BaseNetworkable",
            "global::BaseNetworkable"
        };

        private static readonly string Title = "Invalid prefab name";
        private static readonly string MessageFormat = "String '{0}' does not exist in StringPool{1}";
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
            context.RegisterCompilationStartAction(InitializeCacheAndRegisterActions);
        }

        private void InitializeCacheAndRegisterActions(CompilationStartAnalysisContext context)
        {
            // Initialize caches
            InitializeMethodCache(context.Compilation);

            context.RegisterSyntaxNodeAction(AnalyzeBinaryExpression, SyntaxKind.EqualsExpression);
            context.RegisterSyntaxNodeAction(AnalyzeInvocationExpression, SyntaxKind.InvocationExpression);
        }

        // Cache for fast prefab lookup
        private static readonly HashSet<string> PrefabPaths = new HashSet<string>(
            StringPool.toNumber.Keys
                .Where(p => p.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
                .Select(p => p.ToLowerInvariant())
        );

        private static readonly ConcurrentDictionary<string, bool> PrefabPathCache
            = new ConcurrentDictionary<string, bool>();
        private static readonly ConcurrentDictionary<string, List<string>> PrefixCache
            = new ConcurrentDictionary<string, List<string>>();

        // Cache for methods that use StringPool.Get
        private static readonly ConcurrentDictionary<(string TypeName, string MethodName), MethodConfig> MethodCache
            = new ConcurrentDictionary<(string, string), MethodConfig>();

        private class MethodConfig
        {
            public string TypeName { get; }
            public string MethodName { get; }
            public List<int> ParameterIndices { get; }

            public MethodConfig(string typeName, string methodName, List<int> parameterIndices)
            {
                TypeName = typeName;
                MethodName = methodName;
                ParameterIndices = parameterIndices;
            }
        }

        static BaseNetworkableAnalyzer()
        {
            // Initialize prefix cache
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

        private void InitializeMethodCache(Compilation compilation)
        {
            // Pre-populate MethodCache with known methods
            var knownMethods = new List<MethodConfig>
            {
                new MethodConfig("GameManager", "CreateEntity", new List<int> { 0 }),
                new MethodConfig("BaseEntity", "Spawn", new List<int>()),
                new MethodConfig("PrefabAttribute", "server", new List<int>()),
                new MethodConfig("PrefabAttribute", "client", new List<int>()),
                new MethodConfig("GameManifest", "PathToStringID", new List<int> { 0 }),
                new MethodConfig("StringPool", "Add", new List<int> { 0 }),
                new MethodConfig("GameManager", "FindPrefab", new List<int> { 0 }),
                new MethodConfig("ItemManager", "CreateByName", new List<int> { 0 }),
                new MethodConfig("ItemManager", "FindItemDefinition", new List<int> { 0 }),
                new MethodConfig("GameManager", "LoadPrefab", new List<int> { 0 }),
                new MethodConfig("PrefabAttribute", "Find", new List<int> { 0 })
            };

            foreach (var methodConfig in knownMethods)
            {
                MethodCache.TryAdd((methodConfig.TypeName, methodConfig.MethodName), methodConfig);
            }

            // Analyze the compilation to find methods that call StringPool.Get
            var stringPoolType = compilation.GetTypeByMetadataName("StringPool");
            if (stringPoolType != null)
            {
                var methodsUsingStringPoolGet = new HashSet<IMethodSymbol>();
                foreach (var tree in compilation.SyntaxTrees)
                {
                    var semanticModel = compilation.GetSemanticModel(tree);
                    var invocations = tree.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>();
                    foreach (var invocation in invocations)
                    {
                        var methodSymbol = semanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
                        if (methodSymbol == null) continue;

                        if (IsStringPoolGetMethod(methodSymbol))
                        {
                            var containingMethod = methodSymbol.ContainingSymbol as IMethodSymbol;
                            if (containingMethod != null)
                            {
                                var key = (containingMethod.ContainingType.Name, containingMethod.Name);
                                if (!MethodCache.ContainsKey(key))
                                {
                                    MethodCache.TryAdd(key, new MethodConfig(containingMethod.ContainingType.Name, containingMethod.Name, new List<int>()));
                                }
                            }
                        }
                    }
                }
            }
        }

        private bool IsStringPoolGetMethod(IMethodSymbol methodSymbol)
        {
            return methodSymbol.IsStatic &&
                   methodSymbol.Name == "Get" &&
                   methodSymbol.ContainingType.Name == "StringPool";
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
                return $" Invalid prefab path. Did you mean one of these?\n" +
                       string.Join("\n", suggestions.Select(s => $"  - {s}"));
            }

            return " Invalid prefab path. Make sure it starts with 'assets/prefabs/' and ends with '.prefab'";
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
                        ? $" Did you mean one of these: {string.Join(", ", suggestions)}?"
                        : " Make sure to use a valid prefab short name";

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
                        ? $" Did you mean one of these: {string.Join(", ", suggestions)}?"
                        : " Make sure to use a valid prefab path";

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

        private void AnalyzeInvocationExpression(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;
            Debug.WriteLine($"Analyzing invocation at {invocation.GetLocation()}");

            var semanticModel = context.SemanticModel;
            var methodSymbol = semanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
            if (methodSymbol == null)
                return;

            var methodConfig = GetMethodConfig(methodSymbol);
            if (methodConfig == null)
                return;

            var arguments = invocation.ArgumentList.Arguments;
            foreach (var index in methodConfig.ParameterIndices)
            {
                if (arguments.Count > index)
                {
                    var argument = arguments[index].Expression;
                    AnalyzeArgumentForPrefabPath(context, argument);
                }
            }
        }

        private MethodConfig? GetMethodConfig(IMethodSymbol methodSymbol)
        {
            var key = (methodSymbol.ContainingType.Name, methodSymbol.Name);

            if (MethodCache.TryGetValue(key, out var methodConfig))
            {
                return methodConfig;
            }

            // Optionally, you can add code here to analyze unknown methods and add them to the cache
            return null;
        }

        private void AnalyzeArgumentForPrefabPath(SyntaxNodeAnalysisContext context, ExpressionSyntax argument)
        {
            var semanticModel = context.SemanticModel;

            if (argument is LiteralExpressionSyntax literal)
            {
                var stringValue = literal.Token.ValueText;
                if (!IsValidPrefabPath(stringValue))
                {
                    ReportDiagnostic(
                        context,
                        literal.GetLocation(),
                        stringValue,
                        GetSuggestionMessage(stringValue)
                    );
                }
            }
            else
            {
                var dataFlow = semanticModel.AnalyzeDataFlow(argument);
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
        }

        private Dictionary<string, Location> GetStringLiteralsFromSymbol(ISymbol symbol, SemanticModel semanticModel)
        {
            var literals = new Dictionary<string, Location>();

            // If it's a parameter, find all the places it's called from
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

        private static IEnumerable<InvocationExpressionSyntax> FindMethodCallers(
            MethodDeclarationSyntax method,
            SemanticModel semanticModel)
        {
            var methodSymbol = semanticModel.GetDeclaredSymbol(method);
            if (methodSymbol == null)
                return Enumerable.Empty<InvocationExpressionSyntax>();

            var root = method.SyntaxTree.GetRoot();

            return root.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Where(invocation =>
                {
                    var symbolInfo = semanticModel.GetSymbolInfo(invocation);
                    var symbol = symbolInfo.Symbol;
                    return symbol != null && SymbolEqualityComparer.Default.Equals(symbol, methodSymbol);
                });
        }

        private bool IsValidPrefabNameComparison(
            SyntaxNodeAnalysisContext context,
            MemberAccessExpressionSyntax? leftMember,
            LiteralExpressionSyntax? leftLiteral,
            MemberAccessExpressionSyntax? rightMember,
            LiteralExpressionSyntax? rightLiteral)
        {
            if ((leftMember == null && rightMember == null) || (leftLiteral == null && rightLiteral == null))
                return false;

            if ((leftMember != null && rightMember != null) || (leftLiteral != null && rightLiteral != null))
                return false;

            var memberAccess = leftMember ?? rightMember;

            if (memberAccess == null)
                return false;

            var propertyName = memberAccess.Name.Identifier.Text;

            if (!(propertyName == "PrefabName" || propertyName == "ShortPrefabName"))
                return false;

            var typeInfo = context.SemanticModel.GetTypeInfo(memberAccess.Expression);
            var objectType = typeInfo.Type;
            if (objectType == null)
                return false;

            var currentType = objectType;
            while (currentType != null)
            {
                var currentTypeName = currentType.ToDisplayString();

                if (PossibleBaseNetworkableNames.Contains(currentTypeName))
                    return true;

                currentType = currentType.BaseType;
            }

            return false;
        }

        private void ReportDiagnostic(SyntaxNodeAnalysisContext context, Location location, string message, string suggestion)
        {
            var diagnostic = Diagnostic.Create(
                Rule,
                location,
                message,
                suggestion);

            context.ReportDiagnostic(diagnostic);
        }
    }
}
