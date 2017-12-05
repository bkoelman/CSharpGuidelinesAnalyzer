#if DEBUG
using System;
using System.Collections.Immutable;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace CSharpGuidelinesAnalyzer.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class OperationIsStatementAnalyzer : GuidelineAnalyzer
    {
        public const string DiagnosticId = "AV1000";

        private const string Title = "Operation should be a statement";
        private const string TypeMessageFormat = "Operation '{0}' should be a statement";
        private const string Description = "Operation should be a statement.";

        [NotNull]
        private static readonly AnalyzerCategory Category = AnalyzerCategory.Framework;

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, TypeMessageFormat,
            Category.Name, DiagnosticSeverity.Info, true, Description, Category.HelpLinkUri);

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterOperationAction(AnalyzeOperation, (OperationKind[])Enum.GetValues(typeof(OperationKind)));
        }

        private void AnalyzeOperation(OperationAnalysisContext context)
        {
            if (!context.Operation.IsImplicit && context.Operation.IsStatement())
            {
                Location locationForKeyword = context.Operation.GetLocationForKeyword();
                Location location = locationForKeyword ?? context.Operation.Syntax.GetLocation();

                string keywordText = GetTextAt(location);
                context.ReportDiagnostic(Diagnostic.Create(Rule, location, keywordText));
            }
        }

        [NotNull]
        private static string GetTextAt([NotNull] Location locationForKeyword)
        {
            TextSpan sourceSpan = locationForKeyword.SourceSpan;
            return locationForKeyword.SourceTree.ToString().Substring(sourceSpan.Start, sourceSpan.Length);
        }
    }
}
#endif
