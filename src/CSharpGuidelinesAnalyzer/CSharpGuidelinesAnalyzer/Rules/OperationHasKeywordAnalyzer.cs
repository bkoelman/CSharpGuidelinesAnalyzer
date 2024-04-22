#if DEBUG
using System;
using System.Collections.Immutable;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class OperationHasKeywordAnalyzer : DiagnosticAnalyzer
{
    private const string Title = "Operation should have a keyword";
    private const string MessageFormat = "Operation should have a keyword";
    private const string Description = "Internal analyzer that reports the keyword location for an IOperation instance.";

    public const string DiagnosticId = AnalyzerCategory.RulePrefix + "000000000000";

    [NotNull]
    private static readonly AnalyzerCategory Category = AnalyzerCategory.Framework;

    [NotNull]
    private static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category.DisplayName, DiagnosticSeverity.Hidden, false,
        Description);

    [NotNull]
    private static readonly OperationKind[] OperationKinds = (OperationKind[])Enum.GetValues(typeof(OperationKind));

    [ItemNotNull]
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize([NotNull] AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.SafeRegisterOperationAction(AnalyzeOperation, OperationKinds);
    }

    private void AnalyzeOperation(OperationAnalysisContext context)
    {
        if (!context.Operation.IsImplicit)
        {
            AnalyzeExplicitOperation(context);
        }
    }

    private static void AnalyzeExplicitOperation(OperationAnalysisContext context)
    {
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
        return operation.Parent?.Parent?.Syntax is MethodDeclarationSyntax { Identifier.ValueText: "ReportAtAlternateLocation" };
    }

    private static void AnalyzeLocationForOperation(OperationAnalysisContext context, DoWhileLoopLookupKeywordStrategy doWhileStrategy,
        TryFinallyLookupKeywordStrategy tryFinallyStrategy)
    {
        Location location = context.Operation.TryGetLocationForKeyword(doWhileStrategy, tryFinallyStrategy);

        if (location != null)
        {
            var diagnostic = Diagnostic.Create(Rule, location);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
#endif
