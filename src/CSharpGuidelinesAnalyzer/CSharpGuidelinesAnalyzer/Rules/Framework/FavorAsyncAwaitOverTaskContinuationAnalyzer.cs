using System.Collections.Immutable;
using CSharpGuidelinesAnalyzer.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace CSharpGuidelinesAnalyzer.Rules.Framework;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class FavorAsyncAwaitOverTaskContinuationAnalyzer : DiagnosticAnalyzer
{
    private const string Title = "Call to Task.ContinueWith should be replaced with an await expression";
    private const string MessageFormat = "The call to 'Task.ContinueWith' in '{0}' should be replaced with an await expression";
    private const string Description = "Favor async/await over Task continuations.";

    public const string DiagnosticId = AnalyzerCategory.RulePrefix + "2235";

    private static readonly AnalyzerCategory Category = AnalyzerCategory.Framework;

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
        var taskInfo = new TaskTypeInfo(startContext.Compilation);

        if (!taskInfo.ContinueWithMethodGroup.IsEmpty)
        {
            startContext.SafeRegisterOperationAction(context => AnalyzeInvocation(context, taskInfo), OperationKind.Invocation);
        }
    }

    private static void AnalyzeInvocation(OperationAnalysisContext context, TaskTypeInfo taskInfo)
    {
        var invocation = (IInvocationOperation)context.Operation;

        INamedTypeSymbol? targetMethodContainingType = invocation.TargetMethod.NullableContainingType;

        if (targetMethodContainingType != null && (targetMethodContainingType.IsEqualTo(taskInfo.TaskType) ||
            targetMethodContainingType.ConstructedFrom.IsEqualTo(taskInfo.GenericTaskType)))
        {
            IMethodSymbol openTypedTargetMethod = invocation.TargetMethod.OriginalDefinition;

            if (taskInfo.ContinueWithMethodGroup.Any(method => method.IsEqualTo(openTypedTargetMethod)))
            {
                Location location = GetInvocationLocation(context);

                string name = context.ContainingSymbol.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat);

                var diagnostic = Diagnostic.Create(Rule, location, name);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private static Location GetInvocationLocation(OperationAnalysisContext context)
    {
        SimpleNameSyntax simpleNameSyntax = context.Operation.Syntax.DescendantNodesAndSelf().OfType<SimpleNameSyntax>()
            .First(syntax => syntax.Identifier.ValueText == "ContinueWith");

        return simpleNameSyntax.GetLocation();
    }

    private struct TaskTypeInfo
    {
        public INamedTypeSymbol? TaskType { get; }

        public INamedTypeSymbol? GenericTaskType { get; }

        public ImmutableArray<ISymbol> ContinueWithMethodGroup { get; }

        public TaskTypeInfo(Compilation compilation)
        {
            ArgumentNullException.ThrowIfNull(compilation);

            GenericTaskType = KnownTypes.SystemThreadingTasksTaskT(compilation);
            TaskType = KnownTypes.SystemThreadingTasksTask(compilation);

            ContinueWithMethodGroup = GetTaskContinueWithMethodGroup(TaskType, GenericTaskType);
        }

        private static ImmutableArray<ISymbol> GetTaskContinueWithMethodGroup(INamedTypeSymbol? taskType, INamedTypeSymbol? genericTaskType)
        {
            ImmutableArray<ISymbol> taskContinueWithMethodGroup = taskType?.GetMembers("ContinueWith") ?? ImmutableArray<ISymbol>.Empty;
            ImmutableArray<ISymbol> genericTaskContinueWithMethodGroup = genericTaskType?.GetMembers("ContinueWith") ?? ImmutableArray<ISymbol>.Empty;

            return taskContinueWithMethodGroup.Union(genericTaskContinueWithMethodGroup, SymbolEqualityComparer.IncludeNullability).ToImmutableArray();
        }
    }
}
