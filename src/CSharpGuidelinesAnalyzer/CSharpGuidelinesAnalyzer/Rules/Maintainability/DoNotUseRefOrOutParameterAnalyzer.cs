using System.Collections.Immutable;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Rules.Maintainability
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DoNotDeclareRefOrOutParameterAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV1562";

        private const string Title = "Do not declare a parameter as ref or out";
        private const string MessageFormat = "Parameter '{0}' is declared as ref or out.";
        private const string Description = "Don't use ref or out parameters.";

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

            context.RegisterSyntaxNodeAction(c => c.SkipEmptyName(AnalyzeParameter), SyntaxKind.Parameter);
        }

        private void AnalyzeParameter(SymbolAnalysisContext context)
        {
            var parameter = (IParameterSymbol)context.Symbol;

            if (parameter.ContainingSymbol.IsDeconstructor() || parameter.IsSynthesized())
            {
                return;
            }

            if (!IsRefOrOutParameter(parameter) || IsOutParameterInTryParseMethod(parameter))
            {
                return;
            }

            AnalyzeRefParameter(parameter, context);
        }

        private static bool IsRefOrOutParameter([NotNull] IParameterSymbol parameter)
        {
            return parameter.RefKind == RefKind.Ref || parameter.RefKind == RefKind.Out;
        }

        private static bool IsOutParameterInTryParseMethod([NotNull] IParameterSymbol parameter)
        {
            return parameter.RefKind == RefKind.Out && parameter.ContainingSymbol.Name == "TryParse";
        }

        private void AnalyzeRefParameter([NotNull] IParameterSymbol parameter, SymbolAnalysisContext context)
        {
            ISymbol containingMember = parameter.ContainingSymbol;

            if (!containingMember.IsOverride && !containingMember.HidesBaseMember(context.CancellationToken) &&
                !parameter.IsInterfaceImplementation())
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, parameter.Locations[0], parameter.Name));
            }
        }
    }
}
