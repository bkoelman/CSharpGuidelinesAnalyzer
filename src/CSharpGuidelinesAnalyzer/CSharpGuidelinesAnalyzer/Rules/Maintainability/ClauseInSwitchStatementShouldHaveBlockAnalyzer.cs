using System;
using System.Collections.Immutable;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Rules.Maintainability
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ClauseInSwitchStatementShouldHaveBlockAnalyzer : DiagnosticAnalyzer
    {
        private const string Title = "Missing block in case or default clause of switch statement";
        private const string MessageFormat = "Missing block in case or default clause of switch statement";
        private const string Description = "Always add a block after the keywords if, else, do, while, for, foreach and case.";

        public const string DiagnosticId = AnalyzerCategory.RulePrefix + "1535";

        [NotNull]
        private static readonly AnalyzerCategory Category = AnalyzerCategory.Maintainability;

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category.DisplayName,
            DiagnosticSeverity.Warning, true, Description, Category.GetHelpLinkUri(DiagnosticId));

        [NotNull]
        private static readonly Action<SyntaxNodeAnalysisContext> AnalyzeSwitchSectionAction = AnalyzeSwitchSection;

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSyntaxNodeAction(AnalyzeSwitchSectionAction, SyntaxKind.SwitchSection);
        }

        private static void AnalyzeSwitchSection(SyntaxNodeAnalysisContext context)
        {
            var switchSection = (SwitchSectionSyntax)context.Node;

            if (switchSection.Statements.Count > 0 && !SectionHasBlock(switchSection))
            {
                ReportAtLastLabel(switchSection, context);
            }
        }

        private static bool SectionHasBlock([NotNull] SwitchSectionSyntax switchSection)
        {
            return switchSection.Statements[0] is BlockSyntax;
        }

        private static void ReportAtLastLabel([NotNull] SwitchSectionSyntax switchSection, SyntaxNodeAnalysisContext context)
        {
            SwitchLabelSyntax lastLabel = switchSection.Labels.Last();
            Location location = lastLabel.Keyword.GetLocation();

            var diagnostic = Diagnostic.Create(Rule, location);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
