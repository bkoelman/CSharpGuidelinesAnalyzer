using System.Collections.Immutable;
using System.Linq;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace CSharpGuidelinesAnalyzer.Rules.Naming
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class UseDiscardForUnusedParameterAnalyzer : GuidelineAnalyzer
    {
        // TODO: Also check for non-lambda parameters
        // TODO: Make sure using a discard (not multiple underscores)

        public const string DiagnosticId = "AV1739";

        private const string Title = "Unused parameter should be replaced by discard";
        private const string MessageFormat = "Unused parameter '{0}' should be replaced by a discard.";
        private const string Description = "Use an underscore for irrelevant parameters.";

        [NotNull]
        private static readonly AnalyzerCategory Category = AnalyzerCategory.Naming;

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category.Name, DiagnosticSeverity.Info, true, Description, Category.GetHelpLinkUri(DiagnosticId));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterOperationAction(c => c.SkipInvalid(AnalyzeLambdaExpression), OperationKind.AnonymousFunction);
        }

        private void AnalyzeLambdaExpression(OperationAnalysisContext context)
        {

            var lambdaExpression = (IAnonymousFunctionOperation)context.Operation;

            foreach (IParameterSymbol parameter in lambdaExpression.Symbol.Parameters)
            {
                if (!IsSynthesized(parameter) && !ConsistsOfUnderscoresOnly(parameter.Name))
                {
                    AnalyzeParameterUsage(parameter, lambdaExpression.Symbol, context);
                }
            }
        }

        private bool IsSynthesized([NotNull] IParameterSymbol parameter)
        {
            return !parameter.Locations.Any();
        }

        private bool ConsistsOfUnderscoresOnly([NotNull] string identifierName)
        {
            foreach (char ch in identifierName)
            {
                if (ch != '_')
                {
                    return false;
                }
            }

            return true;
        }

        private static void AnalyzeParameterUsage([NotNull] IParameterSymbol parameter, [NotNull] IMethodSymbol method,
            OperationAnalysisContext context)
        {
            SyntaxNode body = method.TryGetBodySyntaxForMethod(context.CancellationToken);
            if (body != null)
            {
                SemanticModel model = context.Compilation.GetSemanticModel(body.SyntaxTree);
                DataFlowAnalysis dataFlowAnalysis = model.AnalyzeDataFlow(body);
                if (dataFlowAnalysis.Succeeded)
                {
                    if (!dataFlowAnalysis.ReadInside.Contains(parameter) && !dataFlowAnalysis.WrittenInside.Contains(parameter) &&
                        !dataFlowAnalysis.Captured.Contains(parameter))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Rule, parameter.Locations[0], parameter.Name));
                    }
                }
            }
        }
    }
}
