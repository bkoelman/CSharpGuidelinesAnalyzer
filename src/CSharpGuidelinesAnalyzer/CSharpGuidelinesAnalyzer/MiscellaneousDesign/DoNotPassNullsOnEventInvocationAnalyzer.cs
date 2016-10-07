using System.Collections.Immutable;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.MiscellaneousDesign
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DoNotPassNullsOnEventInvocationAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV1235";

        private const string Title = "AV1235";
        private const string MessageFormat = "AV1235";
        private const string Description = "Don't pass null as the sender argument when raising an event.";
        private const string Category = "Miscellaneous Design";

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