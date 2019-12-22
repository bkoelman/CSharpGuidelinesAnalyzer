using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace CSharpGuidelinesAnalyzer.Rules.MemberDesign
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DoNotReturnNullAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV1135";

        private const string Title = "Do not return null for strings, collections or tasks";

        private const string MessageFormat =
            "null is returned from {0} '{1}' which has return type of string, collection or task.";

        private const string Description =
            "Properties, arguments and return values representing strings or collections should never be null.";

        [NotNull]
        private static readonly AnalyzerCategory Category = AnalyzerCategory.MemberDesign;

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category.DisplayName, DiagnosticSeverity.Warning, true, Description, Category.GetHelpLinkUri(DiagnosticId));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        private static readonly ImmutableArray<OperationKind> ReturnOperationKinds =
            ImmutableArray.Create(OperationKind.Return, OperationKind.YieldReturn);

        [NotNull]
        private static readonly Action<CompilationStartAnalysisContext> RegisterCompilationStartAction = RegisterCompilationStart;

#pragma warning disable RS1008 // Avoid storing per-compilation data into the fields of a diagnostic analyzer.
        [NotNull]
        private static readonly Action<OperationAnalysisContext, IList<INamedTypeSymbol>> AnalyzeReturnAction =
            (context, taskTypes) => context.SkipInvalid(_ => AnalyzeReturn(context, taskTypes));
#pragma warning restore RS1008 // Avoid storing per-compilation data into the fields of a diagnostic analyzer.

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(RegisterCompilationStartAction);
        }

        private static void RegisterCompilationStart([NotNull] CompilationStartAnalysisContext startContext)
        {
            IList<INamedTypeSymbol> taskTypes = ResolveTaskTypes(startContext.Compilation).ToList();

            startContext.RegisterOperationAction(context => AnalyzeReturnAction(context, taskTypes), ReturnOperationKinds);
        }

        [NotNull]
        [ItemNotNull]
        private static IEnumerable<INamedTypeSymbol> ResolveTaskTypes([NotNull] Compilation compilation)
        {
            foreach (INamedTypeSymbol taskType in new[]
            {
                KnownTypes.SystemThreadingTasksTaskT(compilation),
                KnownTypes.SystemThreadingTasksTask(compilation),
                KnownTypes.SystemThreadingTasksValueTask(compilation),
                KnownTypes.SystemThreadingTasksValueTaskT(compilation)
            })
            {
                if (taskType != null)
                {
                    yield return taskType;
                }
            }
        }

        private static void AnalyzeReturn(OperationAnalysisContext context,
            [NotNull] [ItemNotNull] IList<INamedTypeSymbol> taskTypes)
        {
            var returnOperation = (IReturnOperation)context.Operation;

            if (returnOperation.ReturnedValue?.Type == null || !ReturnsStringOrCollectionOrTask(returnOperation, taskTypes))
            {
                return;
            }

            if (returnOperation.ReturnedValue.ConstantValue.HasValue && returnOperation.ReturnedValue.ConstantValue.Value == null)
            {
                ReportReturnStatement(returnOperation, context);
            }
        }

        private static bool ReturnsStringOrCollectionOrTask([NotNull] IReturnOperation returnOperation,
            [NotNull] [ItemNotNull] IList<INamedTypeSymbol> taskTypes)
        {
            return ImplementsIEnumerable(returnOperation.ReturnedValue.Type) ||
                IsTask(returnOperation.ReturnedValue.Type, taskTypes);
        }

        private static bool ImplementsIEnumerable([NotNull] ITypeSymbol type)
        {
            return IsEnumerableInterface(type) || type.AllInterfaces.Any(IsEnumerableInterface);
        }

        private static bool IsEnumerableInterface([NotNull] ITypeSymbol type)
        {
            return type.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T ||
                type.SpecialType == SpecialType.System_Collections_IEnumerable;
        }

        private static bool IsTask([NotNull] ITypeSymbol type, [NotNull] [ItemNotNull] IList<INamedTypeSymbol> taskTypes)
        {
            return taskTypes.Any(taskType => taskType.IsEqualTo(type.OriginalDefinition));
        }

        private static void ReportReturnStatement([NotNull] IReturnOperation returnOperation, OperationAnalysisContext context)
        {
            IMethodSymbol method = returnOperation.TryGetContainingMethod(context.Compilation);

            if (method != null && !method.IsSynthesized())
            {
                Location location = returnOperation.ReturnedValue.Syntax.GetLocation();
                string kind = method.GetKind().ToLowerInvariant();

                context.ReportDiagnostic(Diagnostic.Create(Rule, location, kind,
                    method.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat)));
            }
        }
    }
}
