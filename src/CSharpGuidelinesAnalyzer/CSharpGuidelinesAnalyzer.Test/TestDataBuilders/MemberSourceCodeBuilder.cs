using System.Text;

namespace CSharpGuidelinesAnalyzer.Test.TestDataBuilders;

/// <summary />
// Workaround for https://github.com/bkoelman/CSharpGuidelinesAnalyzer/issues/155.
#pragma warning disable AV1500 // Member or local function contains too many statements
internal sealed class MemberSourceCodeBuilder() : SourceCodeBuilder(DefaultNamespaceImports)
#pragma warning restore AV1500 // Member or local function contains too many statements
{
    private readonly List<string> members = [];

    protected override string GetSourceCode()
    {
        var builder = new StringBuilder();

        AppendClassStart(builder);
        AppendClassMembers(builder);
        AppendClassEnd(builder);

        return builder.ToString();
    }

    private static void AppendClassStart(StringBuilder builder)
    {
        builder.AppendLine("public class Test");
        builder.AppendLine("{");
    }

    private void AppendClassMembers(StringBuilder builder)
    {
        string code = GetLinesOfCode(members);
        builder.AppendLine(code);
    }

    private static void AppendClassEnd(StringBuilder builder)
    {
        builder.AppendLine("}");
    }

    public MemberSourceCodeBuilder InDefaultClass(string memberCode)
    {
        Guard.NotNull(memberCode, nameof(memberCode));

        members.Add(memberCode);
        return this;
    }
}
