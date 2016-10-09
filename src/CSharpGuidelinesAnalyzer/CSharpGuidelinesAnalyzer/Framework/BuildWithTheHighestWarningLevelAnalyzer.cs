using System.Collections.Immutable;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Framework
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class BuildWithTheHighestWarningLevelAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV2210";

        private const string Title = "Build with the highest warning level.";
        private const string MessageFormat = "Build with warning level 4.";
        private const string Description = "Build with the highest warning level.";
        private const string Category = "Framework";

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            // If you are running VS2015 Update3, you need to turn on Full Solution Analysis (Tools, Options, Text Editor, 
            // C#, Advanced, Enable full solution analysis) for this analyzer to work. See the bug discussion at: 
            // https://github.com/dotnet/roslyn/issues/11750.

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