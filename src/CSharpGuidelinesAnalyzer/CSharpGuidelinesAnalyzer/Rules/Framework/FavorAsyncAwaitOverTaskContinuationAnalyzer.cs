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
        public const string DiagnosticId = "AV2235";

        private const string Title = "Call to Task.ContinueWith should be replaced with an await expression";

        private const string MessageFormat =
            "The call to 'Task.ContinueWith' in '{0}' should be replaced with an await expression.";

        private const string Description = "Favor async/await over Task continuations.";

        [NotNull]
        private static readonly AnalyzerCategory Category = AnalyzerCategory.Framework;

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category.DisplayName, DiagnosticSeverity.Warning, true, Description, Category.GetHelpLinkUri(DiagnosticId));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(startContext =>
            {
                var taskInfo = new TaskTypeInfo(startContext.Compilation);

                if (!taskInfo.ContinueWithMethodGroup.IsEmpty)
                {
                    startContext.RegisterOperationAction(c => c.SkipInvalid(_ => AnalyzeInvocation(taskInfo, c)),
                        OperationKind.Invocation);
                }
            });
        }

        private void AnalyzeInvocation(TaskTypeInfo taskInfo, OperationAnalysisContext context)
        {
            var invocation = (IInvocationOperation)context.Operation;

            if (invocation.TargetMethod.ContainingType.Equals(taskInfo.TaskType) ||
                invocation.TargetMethod.ContainingType.ConstructedFrom.Equals(taskInfo.GenericTaskType))
            {
                IMethodSymbol openTypedTargetMethod = invocation.TargetMethod.OriginalDefinition;

                if (taskInfo.ContinueWithMethodGroup.Any(method => method.Equals(openTypedTargetMethod)))
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
                .First(x => x.Identifier.ValueText == "ContinueWith");

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

                ContinueWithMethodGroup = GetTaskContinueWithMethodGroup();
            }

            [ItemNotNull]
            private ImmutableArray<ISymbol> GetTaskContinueWithMethodGroup()
            {
                ImmutableArray<ISymbol> taskContinueWithMethodGroup =
                    TaskType?.GetMembers("ContinueWith") ?? ImmutableArray<ISymbol>.Empty;

                ImmutableArray<ISymbol> genericTaskContinueWithMethodGroup =
                    GenericTaskType?.GetMembers("ContinueWith") ?? ImmutableArray<ISymbol>.Empty;

                return taskContinueWithMethodGroup.Union(genericTaskContinueWithMethodGroup).ToImmutableArray();
            }
        }
    }
}
