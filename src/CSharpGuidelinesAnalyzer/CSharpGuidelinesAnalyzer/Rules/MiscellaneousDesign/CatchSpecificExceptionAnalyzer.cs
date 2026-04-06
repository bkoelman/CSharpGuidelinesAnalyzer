using System.Collections.Immutable;
using System.Linq;
using System.Threading;
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

    private static readonly AnalyzerCategory Category = AnalyzerCategory.MiscellaneousDesign;

    private static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category.DisplayName, DiagnosticSeverity.Warning, true,
        Description, Category.GetHelpLinkUri(DiagnosticId));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterCompilationStartAction(RegisterCompilationStart);
    }

    private static void RegisterCompilationStart(CompilationStartAnalysisContext startContext)
    {
        ImmutableArray<INamedTypeSymbol> types = ResolveExceptionTypes(startContext.Compilation);

        if (types.Any())
        {
            startContext.RegisterSyntaxNodeAction(context => AnalyzeCatchClause(context, types), SyntaxKind.CatchClause);
        }
    }

    private static ImmutableArray<INamedTypeSymbol> ResolveExceptionTypes(Compilation compilation)
    {
        INamedTypeSymbol?[] types =
        [
            KnownTypes.SystemException(compilation),
            KnownTypes.SystemSystemException(compilation),
            KnownTypes.SystemApplicationException(compilation)
        ];

        return types.Where(type => type != null).Cast<INamedTypeSymbol>().ToImmutableArray();
    }

    private static void AnalyzeCatchClause(SyntaxNodeAnalysisContext context, ImmutableArray<INamedTypeSymbol> exceptionTypes)
    {
        var catchClause = (CatchClauseSyntax)context.Node;

        if (catchClause.Filter == null)
        {
            ISymbol? exceptionType = TryGetExceptionType(catchClause.Declaration, context.SemanticModel, context.CancellationToken);

            if (exceptionType == null || exceptionTypes.Contains(exceptionType))
            {
                Location location = catchClause.CatchKeyword.GetLocation();

                var diagnostic = Diagnostic.Create(Rule, location);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private static ISymbol? TryGetExceptionType(CatchDeclarationSyntax? declaration, SemanticModel model,
        CancellationToken cancellationToken)
    {
        return declaration != null ? model.GetSymbolInfo(declaration.Type, cancellationToken).Symbol : null;
    }
}
