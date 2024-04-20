#if DEBUG
using System;
using System.Collections.Immutable;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace CSharpGuidelinesAnalyzer.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NullCheckOnNullableValueTypeAnalyzer : DiagnosticAnalyzer
{
    private const string Title = "Expression of nullable value type is checked for null or not-null";
    private const string MessageFormat = "Expression of nullable value type '{0}' is checked for {1} using {2}";
    private const string Description = "Internal analyzer that reports when an expression of type nullable value type is checked for null or not-null.";

    public const string DiagnosticId = AnalyzerCategory.RulePrefix + "00000000";

    [NotNull]
    private static readonly AnalyzerCategory Category = AnalyzerCategory.Framework;

    [NotNull]
    private static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category.DisplayName,
        DiagnosticSeverity.Hidden, false, Description);

    [ItemNotNull]
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize([NotNull] AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterCompilationStartAction(startContext =>
        {
            var scanner = new NullCheckScanner(startContext.Compilation);

            RegisterForOperations(startContext, scanner);
        });
    }

    private void RegisterForOperations([NotNull] CompilationStartAnalysisContext startContext, [NotNull] NullCheckScanner scanner)
    {
        startContext.RegisterOperationAction(context => context.SkipInvalid(_ => AnalyzePropertyReference(context, scanner)),
            OperationKind.PropertyReference);

        startContext.RegisterOperationAction(context => context.SkipInvalid(_ => AnalyzeInvocation(context, scanner)), OperationKind.Invocation);

        startContext.RegisterOperationAction(context => context.SkipInvalid(_ => AnalyzeIsPattern(context, scanner)), OperationKind.IsPattern);

        startContext.RegisterOperationAction(context => context.SkipInvalid(_ => AnalyzeBinaryOperator(context, scanner)), OperationKind.BinaryOperator);
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

    private void ReportForScanResult([CanBeNull] NullCheckScanResult? scanResult, [NotNull] Action<Diagnostic> reportDiagnostic)
    {
        if (scanResult != null)
        {
            IOperation operation = scanResult.Value.Target;
            Location location = operation.Syntax.GetLocation();
            string nullText = scanResult.Value.Operand == NullCheckOperand.IsNull ? "null" : "not-null";

            var diagnostic = Diagnostic.Create(Rule, location, operation.Syntax.ToString(), nullText, scanResult.Value.Method);
            reportDiagnostic(diagnostic);
        }
    }
}
#endif
