using System.Threading.Tasks;
using CSharpGuidelinesAnalyzer.Rules.Maintainability;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.Maintainability
{
    public sealed class DoNotChangeLoopVariableSpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => DoNotChangeLoopVariableAnalyzer.DiagnosticId;

        [Fact]
        internal async Task When_for_loop_declares_no_variables_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        int i = 0;
                        for (; i < 10; i++)
                        {
                            i = 5;
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source);
        }

        [Fact]
        internal async Task When_for_loop_assigns_variable_that_is_declared_outside_loop_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        int i;
                        for (i = 0; i < 10; i++)
                        {
                            i = 5;
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source);
        }

        [Fact]
        internal async Task When_for_loop_variable_is_not_written_to_in_body_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            Console.WriteLine(i);
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source);
        }

        [Fact]
        internal async Task When_for_loop_variable_is_written_to_in_body_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        for (int [|i|] = 0; i < 10; i++)
                        {
                            i = 5;
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Loop variable 'i' should not be written to in loop body");
        }

        [Fact]
        internal async Task When_for_loop_variable_is_passed_by_ref_in_body_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M(ref int x)
                    {
                        for (int [|i|] = 0; i < 10; i++)
                        {
                            M(ref i);
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Loop variable 'i' should not be written to in loop body");
        }

        [Fact]
        internal async Task When_for_loop_declares_multiple_variables_they_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        for (int [|i|] = 0, [|j|] = 5; i < 10; i++)
                        {
                            i++;
                            j -= 3;
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Loop variable 'i' should not be written to in loop body",
                "Loop variable 'j' should not be written to in loop body");
        }

        [Fact]
        internal async Task When_for_loop_variable_shadows_field_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    private int i;

                    void M()
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            this.i = 8;
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source);
        }

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new DoNotChangeLoopVariableAnalyzer();
        }
    }
}
