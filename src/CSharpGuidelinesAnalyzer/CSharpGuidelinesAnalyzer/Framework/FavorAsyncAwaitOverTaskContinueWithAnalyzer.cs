using System.Collections.Immutable;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Semantics;
using System.Linq;

namespace CSharpGuidelinesAnalyzer.Framework
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class FavorAsyncAwaitOverTaskContinueWithAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV2235";

        private const string Title = "Call to Task.ContinueWith should be replaced with an async method";
        private const string MessageFormat = "The call to Task.ContinueWith in {0} should be replaced with an async method.";
        private const string Description = "Favor async/await over the Task.";
        private const string Category = "Framework";

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description,
            helpLinkUri: HelpLinkUris.GetForCategory(Category));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(startContext =>
            {
                if (!AnalysisUtilities.SupportsOperations(startContext.Compilation))
                {
                    return;
                }
                
                var taskType = startContext.Compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
                var continueWithMethodGroup = taskType.GetMembers("ContinueWith");

                if (taskType != null)
                {
                    startContext.RegisterOperationAction(c => AnalyzeInvocation(taskType, continueWithMethodGroup, c), OperationKind.InvocationExpression);
                }
            });
        }

        private void AnalyzeInvocation([NotNull] INamedTypeSymbol taskType, [ItemNotNull] ImmutableArray<ISymbol> continueWithMethodGroup, OperationAnalysisContext context)
        {
            var invocation = (IInvocationExpression)context.Operation;
            if (invocation.TargetMethod.ContainingType.Equals(taskType))
            {
                var targetMethodConstructed = invocation.TargetMethod.ConstructedFrom;

                if (continueWithMethodGroup.Any(method => method.Equals(targetMethodConstructed)))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, context.Operation.Syntax.GetLocation(), 
                        context.ContainingSymbol.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat)));
                }
            }
        }
    }
}