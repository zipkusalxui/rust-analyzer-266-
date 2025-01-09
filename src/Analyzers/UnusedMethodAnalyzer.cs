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
        private static readonly LocalizableString MessageFormatWithHooks = "Method '{0}' is never used.\n" +
            "If you intended this to be a hook, no matching hook was found.\n" +
            "Similar hooks that might match: {1}";
        private static readonly LocalizableString MessageFormatWithPluginHooks = "Method '{0}' is never used.\n" +
            "If you intended this to be a hook, no matching hook was found.\n" +
            "Similar plugin hooks that might match: {1} (from plugins: {2})";
        private static readonly LocalizableString MessageFormatCommand = "Method '{0}' is never used.\n" +
            "If you intended this to be a command, here are the common command signatures:\n" +
            "[Command(\"name\")]\n" +
            "void CommandName(IPlayer player, string command, string[] args)\n\n" +
            "[ChatCommand(\"name\")]\n" +
            "void CommandName(BasePlayer player, string command, string[] args)\n\n" +
            "[ConsoleCommand(\"name\")]\n" +
            "void CommandName(ConsoleSystem.Arg args)";
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

            if (UnityHooksConfiguration.IsHook(methodSymbol))
            {
                return;
            }

            if (HooksConfiguration.IsHook(methodSymbol)) 
            {
                return;
            }

            if(PluginHooksConfiguration.IsHook(methodSymbol))
            {
                return;
            }

            // Check if method is used
            if (!IsMethodUsed(methodSymbol, context))
            {
                bool diagnosticReported = false;

                // Check if method name contains "Command"
                if (methodSymbol.Name.ToLower().Contains("command"))
                {
                    var commandDiagnostic = Diagnostic.Create(
                        new DiagnosticDescriptor(
                            DiagnosticId,
                            Title,
                            MessageFormatCommand,
                            Category,
                            DiagnosticSeverity.Warning,
                            isEnabledByDefault: true,
                            description: Description,
                            helpLinkUri: "https://github.com/legov/rust-analyzer/blob/main/docs/RUST003.md"),
                        methodSymbol.Locations[0],
                        methodSymbol.Name);

                    context.ReportDiagnostic(commandDiagnostic);
                    diagnosticReported = true;
                }

                // Get similar hook suggestions
                var similarHooks = HooksConfiguration.GetSimilarHooks(methodSymbol, 3)
                    .Concat(UnityHooksConfiguration.GetSimilarHooks(methodSymbol, 3));

                var pluginHooks = PluginHooksConfiguration.GetSimilarHooks(methodSymbol, 3).ToList();
                    
                var suggestionsText = string.Join(", ", similarHooks);
                
                if (pluginHooks.Any())
                {
                    var hookNames = string.Join(", ", pluginHooks.Select(h => h.hookName));
                    var pluginNames = string.Join(", ", pluginHooks.Select(h => h.pluginSource));

                    var pluginHookDiagnostic = Diagnostic.Create(
                        new DiagnosticDescriptor(
                            DiagnosticId,
                            Title,
                            MessageFormatWithPluginHooks,
                            Category,
                            DiagnosticSeverity.Warning,
                            isEnabledByDefault: true,
                            description: Description,
                            helpLinkUri: "https://github.com/legov/rust-analyzer/blob/main/docs/RUST003.md"),
                        methodSymbol.Locations[0],
                        methodSymbol.Name,
                        hookNames,
                        pluginNames);

                    context.ReportDiagnostic(pluginHookDiagnostic);
                    diagnosticReported = true;
                }
                else if (!string.IsNullOrEmpty(suggestionsText))
                {
                    var hookDiagnostic = Diagnostic.Create(
                        new DiagnosticDescriptor(
                            DiagnosticId,
                            Title,
                            MessageFormatWithHooks,
                            Category,
                            DiagnosticSeverity.Warning,
                            isEnabledByDefault: true,
                            description: Description,
                            helpLinkUri: "https://github.com/legov/rust-analyzer/blob/main/docs/RUST003.md"),
                        methodSymbol.Locations[0],
                        methodSymbol.Name,
                        suggestionsText);

                    context.ReportDiagnostic(hookDiagnostic);
                    diagnosticReported = true;
                }

                // If no special cases were handled, show the basic message
                if (!diagnosticReported)
                {
                    var basicDiagnostic = Diagnostic.Create(
                        Rule,
                        methodSymbol.Locations[0],
                        methodSymbol.Name);

                    context.ReportDiagnostic(basicDiagnostic);
                }
            }
        }

        private static bool IsMethodUsed(IMethodSymbol method, SyntaxNodeAnalysisContext context)
        {
            // Ignore override methods
            if (method.IsOverride)
            {
                return true;
            }

            // Получаем все вызовы методов в текущем файле
            var root = context.Node.SyntaxTree.GetRoot(context.CancellationToken);
            var allInvocations = root.DescendantNodes().OfType<InvocationExpressionSyntax>();

            // Проверяем каждый вызов метода
            foreach (var invocation in allInvocations)
            {
                var symbolInfo = context.SemanticModel.GetSymbolInfo(invocation);
                
                // Проверяем прямые вызовы
                if (symbolInfo.Symbol != null && SymbolComparer.Equals(symbolInfo.Symbol, method))
                {
                    return true;
                }

                // Проверяем вызовы через generic-методы
                if (symbolInfo.Symbol is IMethodSymbol invokedMethod && 
                    invokedMethod.OriginalDefinition != null && 
                    SymbolComparer.Equals(invokedMethod.OriginalDefinition, method))
                {
                    return true;
                }
            }

            // Проверяем использование метода в качестве делегата или события
            var memberAccesses = root.DescendantNodes().OfType<MemberAccessExpressionSyntax>();
            foreach (var memberAccess in memberAccesses)
            {
                var symbolInfo = context.SemanticModel.GetSymbolInfo(memberAccess);
                if (symbolInfo.Symbol != null && SymbolComparer.Equals(symbolInfo.Symbol, method))
                {
                    return true;
                }
            }

            // Проверяем использование в качестве аргумента метода
            var identifiers = root.DescendantNodes().OfType<IdentifierNameSyntax>();
            foreach (var identifier in identifiers)
            {
                var symbolInfo = context.SemanticModel.GetSymbolInfo(identifier);
                if (symbolInfo.Symbol != null && SymbolComparer.Equals(symbolInfo.Symbol, method))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
