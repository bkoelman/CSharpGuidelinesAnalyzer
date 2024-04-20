using System;
using System.Collections.Immutable;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Rules.Framework;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class BuildWithTheHighestWarningLevelAnalyzer : DiagnosticAnalyzer
{
    private const string Title = "Compiler warnings are not treated as errors";

    private const string MessageFormat =
        "Pass -warnaserror to the compiler or add <TreatWarningsAsErrors>True</TreatWarningsAsErrors> to your project file";

    private const string Description = "Build with the highest warning level.";

    public const string DiagnosticId = AnalyzerCategory.RulePrefix + "2210";

    [NotNull]
    private static readonly AnalyzerCategory Category = AnalyzerCategory.Framework;

    [NotNull]
    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category.DisplayName,
        DiagnosticSeverity.Warning, true, Description, Category.GetHelpLinkUri(DiagnosticId));

    [NotNull]
    private static readonly Action<CompilationAnalysisContext> AnalyzeCompilationOptionsAction = AnalyzeCompilationOptions;

    [ItemNotNull]
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize([NotNull] AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterCompilationAction(AnalyzeCompilationOptionsAction);
    }

    private static void AnalyzeCompilationOptions(CompilationAnalysisContext context)
    {
        if (context.Compilation.Options.GeneralDiagnosticOption != ReportDiagnostic.Error)
        {
            var diagnostic = Diagnostic.Create(Rule, Location.None);
            context.ReportDiagnostic(diagnostic);
        }
    }
}