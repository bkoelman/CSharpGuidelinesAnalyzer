using System;
using System.Collections.Immutable;
using System.IO;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace CSharpGuidelinesAnalyzer.Rules.Maintainability
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class FileShouldBeNamedCorrectlyAnalyzer : DiagnosticAnalyzer
    {
        private const string Title = "File should be named in Pascal casing without underscores or generic arity";

        private const string CasingMessageFormat = "File '{0}' should be named using Pascal casing";
        private const string UnderscoreMessageFormat = "File '{0}' should be named without underscores";
        private const string ArityMessageFormat = "File '{0}' should be named without generic arity";

        private const string Description = "Name a source file to the type it contains.";

        public const string DiagnosticId = AnalyzerCategory.RulePrefix + "1506";

        [NotNull]
        private static readonly AnalyzerCategory Category = AnalyzerCategory.Maintainability;

        [NotNull]
        private static readonly DiagnosticDescriptor CasingRule = new DiagnosticDescriptor(DiagnosticId, Title, CasingMessageFormat, Category.DisplayName,
            DiagnosticSeverity.Info, true, Description, Category.GetHelpLinkUri(DiagnosticId));

        [NotNull]
        private static readonly DiagnosticDescriptor UnderscoreRule = new DiagnosticDescriptor(DiagnosticId, Title, UnderscoreMessageFormat,
            Category.DisplayName, DiagnosticSeverity.Info, true, Description, Category.GetHelpLinkUri(DiagnosticId));

        [NotNull]
        private static readonly DiagnosticDescriptor ArityRule = new DiagnosticDescriptor(DiagnosticId, Title, ArityMessageFormat, Category.DisplayName,
            DiagnosticSeverity.Info, true, Description, Category.GetHelpLinkUri(DiagnosticId));

        [NotNull]
        private static readonly Action<SyntaxTreeAnalysisContext> AnalyzeSyntaxTreeAction = AnalyzeSyntaxTree;

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(CasingRule, UnderscoreRule, ArityRule);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSyntaxTreeAction(AnalyzeSyntaxTreeAction);
        }

        private static void AnalyzeSyntaxTree(SyntaxTreeAnalysisContext context)
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
                Location location = GetLocationForStartOfFile(context);
                context.ReportDiagnostic(Diagnostic.Create(CasingRule, location, fileName));
            }
        }

        private static void AnalyzeUnderscores([NotNull] string fileName, SyntaxTreeAnalysisContext context)
        {
            if (fileName.IndexOf('_') != -1)
            {
                Location location = GetLocationForStartOfFile(context);
                context.ReportDiagnostic(Diagnostic.Create(UnderscoreRule, location, fileName));
            }
        }

        private static void AnalyzeArity([NotNull] string fileName, SyntaxTreeAnalysisContext context)
        {
            if (fileName.IndexOf('`') != -1)
            {
                Location location = GetLocationForStartOfFile(context);
                context.ReportDiagnostic(Diagnostic.Create(ArityRule, location, fileName));
            }
        }

        [NotNull]
        private static Location GetLocationForStartOfFile(SyntaxTreeAnalysisContext context)
        {
            return context.Tree.GetLocation(new TextSpan(0, 0));
        }
    }
}
