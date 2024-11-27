using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;
using System.Collections.Generic;

namespace RustAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class UnusedMethodAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "RUST003";
        private const string Category = "Design";

        private static readonly LocalizableString Title = "Unused method detected";
        private static readonly LocalizableString MessageFormat = "Method '{0}' is never used";
        private static readonly LocalizableString Description = "Methods should be used or removed to maintain clean code.";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            Title,
            MessageFormat,
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: Description,
            helpLinkUri: "https://github.com/legov/rust-analyzer/blob/main/docs/RUST003.md");

        private static readonly SymbolEqualityComparer SymbolComparer = SymbolEqualityComparer.Default;

        private static readonly Dictionary<SpecialType, string> SpecialTypeMap = new Dictionary<SpecialType, string>
        {
            { SpecialType.System_Boolean, "bool" },
            { SpecialType.System_Byte, "byte" },
            { SpecialType.System_SByte, "sbyte" },
            { SpecialType.System_Int16, "short" },
            { SpecialType.System_UInt16, "ushort" },
            { SpecialType.System_Int32, "int" },
            { SpecialType.System_UInt32, "uint" },
            { SpecialType.System_Int64, "long" },
            { SpecialType.System_UInt64, "ulong" },
            { SpecialType.System_Single, "float" },
            { SpecialType.System_Double, "double" },
            { SpecialType.System_Decimal, "decimal" },
            { SpecialType.System_Char, "char" },
            { SpecialType.System_String, "string" },
            { SpecialType.System_Object, "object" }
        };

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(AnalyzeMethodInvocations, SyntaxKind.MethodDeclaration);
        }

        private static void AnalyzeMethodInvocations(SyntaxNodeAnalysisContext context)
        {
            var methodDeclaration = (MethodDeclarationSyntax)context.Node;
            var methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodDeclaration);

            if (methodSymbol == null)
                return;

            // Skip special methods
            if (methodSymbol.MethodKind != MethodKind.Ordinary)
                return;

            // Skip methods with specific attributes
            var attributesToSkip = new[]
            {
                "ChatCommand",
                "Command",
                "ConsoleCommand",
                "HookMethod"
            };

            if (methodSymbol.GetAttributes().Any(attr =>
            {
                var attrName = attr.AttributeClass?.Name;
                return attrName != null && (
                    attributesToSkip.Contains(attrName) ||
                    attributesToSkip.Contains(attrName.Replace("Attribute", ""))
                );
            }))
            {
                return;
            }

            // Check if method is a hook
            if (HooksConfiguration.IsHook(methodSymbol))
            {
                return;
            }

            // Check if method is used
            if (!IsMethodUsed(methodSymbol, context))
            {
                var diagnostic = Diagnostic.Create(
                    Rule,
                    methodSymbol.Locations[0],
                    methodSymbol.Name);

                context.ReportDiagnostic(diagnostic);
            }
        }

        private static bool IsMethodUsed(IMethodSymbol method, SyntaxNodeAnalysisContext context)
        {
            var root = context.Node.SyntaxTree.GetRoot(context.CancellationToken);
            var invocations = root.DescendantNodes()
                .OfType<InvocationExpressionSyntax>();

            foreach (var invocation in invocations)
            {
                var symbolInfo = context.SemanticModel.GetSymbolInfo(invocation);
                if (symbolInfo.Symbol != null && SymbolComparer.Equals(symbolInfo.Symbol, method))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
