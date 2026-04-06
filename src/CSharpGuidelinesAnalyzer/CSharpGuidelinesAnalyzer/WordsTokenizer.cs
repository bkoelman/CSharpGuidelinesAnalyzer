namespace CSharpGuidelinesAnalyzer;

/// <summary>
/// Breaks up camel-case, pascal-case and uppercase identifier names into words.
/// </summary>
public sealed class WordsTokenizer
{
    private readonly string text;

    private int position;

    public WordsTokenizer(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        this.text = text;
    }

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
            char? nextChar = PeekChar();

            return IsLowerCaseLetter(nextChar)
                ? CreateTokenFromScan(startIndex, WordTokenKind.PascalCaseWord)
                : CreateTokenFromScan(startIndex, WordTokenKind.UpperCaseWord);
        }

        return CreateTokenFromScan(startIndex, WordTokenKind.Separators);
    }

    private static bool IsUpperCaseLetter(char? ch)
    {
        return ch != null && char.IsLetter(ch.Value) && char.IsUpper(ch.Value);
    }

    private static bool IsLowerCaseLetter(char? ch)
    {
        return ch != null && char.IsLetter(ch.Value) && !char.IsUpper(ch.Value);
    }

    private static bool IsSeparator(char? ch)
    {
        return ch != null && !IsLowerCaseLetter(ch) && !IsUpperCaseLetter(ch);
    }

    private char? PeekChar()
    {
        return position < text.Length - 1 ? text[position + 1] : null;
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
        while (HasNextCharOfKind(kind))
        {
            position++;
        }

        position++;
    }

    private bool HasNextCharOfKind(WordTokenKind kind)
    {
        char? nextChar = PeekChar();
        return IsTokenKind(nextChar, kind);
    }

    private static bool IsTokenKind(char? ch, WordTokenKind kind)
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

    private string ExtractText(int startIndex)
    {
        return text.Substring(startIndex, position - startIndex);
    }

    private struct InfiniteLoopGuard
    {
        private int iterationCount;

        public void EnterNextIteration(string text, int position)
        {
            iterationCount++;

            if (iterationCount >= 1000)
            {
                throw new Exception($"Internal error: infinite loop detected while tokenizing text '{text}' at position {position}.");
            }
        }
    }
}
