using System.Linq;
using FluentAssertions;
using JetBrains.Annotations;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs
{
    public sealed class WordsTokenizerSpecs
    {
        [Fact]
        public void UnitTests()
        {
            RunTest("");

            RunTest("first",
                new WordToken("first", WordTokenKind.CamelCaseWord));

            RunTest("First",
                new WordToken("First", WordTokenKind.PascalCaseWord));

            RunTest("FIRST",
                new WordToken("FIRST", WordTokenKind.UpperCaseWord));

            RunTest("_____",
                new WordToken("_____", WordTokenKind.Separators));

            RunTest("first_second",
                new WordToken("first", WordTokenKind.CamelCaseWord),
                new WordToken("_", WordTokenKind.Separators),
                new WordToken("second", WordTokenKind.CamelCaseWord));

            RunTest("firstSecond",
                new WordToken("first", WordTokenKind.CamelCaseWord),
                new WordToken("Second", WordTokenKind.PascalCaseWord));

            RunTest("first_Second",
                new WordToken("first", WordTokenKind.CamelCaseWord),
                new WordToken("_", WordTokenKind.Separators),
                new WordToken("Second", WordTokenKind.PascalCaseWord));

            RunTest("FirstSecond",
                new WordToken("First", WordTokenKind.PascalCaseWord),
                new WordToken("Second", WordTokenKind.PascalCaseWord));

            RunTest("First_Second",
                new WordToken("First", WordTokenKind.PascalCaseWord),
                new WordToken("_", WordTokenKind.Separators),
                new WordToken("Second", WordTokenKind.PascalCaseWord));

            RunTest("FIRSTSecond",
                new WordToken("FIRST", WordTokenKind.UpperCaseWord),
                new WordToken("Second", WordTokenKind.PascalCaseWord));

            RunTest("FIRST_Second",
                new WordToken("FIRST", WordTokenKind.UpperCaseWord),
                new WordToken("_", WordTokenKind.Separators),
                new WordToken("Second", WordTokenKind.PascalCaseWord));

            RunTest("firstSECOND",
                new WordToken("first", WordTokenKind.CamelCaseWord),
                new WordToken("SECOND", WordTokenKind.UpperCaseWord));

            RunTest("first_SECOND",
                new WordToken("first", WordTokenKind.CamelCaseWord),
                new WordToken("_", WordTokenKind.Separators),
                new WordToken("SECOND", WordTokenKind.UpperCaseWord));

            RunTest("FIRST_SECOND",
                new WordToken("FIRST", WordTokenKind.UpperCaseWord),
                new WordToken("_", WordTokenKind.Separators),
                new WordToken("SECOND", WordTokenKind.UpperCaseWord));

            RunTest("AFirstS",
                new WordToken("A", WordTokenKind.UpperCaseWord),
                new WordToken("First", WordTokenKind.PascalCaseWord),
                new WordToken("S", WordTokenKind.UpperCaseWord));

            RunTest("A_First_S",
                new WordToken("A", WordTokenKind.UpperCaseWord),
                new WordToken("_", WordTokenKind.Separators),
                new WordToken("First", WordTokenKind.PascalCaseWord),
                new WordToken("_", WordTokenKind.Separators),
                new WordToken("S", WordTokenKind.UpperCaseWord));

            RunTest("_first_",
                new WordToken("_", WordTokenKind.Separators),
                new WordToken("first", WordTokenKind.CamelCaseWord),
                new WordToken("_", WordTokenKind.Separators));
        }

        private static void RunTest([NotNull] string text, [NotNull] params WordToken[] expected)
        {
            var tokenizer = new WordsTokenizer(text);
            WordToken[] tokens = tokenizer.GetTokens().ToArray();

            tokens.Should().HaveSameCount(expected);

            for (int index = 0; index < tokens.Length; index++)
            {
                tokens[index].Should().Be(expected[index]);
            }
        }
    }
}