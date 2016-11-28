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
        public static ICollection<WordToken> GetWordsInList([NotNull] this string identifierName,
            [NotNull] [ItemNotNull] ICollection<string> list)
        {
            Guard.NotNullNorWhiteSpace(identifierName, nameof(identifierName));
            Guard.NotNullNorEmpty(list, nameof(list));

            if (!QuickScanMayContainWordsListed(identifierName, list))
            {
                return ImmutableArray<WordToken>.Empty;
            }

            var tokenizer = new WordsTokenizer(identifierName);
            return tokenizer.GetWords().Where(w => IsListed(w, list)).ToArray();
        }

        public static bool StartsWithWordInList([NotNull] this string identifierName,
            [NotNull] [ItemNotNull] ICollection<string> list)
        {
            Guard.NotNullNorWhiteSpace(identifierName, nameof(identifierName));
            Guard.NotNullNorEmpty(list, nameof(list));

            if (!QuickScanMayContainWordsListed(identifierName, list))
            {
                return false;
            }

            var tokenizer = new WordsTokenizer(identifierName);

            WordToken[] words = tokenizer.GetWords().Take(1).ToArray();
            return words.Any() && IsListed(words.First(), list);
        }

        private static bool QuickScanMayContainWordsListed([NotNull] string text, [NotNull] [ItemNotNull] IEnumerable<string> list)
        {
            return list.Any(word => text.IndexOf(word, StringComparison.OrdinalIgnoreCase) != -1);
        }

        private static bool IsListed(WordToken wordToken, [NotNull] [ItemNotNull] IEnumerable<string> list)
        {
            return list.Any(word => string.Equals(word, wordToken.Text, StringComparison.OrdinalIgnoreCase));
        }
    }
}
