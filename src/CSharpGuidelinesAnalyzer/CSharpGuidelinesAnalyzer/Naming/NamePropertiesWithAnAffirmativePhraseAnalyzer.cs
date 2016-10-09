using System.Collections.Immutable;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Naming
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class NamePropertiesWithAnAffirmativePhraseAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV1715";

        private const string Title = "AV1715";
        private const string MessageFormat = "AV1715";
        private const string Description = "Properly name properties.";
        private const string Category = "Naming";

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description,
            helpLinkUri: HelpLinkUris.GetForCategory(Category));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            //context.EnableConcurrentExecution();
            //context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        }
    }
}