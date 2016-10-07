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

        private const string Title = "AV2210";
        private const string MessageFormat = "AV2210";
        private const string Description = "Build with the highest warning level.";
        private const string Category = "Framework";

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            //context.EnableConcurrentExecution();
            //context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        }
    }
}