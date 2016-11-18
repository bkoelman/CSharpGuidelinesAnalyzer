using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;

namespace CSharpGuidelinesAnalyzer.Extensions
{
    /// <summary />
    internal static class IdentifierExtensions
    {
        public static bool StartsWithAnyWordOf([NotNull] this string identifierName,
            [ItemNotNull] ImmutableArray<string> wordsToFind, TextMatchMode matchMode)
        {
            Guard.NotNullNorWhiteSpace(identifierName, nameof(identifierName));

            string firstWord = GetFirstWord(identifierName);

            var availableWords = new TextCollection(wordsToFind);
            return availableWords.Contains(firstWord, matchMode);
        }

        [NotNull]
        private static string GetFirstWord([NotNull] string identifierName)
        {
            var extractor = new WordsInIdentifierExtractor(identifierName);
            IList<string> wordsInText = extractor.ExtractWords();

            return wordsInText.First();
        }

        [CanBeNull]
        public static string GetFirstWordInSetFromIdentifier([NotNull] this string identifierName,
            [ItemNotNull] [NotNull] ICollection<string> wordsToFind, TextMatchMode matchMode)
        {
            Guard.NotNullNorWhiteSpace(identifierName, nameof(identifierName));

            var extractor = new WordsInIdentifierExtractor(identifierName);
            var wordsInText = new TextCollection(extractor.ExtractWords());

            return wordsToFind.FirstOrDefault(word => wordsInText.Contains(word, matchMode));
        }
    }
}