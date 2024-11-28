using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace RustAnalyzer
{
    /// <summary>
    /// Analyzer to detect invalid prefab names used with BaseNetworkable properties or methods that eventually call StringPool.Get().
    /// </summary>
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
        private static readonly string Description = "Prefab names must exist in StringPool when used with BaseNetworkable properties or methods that eventually call StringPool.Get(). This ensures runtime safety and prevents potential errors.";

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
            context.RegisterCompilationStartAction(InitializePerCompilation);
        }

        private void InitializePerCompilation(CompilationStartAnalysisContext context)
        {
            // Create a new instance of the analyzer for this compilation
            var compilationAnalyzer = new CompilationAnalyzer(context.Compilation);

            // Register syntax node actions
            context.RegisterSyntaxNodeAction(compilationAnalyzer.AnalyzeBinaryExpression, SyntaxKind.EqualsExpression);
            context.RegisterSyntaxNodeAction(compilationAnalyzer.AnalyzeInvocationExpression, SyntaxKind.InvocationExpression);
        }

        /// <summary>
        /// Analyzer instance for a specific compilation.
        /// </summary>
        private class CompilationAnalyzer
        {
            private readonly Compilation _compilation;
            private readonly Dictionary<(string TypeName, string MethodName), MethodConfig> _methodCache;
            private readonly Dictionary<IMethodSymbol, bool> _methodCallCache;

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

            public CompilationAnalyzer(Compilation compilation)
            {
                _compilation = compilation;
                _methodCache = new Dictionary<(string, string), MethodConfig>();
                _methodCallCache = new Dictionary<IMethodSymbol, bool>(SymbolEqualityComparer.Default);

                InitializeMethodCache();
            }

            // /// <summary>
            // /// Initializes the set of valid prefab paths by analyzing StringPool.toNumber.
            // /// </summary>
            // private void InitializePrefabPaths()
            // {
            //     // Assuming StringPool.toNumber is accessible and contains the keys
            //     var stringPoolType = _compilation.GetTypeByMetadataName("StringPool");
            //     if (stringPoolType == null)
            //         return;

            //     var toNumberField = stringPoolType.GetMembers()
            //         .FirstOrDefault(m => m.Kind == SymbolKind.Field && m.Name == "toNumber") as IFieldSymbol;

            //     if (toNumberField == null)
            //         return;

            //     foreach (var syntaxRef in toNumberField.DeclaringSyntaxReferences)
            //     {
            //         var syntax = syntaxRef.GetSyntax() as VariableDeclaratorSyntax;
            //         if (syntax?.Initializer?.Value is ObjectCreationExpressionSyntax objCreation && objCreation.Initializer != null)
            //         {
            //             foreach (var expression in objCreation.Initializer.Expressions)
            //             {
            //                 if (expression is AssignmentExpressionSyntax assignExpr &&
            //                     assignExpr.Left is LiteralExpressionSyntax literal)
            //                 {
            //                     var key = literal.Token.ValueText.ToLowerInvariant();
            //                     _validPrefabPaths.Add(key);
            //                 }
            //             }
            //         }
            //     }
            // }

            private void InitializeMethodCache()
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
                    new MethodConfig("PrefabAttribute", "Find", new List<int> { 0 }),
                    new MethodConfig("StringPool", "Get", new List<int> { 0 }),
                };

                foreach (var methodConfig in knownMethods)
                {
                    var key = (methodConfig.TypeName, methodConfig.MethodName);
                    _methodCache[key] = methodConfig;
                }

                // Analyze the compilation to find methods that call methods in MethodCache
                foreach (var tree in _compilation.SyntaxTrees)
                {
                    var semanticModel = _compilation.GetSemanticModel(tree);
                    var root = tree.GetRoot();

                    var methodDeclarations = root.DescendantNodes()
                        .OfType<MethodDeclarationSyntax>();

                    foreach (var methodDeclaration in methodDeclarations)
                    {
                        var methodSymbol = semanticModel.GetDeclaredSymbol(methodDeclaration);
                        if (methodSymbol == null)
                            continue;

                        // Skip methods that are already in the cache
                        var methodKey = (methodSymbol.ContainingType.Name, methodSymbol.Name);
                        if (_methodCache.ContainsKey(methodKey))
                            continue;

                        // Check if this method calls any methods in MethodCache
                        if (MethodCallsKnownMethod(methodSymbol))
                        {
                            // For simplicity, analyze all parameters
                            var parameterIndices = Enumerable.Range(0, methodSymbol.Parameters.Length).ToList();
                            var methodConfig = new MethodConfig(methodSymbol.ContainingType.Name, methodSymbol.Name, parameterIndices);

                            _methodCache[methodKey] = methodConfig;
                        }
                    }
                }
            }

            private bool MethodCallsKnownMethod(IMethodSymbol methodSymbol)
            {
                if (_methodCallCache.TryGetValue(methodSymbol, out bool result))
                {
                    return result;
                }

                // Prevent recursion
                _methodCallCache[methodSymbol] = false;

                foreach (var syntaxRef in methodSymbol.DeclaringSyntaxReferences)
                {
                    var methodDeclaration = syntaxRef.GetSyntax() as MethodDeclarationSyntax;
                    if (methodDeclaration == null)
                        continue;

                    var semanticModel = _compilation.GetSemanticModel(methodDeclaration.SyntaxTree);

                    var invocations = methodDeclaration.DescendantNodes()
                        .OfType<InvocationExpressionSyntax>();

                    foreach (var invocation in invocations)
                    {
                        var invokedMethodSymbol = semanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
                        if (invokedMethodSymbol == null)
                            continue;

                        // Check if the invoked method is in MethodCache
                        var invokedMethodKey = (invokedMethodSymbol.ContainingType.Name, invokedMethodSymbol.Name);
                        if (_methodCache.ContainsKey(invokedMethodKey))
                        {
                            _methodCallCache[methodSymbol] = true;
                            return true;
                        }

                        // Recursively check if the invoked method calls a known method
                        if (invokedMethodSymbol.MethodKind == MethodKind.Ordinary && !invokedMethodSymbol.Equals(methodSymbol))
                        {
                            if (MethodCallsKnownMethod(invokedMethodSymbol))
                            {
                                _methodCallCache[methodSymbol] = true;
                                return true;
                            }
                        }
                    }
                }

                _methodCallCache[methodSymbol] = false;
                return false;
            }

            public void AnalyzeBinaryExpression(SyntaxNodeAnalysisContext context)
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
                    foreach (var prefabName in StringPool.toNumber.Keys)
                    {
                        var shortName = Path.GetFileNameWithoutExtension(prefabName);
                        if (shortName.Equals(stringValue, StringComparison.OrdinalIgnoreCase))
                        {
                            foundMatch = true;
                            break;
                        }
                    }

                    if (!foundMatch)
                    {
                        var suggestions = FindSimilarShortNames(stringValue);

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
                    if (!IsValidPrefabPath(stringValue))
                    {
                        var suggestions = FindSimilarPrefabs(stringValue);

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

            public void AnalyzeInvocationExpression(SyntaxNodeAnalysisContext context)
            {
                var invocation = (InvocationExpressionSyntax)context.Node;
                Debug.WriteLine($"Analyzing invocation at {invocation.GetLocation()}");

                // Skip generated code
                if (IsGeneratedCode(invocation.SyntaxTree, context.CancellationToken))
                    return;

                var semanticModel = context.SemanticModel;
                var methodSymbol = semanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
                if (methodSymbol == null)
                    return;

                var methodKey = (methodSymbol.ContainingType.Name, methodSymbol.Name);

                // Check if the method is in MethodCache
                if (!_methodCache.TryGetValue(methodKey, out var methodConfig))
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
                    var symbol = semanticModel.GetSymbolInfo(argument).Symbol;
                    if (symbol != null)
                    {
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

            private bool IsValidPrefabPath(string path)
            {
                if (string.IsNullOrWhiteSpace(path))
                    return false;

                path = path.ToLowerInvariant().Replace("\\", "/").Trim();
                return StringPool.toNumber.ContainsKey(path);
            }

            private string GetSuggestionMessage(string invalidPath)
            {
                var suggestions = FindSimilarPrefabs(invalidPath);

                if (suggestions.Any())
                {
                    return $" Invalid prefab path. Did you mean one of these?\n" +
                           string.Join("\n", suggestions.Select(s => $"  - {s}"));
                }

                return " Invalid prefab path. Make sure it starts with 'assets/prefabs/' and ends with '.prefab'";
            }

            private IEnumerable<string> FindSimilarPrefabs(string invalidPath)
            {
                invalidPath = invalidPath.ToLowerInvariant().Replace("\\", "/").Trim();

                return StringPool.toNumber.Keys
                    .Select(p => new { Path = p, Distance = GetLevenshteinDistance(p, invalidPath) })
                    .Where(x => x.Distance <= 5)
                    .OrderBy(x => x.Distance)
                    .Take(3)
                    .Select(x => x.Path);
            }

            private IEnumerable<string> FindSimilarShortNames(string shortName)
            {
                shortName = shortName.ToLowerInvariant();

                return StringPool.toNumber.Keys
                    .Select(p => Path.GetFileNameWithoutExtension(p))
                    .Distinct()
                    .Select(sn => new { ShortName = sn, Distance = GetLevenshteinDistance(sn, shortName) })
                    .Where(x => x.Distance <= 3)
                    .OrderBy(x => x.Distance)
                    .Take(3)
                    .Select(x => x.ShortName);
            }

            private Dictionary<string, Location> GetStringLiteralsFromSymbol(ISymbol symbol, SemanticModel semanticModel)
            {
                var literals = new Dictionary<string, Location>(StringComparer.OrdinalIgnoreCase);

                foreach (var syntaxRef in symbol.DeclaringSyntaxReferences)
                {
                    var node = syntaxRef.GetSyntax();
                    if (node is VariableDeclaratorSyntax variableDeclarator && variableDeclarator.Initializer != null)
                    {
                        var value = variableDeclarator.Initializer.Value;
                        if (value is LiteralExpressionSyntax literal)
                        {
                            literals[literal.Token.ValueText] = literal.GetLocation();
                        }
                    }
                    else if (node is ParameterSyntax parameterSyntax)
                    {
                        // For parameters, find where the method is called
                        var parameterSymbol = semanticModel.GetDeclaredSymbol(parameterSyntax) as IParameterSymbol;
                        if (parameterSymbol != null)
                        {
                            var methodSymbol = parameterSymbol.ContainingSymbol as IMethodSymbol;
                            if (methodSymbol != null)
                            {
                                var callers = FindMethodCallers(methodSymbol, semanticModel.Compilation);
                                foreach (var caller in callers)
                                {
                                    if (caller.ArgumentList.Arguments.Count > parameterSymbol.Ordinal)
                                    {
                                        var arg = caller.ArgumentList.Arguments[parameterSymbol.Ordinal].Expression;
                                        if (arg is LiteralExpressionSyntax argLiteral)
                                        {
                                            literals[argLiteral.Token.ValueText] = argLiteral.GetLocation();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                return literals;
            }

            private IEnumerable<InvocationExpressionSyntax> FindMethodCallers(
                IMethodSymbol methodSymbol,
                Compilation compilation)
            {
                var callers = new List<InvocationExpressionSyntax>();

                foreach (var tree in compilation.SyntaxTrees)
                {
                    var semanticModel = compilation.GetSemanticModel(tree);
                    var root = tree.GetRoot();

                    var invocations = root.DescendantNodes()
                        .OfType<InvocationExpressionSyntax>()
                        .Where(invocation =>
                        {
                            var symbolInfo = semanticModel.GetSymbolInfo(invocation);
                            var symbol = symbolInfo.Symbol;
                            return symbol != null && SymbolEqualityComparer.Default.Equals(symbol, methodSymbol);
                        });

                    callers.AddRange(invocations);
                }

                return callers;
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

            private bool IsGeneratedCode(SyntaxTree tree, CancellationToken cancellationToken)
            {
                if (tree == null)
                    throw new ArgumentNullException(nameof(tree));

                var root = tree.GetRoot(cancellationToken);
                foreach (var trivia in root.GetLeadingTrivia())
                {
                    if (trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) ||
                        trivia.IsKind(SyntaxKind.MultiLineCommentTrivia))
                    {
                        var text = trivia.ToFullString();
                        if (text.Contains("<auto-generated"))
                            return true;
                    }
                }
                return false;
            }

            // Helper method to compute Levenshtein distance
            private int GetLevenshteinDistance(string s, string t)
            {
                if (string.IsNullOrEmpty(s))
                    return t.Length;
                if (string.IsNullOrEmpty(t))
                    return s.Length;

                int[,] d = new int[s.Length + 1, t.Length + 1];

                for (int i = 0; i <= s.Length; i++)
                    d[i, 0] = i;
                for (int j = 0; j <= t.Length; j++)
                    d[0, j] = j;

                for (int i = 1; i <= s.Length; i++)
                {
                    for (int j = 1; j <= t.Length; j++)
                    {
                        int cost = s[i - 1] == t[j - 1] ? 0 : 1;

                        d[i, j] = Math.Min(
                            Math.Min(
                                d[i - 1, j] + 1,    // Deletion
                                d[i, j - 1] + 1),   // Insertion
                            d[i - 1, j - 1] + cost); // Substitution
                    }
                }

                return d[s.Length, t.Length];
            }
        }
    }
}
