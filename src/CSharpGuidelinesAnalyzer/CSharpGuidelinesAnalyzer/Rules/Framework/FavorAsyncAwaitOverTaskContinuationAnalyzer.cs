using System;
using System.Collections.Immutable;
using System.Linq;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace CSharpGuidelinesAnalyzer.Rules.Framework
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class FavorAsyncAwaitOverTaskContinuationAnalyzer : DiagnosticAnalyzer
    {
        private const string Title = "Call to Task.ContinueWith should be replaced with an await expression";
        private const string MessageFormat = "The call to 'Task.ContinueWith' in '{0}' should be replaced with an await expression.";
        private const string Description = "Favor async/await over Task continuations.";

        public const string DiagnosticId = "AV2235";

        [NotNull]
        private static readonly AnalyzerCategory Category = AnalyzerCategory.Framework;

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category.DisplayName,
            DiagnosticSeverity.Warning, true, Description, Category.GetHelpLinkUri(DiagnosticId));

        [NotNull]
        private static readonly Action<CompilationStartAnalysisContext> RegisterCompilationStartAction = RegisterCompilationStart;

        [NotNull]
        private static readonly Action<OperationAnalysisContext, TaskTypeInfo> AnalyzeInvocationAction = (context, taskInfo) =>
            context.SkipInvalid(_ => AnalyzeInvocation(context, taskInfo));

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
            var taskInfo = new TaskTypeInfo(startContext.Compilation);

            if (!taskInfo.ContinueWithMethodGroup.IsEmpty)
            {
                startContext.RegisterOperationAction(context => AnalyzeInvocationAction(context, taskInfo), OperationKind.Invocation);
            }
        }

        private static void AnalyzeInvocation(OperationAnalysisContext context, TaskTypeInfo taskInfo)
        {
            var invocation = (IInvocationOperation)context.Operation;

            if (invocation.TargetMethod.ContainingType.IsEqualTo(taskInfo.TaskType) ||
                invocation.TargetMethod.ContainingType.ConstructedFrom.IsEqualTo(taskInfo.GenericTaskType))
            {
                IMethodSymbol openTypedTargetMethod = invocation.TargetMethod.OriginalDefinition;

                if (taskInfo.ContinueWithMethodGroup.Any(method => method.IsEqualTo(openTypedTargetMethod)))
                {
                    Location location = GetInvocationLocation(context);

                    context.ReportDiagnostic(Diagnostic.Create(Rule, location,
                        context.ContainingSymbol.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat)));
                }
            }
        }

        [NotNull]
        private static Location GetInvocationLocation(OperationAnalysisContext context)
        {
            SimpleNameSyntax simpleNameSyntax = context.Operation.Syntax.DescendantNodesAndSelf().OfType<SimpleNameSyntax>()
                .First(syntax => syntax.Identifier.ValueText == "ContinueWith");

            return simpleNameSyntax.GetLocation();
        }

        private struct TaskTypeInfo
        {
            [CanBeNull]
            public INamedTypeSymbol TaskType { get; }

            [CanBeNull]
            public INamedTypeSymbol GenericTaskType { get; }

            [ItemNotNull]
            public ImmutableArray<ISymbol> ContinueWithMethodGroup { get; }

            public TaskTypeInfo([NotNull] Compilation compilation)
            {
                Guard.NotNull(compilation, nameof(compilation));

                GenericTaskType = KnownTypes.SystemThreadingTasksTaskT(compilation);
                TaskType = KnownTypes.SystemThreadingTasksTask(compilation);

                ContinueWithMethodGroup = GetTaskContinueWithMethodGroup(TaskType, GenericTaskType);
            }

            [ItemNotNull]
            private static ImmutableArray<ISymbol> GetTaskContinueWithMethodGroup([CanBeNull] INamedTypeSymbol taskType,
                [CanBeNull] INamedTypeSymbol genericTaskType)
            {
                ImmutableArray<ISymbol> taskContinueWithMethodGroup = taskType?.GetMembers("ContinueWith") ?? ImmutableArray<ISymbol>.Empty;
                ImmutableArray<ISymbol> genericTaskContinueWithMethodGroup = genericTaskType?.GetMembers("ContinueWith") ?? ImmutableArray<ISymbol>.Empty;

                return taskContinueWithMethodGroup.Union(genericTaskContinueWithMethodGroup).ToImmutableArray();
            }
        }
    }
}
