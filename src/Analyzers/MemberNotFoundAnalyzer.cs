using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

namespace RustAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MemberNotFoundAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "RUST004";
        private const string Category = "Usage";

        private static readonly LocalizableString Title = "Member not found";
        private static readonly LocalizableString MessageFormat = 
            "{0} does not contain a definition for '{1}'.\n" +
            "Similar members found: {2}";
        private static readonly LocalizableString Description = 
            "The member was not found, but there are similar members available.";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            Title,
            MessageFormat,
            Category,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => 
            ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            
            // Подписываемся только на MemberAccessExpression
            context.RegisterSyntaxNodeAction(AnalyzeMemberAccess, SyntaxKind.SimpleMemberAccessExpression);
        }

        private void AnalyzeMemberAccess(SyntaxNodeAnalysisContext context)
        {
            var memberAccess = (MemberAccessExpressionSyntax)context.Node;
            var semanticModel = context.SemanticModel;
            
            var expressionInfo = semanticModel.GetTypeInfo(memberAccess.Expression);
            var typeSymbol = expressionInfo.Type;
            var memberName = memberAccess.Name.Identifier.ValueText;

            if (typeSymbol == null)
            {
                return;
            }

            // Проверяем существует ли член
            var symbol = semanticModel.GetSymbolInfo(memberAccess.Name).Symbol;
            if (symbol != null)
            {
                return; // Член существует, пропускаем
            }

            var members = GetAllMembers(typeSymbol).ToList();
            var similarMembers = members
                .Where(m => StringDistance.GetLevenshteinDistance(memberName, m) <= 3)
                .OrderBy(m => StringDistance.GetLevenshteinDistance(memberName, m))
                .Take(3)
                .ToList();

            if (!similarMembers.Any())
            {
                return;
            }

            var diagnostic = Diagnostic.Create(
                Rule,
                memberAccess.Name.GetLocation(),
                typeSymbol.ToDisplayString(),
                memberName,
                string.Join(", ", similarMembers));

            context.ReportDiagnostic(diagnostic);
        }

        private IEnumerable<string> GetAllMembers(ITypeSymbol type)
        {
            var members = type.GetMembers()
                .Where(m => m.DeclaredAccessibility == Accessibility.Public)
                .Select(m => m.Name)
                .Distinct();

            if (type.BaseType != null)
            {
                members = members.Concat(GetAllMembers(type.BaseType));
            }

            foreach (var iface in type.AllInterfaces)
            {
                members = members.Concat(GetAllMembers(iface));
            }

            return members.Distinct();
        }
    }
} 