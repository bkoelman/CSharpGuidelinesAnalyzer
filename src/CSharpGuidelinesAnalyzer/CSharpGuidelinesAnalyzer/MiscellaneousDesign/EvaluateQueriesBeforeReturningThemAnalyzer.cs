using System.Collections.Immutable;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.MiscellaneousDesign
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class EvaluateQueriesBeforeReturningThemAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV1250";

        private const string Title = "AV1250";
        private const string MessageFormat = "AV1250";
        private const string Description = "Evaluate the result of a LINQ expression before returning it.";
        private const string Category = "Miscellaneous Design";

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