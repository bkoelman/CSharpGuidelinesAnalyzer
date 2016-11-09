using System.Collections.Immutable;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Semantics;

namespace CSharpGuidelinesAnalyzer.Naming
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class UseUnderscoresForUnusedLambdaParametersAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV1739";

        private const string Title = "Unused lambda parameter should be renamed to one or more underscores";

        private const string MessageFormat =
            "Unused lambda parameter '{0}' should be renamed to one or more underscores.";

        private const string Description = "Use an underscore for irrelevant lambda parameters.";
        private const string Category = "Naming";

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category, DiagnosticSeverity.Warning, true, Description, HelpLinkUris.GetForCategory(Category, DiagnosticId));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(startContext =>
            {
                if (startContext.Compilation.SupportsOperations())
                {
                    startContext.RegisterOperationAction(AnalyzeLambdaExpression, OperationKind.LambdaExpression);
                }
            });
        }

        private void AnalyzeLambdaExpression(OperationAnalysisContext context)
        {
            var lambdaExpression = (ILambdaExpression) context.Operation;

            foreach (IParameterSymbol parameter in lambdaExpression.Signature.Parameters)
            {
                if (parameter.Name.Length == 0)
                {
                    continue;
                }

                if (!ConsistsOfUnderscoresOnly(parameter.Name))
                {
                    AnalyzeParameterUsage(parameter, lambdaExpression.Signature, context);
                }
            }
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
                    if (!dataFlowAnalysis.ReadInside.Contains(parameter) &&
                        !dataFlowAnalysis.WrittenInside.Contains(parameter) &&
                        !dataFlowAnalysis.Captured.Contains(parameter))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Rule, parameter.Locations[0], parameter.Name));
                    }
                }
            }
        }
    }
}