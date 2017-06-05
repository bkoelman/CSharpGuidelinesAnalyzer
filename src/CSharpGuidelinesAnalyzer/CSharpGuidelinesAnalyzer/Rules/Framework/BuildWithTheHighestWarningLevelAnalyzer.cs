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

        private const string Title = "Compiler warning level is set too low";
        private const string MessageFormat = "Build with warning level 4.";
        private const string Description = "Build with the highest warning level.";
        private const string Category = "Framework";

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category,
            DiagnosticSeverity.Warning, true, Description, HelpLinkUris.GetForCategory(Category, DiagnosticId));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationAction(AnalyzeWarningLevel);
        }

        private void AnalyzeWarningLevel(CompilationAnalysisContext context)
        {
            if (context.Compilation.Options.WarningLevel < 4)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, Location.None));
            }
        }
    }
}
