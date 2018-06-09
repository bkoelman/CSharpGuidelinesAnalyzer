using System.Collections.Immutable;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace CSharpGuidelinesAnalyzer.Rules.Maintainability
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class AvoidUsingNamedArgumentAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV1555";

        private const string Title = "Avoid using non-(nullable-)boolean named arguments";
        private const string MessageFormat = "Parameter '{0}' in the call to '{1}' is invoked with a named argument.";
        private const string Description = "Avoid using named arguments.";

        [NotNull]
        private static readonly AnalyzerCategory Category = AnalyzerCategory.Maintainability;

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category.DisplayName, DiagnosticSeverity.Warning, true, Description, Category.GetHelpLinkUri(DiagnosticId));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterOperationAction(c => c.SkipInvalid(AnalyzeArgument), OperationKind.Argument);
        }

        private void AnalyzeArgument(OperationAnalysisContext context)
        {
            var argument = (IArgumentOperation)context.Operation;

            if (!argument.Parameter.Type.IsBooleanOrNullableBoolean())
            {
                var syntax = argument.Syntax as ArgumentSyntax;
                if (syntax?.NameColon != null)
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, syntax.NameColon.GetLocation(), argument.Parameter.Name,
                        FormatSymbol(argument.Parameter.ContainingSymbol)));
                }
            }
        }

        [NotNull]
        private string FormatSymbol([NotNull] ISymbol symbol)
        {
            return symbol.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat);
        }
    }
}
