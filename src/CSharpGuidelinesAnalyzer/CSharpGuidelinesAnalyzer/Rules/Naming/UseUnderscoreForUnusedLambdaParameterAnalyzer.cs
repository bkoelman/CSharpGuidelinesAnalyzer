using System;
using System.Collections.Immutable;
using System.Linq;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace CSharpGuidelinesAnalyzer.Rules.Naming
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class UseUnderscoreForUnusedLambdaParameterAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV1739";

        private const string Title = "Unused lambda parameter should be renamed to underscore(s)";
        private const string MessageFormat = "Unused {0} parameter '{1}' should be renamed to underscore(s).";
        private const string Description = "Use an underscore for irrelevant lambda parameters.";

        [NotNull]
        private static readonly AnalyzerCategory Category = AnalyzerCategory.Naming;

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category.DisplayName, DiagnosticSeverity.Info, true, Description, Category.GetHelpLinkUri(DiagnosticId));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        [NotNull]
        private static readonly Action<OperationAnalysisContext> AnalyzeLambdaExpressionAction =
            context => context.SkipInvalid(AnalyzeLambdaExpression);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterOperationAction(AnalyzeLambdaExpressionAction, OperationKind.AnonymousFunction);
        }

        private static void AnalyzeLambdaExpression(OperationAnalysisContext context)
        {
            var lambdaExpression = (IAnonymousFunctionOperation)context.Operation;

            if (!lambdaExpression.Symbol.Parameters.Any(IsRegularParameter))
            {
                return;
            }

            SyntaxNode bodySyntax = lambdaExpression.Symbol.TryGetBodySyntaxForMethod(context.CancellationToken);
            if (bodySyntax == null)
            {
                return;
            }

            AnalyzeParameterUsage(lambdaExpression.Symbol.Parameters, bodySyntax, context);
        }

        private static void AnalyzeParameterUsage([ItemNotNull] ImmutableArray<IParameterSymbol> parameters,
            [NotNull] SyntaxNode bodySyntax, OperationAnalysisContext context)
        {
            DataFlowAnalysis dataFlowAnalysis = TryAnalyzeDataFlow(bodySyntax, context.Compilation);
            if (dataFlowAnalysis == null)
            {
                return;
            }

            foreach (IParameterSymbol parameter in parameters)
            {
                if (IsRegularParameter(parameter) && !IsParameterUsed(parameter, dataFlowAnalysis))
                {
                    string functionKind = context.Operation.Syntax is AnonymousMethodExpressionSyntax
                        ? "anonymous method"
                        : "lambda";
                    context.ReportDiagnostic(Diagnostic.Create(Rule, parameter.Locations[0], functionKind, parameter.Name));
                }
            }
        }

        private static bool IsRegularParameter([NotNull] IParameterSymbol parameter)
        {
            return !parameter.IsSynthesized() && !ConsistsOfUnderscoresOnly(parameter.Name);
        }

        private static bool ConsistsOfUnderscoresOnly([NotNull] string identifierName)
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

        [CanBeNull]
        private static DataFlowAnalysis TryAnalyzeDataFlow([NotNull] SyntaxNode bodySyntax, [NotNull] Compilation compilation)
        {
            SemanticModel model = compilation.GetSemanticModel(bodySyntax.SyntaxTree);
            return model.SafeAnalyzeDataFlow(bodySyntax);
        }

        private static bool IsParameterUsed([NotNull] IParameterSymbol parameter, [NotNull] DataFlowAnalysis dataFlowAnalysis)
        {
            return dataFlowAnalysis.ReadInside.Contains(parameter) || dataFlowAnalysis.WrittenInside.Contains(parameter) ||
                dataFlowAnalysis.Captured.Contains(parameter);
        }
    }
}
