#if DEBUG
using System;
using System.Collections.Immutable;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace CSharpGuidelinesAnalyzer.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class OperationIsStatementAnalyzer : DiagnosticAnalyzer
{
    private const string Title = "Operation should be a statement";
    private const string MessageFormat = "Operation '{0}' should be a statement";
    private const string Description = "Internal analyzer that reports when an IOperation instance represents a statement.";

    public const string DiagnosticId = AnalyzerCategory.RulePrefix + "0000000000000000";

    [NotNull]
    private static readonly AnalyzerCategory Category = AnalyzerCategory.Framework;

    [NotNull]
    private static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category.DisplayName,
        DiagnosticSeverity.Hidden, false, Description);

    [NotNull]
    private static readonly OperationKind[] OperationKinds = (OperationKind[])Enum.GetValues(typeof(OperationKind));

    [ItemNotNull]
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize([NotNull] AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterOperationAction(operationContext => operationContext.SkipInvalid(AnalyzeOperation), OperationKinds);
    }

    private void AnalyzeOperation(OperationAnalysisContext context)
    {
        if (!context.Operation.IsImplicit && context.Operation.IsStatement())
        {
            Location locationForKeyword = context.Operation.TryGetLocationForKeyword();
            Location location = locationForKeyword ?? context.Operation.Syntax.GetLocation();

            string keywordText = GetTextAt(location);

            var diagnostic = Diagnostic.Create(Rule, location, keywordText);
            context.ReportDiagnostic(diagnostic);
        }
    }

    [NotNull]
    private static string GetTextAt([NotNull] Location locationForKeyword)
    {
        TextSpan sourceSpan = locationForKeyword.SourceSpan;
        return locationForKeyword.SourceTree.ToString().Substring(sourceSpan.Start, sourceSpan.Length);
    }
}
#endif
