using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace CSharpGuidelinesAnalyzer.Extensions
{
    /// <summary />
    internal static class IdentifierExtensions
    {
        public static bool StartsWithAnyWordOf([NotNull] this string identiferName,
            [ItemNotNull] ImmutableArray<string> wordsToFind, TextMatchMode matchMode)
        {
            Guard.NotNullNorWhiteSpace(identiferName, nameof(identiferName));

            IList<string> wordsInText = ExtractWords(identiferName);
            string firstWordInText = wordsInText.First();

            if (wordsToFind.Contains(firstWordInText))
            {
                return true;
            }

            if (matchMode == TextMatchMode.AllowLowerCaseMatch)
            {
                ImmutableArray<string> lowerCaseWordsToFind =
                    wordsToFind.Select(w => w.ToLowerInvariant()).ToImmutableArray();
                if (lowerCaseWordsToFind.Contains(firstWordInText.ToLowerInvariant()))
                {
                    return true;
                }
            }

            return false;
        }

        [CanBeNull]
        public static string GetFirstWordInSetFromIdentifier([NotNull] this string identiferName,
            [ItemNotNull] ImmutableArray<string> wordsToFind, TextMatchMode matchMode)
        {
            Guard.NotNullNorWhiteSpace(identiferName, nameof(identiferName));

            IList<string> wordsInText = ExtractWords(identiferName);

            ImmutableArray<string> lowerCaseWordsInText = matchMode == TextMatchMode.AllowLowerCaseMatch
                ? wordsInText.Select(w => w.ToLowerInvariant()).ToImmutableArray()
                : ImmutableArray<string>.Empty;

            foreach (string wordToFind in wordsToFind)
            {
                if (wordsInText.Contains(wordToFind))
                {
                    return wordToFind;
                }

                if (matchMode == TextMatchMode.AllowLowerCaseMatch)
                {
                    if (lowerCaseWordsInText.Contains(wordToFind.ToLowerInvariant()))
                    {
                        return wordToFind;
                    }
                }
            }

            return null;
        }

        [NotNull]
        [ItemNotNull]
        private static IList<string> ExtractWords([NotNull] string identifierName)
        {
            var words = new List<string>();

            var builder = new StringBuilder();
            foreach (char ch in identifierName)
            {
                if (IsStartOfNextWord(ch))
                {
                    FlushBuilder(words, builder);

                    if (!char.IsUpper(ch))
                    {
                        continue;
                    }
                }

                builder.Append(ch);
            }

            FlushBuilder(words, builder);

            return words;
        }

        private static bool IsStartOfNextWord(char ch)
        {
            return char.IsUpper(ch) || char.IsWhiteSpace(ch) || char.IsPunctuation(ch) || char.IsDigit(ch) ||
                char.IsSymbol(ch);
        }

        private static void FlushBuilder([NotNull] [ItemNotNull] List<string> words, [NotNull] StringBuilder builder)
        {
            if (builder.Length > 0)
            {
                words.Add(builder.ToString());
                builder.Clear();
            }
        }
    }
}