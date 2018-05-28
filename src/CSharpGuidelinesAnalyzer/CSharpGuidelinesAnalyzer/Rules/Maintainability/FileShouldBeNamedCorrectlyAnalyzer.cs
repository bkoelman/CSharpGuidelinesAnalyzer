using System.Collections.Immutable;
using System.IO;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Rules.Maintainability
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class FileShouldBeNamedCorrectlyAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV1506";

        private const string Title = "File should be named in Pascal casing without underscores or generic arity.";

        private const string CasingMessageFormat = "File '{0}' should be named using Pascal casing.";
        private const string UnderscoreMessageFormat = "File '{0}' should be named without underscores.";
        private const string ArityMessageFormat = "File '{0}' should be named without generic arity.";

        private const string Description = "Name a source file to the type it contains.";

        [NotNull]
        private static readonly AnalyzerCategory Category = AnalyzerCategory.Maintainability;

        [NotNull]
        private static readonly DiagnosticDescriptor CasingRule = new DiagnosticDescriptor(DiagnosticId, Title,
            CasingMessageFormat, Category.DisplayName, DiagnosticSeverity.Info, true, Description,
            Category.GetHelpLinkUri(DiagnosticId));

        [NotNull]
        private static readonly DiagnosticDescriptor UnderscoreRule = new DiagnosticDescriptor(DiagnosticId, Title,
            UnderscoreMessageFormat, Category.DisplayName, DiagnosticSeverity.Info, true, Description,
            Category.GetHelpLinkUri(DiagnosticId));

        [NotNull]
        private static readonly DiagnosticDescriptor ArityRule = new DiagnosticDescriptor(DiagnosticId, Title, ArityMessageFormat,
            Category.DisplayName, DiagnosticSeverity.Info, true, Description, Category.GetHelpLinkUri(DiagnosticId));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(CasingRule, UnderscoreRule, ArityRule);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSyntaxTreeAction(AnalyzeSyntaxTree);
        }

        private void AnalyzeSyntaxTree(SyntaxTreeAnalysisContext context)
        {
            string fileName = Path.GetFileName(context.Tree.FilePath);

            if (string.IsNullOrEmpty(fileName))
            {
                return;
            }

            AnalyzeCasing(fileName, context);
            AnalyzeUnderscores(fileName, context);
            AnalyzeArity(fileName, context);
        }

        private static void AnalyzeCasing([NotNull] string fileName, SyntaxTreeAnalysisContext context)
        {
            if (char.IsLower(fileName[0]))
            {
                context.ReportDiagnostic(Diagnostic.Create(CasingRule, Location.None, fileName));
            }
        }

        private static void AnalyzeUnderscores([NotNull] string fileName, SyntaxTreeAnalysisContext context)
        {
            if (fileName.IndexOf('_') != -1)
            {
                context.ReportDiagnostic(Diagnostic.Create(UnderscoreRule, Location.None, fileName));
            }
        }

        private static void AnalyzeArity([NotNull] string fileName, SyntaxTreeAnalysisContext context)
        {
            if (fileName.IndexOf('`') != -1)
            {
                context.ReportDiagnostic(Diagnostic.Create(ArityRule, Location.None, fileName));
            }
        }
    }
}
