using System.Collections.Immutable;
using System.Linq;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace CSharpGuidelinesAnalyzer.Rules.Framework
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class FavorAsyncAwaitOverTaskContinueWithAnalyzer : GuidelineAnalyzer
    {
        public const string DiagnosticId = "AV2235";

        private const string Title = "Call to Task.ContinueWith should be replaced with an async method";
        private const string MessageFormat = "The call to 'Task.ContinueWith' in '{0}' should be replaced with an async method.";
        private const string Description = "Favor async/await over the Task.";
        private const string Category = "Framework";

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category,
            DiagnosticSeverity.Warning, true, Description, HelpLinkUris.GetForCategory(Category, DiagnosticId));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(startContext =>
            {
                if (startContext.Compilation.SupportsOperations())
                {
                    RegisterAnalyzeCompilation(startContext);
                }
            });
        }

        private void RegisterAnalyzeCompilation([NotNull] CompilationStartAnalysisContext startContext)
        {
            INamedTypeSymbol taskType = startContext.Compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
            if (taskType != null)
            {
                ImmutableArray<ISymbol> continueWithMethodGroup = taskType.GetMembers("ContinueWith");

                startContext.RegisterOperationAction(
                    c => c.SkipInvalid(_ => AnalyzeInvocation(taskType, continueWithMethodGroup, c)),
                    OperationKind.Invocation);
            }
        }

        private void AnalyzeInvocation([NotNull] INamedTypeSymbol taskType,
            [ItemNotNull] ImmutableArray<ISymbol> continueWithMethodGroup, OperationAnalysisContext context)
        {
            var invocation = (IInvocationOperation)context.Operation;

            if (invocation.TargetMethod.ContainingType.Equals(taskType))
            {
                IMethodSymbol targetMethodConstructed = invocation.TargetMethod.ConstructedFrom;

                if (continueWithMethodGroup.Any(method => method.Equals(targetMethodConstructed)))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, context.Operation.Syntax.GetLocation(),
                        context.ContainingSymbol.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat)));
                }
            }
        }
    }
}
