using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using RustAnalyzer.Utils;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace RustAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MemberNotFoundAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "RUST004";
        private const string Category = "Usage";

        private static readonly LocalizableString Title = "Member not found";
        private static readonly LocalizableString MessageFormat =
            "error[E0599]: no {0} named `{1}` found for type `{2}` in the current scope\n" +
            "  --> {4}:{5}:{6}\n" +
            "   |\n" +
            "{5,2} | {7}\n" + // Source line
            "   | {8} {0} not found in `{2}`\n" + // Pointer and explanation
            "   |\n" +
            "   = note: the type `{2}` does not have a {0} named `{1}`\n" +
            "   = help: did you mean one of these?\n" +
            "{3}";

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
            if (context == null) return;

            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

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
                return;

            var symbol = semanticModel.GetSymbolInfo(memberAccess.Name).Symbol;
            if (symbol != null)
                return;

            // Retrieve all members of the type, excluding compiler-generated and inaccessible members
            var members = typeSymbol.GetMembers()
                .Where(m => !IsCompilerGenerated(m) && IsAccessibleMember(m))
                .ToList();

            // Find similar members
            var suggestions = FindSimilarMembers(memberName, members);

            // If no suggestions are found, add a fallback message
            if (string.IsNullOrEmpty(suggestions))
                suggestions = "           - (no similar members)";

            // Get the location and line information
            var location = memberAccess.Name.GetLocation();
            var lineSpan = location.GetLineSpan();
            var startLinePosition = lineSpan.StartLinePosition;

            var sourceText = location.SourceTree?.GetText();
            if (sourceText == null) return;

            var lineText = sourceText.Lines[startLinePosition.Line].ToString();

            // Compute visual column and create the pointer line
            int charColumn = startLinePosition.Character;
            int visualColumn = TextAlignmentUtils.ComputeVisualColumn(lineText, charColumn);
            string pointerLine = TextAlignmentUtils.CreatePointerLine(lineText, charColumn, memberName.Length);

            // Retrieve the file name
            var fileName = System.IO.Path.GetFileName(location.SourceTree?.FilePath ?? string.Empty);

            // Create the diagnostic
            var diagnostic = Diagnostic.Create(
                Rule,
                location,
                DetermineMemberKind(memberAccess, semanticModel), // {0} - "method", "property", etc.
                memberName,                                       // {1} - Member name
                typeSymbol.ToDisplayString(),                    // {2} - Type where the member is missing
                suggestions,                                     // {3} - Similar members
                fileName,                                        // {4} - File name
                startLinePosition.Line + 1,                      // {5} - Line number
                charColumn + 1,                                  // {6} - Column number
                lineText,                                        // {7} - Source line
                pointerLine                                      // {8} - Pointer line
            );

            context.ReportDiagnostic(diagnostic);
        }

        private string DetermineMemberKind(MemberAccessExpressionSyntax memberAccess, SemanticModel semanticModel)
        {
            if (memberAccess.Parent is InvocationExpressionSyntax)
                return "method";

            var binding = semanticModel.GetSymbolInfo(memberAccess);
            if (binding.CandidateSymbols.Any(s => s is IMethodSymbol))
                return "method";
            if (binding.CandidateSymbols.Any(s => s is IPropertySymbol))
                return "property";
            if (binding.CandidateSymbols.Any(s => s is IFieldSymbol))
                return "field";

            return "member";
        }

        private string FindSimilarMembers(string targetName, List<ISymbol> members)
        {
            var similarMembers = members
                .Select(m => new
                {
                    Symbol = m,
                    Score = CalculateSimilarityScore(targetName, m.Name)
                })
                .Where(x => x.Score > 0)
                .OrderByDescending(x => x.Score)
                .ThenBy(x => x.Symbol.Name)
                .Take(5) // Limit to 5 suggestions
                .Select(x => FormatMemberSuggestion(x.Symbol))
                .ToList();

            return string.Join("\n", similarMembers);
        }

        private double CalculateSimilarityScore(string target, string candidate)
        {
            double score = 0.0;

            // Exact prefix match
            if (candidate.StartsWith(target, System.StringComparison.OrdinalIgnoreCase))
            {
                score += 10.0;
            }

            // Substring match
            if (candidate.IndexOf(target, System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                score += 5.0;
            }

            // Levenshtein distance
            int distance = StringDistance.GetLevenshteinDistance(target, candidate);
            if (distance <= 3)
            {
                score += 5.0 - distance; // Closer distance gets higher score
            }

            return score;
        }

        private string FormatMemberSuggestion(ISymbol symbol)
        {
            // For methods, include their signatures
            if (symbol is IMethodSymbol methodSymbol)
            {
                string parameters = string.Join(", ", methodSymbol.Parameters.Select(p => $"{p.Type.ToDisplayString()} {p.Name}"));
                return $"           - `{methodSymbol.Name}({parameters})`";
            }

            // For other members, just return their name
            return $"           - `{symbol.Name}`";
        }

        private bool IsCompilerGenerated(ISymbol symbol)
        {
            // Exclude compiler-generated members
            return symbol.Name.StartsWith("<") && symbol.Name.EndsWith(">");
        }

        private bool IsAccessibleMember(ISymbol symbol)
        {
            // Only include accessible members (public, protected, internal, etc.)
            return symbol.DeclaredAccessibility != Accessibility.Private;
        }
    }
}
