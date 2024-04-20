using System.Collections.Immutable;
using System.Text;
using CSharpGuidelinesAnalyzer.Test.RoslynTestFramework;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace CSharpGuidelinesAnalyzer.Test.TestDataBuilders;

/// <summary />
internal abstract class SourceCodeBuilder : ITestDataBuilder<ParsedSourceCode>
{
    protected static readonly ImmutableArray<string> DefaultNamespaceImports = ["System"];

    public static readonly AnalyzerTestContext DefaultTestContext =
        new(string.Empty, [], new AnalyzerOptions([]));

    private readonly HashSet<string> namespaceImports;

    internal readonly CodeEditor Editor;

    private AnalyzerTestContext testContext = DefaultTestContext;

    protected SourceCodeBuilder(IEnumerable<string> implicitNamespaceImports)
    {
        namespaceImports = implicitNamespaceImports.ToHashSet();
        Editor = new CodeEditor(this);
    }

    public ParsedSourceCode Build()
    {
        string sourceText = GetCompleteSourceText();

        return new ParsedSourceCode(sourceText, testContext);
    }

    private string GetCompleteSourceText()
    {
        var sourceBuilder = new StringBuilder();

        WriteNamespaceImports(sourceBuilder);

        string sourceCode = GetSourceCode();
        sourceBuilder.Append(sourceCode);

        return sourceBuilder.ToString();
    }

    private void WriteNamespaceImports(StringBuilder sourceBuilder)
    {
        if (namespaceImports.Any())
        {
            foreach (string namespaceImport in namespaceImports)
            {
                sourceBuilder.AppendLine($"using {namespaceImport};");
            }

            sourceBuilder.AppendLine();
        }
    }

    protected abstract string GetSourceCode();

    protected string GetLinesOfCode(IEnumerable<string> codeBlocks)
    {
        Guard.NotNull(codeBlocks, nameof(codeBlocks));

        var builder = new StringBuilder();
        AppendCodeBlocks(codeBlocks, builder);

        return builder.ToString();
    }

    private static void AppendCodeBlocks(IEnumerable<string> codeBlocks, StringBuilder builder)
    {
        bool isInFirstBlock = true;

        foreach (string codeBlock in codeBlocks)
        {
            if (isInFirstBlock)
            {
                isInFirstBlock = false;
            }
            else
            {
                builder.AppendLine();
            }

            string trimmed = codeBlock.TrimEnd();
            AppendCodeBlock(trimmed, builder);
        }
    }

    private static void AppendCodeBlock(string codeBlock, StringBuilder builder)
    {
        bool isOnFirstLineInBlock = true;

        foreach (string line in GetLinesInText(codeBlock))
        {
            if (isOnFirstLineInBlock)
            {
                if (line.Trim().Length == 0)
                {
                    continue;
                }

                isOnFirstLineInBlock = false;
            }

            builder.AppendLine(line);
        }
    }

    private static IEnumerable<string> GetLinesInText(string text)
    {
        using var reader = new StringReader(text);

        while (reader.ReadLine() is { } line)
        {
            yield return line;
        }
    }

    internal sealed class CodeEditor
    {
        private readonly SourceCodeBuilder owner;

        public CodeEditor(SourceCodeBuilder owner)
        {
            Guard.NotNull(owner, nameof(owner));
            this.owner = owner;
        }

        public void UpdateTestContext(Func<AnalyzerTestContext, AnalyzerTestContext> change)
        {
            Guard.NotNull(change, nameof(change));

            owner.testContext = change(owner.testContext);
        }

        public void IncludeNamespaceImport(string codeNamespace)
        {
            owner.namespaceImports.Add(codeNamespace);
        }
    }
}
