using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace CSharpGuidelinesAnalyzer.Suppressions.Storage
{
    public sealed class SuppressionRoot
    {
        [NotNull]
        public Version Version { get; }

        [NotNull]
        [ItemNotNull]
        public ICollection<SuppressionRule> Rules { get; }

        public SuppressionRoot([NotNull] Version version, [NotNull] [ItemNotNull] IEnumerable<SuppressionRule> rules)
        {
            Guard.NotNull(version, nameof(version));
            Guard.NotNull(rules, nameof(rules));

            Version = version;
            Rules = new HashSet<SuppressionRule>(rules);
        }
    }
}
