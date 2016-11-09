using System.Collections.Generic;
using CSharpGuidelinesAnalyzer.Rules.Maintainability;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.Maintainability
{
    public class AssignVariablesInSeparateStatementsSpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => AssignVariablesInSeparateStatementsAnalyzer.DiagnosticId;

        [Fact]
        public void When_two_variables_are_declared_in_separate_statements_it_must_be_skipped()
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
        public void When_two_variables_are_declared_in_a_single_statement_it_must_be_skipped()
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
        public void When_two_variables_are_assigned_in_separate_statements_it_must_be_skipped()
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
        public void When_two_variables_are_declared_and_assigned_in_separate_statements_it_must_be_skipped()
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
        public void When_two_variables_are_assigned_in_a_single_statement_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        int i;
                        int j;
                        [|i = j = 5;|]
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "'i' and 'j' are assigned in a single statement.");
        }

        [Fact]
        public void When_two_variables_are_assigned_multiple_times_in_a_single_statement_it_must_be_reported()
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
        public void When_two_variables_are_declared_and_assigned_in_a_single_statement_it_must_be_reported()
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
        public void When_three_variables_are_assigned_in_a_single_statement_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        int i;
                        int j;
                        int k;
                        [|i = (true ? (j = 5) : (k = 7));|]
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "'i', 'j' and 'k' are assigned in a single statement.");
        }

        [Fact]
        public void When_multiple_identifiers_are_assigned_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
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
        public void When_multiple_identifiers_are_declared_and_assigned_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
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
        public void When_multiple_identifiers_are_assigned_in_a_for_loop_statement_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .InGlobalScope(@"
                    abstract class C
                    {
                        private int field1;

                        private int Property1 { get; set; }

                        public abstract int this[int index] { get; set; }

                        void M(ref int parameter1)
                        {
                            int local1;

                            [|for (local1 = 1, field1 = 2, Property1 = 3, this[0] = 4; true; parameter1 = 5)
                            {
                            }|]
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "'local1', 'C.field1', 'C.Property1', 'C.this[int]' and 'parameter1' are assigned in a single statement.");
        }

        [Fact]
        public void When_multiple_identifiers_are_assigned_in_the_body_of_a_for_loop_statement_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void M()
                        {
                            for (int index = 0; index < 10; index++)
                            {
                                int local1 = 5;
                                int local2 = 8;
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        public void When_multiple_identifiers_are_assigned_in_a_foreach_loop_statement_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .InGlobalScope(@"
                    abstract class C
                    {
                        private int field1;

                        private int Property1 { get; set; }

                        public abstract int this[int index] { get; set; }

                        void M(ref int parameter1)
                        {
                            int local1;

                            [|foreach (var itr in new int[local1 = field1 = Property1 = this[0] = parameter1 = 5])
                            {
                            }|]
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "'local1', 'C.field1', 'C.Property1', 'C.this[int]' and 'parameter1' are assigned in a single statement.");
        }

        [Fact]
        public void When_multiple_identifiers_are_assigned_in_the_body_of_a_foreach_loop_statement_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void M(int[] items)
                        {
                            foreach (var item in items)
                            {
                                int local1 = 5;
                                int local2 = 8;
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        public void When_multiple_identifiers_are_assigned_in_a_while_loop_statement_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .InGlobalScope(@"
                    abstract class C
                    {
                        private int field1;

                        private int Property1 { get; set; }

                        public abstract int this[int index] { get; set; }

                        void M(ref int parameter1)
                        {
                            int local1;

                            [|while ((local1 = field1 = Property1 = this[0] = parameter1 = 5) > 0)
                            {
                            }|]
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "'local1', 'C.field1', 'C.Property1', 'C.this[int]' and 'parameter1' are assigned in a single statement.");
        }

        [Fact]
        public void When_multiple_identifiers_are_assigned_in_the_body_of_a_while_loop_statement_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void M()
                        {
                            while (true)
                            {
                                int local1 = 5;
                                int local2 = 8;
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        public void When_multiple_identifiers_are_assigned_in_a_do_while_loop_statement_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .InGlobalScope(@"
                    abstract class C
                    {
                        private int field1;

                        private int Property1 { get; set; }

                        public abstract int this[int index] { get; set; }

                        void M(ref int parameter1)
                        {
                            int local1;

                            [|do
                            {
                            }
                            while ((local1 = field1 = Property1 = this[0] = parameter1 = 5) > 0);|]
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "'local1', 'C.field1', 'C.Property1', 'C.this[int]' and 'parameter1' are assigned in a single statement.");
        }

        [Fact]
        public void When_multiple_identifiers_are_assigned_in_the_body_of_a_do_while_loop_statement_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void M()
                        {
                            do
                            {
                                int local1 = 5;
                                int local2 = 8;
                            }
                            while (true);
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        public void When_multiple_identifiers_are_assigned_in_an_if_statement_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .InGlobalScope(@"
                    abstract class C
                    {
                        private int field1;

                        private int Property1 { get; set; }

                        public abstract int this[int index] { get; set; }

                        void M(ref int parameter1)
                        {
                            int local1;

                            [|if ((local1 = field1 = Property1 = this[0] = parameter1 = 5) > 0)
                            {
                            }|]
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "'local1', 'C.field1', 'C.Property1', 'C.this[int]' and 'parameter1' are assigned in a single statement.");
        }

        [Fact]
        public void When_multiple_identifiers_are_assigned_in_the_body_of_an_if_statement_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        private int Property1 { get; set; }

                        void M()
                        {
                            if (Property1 > 0)
                            {
                                int local1 = 5;
                                int local2 = 8;
                            }
                            else
                            {
                                int local3 = 5;
                                int local4 = 8;
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        public void When_multiple_identifiers_are_assigned_in_a_switch_statement_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .InGlobalScope(@"
                    abstract class C
                    {
                        private int field1;

                        private int Property1 { get; set; }

                        public abstract int this[int index] { get; set; }

                        void M(ref int parameter1)
                        {
                            int local1;

                            [|switch (local1 = field1 = Property1 = this[0] = parameter1 = 4)
                            {
                            }|]
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "'local1', 'C.field1', 'C.Property1', 'C.this[int]' and 'parameter1' are assigned in a single statement.");
        }

        [Fact]
        public void When_multiple_identifiers_are_assigned_in_the_case_clause_of_a_switch_statement_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void M(int value)
                        {
                            switch (value)
                            {
                                default:
                                    int local1 = 5;
                                    int local2 = 8;
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
        public void When_multiple_identifiers_are_assigned_in_a_return_statement_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .InGlobalScope(@"
                    abstract class C
                    {
                        private int field1;

                        private int Property1 { get; set; }

                        public abstract int this[int index] { get; set; }

                        int M(ref int parameter1)
                        {
                            int local1;

                            [|return (local1 = field1 = Property1 = this[0] = parameter1 = 4);|]
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "'local1', 'C.field1', 'C.Property1', 'C.this[int]' and 'parameter1' are assigned in a single statement.");
        }

        [Fact]
        public void When_multiple_identifiers_are_assigned_in_a_yield_return_statement_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .Using(typeof (IEnumerable<>).Namespace)
                .InGlobalScope(@"
                    abstract class C
                    {
                        private int field1;

                        private int Property1 { get; set; }

                        public abstract int this[int index] { get; set; }

                        IEnumerable<int> M()
                        {
                            int local1;

                            [|yield return (local1 = field1 = Property1 = this[0] = 4);|]
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "'local1', 'C.field1', 'C.Property1' and 'C.this[int]' are assigned in a single statement.");
        }

        [Fact]
        public void When_multiple_identifiers_are_assigned_in_a_lock_statement_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .InGlobalScope(@"
                    abstract class C
                    {
                        private object field1;

                        private object Property1 { get; set; }

                        public abstract object this[int index] { get; set; }

                        void M(ref object parameter1)
                        {
                            object local1;

                            [|lock (local1 = field1 = Property1 = this[0] = parameter1 = new object())
                            {
                            }|]
                        }
                    }
                    ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "'local1', 'C.field1', 'C.Property1', 'C.this[int]' and 'parameter1' are assigned in a single statement.");
        }

        [Fact]
        public void When_multiple_identifiers_are_assigned_in_the_body_of_a_lock_statement_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        private object guard = new object();

                        void M()
                        {
                            lock (guard)
                            {
                                int local1 = 5;
                                int local2 = 8;
                            }
                        }
                    }
                    ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        public void When_multiple_identifiers_are_assigned_in_a_using_statement_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .InGlobalScope(@"
                    abstract class C
                    {
                        private IDisposable field1;

                        private IDisposable Property1 { get; set; }

                        public abstract IDisposable this[int index] { get; set; }

                        void M(ref IDisposable parameter1)
                        {
                            IDisposable local1;

                            [|using (local1 = field1 = Property1 = this[0] = parameter1 = null)
                            {
                            }|]
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "'local1', 'C.field1', 'C.Property1', 'C.this[int]' and 'parameter1' are assigned in a single statement.");
        }

        [Fact]
        public void When_multiple_identifiers_are_assigned_in_the_body_of_a_using_statement_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        private IDisposable field1;

                        void M()
                        {
                            using (var temp = field1)
                            {
                                int local1 = 5;
                                int local2 = 8;
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        public void When_multiple_identifiers_are_assigned_in_a_throw_statement_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .InGlobalScope(@"
                    abstract class C
                    {
                        private Exception field1;

                        private Exception Property1 { get; set; }

                        public abstract Exception this[int index] { get; set; }

                        void M(ref Exception parameter1)
                        {
                            Exception local1;

                            [|throw local1 = field1 = Property1 = this[0] = parameter1 = new Exception();|]
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "'local1', 'C.field1', 'C.Property1', 'C.this[int]' and 'parameter1' are assigned in a single statement.");
        }

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new AssignVariablesInSeparateStatementsAnalyzer();
        }
    }
}