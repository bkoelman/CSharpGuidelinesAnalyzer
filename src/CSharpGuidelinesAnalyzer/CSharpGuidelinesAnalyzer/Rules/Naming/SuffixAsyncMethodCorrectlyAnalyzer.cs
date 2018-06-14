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
        public const string DiagnosticId = "AV1755";

        private const string Title = "Name of async method should end with Async or TaskAsync";
        private const string MessageFormat = "Name of async {0} '{1}' should end with Async or TaskAsync.";
        private const string Description = "Postfix asynchronous methods with Async or TaskAsync.";

        [NotNull]
        private static readonly AnalyzerCategory Category = AnalyzerCategory.Naming;

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category.DisplayName, DiagnosticSeverity.Warning, true, Description, Category.GetHelpLinkUri(DiagnosticId));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSymbolAction(c => c.SkipEmptyName(AnalyzeMethod), SymbolKind.Method);
            context.RegisterOperationAction(c => c.SkipInvalid(AnalyzeLocalFunction), OperationKind.LocalFunction);
        }

        private void AnalyzeMethod(SymbolAnalysisContext context)
        {
            var method = (IMethodSymbol)context.Symbol;

            if (RequiresReport(method, context.Compilation, context.CancellationToken))
            {
                ReportAt(method, context.ReportDiagnostic);
            }
        }

        private void AnalyzeLocalFunction(OperationAnalysisContext context)
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
                !IsEntryPoint(method, compilation, cancellationToken);
        }

        private static bool IsEntryPoint([NotNull] IMethodSymbol method, [NotNull] Compilation compilation,
            CancellationToken cancellationToken)
        {
            IMethodSymbol entryPoint =
                method.MethodKind == MethodKind.Ordinary ? compilation.GetEntryPoint(cancellationToken) : null;

            return Equals(method, entryPoint);
        }

        private static void ReportAt([NotNull] IMethodSymbol method, [NotNull] Action<Diagnostic> reportDiagnostic)
        {
            reportDiagnostic(Diagnostic.Create(Rule, method.Locations[0], method.GetKind().ToLowerInvariant(),
                method.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat)));
        }
    }
}
