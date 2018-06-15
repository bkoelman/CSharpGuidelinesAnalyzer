﻿#if DEBUG
using System;
using System.Collections.Immutable;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class OperationHasKeywordAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV0000";

        private const string Title = "Operation should have a keyword";
        private const string MessageFormat = "Operation should have a keyword";
        private const string Description = "Internal analyzer that reports the keyword location for an IOperation instance.";

        [NotNull]
        private static readonly AnalyzerCategory Category = AnalyzerCategory.Framework;

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category.DisplayName, DiagnosticSeverity.Hidden, false, Description, Category.GetHelpLinkUri(DiagnosticId));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterOperationAction(c => c.SkipInvalid(AnalyzeOperation),
                (OperationKind[])Enum.GetValues(typeof(OperationKind)));
        }

        private void AnalyzeOperation(OperationAnalysisContext context)
        {
            if (context.Operation.IsImplicit)
            {
                return;
            }

            var doWhileStrategy = DoWhileLoopLookupKeywordStrategy.PreferDoKeyword;
            var tryFinallyStrategy = TryFinallyLookupKeywordStrategy.PreferTryKeyword;

            if (IsReportAtAlternateLocation(context.Operation))
            {
                doWhileStrategy = DoWhileLoopLookupKeywordStrategy.PreferWhileKeyword;
                tryFinallyStrategy = TryFinallyLookupKeywordStrategy.PreferFinallyKeyword;
            }

            AnalyzeLocationForOperation(context, doWhileStrategy, tryFinallyStrategy);
        }

        private static bool IsReportAtAlternateLocation([NotNull] IOperation operation)
        {
            return operation.Parent?.Parent?.Syntax is MethodDeclarationSyntax methodSyntax &&
                methodSyntax.Identifier.ValueText == "ReportAtAlternateLocation";
        }

        private static void AnalyzeLocationForOperation(OperationAnalysisContext context,
            DoWhileLoopLookupKeywordStrategy doWhileStrategy, TryFinallyLookupKeywordStrategy tryFinallyStrategy)
        {
            Location location = context.Operation.TryGetLocationForKeyword(doWhileStrategy, tryFinallyStrategy);
            if (location != null)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, location));
            }
        }
    }
}
#endif
