using System.Collections.Immutable;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Documentation
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DocumentAllInternalMembersAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV2305";

        private const string Title = "AV2305";
        private const string MessageFormat = "AV2305";
        private const string Description = "Document all public, protected and internal types and members.";
        private const string Category = "Documentation";

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