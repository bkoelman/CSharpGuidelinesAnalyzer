using System;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Rules.MiscellaneousDesign
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CatchSpecificExceptionAnalyzer : DiagnosticAnalyzer
    {
        private const string Title = "Catch a specific exception instead of Exception, SystemException or ApplicationException";
        private const string MessageFormat = "Catch a specific exception instead of Exception, SystemException or ApplicationException";
        private const string Description = "Don't swallow errors by catching generic exceptions.";

        public const string DiagnosticId = "AV1210";

        [NotNull]
        private static readonly AnalyzerCategory Category = AnalyzerCategory.MiscellaneousDesign;

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category.DisplayName,
            DiagnosticSeverity.Warning, true, Description, Category.GetHelpLinkUri(DiagnosticId));

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
            ImmutableArray<INamedTypeSymbol>.Builder builder = ImmutableArray.CreateBuilder<INamedTypeSymbol>(3);

            AddTypeToBuilder(KnownTypes.SystemException(compilation), builder);
            AddTypeToBuilder(KnownTypes.SystemSystemException(compilation), builder);
            AddTypeToBuilder(KnownTypes.SystemApplicationException(compilation), builder);

            return !builder.Any() ? ImmutableArray<INamedTypeSymbol>.Empty : builder.ToImmutable();
        }

        private static void AddTypeToBuilder([CanBeNull] INamedTypeSymbol type, [NotNull] [ItemNotNull] ImmutableArray<INamedTypeSymbol>.Builder builder)
        {
            if (type != null)
            {
                builder.Add(type);
            }
        }

        private static void AnalyzeCatchClause(SyntaxNodeAnalysisContext context, [ItemNotNull] ImmutableArray<INamedTypeSymbol> exceptionTypes)
        {
            var catchClause = (CatchClauseSyntax)context.Node;

            if (catchClause.Filter == null)
            {
                ISymbol exceptionType = TryGetExceptionType(catchClause.Declaration, context.SemanticModel);

                if (exceptionType == null || exceptionTypes.Contains(exceptionType))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, catchClause.CatchKeyword.GetLocation()));
                }
            }
        }

        [CanBeNull]
        private static ISymbol TryGetExceptionType([CanBeNull] CatchDeclarationSyntax declaration, [NotNull] SemanticModel model)
        {
            return declaration != null ? model.GetSymbolInfo(declaration.Type).Symbol : null;
        }
    }
}
