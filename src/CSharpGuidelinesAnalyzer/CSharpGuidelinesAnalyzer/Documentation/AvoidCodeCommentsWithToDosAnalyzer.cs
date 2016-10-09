using System.Collections.Immutable;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Documentation
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class AvoidCodeCommentsWithToDosAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV2318";

        private const string Title = "AV2318";
        private const string MessageFormat = "AV2318";
        private const string Description = "Don't use comments for tracking work to be done later.";
        private const string Category = "Documentation";

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