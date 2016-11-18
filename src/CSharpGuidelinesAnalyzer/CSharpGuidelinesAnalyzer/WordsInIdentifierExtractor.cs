using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;

namespace CSharpGuidelinesAnalyzer
{
    /// <summary>
    /// Breaks up camel-cased identifier names into separate words.
    /// </summary>
    internal struct WordsInIdentifierExtractor
    {
        [NotNull]
        private readonly string identifierName;

        [NotNull]
        [ItemNotNull]
        private readonly List<string> words;

        [NotNull]
        private readonly StringBuilder builder;

        public WordsInIdentifierExtractor([NotNull] string identifierName)
        {
            Guard.NotNullNorWhiteSpace(identifierName, nameof(identifierName));

            this.identifierName = identifierName;
            words = new List<string>();
            builder = new StringBuilder();
        }

        [NotNull]
        [ItemNotNull]
        public IList<string> ExtractWords()
        {
            Reset();

            foreach (char ch in identifierName)
            {
                ProcessCharacter(ch);
            }

            FlushBuilder();

            return words;
        }

        private void Reset()
        {
            words.Clear();
            builder.Clear();
        }

        private void ProcessCharacter(char ch)
        {
            if (IsStartOfNextWord(ch))
            {
                FlushBuilder();

                if (!char.IsUpper(ch))
                {
                    return;
                }
            }

            builder.Append(ch);
        }

        private static bool IsStartOfNextWord(char ch)
        {
            return char.IsUpper(ch) || char.IsWhiteSpace(ch) || char.IsPunctuation(ch) || char.IsDigit(ch) ||
                char.IsSymbol(ch);
        }

        private void FlushBuilder()
        {
            if (builder.Length > 0)
            {
                string word = builder.ToString();
                words.Add(word);

                builder.Clear();
            }
        }
    }
}