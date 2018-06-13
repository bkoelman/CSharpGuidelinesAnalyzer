using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace CSharpGuidelinesAnalyzer
{
    /// <summary>
    /// Breaks up camel-case, pascal-case and uppercase identifier names into words.
    /// </summary>
    public sealed class WordsTokenizer
    {
        [NotNull]
        private readonly string text;

        private int position;

        public WordsTokenizer([NotNull] string text)
        {
            Guard.NotNull(text, nameof(text));
            this.text = text;
        }

        [NotNull]
        public IEnumerable<WordToken> GetWords()
        {
            foreach (WordToken token in GetTokens())
            {
                if (token.Kind != WordTokenKind.Separators)
                {
                    yield return token;
                }
            }
        }

        [NotNull]
        public IEnumerable<WordToken> GetTokens()
        {
            Reset();

            var guard = new InfiniteLoopGuard();

            while (position < text.Length)
            {
                guard.EnterNextIteration(text, position);

                WordToken nextToken = GetNextToken();
                yield return nextToken;
            }
        }

        private void Reset()
        {
            position = 0;
        }

        private WordToken GetNextToken()
        {
            int startIndex = position;
            char ch = text[position];

            if (IsLowerCaseLetter(ch))
            {
                return CreateTokenFromScan(startIndex, WordTokenKind.CamelCaseWord);
            }

            if (IsUpperCaseLetter(ch))
            {
                return IsLowerCaseLetter(PeekChar())
                    ? CreateTokenFromScan(startIndex, WordTokenKind.PascalCaseWord)
                    : CreateTokenFromScan(startIndex, WordTokenKind.UpperCaseWord);
            }

            return CreateTokenFromScan(startIndex, WordTokenKind.Separators);
        }

        private static bool IsUpperCaseLetter([CanBeNull] char? ch)
        {
            return ch != null && char.IsLetter(ch.Value) && char.IsUpper(ch.Value);
        }

        private static bool IsLowerCaseLetter([CanBeNull] char? ch)
        {
            return ch != null && char.IsLetter(ch.Value) && !char.IsUpper(ch.Value);
        }

        private static bool IsSeparator([CanBeNull] char? ch)
        {
            return ch != null && !IsLowerCaseLetter(ch) && !IsUpperCaseLetter(ch);
        }

        [CanBeNull]
        private char? PeekChar()
        {
            return position < text.Length - 1 ? text[position + 1] : (char?)null;
        }

        private WordToken CreateTokenFromScan(int startIndex, WordTokenKind kind)
        {
            ConsumeWhile(kind);

            if (kind == WordTokenKind.UpperCaseWord)
            {
                bool nextCharIsLowerCaseLetter = position < text.Length && IsLowerCaseLetter(text[position]);
                if (nextCharIsLowerCaseLetter)
                {
                    PutBackLastUpperCaseLetterThatBelongsToNextWord();
                }
            }

            return CreateTokenFrom(startIndex, kind);
        }

        private void ConsumeWhile(WordTokenKind kind)
        {
            while (IsTokenKind(PeekChar(), kind))
            {
                position++;
            }

            position++;
        }

        private static bool IsTokenKind([CanBeNull] char? ch, WordTokenKind kind)
        {
            switch (kind)
            {
                case WordTokenKind.CamelCaseWord:
                case WordTokenKind.PascalCaseWord:
                {
                    return IsLowerCaseLetter(ch);
                }
                case WordTokenKind.UpperCaseWord:
                {
                    return IsUpperCaseLetter(ch);
                }
                case WordTokenKind.Separators:
                {
                    return IsSeparator(ch);
                }
                default:
                {
                    throw new NotSupportedException($"Unexpected token kind {kind}.");
                }
            }
        }

        private void PutBackLastUpperCaseLetterThatBelongsToNextWord()
        {
            position--;
        }

        private WordToken CreateTokenFrom(int startIndex, WordTokenKind kind)
        {
            string value = ExtractText(startIndex);
            return new WordToken(value, kind);
        }

        [NotNull]
        private string ExtractText(int startIndex)
        {
            return text.Substring(startIndex, position - startIndex);
        }

        private struct InfiniteLoopGuard
        {
            private int iterationCount;

            public void EnterNextIteration([NotNull] string text, int position)
            {
                iterationCount++;

                if (iterationCount >= 1000)
                {
                    throw new Exception(
                        $"Internal error: infinite loop detected while tokenizing text '{text}' at position {position}.");
                }
            }
        }
    }
}
