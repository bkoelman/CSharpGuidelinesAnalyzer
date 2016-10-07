using System.Collections.Immutable;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Framework
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class OnlyUseDynamicForUnknownTypesAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV2230";

        private const string Title = "AV2230";
        private const string MessageFormat = "AV2230";
        private const string Description = "Only use the dynamic keyword when talking to a dynamic object.";
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