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

        private const string Title = "Call to Task.ContinueWith should be replaced with an async method";
        private const string MessageFormat = "The call to 'Task.ContinueWith' in '{0}' should be replaced with an async method.";
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
                INamedTypeSymbol taskType = KnownTypes.SystemThreadingTasksTask(startContext.Compilation);
                INamedTypeSymbol genericTaskType = KnownTypes.SystemThreadingTasksTaskT(startContext.Compilation);

                ImmutableArray<ISymbol> continueWithMethodGroup = GetTaskContinueWithMethodGroup(taskType, genericTaskType);

                if (!continueWithMethodGroup.IsEmpty)
                {
                    startContext.RegisterOperationAction(
                        c => c.SkipInvalid(_ => AnalyzeInvocation(taskType, genericTaskType, continueWithMethodGroup, c)),
                        OperationKind.Invocation);
                }
            });
        }

        [ItemNotNull]
        private ImmutableArray<ISymbol> GetTaskContinueWithMethodGroup([CanBeNull] INamedTypeSymbol taskType,
            [CanBeNull] INamedTypeSymbol genericTaskType)
        {
            ImmutableArray<ISymbol> taskContinueWithMethodGroup =
                taskType?.GetMembers("ContinueWith") ?? ImmutableArray<ISymbol>.Empty;

            ImmutableArray<ISymbol> genericTaskContinueWithMethodGroup =
                genericTaskType?.GetMembers("ContinueWith") ?? ImmutableArray<ISymbol>.Empty;

            return taskContinueWithMethodGroup.Union(genericTaskContinueWithMethodGroup).ToImmutableArray();
        }

        private void AnalyzeInvocation([CanBeNull] INamedTypeSymbol taskType, [CanBeNull] INamedTypeSymbol genericTaskType,
            [ItemNotNull] ImmutableArray<ISymbol> continueWithMethodGroup, OperationAnalysisContext context)
        {
            var invocation = (IInvocationOperation)context.Operation;

            if (invocation.TargetMethod.ContainingType.Equals(taskType) ||
                invocation.TargetMethod.ContainingType.ConstructedFrom.Equals(genericTaskType))
            {
                IMethodSymbol openTypedTargetMethod = invocation.TargetMethod.OriginalDefinition;

                if (continueWithMethodGroup.Any(method => method.Equals(openTypedTargetMethod)))
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
            IdentifierNameSyntax identifierNameSyntax = context.Operation.Syntax.DescendantNodesAndSelf()
                .OfType<IdentifierNameSyntax>().First(x => x.Identifier.ValueText == "ContinueWith");

            return identifierNameSyntax.GetLocation();
        }
    }
}
