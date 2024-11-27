using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;
using System.IO;
using System.Diagnostics;

namespace RustAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class BaseNetworkableAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "RUST0005";
        
        private static readonly string[] PossibleBaseNetworkableNames = new[]
        {
            "BaseNetworkable",
            "global::BaseNetworkable"
        };

        private static readonly string Title = "Invalid PrefabName comparison";
        private static readonly string MessageFormat = "String '{0}' does not exist in StringPool. Make sure to use a valid prefab name and StringPool.Get()";
        private static readonly string Description = "Direct string comparisons with BaseNetworkable's PrefabName or ShortPrefabName should use StringPool.Get() and the string must exist in StringPool.";

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
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeBinaryExpression, SyntaxKind.EqualsExpression);
        }

        private void AnalyzeBinaryExpression(SyntaxNodeAnalysisContext context)
        {
            var binaryExpression = (BinaryExpressionSyntax)context.Node;
            
            // Check for BaseNetworkable property access
            var leftMemberAccess = binaryExpression.Left as MemberAccessExpressionSyntax;
            var rightMemberAccess = binaryExpression.Right as MemberAccessExpressionSyntax;

            // Check for string literal
            var leftLiteral = binaryExpression.Left as LiteralExpressionSyntax;
            var rightLiteral = binaryExpression.Right as LiteralExpressionSyntax;

            if (!IsValidPrefabNameComparison(context, leftMemberAccess, leftLiteral, rightMemberAccess, rightLiteral))
            {
                Debug.WriteLine("IsValidPrefabNameComparison - false");
                return;
            }

            Debug.WriteLine("IsValidPrefabNameComparison - true");

            var literalExpression = leftLiteral ?? rightLiteral;
            var memberAccess = leftMemberAccess ?? rightMemberAccess;
            var stringValue = literalExpression.Token.ValueText;

            // Check if this is ShortPrefabName comparison
            bool isShortPrefabName = memberAccess.Name.Identifier.Text == "ShortPrefabName";

            Debug.WriteLine($"isShortPrefabName - {isShortPrefabName}");
            Debug.WriteLine($"Checking string value: {stringValue}");
            Debug.WriteLine($"StringPool.toNumber is null: {StringPool.toNumber == null}");
            if (StringPool.toNumber != null)
            {
                Debug.WriteLine($"StringPool.toNumber count: {StringPool.toNumber.Count}");
                Debug.WriteLine($"StringPool contains key: {StringPool.toNumber.ContainsKey(stringValue)}");
                if (StringPool.toNumber.Count > 0)
                {
                    Debug.WriteLine($"First few keys in StringPool: {string.Join(", ", StringPool.toNumber.Keys.Take(5))}");
                }
            }

            if (isShortPrefabName)
            {
                // For ShortPrefabName, we need to check if any PrefabName in StringPool would match when shortened
                bool foundMatch = false;
                if (StringPool.toNumber != null)
                {
                    foreach (var prefabName in StringPool.toNumber.Keys)
                    {
                        var shortName = Path.GetFileNameWithoutExtension(prefabName);
                        Debug.WriteLine($"Comparing short name: {shortName} with {stringValue}");
                        if (shortName == stringValue)
                        {
                            foundMatch = true;
                            break;
                        }
                    }
                }

                if (!foundMatch)
                {
                    var diagnostic = Diagnostic.Create(
                        Rule,
                        literalExpression.GetLocation(),
                        stringValue);
                    context.ReportDiagnostic(diagnostic);
                }
            }
            else
            {
                // For PrefabName, directly check if it exists in StringPool
                if (StringPool.toNumber == null || !StringPool.toNumber.ContainsKey(stringValue))
                {
                    var diagnostic = Diagnostic.Create(
                        Rule,
                        literalExpression.GetLocation(),
                        stringValue);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }

        private bool IsValidPrefabNameComparison(
            SyntaxNodeAnalysisContext context,
            MemberAccessExpressionSyntax leftMember,
            LiteralExpressionSyntax leftLiteral,
            MemberAccessExpressionSyntax rightMember,
            LiteralExpressionSyntax rightLiteral)
        {
            // We need exactly one member access and one string literal
            if ((leftMember == null && rightMember == null) || (leftLiteral == null && rightLiteral == null))
            {
                Debug.WriteLine("Failed: No member access or string literal found");
                return false;
            }

            if ((leftMember != null && rightMember != null) || (leftLiteral != null && rightLiteral != null))
            {
                Debug.WriteLine("Failed: Found both members or both literals");
                return false;
            }

            var memberAccess = leftMember ?? rightMember;
            
            // Check if the member is PrefabName or ShortPrefabName on BaseNetworkable
            if (memberAccess == null)
            {
                Debug.WriteLine("Failed: memberAccess is null");
                return false;
            }

            var propertyName = memberAccess.Name.Identifier.Text;
            Debug.WriteLine($"Property name: {propertyName}");
            
            if (!(propertyName == "PrefabName" || propertyName == "ShortPrefabName"))
            {
                Debug.WriteLine($"Failed: Property {propertyName} is not PrefabName or ShortPrefabName");
                return false;
            }

            // Get the type info
            var typeInfo = context.SemanticModel.GetTypeInfo(memberAccess.Expression);
            var objectType = typeInfo.Type;
            if (objectType == null)
            {
                Debug.WriteLine("Failed: objectType is null");
                return false;
            }

            Debug.WriteLine($"Checking type: {objectType.ToDisplayString()}");

            // Check if the type is BaseNetworkable or any of its descendants
            var currentType = objectType;
            while (currentType != null)
            {
                var currentTypeName = currentType.ToDisplayString();
                
                // Check all possible BaseNetworkable names
                if (PossibleBaseNetworkableNames.Contains(currentTypeName))
                {
                    Debug.WriteLine($"Success: Found BaseNetworkable type: {currentTypeName}");
                    return true;
                }
                    
                currentType = currentType.BaseType;
            }

            Debug.WriteLine("Failed: Type is not BaseNetworkable or its descendant");
            return false;
        }
    }
}
