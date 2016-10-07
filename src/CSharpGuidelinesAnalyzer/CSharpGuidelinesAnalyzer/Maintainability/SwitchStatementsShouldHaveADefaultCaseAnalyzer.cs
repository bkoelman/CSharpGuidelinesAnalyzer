using System.Collections.Immutable;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Maintainability
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class SwitchStatementsShouldHaveADefaultCaseAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV1536";

        private const string Title = "AV1536";
        private const string MessageFormat = "AV1536";
        private const string Description = "Always add a default block after the last case in a switch statement.";
        private const string Category = "Maintainability";

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