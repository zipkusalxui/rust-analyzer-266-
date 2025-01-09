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

            var members = GetAllMembers(typeSymbol).ToList();
            var similarMembers = FindSimilarMembers(memberName, members)
                .OrderByDescending(m => m.Score)
                .Take(3)
                .ToList();

            if (!similarMembers.Any())
            {
                return;
            }

            var suggestions = string.Join(", ", similarMembers.Select(m => $"{m.Name} ({m.Reason})"));

            var diagnostic = Diagnostic.Create(
                Rule,
                memberAccess.Name.GetLocation(),
                typeSymbol.ToDisplayString(),
                memberName,
                suggestions);

            context.ReportDiagnostic(diagnostic);
        }

        private IEnumerable<SimilarMember> FindSimilarMembers(string target, IEnumerable<string> members)
        {
            var results = new List<SimilarMember>();
            var targetWords = SplitCamelCase(target);

            foreach (var member in members)
            {
                var memberWords = SplitCamelCase(member);
                double score = 0;
                string reason = "";

                // 1. Точное совпадение префикса
                if (member.StartsWith(target, System.StringComparison.OrdinalIgnoreCase))
                {
                    score = 1.0;
                    reason = "completion";
                }
                
                // 2. Совпадение по общим префиксам
                else if (CommonPrefixes.Any(p => target.StartsWith(p, System.StringComparison.OrdinalIgnoreCase) 
                    && member.StartsWith(p, System.StringComparison.OrdinalIgnoreCase)))
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
                    var distance = StringDistance.GetLevenshteinDistance(target, member);
                    var maxLength = System.Math.Max(target.Length, member.Length);
                    var similarity = 1 - (double)distance / maxLength;
                    
                    if (similarity > 0.5)
                    {
                        score = similarity * 0.5; // Понижаем вес для этой метрики
                        reason = "similar spelling";
                    }
                }

                if (score > 0)
                {
                    results.Add(new SimilarMember { Name = member, Score = score, Reason = reason });
                }
            }

            return results;
        }

        private List<string> SplitCamelCase(string input)
        {
            return Regex.Split(input, @"(?<!^)(?=[A-Z])")
                .SelectMany(s => s.Split(new[] { '_' }, System.StringSplitOptions.RemoveEmptyEntries))
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList();
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