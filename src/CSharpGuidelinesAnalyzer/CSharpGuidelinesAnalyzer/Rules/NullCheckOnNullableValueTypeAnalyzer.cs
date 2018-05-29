using System;
using System.Collections.Immutable;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace CSharpGuidelinesAnalyzer.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class NullCheckOnNullableValueTypeAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV0000";

        private const string Title = "Expression of nullable value type is checked for null or not-null";
        private const string TypeMessageFormat = "Expression of nullable value type '{0}' is checked for {1} using {2}";

        private const string Description =
            "Internal analyzer that reports when an expression of type nullable value type is checked for null or not-null.";

        [NotNull]
        private static readonly AnalyzerCategory Category = AnalyzerCategory.Framework;

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, TypeMessageFormat,
            Category.DisplayName, DiagnosticSeverity.Hidden, false, Description, Category.GetHelpLinkUri(DiagnosticId));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(startContext =>
            {
                var scanner = new NullCheckScanner(startContext.Compilation);

                startContext.RegisterOperationAction(c => c.SkipInvalid(_ => AnalyzePropertyReference(c, scanner)),
                    OperationKind.PropertyReference);
                startContext.RegisterOperationAction(c => c.SkipInvalid(_ => AnalyzeInvocation(c, scanner)),
                    OperationKind.Invocation);
                startContext.RegisterOperationAction(c => c.SkipInvalid(_ => AnalyzeIsPattern(c, scanner)),
                    OperationKind.IsPattern);
                startContext.RegisterOperationAction(c => c.SkipInvalid(_ => AnalyzeBinaryOperator(c, scanner)),
                    OperationKind.BinaryOperator);
            });
        }

        private void AnalyzePropertyReference(OperationAnalysisContext context, [NotNull] NullCheckScanner scanner)
        {
            var propertyReference = (IPropertyReferenceOperation)context.Operation;

            NullCheckScanResult? scanResult = scanner.ScanPropertyReference(propertyReference);
            ReportForScanResult(scanResult, context.ReportDiagnostic);
        }

        private void AnalyzeInvocation(OperationAnalysisContext context, [NotNull] NullCheckScanner scanner)
        {
            var invocation = (IInvocationOperation)context.Operation;

            NullCheckScanResult? scanResult = scanner.ScanInvocation(invocation);
            ReportForScanResult(scanResult, context.ReportDiagnostic);
        }

        private void AnalyzeIsPattern(OperationAnalysisContext context, [NotNull] NullCheckScanner scanner)
        {
            var isPattern = (IIsPatternOperation)context.Operation;

            NullCheckScanResult? scanResult = scanner.ScanIsPattern(isPattern);
            ReportForScanResult(scanResult, context.ReportDiagnostic);
        }

        private void AnalyzeBinaryOperator(OperationAnalysisContext context, [NotNull] NullCheckScanner scanner)
        {
            var binaryOperator = (IBinaryOperation)context.Operation;

            NullCheckScanResult? scanResult = scanner.ScanBinaryOperator(binaryOperator);
            ReportForScanResult(scanResult, context.ReportDiagnostic);
        }

        private void ReportForScanResult([CanBeNull] NullCheckScanResult? scanResult,
            [NotNull] Action<Diagnostic> reportDiagnostic)
        {
            if (scanResult != null)
            {
                ReportAtOperation(scanResult.Value.Target, reportDiagnostic, scanResult.Value.IsInverted,
                    scanResult.Value.Kind);
            }
        }

        private static void ReportAtOperation([NotNull] IOperation operation, [NotNull] Action<Diagnostic> reportDiagnostic,
            bool isInverted, NullCheckKind nullCheckKind)
        {
            reportDiagnostic(Diagnostic.Create(Rule, operation.Syntax.GetLocation(), operation.Syntax.ToString(),
                isInverted ? "not-null" : "null", nullCheckKind));
        }
    }
}
