using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace RustAnalyzer
{
    /// <summary>
    /// Analyzer that detects empty methods in Rust plugins.
    /// Empty methods might indicate incomplete implementation or forgotten code.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class EmptyMethodAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "RUST001";
        private const string Category = "Design";

        private static readonly LocalizableString Title = "Empty method detected";
        private static readonly LocalizableString MessageFormat = "Method '{0}' has an empty body";
        private static readonly LocalizableString Description = "Methods should contain implementation and not be empty.";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            Title,
            MessageFormat,
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: Description,
            helpLinkUri: "https://github.com/legov/rust-analyzer/blob/main/docs/RUST001.md");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            
            context.RegisterSyntaxNodeAction(AnalyzeMethodDeclaration, SyntaxKind.MethodDeclaration);
        }

        private static void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
        {
            var methodDeclaration = (MethodDeclarationSyntax)context.Node;
            var methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodDeclaration);

            if (methodSymbol == null)
                return;

            // Skip special methods
            if (methodSymbol.MethodKind != MethodKind.Ordinary)
                return;

            // Skip methods without body (abstract/interface methods)
            if (methodDeclaration.Body == null)
                return;

            // Skip methods with expression body
            if (methodDeclaration.ExpressionBody != null)
                return;

            // Check if the method body is empty (contains no statements)
            if (methodDeclaration.Body.Statements.Count == 0)
            {
                var diagnostic = Diagnostic.Create(
                    Rule,
                    methodDeclaration.GetLocation(),
                    methodDeclaration.Identifier.Text);

                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
