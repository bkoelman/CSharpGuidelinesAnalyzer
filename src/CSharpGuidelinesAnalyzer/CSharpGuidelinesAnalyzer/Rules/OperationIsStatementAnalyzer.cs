
#if DEBUG
using System.Collections.Immutable;
using CSharpGuidelinesAnalyzer.Extensions;
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

    private static readonly AnalyzerCategory Category = AnalyzerCategory.Framework;

    private static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category.DisplayName, DiagnosticSeverity.Hidden, false,
        Description);

    private static readonly OperationKind[] OperationKinds = (OperationKind[])Enum.GetValues(typeof(OperationKind));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.SafeRegisterOperationAction(AnalyzeOperation, OperationKinds);
    }

    private void AnalyzeOperation(OperationAnalysisContext context)
    {
        if (!context.Operation.IsImplicit && context.Operation.IsStatement())
        {
            Location? locationForKeyword = context.Operation.TryGetLocationForKeyword();
            Location location = locationForKeyword ?? context.Operation.Syntax.GetLocation();

            string? keywordText = GetTextAt(location);

            if (keywordText != null)
            {
                var diagnostic = Diagnostic.Create(Rule, location, keywordText);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private static string? GetTextAt(Location locationForKeyword)
    {
        TextSpan sourceSpan = locationForKeyword.SourceSpan;
        return locationForKeyword.SourceTree?.ToString().Substring(sourceSpan.Start, sourceSpan.Length);
    }
}
#endif
