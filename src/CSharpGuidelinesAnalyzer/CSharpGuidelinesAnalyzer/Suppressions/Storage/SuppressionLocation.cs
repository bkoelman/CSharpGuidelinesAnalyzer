using JetBrains.Annotations;

namespace CSharpGuidelinesAnalyzer.Suppressions.Storage
{
    public sealed class SuppressionLocation
    {
        public SuppressionOffset RelativeTo { get; }

        [NotNull]
        public string Value { get; }

        [NotNull]
        public SuppressionPosition Position { get; }

        public SuppressionLocation(SuppressionOffset relativeTo, [NotNull] string value, [NotNull] SuppressionPosition position)
        {
            Guard.NotNull(value, nameof(value));
            Guard.NotNull(position, nameof(position));

            RelativeTo = relativeTo;
            Position = position;
            Value = value;
        }

        public override string ToString()
        {
            return $"{RelativeTo}:{Value}@{Position}";
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }
}
