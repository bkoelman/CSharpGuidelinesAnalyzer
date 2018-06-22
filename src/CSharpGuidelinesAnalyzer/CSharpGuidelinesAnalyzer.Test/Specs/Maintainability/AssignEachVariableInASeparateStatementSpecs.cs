using System;
using System.Collections.Generic;
using System.IO;
using CSharpGuidelinesAnalyzer.Rules.Maintainability;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.Maintainability
{
    public sealed class AssignEachVariableInASeparateStatementSpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => AssignEachVariableInASeparateStatementAnalyzer.DiagnosticId;

        [Fact]
        internal void When_two_variables_are_declared_in_separate_statements_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        int i;
                        int j;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_two_variables_are_declared_in_a_single_statement_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        int i, j;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_two_variables_are_assigned_in_separate_statements_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        int i, j;
                        i = 5;
                        j = 8;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_two_variables_are_assigned_in_a_single_statement_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        int i, j;
                        [|i = j = 5;|]
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "'i' and 'j' are assigned in a single statement.");
        }

        [Fact]
        internal void When_two_variables_are_assigned_in_a_single_compound_statement_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        int i = 5;
                        int j = 8;
                        [|i = j += i;|]                        
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "'i' and 'j' are assigned in a single statement.");
        }

        [Fact]
        internal void When_two_variables_are_assigned_in_a_single_increment_statement_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        int i, j = 5;
                        [|i = j++;|]
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "'i' and 'j' are assigned in a single statement.");
        }

        [Fact]
        internal void When_two_variables_are_declared_and_assigned_in_separate_statements_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        int i = 5;
                        int j = 8;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_two_variables_are_declared_and_assigned_in_a_single_statement_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        [|int i = 5, j = 8;|]
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "'i' and 'j' are assigned in a single statement.");
        }

        [Fact]
        internal void When_two_variables_are_declared_and_assigned_in_a_single_compound_statement_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        int i = 5;
                        int j = 8;
                        [|int k = j += i;|]
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "'k' and 'j' are assigned in a single statement.");
        }

        [Fact]
        internal void When_two_variables_are_declared_and_assigned_in_a_single_decrement_statement_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        int j = 5;
                        [|int i = --j;|]
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "'i' and 'j' are assigned in a single statement.");
        }

        [Fact]
        internal void When_variable_is_declared_and_two_variables_are_assigned_in_a_single_statement_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        int j;
                        [|int i = j = 5;|]
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "'i' and 'j' are assigned in a single statement.");
        }

        [Fact]
        internal void When_two_variables_are_assigned_multiple_times_in_a_single_statement_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        int i;
                        int j;
                        [|i = j = j = 5;|]
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "'i' and 'j' are assigned in a single statement.");
        }

        [Fact]
        internal void When_three_variables_are_assigned_in_a_single_statement_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        int i, j, k;
                        [|i = (true ? (j = 5) : (k = 7));|]
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "'i', 'j' and 'k' are assigned in a single statement.");
        }

        [Fact]
        internal void When_multiple_identifiers_are_assigned_in_a_single_statement_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    abstract class C
                    {
                        private int field1;

                        private int Property1 { get; set; }

                        public abstract int this[int index] { get; set; }

                        void M(ref int parameter1)
                        {
                            int local1;

                            [|local1 = field1 = Property1 = this[0] = parameter1 = 5;|]
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "'local1', 'C.field1', 'C.Property1', 'C.this[int]' and 'parameter1' are assigned in a single statement.");
        }

        [Fact]
        internal void When_variable_is_declared_and_multiple_identifiers_are_assigned_in_a_single_statement_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    abstract class C
                    {
                        private int field1;

                        private int Property1 { get; set; }

                        public abstract int this[int index] { get; set; }

                        void M(ref int parameter1)
                        {
                            int local1;

                            [|int decl1 = local1 = field1 = Property1 = this[0] = parameter1 = 5;|]
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "'decl1', 'local1', 'C.field1', 'C.Property1', 'C.this[int]' and 'parameter1' are assigned in a single statement.");
        }

        [Fact]
        internal void When_two_variables_are_assigned_in_the_initializer_of_a_for_loop_statement_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        int i, j, k = 8;

                        for ([|i = j = 5|]; (k = 1) > 0; k -= 2)
                        {
                            k = 3;
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "'i' and 'j' are assigned in a single statement.");
        }

        [Fact]
        internal void When_two_variables_are_sequentially_assigned_in_the_initializer_of_a_for_loop_statement_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        int i, j, k = 8;

                        for (i = 1, j = 5; (k = 1) > 0; k -= 2)
                        {
                            k = 3;
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void
            When_two_variables_are_declared_and_assigned_in_the_initializer_of_a_for_loop_statement_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        int k = 8;

                        for ([|int i = 1, j = 5|]; (k += 1) > 0; k -= 2)
                        {
                            k = 3;
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "'i' and 'j' are assigned in a single statement.");
        }

        [Fact]
        internal void When_two_variables_are_assigned_in_the_condition_of_a_for_loop_statement_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        int i, j, k;

                        for (k = 1; [|(i = j = 5) > 0|]; k -= 2)
                        {
                            k = 3;
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "'i' and 'j' are assigned in a single statement.");
        }

        [Fact]
        internal void When_two_variables_are_assigned_in_the_post_expression_of_a_for_loop_statement_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        int i, j, k;

                        for (k = 1; (k += 1) > 0; [|i = j = 5|])
                        {
                            k = 3;
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "'i' and 'j' are assigned in a single statement.");
        }

        [Fact]
        internal void
            When_two_variables_are_sequentially_assigned_in_the_post_expression_of_a_for_loop_statement_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        int i, j, k;

                        for (k = 1; (k += 1) > 0; i = 5, j = 8)
                        {
                            k = 3;
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void
            When_two_variables_are_assigned_in_separate_statements_in_the_body_of_a_for_loop_statement_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        int i, j;

                        for (int index = 0; index < 10; index++)
                        {
                            i = 5;
                            j = 8;
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void
            When_two_variables_are_assigned_in_a_single_statement_in_the_body_of_a_for_loop_statement_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        int i, j;

                        for (int index = 0; index < 10; index++)
                        {
                            [|i = j = 5;|]
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "'i' and 'j' are assigned in a single statement.");
        }

        [Fact]
        internal void When_two_variables_are_assigned_in_the_collection_of_a_foreach_loop_statement_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        int i, j, k;

                        [|foreach|] (var itr in new int[i = j = 5])
                        {
                            k = 3;
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "'i' and 'j' are assigned in a single statement.");
        }

        [Fact]
        internal void
            When_two_variables_are_assigned_in_separate_statements_in_the_body_of_a_foreach_loop_statement_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        int i, j;

                        foreach (var item in new int[0])
                        {
                            i = 5;
                            j = 8;
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void
            When_two_variables_are_assigned_in_a_single_statement_in_the_body_of_a_foreach_loop_statement_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        int i, j;

                        foreach (var item in new int[0])
                        {
                            [|i = j = 5;|]
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "'i' and 'j' are assigned in a single statement.");
        }

        [Fact]
        internal void When_two_variables_are_assigned_in_the_condition_of_a_while_loop_statement_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        int i, j, k;

                        [|while|] ((i = j = 5) > 0)
                        {
                            k = 3;
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "'i' and 'j' are assigned in a single statement.");
        }

        [Fact]
        internal void
            When_two_variables_are_assigned_in_separate_statements_in_the_body_of_a_while_loop_statement_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        int i, j;

                        while (true)
                        {
                            i = 5;
                            j = 8;
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void
            When_two_variables_are_assigned_in_a_single_statement_in_the_body_of_a_while_loop_statement_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        int i, j;

                        while (true)
                        {
                            [|i = j = 5;|]
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "'i' and 'j' are assigned in a single statement.");
        }

        [Fact]
        internal void When_two_variables_are_assigned_in_the_condition_of_a_do_while_loop_statement_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        int i, j, k;

                        do
                        {
                            k = 3;
                        }
                        [|while|] ((i = j = 5) > 0);
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "'i' and 'j' are assigned in a single statement.");
        }

        [Fact]
        internal void
            When_two_variables_are_assigned_in_separate_statements_in_the_body_of_a_do_while_loop_statement_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        int i, j;

                        do
                        {
                            i = 5;
                            j = 8;
                        }
                        while (true);
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void
            When_two_variables_are_assigned_in_a_single_statement_in_the_body_of_a_do_while_loop_statement_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        int i, j;

                        do
                        {
                            [|i = j = 5;|]
                        }
                        while (true);
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "'i' and 'j' are assigned in a single statement.");
        }

        [Fact]
        internal void When_two_variables_are_assigned_in_the_condition_of_an_if_statement_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        int i, j, k;

                        [|if|] ((i = j = 5) > 0)
                        {
                            k = 3;
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "'i' and 'j' are assigned in a single statement.");
        }

        [Fact]
        internal void When_two_variables_are_assigned_in_separate_statements_in_the_bodies_of_an_if_statement_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        int i, j, x, y;

                        if (true)
                        {
                            i = 5;
                            j = 8;
                        }
                        else
                        {
                            x = 5;
                            y = 8;
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_two_variables_are_assigned_in_a_single_statement_in_the_bodies_of_an_if_statement_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        int i, j, x, y;

                        if (true)
                        {
                            [|i = j = 5;|]
                        }
                        else
                        {
                            [|x = y = 5;|]
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "'i' and 'j' are assigned in a single statement.",
                "'x' and 'y' are assigned in a single statement.");
        }

        [Fact]
        internal void When_two_variables_are_assigned_in_the_value_of_a_switch_statement_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        int i, j, k;

                        [|switch|] (i = j = 5)
                        {
                            case 1:
                            {
                                k = 3;
                                break;
                            }
                            default:
                            {
                                k = 4;
                                break;
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "'i' and 'j' are assigned in a single statement.");
        }

        [Fact]
        internal void
            When_two_variables_are_assigned_in_separate_statements_in_the_clauses_of_a_switch_statement_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        int i, j;

                        switch (true)
                        {
                            case true:
                            {
                                i = 5;
                                j = 8;
                                break;
                            }
                            default:
                            {
                                i = 5;
                                j = 8;
                                break;
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void
            When_two_variables_are_assigned_in_a_single_statement_in_the_clauses_of_a_switch_statement_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        int i, j, x, y;

                        switch (true)
                        {
                            case true:
                            {
                                [|i = j = 5;|]
                                break;
                            }
                            default:
                            {
                                [|x = y = 5;|]
                                break;
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "'i' and 'j' are assigned in a single statement.",
                "'x' and 'y' are assigned in a single statement.");
        }

        [Fact]
        internal void When_two_variables_are_assigned_in_a_return_statement_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    int M()
                    {
                        int i, j;

                        [|return|] i = j = 5;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "'i' and 'j' are assigned in a single statement.");
        }

        [Fact]
        internal void When_two_variables_are_assigned_in_a_yield_return_statement_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .Using(typeof(IEnumerable<>).Namespace)
                .InDefaultClass(@"
                    IEnumerable<int> M()
                    {
                        int i, j;

                        [|yield return|] i = j = 5;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "'i' and 'j' are assigned in a single statement.");
        }

        [Fact]
        internal void When_two_variables_are_assigned_in_the_expression_of_a_lock_statement_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        object o, p;
                        int k;

                        [|lock|] (o = p = new object())
                        {
                            k = 3;
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "'o' and 'p' are assigned in a single statement.");
        }

        [Fact]
        internal void When_two_variables_are_assigned_in_separate_statements_in_the_body_of_a_lock_statement_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        int i, j;

                        lock (this)
                        {
                            i = 5;
                            j = 8;
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_two_variables_are_assigned_in_a_single_statement_in_the_body_of_a_lock_statement_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        int i, j;

                        lock (this)
                        {
                            [|i = j = 5;|]
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "'i' and 'j' are assigned in a single statement.");
        }

        [Fact]
        internal void When_two_variables_are_assigned_in_the_expression_of_a_using_statement_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .Using(typeof(IDisposable).Namespace)
                .InDefaultClass(@"
                    void M()
                    {
                        IDisposable i, j;
                        int k;

                        [|using|] (i = j = null)
                        {
                            k = 3;
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "'i' and 'j' are assigned in a single statement.");
        }

        [Fact]
        internal void
            When_variable_is_declared_and_two_variables_are_assigned_in_the_expression_of_a_using_statement_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .Using(typeof(IDisposable).Namespace)
                .InDefaultClass(@"
                    void M()
                    {
                        IDisposable j;
                        int k;

                        [|using|] (IDisposable i = j = null)
                        {
                            k = 3;
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "'i' and 'j' are assigned in a single statement.");
        }

        [Fact]
        internal void
            When_two_variables_are_declared_and_assigned_in_the_expression_of_a_using_statement_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .Using(typeof(MemoryStream).Namespace)
                .InDefaultClass(@"
                    void M()
                    {
                        int k;

                        [|using|] (MemoryStream ms1 = new MemoryStream(), ms2 = new MemoryStream())
                        {
                            k = 3;
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "'ms1' and 'ms2' are assigned in a single statement.");
        }

        [Fact]
        internal void When_two_variables_are_assigned_in_separate_statements_in_the_body_of_a_using_statement_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        int i, j;

                        using (null)
                        {
                            i = 5;
                            j = 8;
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_two_variables_are_assigned_in_a_single_statement_in_the_body_of_a_using_statement_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        int i, j;

                        using (null)
                        {
                            [|i = j = 5;|]
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "'i' and 'j' are assigned in a single statement.");
        }

        [Fact]
        internal void When_two_variables_are_assigned_in_a_throw_statement_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .Using(typeof(Exception).Namespace)
                .InDefaultClass(@"
                    int M()
                    {
                        Exception i, j;

                        [|throw|] i = j = new Exception();
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "'i' and 'j' are assigned in a single statement.");
        }

        [Fact]
        internal void When_two_variables_are_assigned_in_separate_statements_in_a_lambda_body_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .Using(typeof(Action).Namespace)
                .InDefaultClass(@"
                    void M()
                    {
                        int i, j;

                        Action action = () =>
                        {
                            i = 5;
                            j = 8;
                        };
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_two_variables_are_assigned_in_a_single_statement_in_a_lambda_body_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .Using(typeof(Action).Namespace)
                .InDefaultClass(@"
                    void M()
                    {
                        int i, j;

                        Action action = () =>
                        {
                            [|i = j = 5;|]
                        };
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "'i' and 'j' are assigned in a single statement.");
        }

        [Fact]
        internal void When_two_variables_are_assigned_in_a_single_statement_in_a_lambda_expression_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .Using(typeof(Action).Namespace)
                .InDefaultClass(@"
                    void M()
                    {
                        int i, j;

                        Action action = () => [|i = j = 5|];
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "'i' and 'j' are assigned in a single statement.");
        }

        [Fact]
        internal void When_a_property_is_declared_and_assigned_in_an_anonymous_type_creation_expression_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        var instance = new { Abc = string.Empty };
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_members_are_assigned_in_an_object_creation_expression_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        var instance = new X { F = 1, P = 2 };
                    }

                    class X
                    {
                        public int F;

                        public int P { get; set; }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_members_are_assigned_in_an_anonymous_object_creation_expression_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        var instance = new { F = 1, P = 2 };
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_members_are_assigned_in_a_type_parameter_object_creation_expression_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M<T>()
                        where T : I, new()
                    {
                        var instance = new T { P1 = 1, P2 = 2 };
                    }

                    interface I
                    {
                        int P1 { get; set; }
                        int P2 { get; set; }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_members_are_assigned_in_a_dynamic_object_creation_expression_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M(dynamic d)
                    {
                        var instance = new X(d) { P = 1 };
                    }

                    class X
                    {
                        public X(dynamic d) => throw null;

                        public int P { get; set; }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_two_variables_are_declared_and_assigned_in_a_declaration_expression_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        bool result = N(out var a, out var b);

                        int x, y;
                        bool z;

                        z = N(out x, out y);
                    }

                    bool N(out int i, out int j) => throw null;
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_two_variables_are_declared_and_assigned_in_a_tuple_declaration_expression_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        (int a, int b) = GetTuple();
                        (var c, var d) = GetTuple();
                        (int, int) t = GetTuple();

                        var (x, y) = GetTuple();
                        var (_, _) = GetTuple();
                    }

                    (int, int) GetTuple() => throw null;
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_two_variables_are_declared_and_assigned_in_an_is_pattern_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M(object a, object b)
                    {
                        if (a is string s1 && b is string s2)
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_two_variables_are_declared_and_assigned_in_a_pattern_matching_statement_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M(object o)
                    {
                        switch (o)
                        {
                            case string s when s.Length > 0:
                            {
                                throw null;
                            }
                            case int i when i > 0:
                            {
                                throw null;
                            }
                            case null:
                            {
                                return;
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new AssignEachVariableInASeparateStatementAnalyzer();
        }
    }
}
