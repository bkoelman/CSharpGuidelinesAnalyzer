namespace CSharpGuidelinesAnalyzer.Suppressions.Storage
{
    public sealed class SuppressionPosition
    {
        public int Line { get; }
        public int Column { get; }

        public SuppressionPosition(int line, int column)
        {
            Line = line;
            Column = column;
        }

        public override string ToString()
        {
            return $"{Line},{Column}";
        }
    }
}
