using System.Collections.Immutable;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Rules.Framework
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class BuildWithTheHighestWarningLevelAnalyzer : GuidelineAnalyzer
    {
        public const string DiagnosticId = "AV2210";

        private const string Title = "Compiler warnings are not treated as errors or warning level is too low";
        private const string WarningLevelMessageFormat = "Build with warning level 4.";
        private const string WarningAsErrorMessageFormat = "Build with -warnaserror.";
        private const string Description = "Build with the highest warning level.";

        [NotNull]
        private static readonly AnalyzerCategory Category = AnalyzerCategory.Framework;

        [NotNull]
        private static readonly DiagnosticDescriptor WarningLevelRule = new DiagnosticDescriptor(DiagnosticId, Title,
            WarningLevelMessageFormat, Category.DisplayName, DiagnosticSeverity.Warning, true, Description,
            Category.GetHelpLinkUri(DiagnosticId));

        [NotNull]
        private static readonly DiagnosticDescriptor WarningAsErrorRule = new DiagnosticDescriptor(DiagnosticId, Title,
            WarningAsErrorMessageFormat, Category.DisplayName, DiagnosticSeverity.Warning, true, Description,
            Category.GetHelpLinkUri(DiagnosticId));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(WarningLevelRule, WarningAsErrorRule);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationAction(AnalyzeCompilationOptions);
        }

        private void AnalyzeCompilationOptions(CompilationAnalysisContext context)
        {
            CompilationOptions options = context.Compilation.Options;

            if (options.GeneralDiagnosticOption != ReportDiagnostic.Error)
            {
                context.ReportDiagnostic(Diagnostic.Create(WarningAsErrorRule, Location.None));
            }

            if (options.WarningLevel < 4)
            {
                context.ReportDiagnostic(Diagnostic.Create(WarningLevelRule, Location.None));
            }
        }
    }
}
