using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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

            // Теперь подписываемся на MemberAccessExpression
            context.RegisterSyntaxNodeAction(AnalyzeMemberAccess, SyntaxKind.SimpleMemberAccessExpression);
        }

        private void AnalyzeMemberAccess(SyntaxNodeAnalysisContext context)
        {
            var memberAccess = (MemberAccessExpressionSyntax)context.Node;
            var semanticModel = context.SemanticModel;

            var expressionInfo = semanticModel.GetTypeInfo(memberAccess.Expression);
            var typeSymbol = expressionInfo.Type;
            var memberName = memberAccess.Name.Identifier.ValueText;

            Debug.WriteLine($"Analyzing member access: {memberName} in type {typeSymbol?.ToDisplayString() ?? "null"}");
            if (typeSymbol == null)
            {
                Debug.WriteLine("Type symbol is null, skipping");
                return;
            }

            // Проверяем, не существует ли уже символ (тогда всё нормально)
            var symbol = semanticModel.GetSymbolInfo(memberAccess.Name).Symbol;
            if (symbol != null)
            {
                Debug.WriteLine($"Member {memberName} exists, skipping");
                return;
            }

            // Ищем объявление класса
            var classDeclaration = memberAccess.FirstAncestorOrSelf<ClassDeclarationSyntax>();
            if (classDeclaration == null)
            {
                Debug.WriteLine("Class declaration not found, skipping");
                return;
            }
            Debug.WriteLine($"Class name: {classDeclaration.Identifier.Text}");

            // Смотрим, есть ли такой метод в данном классе
            var methodsInClass = classDeclaration.Members
                .OfType<MethodDeclarationSyntax>()
                .Where(m => m.Identifier.Text == memberName)
                .ToList();

            Debug.WriteLine($"Found {methodsInClass.Count} methods with name {memberName} in class {classDeclaration.Identifier.Text}");

            // Если метод существует, значит это ложная тревога
            if (methodsInClass.Count > 0)
            {
                Debug.WriteLine("Method exists in class, so skipping any error reports");
                return;
            }

            // Иначе ищем похожие члены (все доступные, не только public)
            var members = GetAllMembersWithSymbols(typeSymbol, semanticModel, context.Node.SpanStart).ToList();
            Debug.WriteLine($"Found {members.Count} members in type {typeSymbol}");

            var similarMembers = FindSimilarMembers(memberName, members)
                .OrderByDescending(m => m.Score)
                .Take(6)
                .ToList();

            Debug.WriteLine($"Found {similarMembers.Count} similar members");
            foreach (var member in similarMembers)
            {
                Debug.WriteLine($"Similar member: {member.DisplayName} (score: {member.Score}, reason: {member.Reason})");
            }

            if (!similarMembers.Any())
            {
                Debug.WriteLine("No similar members found, skipping");
                return;
            }

            var suggestions = string.Join(", ", similarMembers.Select(m => $"{m.DisplayName} ({m.Reason})"));
            Debug.WriteLine($"Reporting diagnostic with suggestions: {suggestions}");

            var diagnostic = Diagnostic.Create(
                Rule,
                memberAccess.Name.GetLocation(),
                typeSymbol.ToDisplayString(),
                memberName,
                suggestions);

            context.ReportDiagnostic(diagnostic);
        }

        /// <summary>
        /// Собираем все члены (поля, методы, свойства и т.д.), которые доступны из позиции position.
        /// </summary>
        private IEnumerable<(string Name, ISymbol Symbol)> GetAllMembersWithSymbols(
            ITypeSymbol type,
            SemanticModel semanticModel,
            int position)
        {
            // Берём всех членов типа, которые доступны из данной точки
            var members = type.GetMembers()
                .Where(m => semanticModel.IsAccessible(position, m))
                .Select(m => (m.Name, Symbol: m));

            // Рекурсивно идём по базовым классам
            if (type.BaseType != null)
            {
                members = members.Concat(GetAllMembersWithSymbols(type.BaseType, semanticModel, position));
            }

            // И по всем интерфейсам
            foreach (var iface in type.AllInterfaces)
            {
                members = members.Concat(GetAllMembersWithSymbols(iface, semanticModel, position));
            }

            // Убираем дубли по имени
            return members
                .GroupBy(m => m.Name)
                .Select(g => g.First());
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
                else if (CommonPrefixes.Any(p =>
                    target.StartsWith(p, System.StringComparison.OrdinalIgnoreCase) &&
                    memberName.StartsWith(p, System.StringComparison.OrdinalIgnoreCase)))
                {
                    score = 0.9;
                    reason = "common prefix";
                }
                // 3. Совпадение по словам (camelCase)
                else
                {
                    var commonWords = targetWords.Intersect(memberWords, System.StringComparer.OrdinalIgnoreCase).Count();
                    if (commonWords > 0)
                    {
                        score = 0.7 * commonWords / System.Math.Max(targetWords.Count, memberWords.Count);
                        reason = "similar words";
                    }
                }

                // 4. Расстояние Левенштейна
                if (score == 0)
                {
                    var distance = StringDistance.GetLevenshteinDistance(target, memberName);
                    var maxLength = System.Math.Max(target.Length, memberName.Length);
                    var similarity = 1 - (double)distance / maxLength;

                    if (similarity > 0.5)
                    {
                        score = similarity * 0.5;
                        reason = "similar spelling";
                    }
                }

                if (score > 0)
                {
                    var displayName = GetDisplayName(symbol);
                    results.Add(new SimilarMember
                    {
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

        private List<string> SplitCamelCase(string input)
        {
            return Regex.Split(input, @"(?<!^)(?=[A-Z])")
                .SelectMany(s => s.Split(new[] { '_' }, System.StringSplitOptions.RemoveEmptyEntries))
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList();
        }
    }
}
