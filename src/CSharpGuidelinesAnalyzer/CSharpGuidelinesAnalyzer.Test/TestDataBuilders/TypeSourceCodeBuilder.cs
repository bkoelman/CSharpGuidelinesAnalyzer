using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;

namespace CSharpGuidelinesAnalyzer.Test.TestDataBuilders
{
    /// <summary />
    internal sealed class TypeSourceCodeBuilder : SourceCodeBuilder
    {
        private readonly List<string> types = new List<string>();

        public TypeSourceCodeBuilder()
            : base(DefaultNamespaceImports)
        {
        }

        protected override string GetSourceCode()
        {
            var builder = new StringBuilder();

            AppendTypes(builder);

            return builder.ToString();
        }

        private void AppendTypes(StringBuilder builder)
        {
            string code = GetLinesOfCode(types);
            builder.AppendLine(code);
        }

        public TypeSourceCodeBuilder InGlobalScope(string typeCode)
        {
            Guard.NotNull(typeCode, nameof(typeCode));

            types.Add(typeCode);
            return this;
        }
    }
}
