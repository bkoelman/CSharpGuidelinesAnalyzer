using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;

namespace CSharpGuidelinesAnalyzer.Test.TestDataBuilders
{
    /// <summary />
    internal sealed class MemberSourceCodeBuilder : SourceCodeBuilder
    {
        [NotNull]
        [ItemNotNull]
        private readonly List<string> members = new List<string>();

        protected override string GetSourceCode()
        {
            var builder = new StringBuilder();
            builder.AppendLine("public class Test");
            builder.AppendLine("{");

            int index = 0;
            foreach (string member in members)
            {
                if (index > 0)
                {
                    builder.AppendLine();
                }

                builder.AppendLine(member.Trim());
                index++;
            }

            builder.AppendLine("}");
            return builder.ToString();
        }

        [NotNull]
        public MemberSourceCodeBuilder InDefaultClass([NotNull] string memberCode)
        {
            Guard.NotNull(memberCode, nameof(memberCode));

            members.Add(memberCode);
            return this;
        }
    }
}