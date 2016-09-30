using System.Collections.Generic;
using System.Text;
using CSharpGuidelinesAnalyzer.Utilities;
using JetBrains.Annotations;

namespace CSharpGuidelinesAnalyzer.Test.TestDataBuilders
{
    public class ClassSourceCodeBuilder : SourceCodeBuilder
    {
        [NotNull]
        [ItemNotNull]
        private readonly List<string> classes = new List<string>();

        private bool generateNamespace;

        protected override string GetSourceCode()
        {
            var builder = new StringBuilder();

            if (generateNamespace)
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

            if (generateNamespace)
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
            generateNamespace = false;
            return this;
        }
    }
}