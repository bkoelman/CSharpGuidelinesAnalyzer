using System;
using System.Collections.Immutable;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace CSharpGuidelinesAnalyzer.Rules.Maintainability
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DoNotNestMethodCallsAnalyzer : DiagnosticAnalyzer
    {
        private const string Title = "Method argument calls a nested method";
        private const string MessageFormat = "Argument for parameter '{0}' in method call to '{1}' calls nested method '{2}'.";
        private const string Description = "Write code that is easy to debug";

        public const string DiagnosticId = AnalyzerCategory.RulePrefix + "1580";

        [NotNull]
        private static readonly AnalyzerCategory Category = AnalyzerCategory.Maintainability;

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category.DisplayName,
            DiagnosticSeverity.Warning, true, Description, Category.GetHelpLinkUri(DiagnosticId));

        [NotNull]
        private static readonly Action<OperationAnalysisContext> AnalyzeArgumentAction = context => context.SkipInvalid(AnalyzeArgument);

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterOperationAction(AnalyzeArgumentAction, OperationKind.Argument);
        }

        private static void AnalyzeArgument(OperationAnalysisContext context)
        {
            var argument = (IArgumentOperation)context.Operation;

            if (argument.Value is IInvocationOperation invocation)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.Syntax.GetLocation(), argument.Parameter.Name,
                    argument.Parameter.ContainingSymbol, invocation.TargetMethod));
            }
        }
    }
}
