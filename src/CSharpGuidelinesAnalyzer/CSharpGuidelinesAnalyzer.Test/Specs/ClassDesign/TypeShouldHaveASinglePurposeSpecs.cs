using CSharpGuidelinesAnalyzer.Rules.ClassDesign;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.ClassDesign
{
    public sealed class TypeShouldHaveASinglePurposeSpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => TypeShouldHaveASinglePurposeAnalyzer.DiagnosticId;

        [Fact]
        internal void When_class_name_contains_the_word_And_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class [|CustomerAndOrder|]
                    {
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Type 'CustomerAndOrder' contains the word 'and', which suggests it has multiple purposes.");
        }

        [Fact]
        internal void When_struct_name_contains_the_word_and_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    struct [|customer_and_order|]
                    {
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Type 'customer_and_order' contains the word 'and', which suggests it has multiple purposes.");
        }

        [Fact]
        internal void When_uppercase_interface_name_contains_the_word_and_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    interface [|CUSTOMER_AND_ORDER|]
                    {
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Type 'CUSTOMER_AND_ORDER' contains the word 'and', which suggests it has multiple purposes.");
        }

        [Fact]
        internal void When_enum_name_contains_the_word_and_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    enum [|Match1And2Again3|]
                    {
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Type 'Match1And2Again3' contains the word 'and', which suggests it has multiple purposes.");
        }

        [Fact]
        internal void When_uppercase_enum_name_contains_the_word_and_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    enum [|MATCH1AND2AGAIN3|]
                    {
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Type 'MATCH1AND2AGAIN3' contains the word 'and', which suggests it has multiple purposes.");
        }

        [Fact]
        internal void When_delegate_name_contains_the_word_And_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    delegate void [|CustomerAndOrder|]();
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Type 'CustomerAndOrder' contains the word 'and', which suggests it has multiple purposes.");
        }

        [Fact]
        internal void When_class_name_contains_the_word_Land_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class TheBigLandIsHere
                    {
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_struct_name_contains_the_word_Andy_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    struct VisitAndyWithJohn
                    {
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_class_name_consists_of_the_word_And_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class And
                    {
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_class_name_starts_with_the_word_And_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class AndComputer
                    {
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_class_name_ends_with_the_word_And_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class LogicalAnd
                    {
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new TypeShouldHaveASinglePurposeAnalyzer();
        }
    }
}
