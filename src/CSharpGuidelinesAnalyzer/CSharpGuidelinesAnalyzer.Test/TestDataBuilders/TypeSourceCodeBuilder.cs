using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;

namespace CSharpGuidelinesAnalyzer.Test.TestDataBuilders
{
    /// <summary />
    internal sealed class TypeSourceCodeBuilder : SourceCodeBuilder
    {
        [NotNull]
        [ItemNotNull]
        private readonly List<string> types = new List<string>();

        private bool doGenerateNamespace;

        protected override string GetSourceCode()
        {
            var builder = new StringBuilder();

            AppendNamespaceStart(builder);
            AppendTypes(builder);
            AppendNamespaceEnd(builder);

            return builder.ToString();
        }

        private void AppendNamespaceStart([NotNull] StringBuilder builder)
        {
            if (doGenerateNamespace)
            {
                builder.AppendLine("namespace TestNamespace");
                builder.AppendLine("{");
            }
        }

        private void AppendTypes([NotNull] StringBuilder builder)
        {
            int index = 0;
            foreach (string type in types)
            {
                if (index > 0)
                {
                    builder.AppendLine();
                }

                builder.AppendLine(type.Trim());
                index++;
            }
        }

        private void AppendNamespaceEnd([NotNull] StringBuilder builder)
        {
            if (doGenerateNamespace)
            {
                builder.AppendLine("}");
            }
        }

        [NotNull]
        public TypeSourceCodeBuilder InGlobalScope([NotNull] string typeCode)
        {
            Guard.NotNull(typeCode, nameof(typeCode));

            types.Add(typeCode);
            doGenerateNamespace = false;
            return this;
        }
    }
}