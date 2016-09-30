using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using CSharpGuidelinesAnalyzer.Utilities;
using FluentAssertions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Text;

namespace CSharpGuidelinesAnalyzer.Test.RoslynTestFramework
{
    public static class Verify
    {
        public static void CodeAction([NotNull] CodeAction codeAction, [NotNull] Document document,
            [NotNull] string expectedCode)
        {
            if (codeAction.GetType().Name == "SolutionChangeAction")
            {
                return;
            }

            Guard.NotNull(codeAction, nameof(codeAction));
            Guard.NotNull(document, nameof(document));
            Guard.NotNull(expectedCode, nameof(expectedCode));

            ImmutableArray<CodeActionOperation> operations =
                codeAction.GetOperationsAsync(CancellationToken.None).Result;

            operations.Should().HaveCount(1);

            CodeActionOperation operation = operations.Single();
            Workspace workspace = document.Project.Solution.Workspace;
            operation.Apply(workspace, CancellationToken.None);

            Document newDocument = workspace.CurrentSolution.GetDocument(document.Id);

            SourceText sourceText = newDocument.GetTextAsync().Result;
            string text = sourceText.ToString();

            text.Should().Be(expectedCode);
        }
    }
}