using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

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

        private static readonly string[] CommonPrefixes = new[] { "is", "get", "set", "has", "can", "should", "will", "on" };

        private class SimilarMember
        {
            public string Name { get; set; }
            public string DisplayName { get; set; }
            public double Score { get; set; }
            public string Reason { get; set; }
        }

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

            var members = GetAllMembersWithSymbols(typeSymbol).ToList();
            var similarMembers = FindSimilarMembers(memberName, members)
                .OrderByDescending(m => m.Score)
                .Take(6)
                .ToList();

            if (!similarMembers.Any())
            {
                return;
            }

            var suggestions = string.Join(", ", similarMembers.Select(m => $"{m.DisplayName} ({m.Reason})"));

            var diagnostic = Diagnostic.Create(
                Rule,
                memberAccess.Name.GetLocation(),
                typeSymbol.ToDisplayString(),
                memberName,
                suggestions);

            context.ReportDiagnostic(diagnostic);
        }

        private IEnumerable<SimilarMember> FindSimilarMembers(string target, IEnumerable<(string Name, ISymbol Symbol)> members)
        {
            var results = new List<SimilarMember>();
            var targetWords = SplitCamelCase(target);

            foreach (var (memberName, symbol) in members)
            {
                var memberWords = SplitCamelCase(memberName);
                double score = 0;
                string reason = "";

                // 1. Точное совпадение префикса
                if (memberName.StartsWith(target, System.StringComparison.OrdinalIgnoreCase))
                {
                    score = 1.0;
                    reason = "completion";
                }
                
                // 2. Совпадение по общим префиксам
                else if (CommonPrefixes.Any(p => target.StartsWith(p, System.StringComparison.OrdinalIgnoreCase) 
                    && memberName.StartsWith(p, System.StringComparison.OrdinalIgnoreCase)))
                {
                    score = 0.9;
                    reason = "common prefix";
                }

                // 3. Совпадение по словам
                else
                {
                    var commonWords = targetWords.Intersect(memberWords, System.StringComparer.OrdinalIgnoreCase).Count();
                    if (commonWords > 0)
                    {
                        score = 0.7 * commonWords / System.Math.Max(targetWords.Count, memberWords.Count);
                        reason = "similar words";
                    }
                }

                // 4. Расстояние Левенштейна с учетом длины
                if (score == 0)
                {
                    var distance = StringDistance.GetLevenshteinDistance(target, memberName);
                    var maxLength = System.Math.Max(target.Length, memberName.Length);
                    var similarity = 1 - (double)distance / maxLength;
                    
                    if (similarity > 0.5)
                    {
                        score = similarity * 0.5; // Понижаем вес для этой метрики
                        reason = "similar spelling";
                    }
                }

                if (score > 0)
                {
                    var displayName = GetDisplayName(symbol);
                    results.Add(new SimilarMember { 
                        Name = memberName, 
                        DisplayName = displayName,
                        Score = score, 
                        Reason = reason 
                    });
                }
            }

            return results;
        }

        private string GetDisplayName(ISymbol symbol)
        {
            if (symbol is IMethodSymbol method)
            {
                var parameters = string.Join(", ", method.Parameters.Select(p => $"{p.Type.Name} {p.Name}"));
                return $"{method.Name}({parameters})";
            }
            return symbol.Name;
        }

        private IEnumerable<(string Name, ISymbol Symbol)> GetAllMembersWithSymbols(ITypeSymbol type)
        {
            var members = type.GetMembers()
                .Where(m => m.DeclaredAccessibility == Accessibility.Public)
                .Select(m => (m.Name, Symbol: m));

            if (type.BaseType != null)
            {
                members = members.Concat(GetAllMembersWithSymbols(type.BaseType));
            }

            foreach (var iface in type.AllInterfaces)
            {
                members = members.Concat(GetAllMembersWithSymbols(iface));
            }

            return members
                .GroupBy(m => m.Name)
                .Select(g => g.First());
        }

        private List<string> SplitCamelCase(string input)
        {
            return Regex.Split(input, @"(?<!^)(?=[A-Z])")
                .SelectMany(s => s.Split(new[] { '_' }, System.StringSplitOptions.RemoveEmptyEntries))
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList();
        }
    }
} 