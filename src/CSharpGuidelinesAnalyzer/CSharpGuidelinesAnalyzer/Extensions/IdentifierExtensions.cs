using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;

namespace CSharpGuidelinesAnalyzer.Extensions
{
    /// <summary />
    internal static class IdentifierExtensions
    {
        [NotNull]
        public static IReadOnlyCollection<WordToken> GetWordsInList([NotNull] this string identifierName, [NotNull] [ItemNotNull] ICollection<string> list)
        {
            Guard.NotNull(identifierName, nameof(identifierName));
            Guard.NotNullNorEmpty(list, nameof(list));

            if (string.IsNullOrWhiteSpace(identifierName) || !QuickScanMayContainWordsListed(identifierName, list))
            {
                return ImmutableArray<WordToken>.Empty;
            }

            var tokenizer = new WordsTokenizer(identifierName);
            return tokenizer.GetWords().Where(word => IsListed(word, list)).ToArray();
        }

        public static bool ContainsWordInTheMiddle([NotNull] this string identifierName, [NotNull] string word)
        {
            Guard.NotNull(identifierName, nameof(identifierName));
            Guard.NotNullNorWhiteSpace(word, nameof(word));

            if (string.IsNullOrWhiteSpace(identifierName) || !QuickScanMayContainWord(identifierName, word))
            {
                return false;
            }

            var tokenizer = new WordsTokenizer(identifierName);
            WordToken[] identifierTokens = tokenizer.GetWords().ToArray();

            return TokenSetContainsWordInTheMiddle(identifierTokens, word);
        }

        private static bool TokenSetContainsWordInTheMiddle([NotNull] IReadOnlyList<WordToken> tokenSet, [NotNull] string word)
        {
            for (int index = 1; index < tokenSet.Count - 1; index++)
            {
                WordToken token = tokenSet[index];

                if (string.Equals(word, token.Text, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool StartsWithWordInList([NotNull] this string identifierName, [NotNull] [ItemNotNull] ICollection<string> list)
        {
            Guard.NotNull(identifierName, nameof(identifierName));
            Guard.NotNullNorEmpty(list, nameof(list));

            if (string.IsNullOrWhiteSpace(identifierName) || !QuickScanMayContainWordsListed(identifierName, list))
            {
                return false;
            }

            var tokenizer = new WordsTokenizer(identifierName);

            WordToken[] words = tokenizer.GetWords().Take(1).ToArray();
            return words.Any() && IsListed(words[0], list);
        }

        private static bool QuickScanMayContainWordsListed([NotNull] string text, [NotNull] [ItemNotNull] IEnumerable<string> list)
        {
            return list.Any(word => QuickScanMayContainWord(text, word));
        }

        private static bool QuickScanMayContainWord([NotNull] string text, [NotNull] string word)
        {
            return text.IndexOf(word, StringComparison.OrdinalIgnoreCase) != -1;
        }

        private static bool IsListed(WordToken wordToken, [NotNull] [ItemNotNull] IEnumerable<string> list)
        {
            return list.Any(word => string.Equals(word, wordToken.Text, StringComparison.OrdinalIgnoreCase));
        }
    }
}
