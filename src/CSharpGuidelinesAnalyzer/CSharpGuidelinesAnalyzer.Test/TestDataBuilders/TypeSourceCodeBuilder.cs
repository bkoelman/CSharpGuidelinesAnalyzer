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

        protected override string GetSourceCode()
        {
            var builder = new StringBuilder();

            AppendTypes(builder);

            return builder.ToString();
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

        [NotNull]
        public TypeSourceCodeBuilder InGlobalScope([NotNull] string typeCode)
        {
            Guard.NotNull(typeCode, nameof(typeCode));

            types.Add(typeCode);
            return this;
        }
    }
}
