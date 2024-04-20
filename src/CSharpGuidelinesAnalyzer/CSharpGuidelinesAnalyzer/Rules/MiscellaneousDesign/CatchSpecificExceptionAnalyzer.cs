using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Rules.MiscellaneousDesign;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CatchSpecificExceptionAnalyzer : DiagnosticAnalyzer
{
    private const string Title = "Catch a specific exception instead of Exception, SystemException or ApplicationException";
    private const string MessageFormat = "Catch a specific exception instead of Exception, SystemException or ApplicationException";
    private const string Description = "Don't swallow errors by catching generic exceptions.";

    public const string DiagnosticId = AnalyzerCategory.RulePrefix + "1210";

    [NotNull]
    private static readonly AnalyzerCategory Category = AnalyzerCategory.MiscellaneousDesign;

    [NotNull]
    private static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category.DisplayName, DiagnosticSeverity.Warning, true,
        Description, Category.GetHelpLinkUri(DiagnosticId));

    [NotNull]
    private static readonly Action<CompilationStartAnalysisContext> RegisterCompilationStartAction = RegisterCompilationStart;

#pragma warning disable RS1008 // Avoid storing per-compilation data into the fields of a diagnostic analyzer.
    [NotNull]
    private static readonly Action<SyntaxNodeAnalysisContext, ImmutableArray<INamedTypeSymbol>> AnalyzeCatchClauseAction = AnalyzeCatchClause;
#pragma warning restore RS1008 // Avoid storing per-compilation data into the fields of a diagnostic analyzer.

    [ItemNotNull]
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize([NotNull] AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterCompilationStartAction(RegisterCompilationStartAction);
    }

    private static void RegisterCompilationStart([NotNull] CompilationStartAnalysisContext startContext)
    {
        ImmutableArray<INamedTypeSymbol> types = ResolveExceptionTypes(startContext.Compilation);

        if (types.Any())
        {
            startContext.RegisterSyntaxNodeAction(context => AnalyzeCatchClauseAction(context, types), SyntaxKind.CatchClause);
        }
    }

    [ItemNotNull]
    private static ImmutableArray<INamedTypeSymbol> ResolveExceptionTypes([NotNull] Compilation compilation)
    {
        INamedTypeSymbol[] types =
        [
            KnownTypes.SystemException(compilation),
            KnownTypes.SystemSystemException(compilation),
            KnownTypes.SystemApplicationException(compilation)
        ];

        return types.Where(type => type != null).ToImmutableArray();
    }

    private static void AnalyzeCatchClause(SyntaxNodeAnalysisContext context, [ItemNotNull] ImmutableArray<INamedTypeSymbol> exceptionTypes)
    {
        var catchClause = (CatchClauseSyntax)context.Node;

        if (catchClause.Filter == null)
        {
            ISymbol exceptionType = TryGetExceptionType(catchClause.Declaration, context.SemanticModel, context.CancellationToken);

            if (exceptionType == null || exceptionTypes.Contains(exceptionType))
            {
                Location location = catchClause.CatchKeyword.GetLocation();

                var diagnostic = Diagnostic.Create(Rule, location);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    [CanBeNull]
    private static ISymbol TryGetExceptionType([CanBeNull] CatchDeclarationSyntax declaration, [NotNull] SemanticModel model,
        CancellationToken cancellationToken)
    {
        return declaration != null ? model.GetSymbolInfo(declaration.Type, cancellationToken).Symbol : null;
    }
}
