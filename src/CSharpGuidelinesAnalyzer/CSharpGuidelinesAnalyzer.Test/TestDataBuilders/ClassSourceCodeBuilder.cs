using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;

namespace CSharpGuidelinesAnalyzer.Test.TestDataBuilders
{
    public class ClassSourceCodeBuilder : SourceCodeBuilder
    {
        [NotNull]
        [ItemNotNull]
        private readonly List<string> classes = new List<string>();

        private bool doGenerateNamespace;

        protected override string GetSourceCode()
        {
            var builder = new StringBuilder();

            if (doGenerateNamespace)
            {
                builder.AppendLine("namespace TestNamespace");
                builder.AppendLine("{");
            }

            int index = 0;
            foreach (string classCode in classes)
            {
                if (index > 0)
                {
                    builder.AppendLine();
                }

                builder.AppendLine(classCode.Trim());
                index++;
            }

            if (doGenerateNamespace)
            {
                builder.AppendLine("}");
            }

            return builder.ToString();
        }

        [NotNull]
        public ClassSourceCodeBuilder InGlobalScope([NotNull] string classCode)
        {
            Guard.NotNull(classCode, nameof(classCode));

            classes.Add(classCode);
            doGenerateNamespace = false;
            return this;
        }
    }
}