using System;
using System.Collections.Immutable;
using System.Linq;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace CSharpGuidelinesAnalyzer.Rules.MiscellaneousDesign
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CatchSpecificExceptionAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV1210";

        private const string Title = "Catch a specific exception instead of Exception, SystemException or ApplicationException";

        private const string MessageFormat =
            "Catch a specific exception instead of Exception, SystemException or ApplicationException.";

        private const string Description = "Don't swallow errors by catching generic exceptions";

        [NotNull]
        private static readonly AnalyzerCategory Category = AnalyzerCategory.MiscellaneousDesign;

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category.DisplayName, DiagnosticSeverity.Warning, true, Description, Category.GetHelpLinkUri(DiagnosticId));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        [NotNull]
        private static readonly Action<CompilationStartAnalysisContext> RegisterCompilationStartAction = RegisterCompilationStart;

#pragma warning disable RS1008 // Avoid storing per-compilation data into the fields of a diagnostic analyzer.
        [NotNull]
        private static readonly Action<OperationAnalysisContext, ImmutableArray<INamedTypeSymbol>> AnalyzeCatchClauseAction =
            (context, types) => context.SkipInvalid(_ => AnalyzeCatchClause(context, types));
#pragma warning restore RS1008 // Avoid storing per-compilation data into the fields of a diagnostic analyzer.

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
                startContext.RegisterOperationAction(context => AnalyzeCatchClauseAction(context, types),
                    OperationKind.CatchClause);
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

        private static void AddTypeToBuilder([CanBeNull] INamedTypeSymbol type,
            [NotNull] [ItemNotNull] ImmutableArray<INamedTypeSymbol>.Builder builder)
        {
            if (type != null)
            {
                builder.Add(type);
            }
        }

        private static void AnalyzeCatchClause(OperationAnalysisContext context,
            [ItemNotNull] ImmutableArray<INamedTypeSymbol> exceptionTypes)
        {
            var catchClause = (ICatchClauseOperation)context.Operation;

            if (catchClause.Filter != null)
            {
                return;
            }

            if (catchClause.ExceptionType == null || exceptionTypes.Contains(catchClause.ExceptionType))
            {
                Location location = catchClause.TryGetLocationForKeyword();
                if (location != null)
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, location));
                }
            }
        }
    }
}
