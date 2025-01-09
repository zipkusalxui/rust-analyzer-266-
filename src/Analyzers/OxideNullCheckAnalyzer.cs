using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;
using RustAnalyzer;

namespace OxideAnalyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class OxideNullCheckAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "OXD001";

        private static readonly LocalizableString Title = 
            "Property should be checked for null";

        private static readonly LocalizableString MessageFormat = 
            "Property '{0}' at line {1} should be checked for null before use";

        private static readonly LocalizableString Description = 
            "Properties in Oxide/uMod plugins should be checked for null to prevent NullReferenceException.";

        private const string Category = "Usage";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId, 
            Title, 
            MessageFormat, 
            Category, 
            DiagnosticSeverity.Warning, 
            isEnabledByDefault: true, 
            description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => 
            ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(
                GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
        }

        private void AnalyzeMethod(SyntaxNodeAnalysisContext context)
        {
            var methodDeclaration = (MethodDeclarationSyntax)context.Node;
            var methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodDeclaration);
            
            if (methodSymbol == null) return;

            if (!HooksConfiguration.IsHook(methodSymbol) && 
                !PluginHooksConfiguration.IsHook(methodSymbol))
                return;

            var parameters = methodDeclaration.ParameterList.Parameters;
            
            foreach (var parameter in parameters)
            {
                if (parameter.Type == null) continue;
                
                var parameterType = context.SemanticModel.GetTypeInfo(parameter.Type);
                if (parameterType.Type == null || parameterType.Type.IsValueType)
                    continue;

                // Находим все обращения к свойствам параметра
                var memberAccesses = methodDeclaration.DescendantNodes()
                    .OfType<MemberAccessExpressionSyntax>()
                    .Where(ma => 
                    {
                        // Проверяем, что это обращение к нашему параметру
                        if (!(ma.Expression is IdentifierNameSyntax id) || 
                            id.Identifier.Text != parameter.Identifier.Text)
                            return false;

                        // Получаем символ члена
                        var memberSymbol = context.SemanticModel.GetSymbolInfo(ma).Symbol;
                        
                        // Проверяем что это свойство или поле, а не метод
                        return memberSymbol is IPropertySymbol || memberSymbol is IFieldSymbol;
                    })
                    .ToList();

                foreach (var memberAccess in memberAccesses)
                {
                    var memberName = memberAccess.Name.Identifier.Text;
                    var lineNumber = memberAccess.GetLocation()
                        .GetLineSpan()
                        .StartLinePosition
                        .Line + 1;

                    // Проверяем, находится ли это обращение внутри проверки на null
                    var isInsideNullCheck = false;
                    var currentNode = memberAccess.Parent;

                    // Собираем все проверки if до текущего использования
                    var previousChecks = methodDeclaration.DescendantNodes()
                        .OfType<IfStatementSyntax>()
                        .TakeWhile(ifStmt => ifStmt.SpanStart < memberAccess.SpanStart)
                        .Select(ifStmt => ifStmt.Condition.ToString())
                        .ToList();

                    // Проверяем все предыдущие условия
                    if (previousChecks.Any(condition => 
                        condition.Contains($"{parameter.Identifier.Text} == null") ||
                        condition.Contains($"{parameter.Identifier.Text}?.") ||
                        condition.Contains($"{parameter.Identifier.Text} is null") ||
                        condition.Contains($"{parameter.Identifier.Text}.{memberName} == null") ||
                        condition.Contains($"{parameter.Identifier.Text}?.{memberName}") ||
                        condition.Contains($"{parameter.Identifier.Text}.{memberName} is null")))
                    {
                        isInsideNullCheck = true;
                    }
                    else
                    {
                        // Проверяем родительские if'ы
                        while (currentNode != null && currentNode != methodDeclaration)
                        {
                            if (currentNode is IfStatementSyntax ifStatement)
                            {
                                var condition = ifStatement.Condition.ToString();
                                if (condition.Contains($"{parameter.Identifier.Text} == null") ||
                                    condition.Contains($"{parameter.Identifier.Text}?.") ||
                                    condition.Contains($"{parameter.Identifier.Text} is null") ||
                                    condition.Contains($"{parameter.Identifier.Text}.{memberName} == null") ||
                                    condition.Contains($"{parameter.Identifier.Text}?.{memberName}") ||
                                    condition.Contains($"{parameter.Identifier.Text}.{memberName} is null"))
                                {
                                    isInsideNullCheck = true;
                                    break;
                                }
                            }
                            currentNode = currentNode.Parent;
                        }
                    }

                    if (!isInsideNullCheck)
                    {
                        var diagnostic = Diagnostic.Create(Rule, 
                            memberAccess.GetLocation(), 
                            $"{parameter.Identifier.Text}.{memberName}",
                            lineNumber);
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }
    }
} 