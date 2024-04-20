using System.Text;

namespace CSharpGuidelinesAnalyzer.Test.TestDataBuilders;

/// <summary />
internal sealed class MemberSourceCodeBuilder : SourceCodeBuilder
{
    private readonly List<string> members = [];

    public MemberSourceCodeBuilder()
        : base(DefaultNamespaceImports)
    {
    }

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
