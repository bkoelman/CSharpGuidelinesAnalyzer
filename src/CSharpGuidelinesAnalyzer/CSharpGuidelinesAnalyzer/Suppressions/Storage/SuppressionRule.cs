using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace CSharpGuidelinesAnalyzer.Suppressions.Storage
{
    public sealed class SuppressionRule
    {
        [NotNull]
        public string Id { get; }

        [NotNull]
        [ItemNotNull]
        public ICollection<SuppressionLocation> Locations { get; }

        public SuppressionRule([NotNull] string id, [NotNull] [ItemNotNull] IEnumerable<SuppressionLocation> locations)
        {
            Guard.NotNull(id, nameof(id));
            Guard.NotNull(locations, nameof(locations));

            Id = id;
            Locations = new HashSet<SuppressionLocation>(locations);
        }

        public override string ToString()
        {
            return $"{Id} Count={Locations.Count}";
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
