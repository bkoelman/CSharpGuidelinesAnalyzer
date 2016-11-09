using System.Collections.Immutable;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Rules.Maintainability
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class AvoidBooleanParametersAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV1564";

        private const string Title = "Parameter is of type bool or bool?";
        private const string MessageFormat = "Parameter '{0}' is of type '{1}'.";
        private const string Description = "Avoid methods that take a bool flag.";
        private const string Category = "Maintainability";

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category, DiagnosticSeverity.Warning, true, Description, HelpLinkUris.GetForCategory(Category, DiagnosticId));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSyntaxNodeAction(c => AnalyzeParameter(c.ToSymbolContext()), SyntaxKind.Parameter);
        }

        private void AnalyzeParameter(SymbolAnalysisContext context)
        {
            var parameter = (IParameterSymbol) context.Symbol;

            if (parameter.Name.Length == 0)
            {
                return;
            }

            if (parameter.Type.SpecialType == SpecialType.System_Boolean || parameter.Type.IsNullableBoolean())
            {
                AnalyzeBooleanParameter(parameter, context);
            }
        }

        private void AnalyzeBooleanParameter([NotNull] IParameterSymbol parameter, SymbolAnalysisContext context)
        {
            ISymbol containingMember = parameter.ContainingSymbol;
            if (containingMember.IsOverride)
            {
                return;
            }

            if (containingMember.HidesBaseMember(context.CancellationToken))
            {
                return;
            }

            if (parameter.IsInterfaceImplementation())
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(Rule, parameter.Locations[0], parameter.Name, parameter.Type));
        }
    }
}