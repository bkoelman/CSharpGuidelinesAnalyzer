using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Rules.Maintainability;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidConditionWithDoubleNegationAnalyzer : DiagnosticAnalyzer
{
    private const string Title = "Logical not operator is applied on a member which has a negation in its name";
    private const string MessageFormat = "Logical not operator is applied on {0} '{1}', which has a negation in its name";
    private const string Description = "Avoid conditions with double negatives.";

    public const string DiagnosticId = AnalyzerCategory.RulePrefix + "1502";

    [NotNull]
    private static readonly AnalyzerCategory Category = AnalyzerCategory.Maintainability;

    [NotNull]
    private static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category.DisplayName, DiagnosticSeverity.Warning, true,
        Description, Category.GetHelpLinkUri(DiagnosticId));

    [ItemNotNull]
    private static readonly ImmutableArray<string> NegatingWords = ImmutableArray.Create("no", "not");

    [ItemNotNull]
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize([NotNull] AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterSyntaxNodeAction(AnalyzeNotExpression, SyntaxKind.LogicalNotExpression);
    }

    private static void AnalyzeNotExpression(SyntaxNodeAnalysisContext context)
    {
        var notExpression = (PrefixUnaryExpressionSyntax)context.Node;

        ISymbol symbol = TryGetNegatingSymbol(notExpression.Operand, context.SemanticModel, context.CancellationToken);

        if (symbol != null)
        {
            Location location = notExpression.GetLocation();

            var diagnostic = Diagnostic.Create(Rule, location, symbol.GetKind().ToLowerInvariant(), symbol.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }

    [CanBeNull]
    private static ISymbol TryGetNegatingSymbol([CanBeNull] ExpressionSyntax operand, [NotNull] SemanticModel model, CancellationToken cancellationToken)
    {
        if (operand != null)
        {
            ISymbol symbol = model.GetSymbolInfo(operand, cancellationToken).Symbol;

            if (symbol != null && ContainsNegatingWord(symbol.Name))
            {
                return symbol;
            }
        }

        return null;
    }

    private static bool ContainsNegatingWord([NotNull] string name)
    {
        return name.GetWordsInList(NegatingWords).Any();
    }
}
