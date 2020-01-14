using System;
using System.Collections.Immutable;
using System.Threading;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace CSharpGuidelinesAnalyzer.Rules.Naming
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class SuffixAsyncMethodCorrectlyAnalyzer : DiagnosticAnalyzer
    {
        private const string Title = "Name of async method should end with Async or TaskAsync";
        private const string MessageFormat = "Name of async {0} '{1}' should end with Async or TaskAsync.";
        private const string Description = "Postfix asynchronous methods with Async or TaskAsync.";

        public const string DiagnosticId = "AV1755";

        [NotNull]
        private static readonly AnalyzerCategory Category = AnalyzerCategory.Naming;

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category.DisplayName, DiagnosticSeverity.Warning, true, Description, Category.GetHelpLinkUri(DiagnosticId));

        [NotNull]
        private static readonly Action<SymbolAnalysisContext> AnalyzeMethodAction = context =>
            context.SkipEmptyName(AnalyzeMethod);

        [NotNull]
        private static readonly Action<OperationAnalysisContext> AnalyzeLocalFunctionAction = context =>
            context.SkipInvalid(AnalyzeLocalFunction);

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSymbolAction(AnalyzeMethodAction, SymbolKind.Method);
            context.RegisterOperationAction(AnalyzeLocalFunctionAction, OperationKind.LocalFunction);
        }

        private static void AnalyzeMethod(SymbolAnalysisContext context)
        {
            var method = (IMethodSymbol)context.Symbol;

            if (RequiresReport(method, context.Compilation, context.CancellationToken))
            {
                ReportAt(method, context.ReportDiagnostic);
            }
        }

        private static void AnalyzeLocalFunction(OperationAnalysisContext context)
        {
            var operation = (ILocalFunctionOperation)context.Operation;

            if (RequiresReport(operation.Symbol, context.Compilation, context.CancellationToken))
            {
                ReportAt(operation.Symbol, context.ReportDiagnostic);
            }
        }

        private static bool RequiresReport([NotNull] IMethodSymbol method, [NotNull] Compilation compilation,
            CancellationToken cancellationToken)
        {
            return method.IsAsync && !method.Name.EndsWith("Async", StringComparison.Ordinal) && !method.IsSynthesized() &&
                !method.IsUnitTestMethod() && !method.IsEntryPoint(compilation, cancellationToken);
        }

        private static void ReportAt([NotNull] IMethodSymbol method, [NotNull] Action<Diagnostic> reportDiagnostic)
        {
            reportDiagnostic(Diagnostic.Create(Rule, method.Locations[0], method.GetKind().ToLowerInvariant(),
                method.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat)));
        }
    }
}
