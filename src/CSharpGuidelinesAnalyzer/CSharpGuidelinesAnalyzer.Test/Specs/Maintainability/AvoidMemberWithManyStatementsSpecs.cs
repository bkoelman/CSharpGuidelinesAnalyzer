using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CSharpGuidelinesAnalyzer.Rules.Maintainability;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using FluentAssertions;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.Maintainability
{
    public sealed class AvoidMemberWithManyStatementsSpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => AvoidMemberWithManyStatementsAnalyzer.DiagnosticId;

        #region Different places where code blocks may occur

        [Fact]
        internal async Task When_method_body_contains_eight_statements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        int [|M|](string s)
                        {
                            ; ; ; ;
                            ; ; ;
                            throw null;
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Method 'C.M(string)' contains 8 statements, which exceeds the maximum of 7 statements");
        }

        [Fact]
        internal async Task When_method_expression_body_contains_eight_statements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        Func<int> [|M|](string s) =>
                        () =>
                        {
                            ; ; ; ;
                            ; ; ;
                            throw null;
                        };
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Method 'C.M(string)' contains 8 statements, which exceeds the maximum of 7 statements");
        }

        [Fact]
        internal async Task When_operator_body_contains_eight_statements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        public static C operator[|++|](C c)
                        {
                            ; ; ; ;
                            ; ; ;
                            throw null;
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Method 'C.operator ++(C)' contains 8 statements, which exceeds the maximum of 7 statements");
        }

        [Fact]
        internal async Task When_implicit_conversion_operator_body_contains_eight_statements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        public static implicit operator [|C|](string s)
                        {
                            ; ; ; ;
                            ; ; ;
                            throw null;
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Method 'C.implicit operator C(string)' contains 8 statements, which exceeds the maximum of 7 statements");
        }

        [Fact]
        internal async Task When_explicit_conversion_operator_body_contains_eight_statements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        public static explicit operator [|C|](string s)
                        {
                            ; ; ; ;
                            ; ; ;
                            throw null;
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Method 'C.explicit operator C(string)' contains 8 statements, which exceeds the maximum of 7 statements");
        }

        [Fact]
        internal async Task When_explicit_conversion_operator_expression_body_contains_eight_statements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        public static explicit operator [|C|](int i) => M(() =>
                        {
                            ; ; ; ;
                            ; ; ;
                            throw null;
                        });

                        static C M(Action a) => throw null;
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Method 'C.explicit operator C(int)' contains 8 statements, which exceeds the maximum of 7 statements");
        }

        [Fact]
        internal async Task When_constructor_body_contains_eight_statements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        public [|C|](string s)
                        {
                            ; ; ; ;
                            ; ; ;
                            throw null;
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Method 'C.C(string)' contains 8 statements, which exceeds the maximum of 7 statements");
        }

        [Fact]
        internal async Task When_constructor_expression_body_contains_eight_statements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .Using(typeof(Task).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        public [|C|](Task t) =>
                            t.ContinueWith(x =>
                            {
                                ; ; ; ;
                                ; ; ;
                                throw null;
                            });
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Method 'C.C(Task)' contains 8 statements, which exceeds the maximum of 7 statements");
        }

        [Fact]
        internal async Task When_constructor_initializer_contains_eight_statements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class B
                    {
                        protected B(Func<int> f) => throw null;
                    }

                    class C : B
                    {
                        [|C|](string s)
                            : base(() =>
                            {
                                ; ; ; ;
                                throw null;
                            })
                        {
                                ; ; ;
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Method 'C.C(string)' contains 8 statements, which exceeds the maximum of 7 statements");
        }

        [Fact]
        internal async Task When_static_constructor_body_contains_eight_statements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        static [|C|]()
                        {
                            ; ; ; ;
                            ; ; ;
                            throw null;
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Method 'C.C()' contains 8 statements, which exceeds the maximum of 7 statements");
        }

        [Fact]
        internal async Task When_static_constructor_expression_body_contains_eight_statements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .Using(typeof(Task).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        static Task t;

                        static [|C|]() =>
                            t.ContinueWith(x =>
                            {
                                ; ; ; ;
                                ; ; ;
                                throw null;
                            });
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Method 'C.C()' contains 8 statements, which exceeds the maximum of 7 statements");
        }

        [Fact]
        internal async Task When_destructor_body_contains_eight_statements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        ~[|C|]()
                        {
                            ; ; ; ;
                            ; ; ;
                            throw null;
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Method 'C.~C()' contains 8 statements, which exceeds the maximum of 7 statements");
        }

        [Fact]
        internal async Task When_destructor_expression_body_contains_eight_statements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .Using(typeof(Task).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        static Task t;

                        ~[|C|]() =>
                            t.ContinueWith(x =>
                            {
                                ; ; ; ;
                                ; ; ;
                                throw null;
                            });
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Method 'C.~C()' contains 8 statements, which exceeds the maximum of 7 statements");
        }

        [Fact]
        internal async Task When_property_initializer_contains_eight_statements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        Func<int> [|P|] { get; } = () =>
                        {
                            ; ; ; ;
                            ; ; ;
                            throw null;
                        };
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Initializer for 'C.P' contains 8 statements, which exceeds the maximum of 7 statements");
        }

        [Fact]
        internal async Task When_property_expression_body_contains_eight_statements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        Func<int> [|P|] =>
                            () =>
                            {
                                ; ; ; ;
                                ; ; ;
                                throw null;
                            };
                        }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Property accessor 'C.P.get' contains 8 statements, which exceeds the maximum of 7 statements");
        }

        [Fact]
        internal async Task When_property_getter_body_contains_eight_statements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        string P
                        {
                            [|get|]
                            {
                                ; ; ; ;
                                ; ; ;
                                throw null;
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Property accessor 'C.P.get' contains 8 statements, which exceeds the maximum of 7 statements");
        }

        [Fact]
        internal async Task When_property_getter_expression_body_contains_eight_statements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        Func<int> P
                        {
                            [|get|] =>
                                () =>
                                {
                                    ; ; ; ;
                                    ; ; ;
                                    throw null;
                                };
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Property accessor 'C.P.get' contains 8 statements, which exceeds the maximum of 7 statements");
        }

        [Fact]
        internal async Task When_property_setter_body_contains_eight_statements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        string P
                        {
                            [|set|]
                            {
                                ; ; ; ;
                                ; ; ;
                                throw null;
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Property accessor 'C.P.set' contains 8 statements, which exceeds the maximum of 7 statements");
        }

        [Fact]
        internal async Task When_property_setter_expression_body_contains_eight_statements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        Func<int> P
                        {
                            [|set|] =>
                                value = () =>
                                {
                                    ; ; ; ;
                                    ; ; ;
                                    throw null;
                                };
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Property accessor 'C.P.set' contains 8 statements, which exceeds the maximum of 7 statements");
        }

        [Fact]
        internal async Task When_indexer_expression_body_contains_eight_statements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        Func<int> [|this|][int index] =>
                            () =>
                            {
                                ; ; ; ;
                                ; ; ;
                                throw null;
                            };
                        }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Property accessor 'C.this[int].get' contains 8 statements, which exceeds the maximum of 7 statements");
        }

        [Fact]
        internal async Task When_indexer_getter_body_contains_eight_statements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        string this[int index]
                        {
                            [|get|]
                            {
                                ; ; ; ;
                                ; ; ;
                                throw null;
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Property accessor 'C.this[int].get' contains 8 statements, which exceeds the maximum of 7 statements");
        }

        [Fact]
        internal async Task When_indexer_setter_body_contains_eight_statements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        string this[int index]
                        {
                            [|set|]
                            {
                                ; ; ; ;
                                ; ; ;
                                throw null;
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Property accessor 'C.this[int].set' contains 8 statements, which exceeds the maximum of 7 statements");
        }

        [Fact]
        internal async Task When_event_initializer_contains_eight_statements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        event EventHandler [|E|] = M(() =>
                        {
                            ; ; ; ;
                            ; ; ;
                            throw null;
                        });

                        static EventHandler M(Action a) => throw null;
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Initializer for 'C.E' contains 8 statements, which exceeds the maximum of 7 statements");
        }

        [Fact]
        internal async Task When_event_adder_body_contains_eight_statements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        event EventHandler E
                        {
                            [|add|]
                            {
                                ; ; ; ;
                                ; ; ;
                                throw null;
                            }
                            remove
                            {
                                throw null;
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Event accessor 'C.E.add' contains 8 statements, which exceeds the maximum of 7 statements");
        }

        [Fact]
        internal async Task When_event_remover_body_contains_eight_statements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        event EventHandler E
                        {
                            add
                            {
                                throw null;
                            }
                            [|remove|]
                            {
                                ; ; ; ;
                                ; ; ;
                                throw null;
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Event accessor 'C.E.remove' contains 8 statements, which exceeds the maximum of 7 statements");
        }

        [Fact]
        internal async Task When_field_initializer_contains_eight_statements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        Action [|f|] = () =>
                        {
                            ; ; ; ;
                            ; ; ;
                            throw null;
                        };
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Initializer for 'C.f' contains 8 statements, which exceeds the maximum of 7 statements");
        }

        #endregion

        #region Different kinds of statements

        [Fact]
        internal async Task When_method_contains_eight_empty_statements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void [|M|]()
                        {
                            ; ;
                            ; ;
                            ; ;
                            ; ;
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Method 'C.M()' contains 8 statements, which exceeds the maximum of 7 statements");
        }

        [Fact]
        internal async Task When_method_contains_seven_empty_statements_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void M()
                        {
                            ; ;
                            ; ;
                            ; ;

                            // Block scopes are not counted
                            {
                                {
                                    ;
                                }
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source);
        }

        [Fact]
        internal async Task When_method_contains_eight_declaration_statements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void [|M|]()
                        {
                            int statement1 = 1;
                            int statement2 = 2;
                            int statement3 = 3;
                            int statement4 = 4;

                            int statement5 = 5;
                            int statement6 = 6;
                            int statement7 = 7;
                            int statement8 = 8;
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Method 'C.M()' contains 8 statements, which exceeds the maximum of 7 statements");
        }

        [Fact]
        internal async Task When_method_contains_seven_declaration_statements_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void M()
                        {
                            int statement1 = 1;
                            int statement2 = 2;
                            int statement3 = 3;
                            int statement4 = 4;

                            int statement5 = 5;
                            int statement6 = 6;
                            int statement7 = 7, other = 8;
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source);
        }

        [Fact]
        internal async Task When_method_contains_eight_expression_statements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .Using(typeof(IEnumerable).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        private int i;

                        void [|M|]()
                        {
                            i = 1;
                            i++;
                            i += 3;
                            i--;

                            i += (true ? 5 : 0);
                            i *= 6;
                            i--;
                            i += 8;
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Method 'C.M()' contains 8 statements, which exceeds the maximum of 7 statements");
        }

        [Fact]
        internal async Task When_method_contains_seven_expression_statements_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .Using(typeof(IEnumerable).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        private int i;

                        void M()
                        {
                            i = 1;
                            i++;
                            i += 3;
                            i--;

                            i += (true ? 5 : 0);
                            i *= 6;
                            i--;
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source);
        }

        [Fact]
        internal async Task When_method_contains_eight_for_loop_statements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void [|M|]()
                        {
                            for (int a = 0; a < 1000; a++)
                                for (int b = 0; b < 1000; b++)
                                    for (int c = 0; c < 1000; c++)
                                        for (int d = 0; d < 1000; d++)
                                        {
                                            for (int e = 0; e < 1000; e++)
                                                for (int f = 0; f < 1000; f++)
                                                    for (int g = 0; g < 1000; g++)
                                                        for (int h = 0; h < 1000; h++)
                                                        {
                                                        }
                                        }
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Method 'C.M()' contains 8 statements, which exceeds the maximum of 7 statements");
        }

        [Fact]
        internal async Task When_method_contains_seven_for_loop_statements_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void M()
                        {
                            for (int a = 0; a < 1000; a++)
                                for (int b = 0; b < 1000; b++)
                                    for (int c = 0; c < 1000; c++)
                                        for (int d = 0; d < 1000; d++)
                                        {
                                            for (int e = 0; e < 1000; e++)
                                                for (int f = 0; f < 1000; f++)
                                                    for (int g = 0; g < 1000; g++)
                                                    {
                                                    }
                                        }
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source);
        }

        [Fact]
        internal async Task When_method_contains_eight_foreach_loop_statements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .Using(typeof(List<>).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        void [|M|](List<(int, string)> source)
                        {
                            foreach (var level1 in new string[0])
                                foreach (var level2 in new string[0])
                                    foreach (var level3 in new string[0])
                                        foreach (var level4 in new string[0])
                                        {
                                            foreach (var level5 in new string[0])
                                                foreach (var level6 in new string[0])
                                                    foreach (var level7 in new string[0])
                                                        foreach (var (x, y) in source)
                                                        {
                                                        }
                                        }
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Method 'C.M(List<(int, string)>)' contains 8 statements, which exceeds the maximum of 7 statements");
        }

        [Fact]
        internal async Task When_method_contains_seven_foreach_loop_statements_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .Using(typeof(List<>).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        void M(List<(int, string)> source)
                        {
                            foreach (var level1 in new string[0])
                                foreach (var level2 in new string[0])
                                    foreach (var level3 in new string[0])
                                        foreach (var level4 in new string[0])
                                        {
                                            foreach (var level5 in new string[0])
                                                foreach (var level6 in new string[0])
                                                    foreach (var (x, y) in source)
                                                    {
                                                    }
                                        }
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source);
        }

        [Fact]
        internal async Task When_method_contains_eight_while_loop_statements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void [|M|]()
                        {
                            while (true)
                                while (true)
                                    while (true)
                                        while (true)

                                            while (true)
                                                while (true)
                                                    while (true)
                                                        while (true)
                                                        {
                                                        }
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Method 'C.M()' contains 8 statements, which exceeds the maximum of 7 statements");
        }

        [Fact]
        internal async Task When_method_contains_seven_while_loop_statements_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void M()
                        {
                            while (true)
                                while (true)
                                    while (true)
                                        while (true)

                                            while (true)
                                                while (true)
                                                    while (true)
                                                    {
                                                    }
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source);
        }

        [Fact]
        internal async Task When_method_contains_eight_do_while_loop_statements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void [|M|]()
                        {
                            do
                            {
                                do
                                {
                                    do
                                    {
                                        do
                                        {
                                        }
                                        while (true);
                                    }
                                    while (true);
                                }
                                while (true);
                            }
                            while (true);

                            do
                            {
                                do
                                {
                                    do
                                    {
                                        do
                                        {
                                        }
                                        while (true);
                                    }
                                    while (true);
                                }
                                while (true);
                            }
                            while (true);
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Method 'C.M()' contains 8 statements, which exceeds the maximum of 7 statements");
        }

        [Fact]
        internal async Task When_method_contains_seven_do_while_loop_statements_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void M()
                        {
                            do
                            {
                                do
                                {
                                    do
                                    {
                                        do
                                        {
                                        }
                                        while (true);
                                    }
                                    while (true);
                                }
                                while (true);
                            }
                            while (true);

                            do
                            {
                                do
                                {
                                    do
                                    {
                                    }
                                    while (true);
                                }
                                while (true);
                            }
                            while (true);
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source);
        }

        [Fact]
        internal async Task When_method_contains_eight_if_statements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void [|M|]()
                        {
                            if (true)
                            {
                                if (true)
                                {
                                }
                                else
                                {
                                    if (true)
                                    {
                                        if (true)
                                        {
                                        }
                                        else
                                        {
                                            if (true)
                                            {
                                            }
                                            else
                                            {
                                            }
                                        }
                                    }
                                    else
                                    {
                                    }
                                }
                            }
                            else
                            {
                            }

                            if (true)
                            {
                                if (true)
                                {
                                }
                                else
                                {
                                }
                            }
                            else
                            {
                            }

                            if (true)
                            {
                            }
                            else
                            {
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Method 'C.M()' contains 8 statements, which exceeds the maximum of 7 statements");
        }

        [Fact]
        internal async Task When_method_contains_seven_if_statements_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void M()
                        {
                            if (true)
                            {
                                if (true)
                                {
                                }
                                else
                                {
                                    if (true)
                                    {
                                        if (true)
                                        {
                                        }
                                        else
                                        {
                                            if (true ? false : true)
                                            {
                                            }
                                            else
                                            {
                                            }
                                        }
                                    }
                                    else
                                    {
                                    }
                                }
                            }
                            else
                            {
                            }

                            if (true)
                            {
                                if (true)
                                {
                                }
                                else
                                {
                                }
                            }
                            else
                            {
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source);
        }

        [Fact]
        internal async Task When_method_contains_eight_local_function_declarations_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void M()
                        {
                            void L1()
                            {
                            }

                            void L2()
                            {
                            }

                            void L3()
                            {
                            }

                            void L4()
                            {
                            }

                            void L5()
                            {
                            }

                            void L6()
                            {
                            }

                            void L7()
                            {
                            }

                            void L8()
                            {
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source);
        }

        [Fact]
        internal async Task When_method_contains_eight_local_function_invocations_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void [|M|]()
                        {
                            void L()
                            {
                            }

                            L();
                            L();
                            L();
                            L();

                            L();
                            L();
                            L();
                            L();
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Method 'C.M()' contains 8 statements, which exceeds the maximum of 7 statements");
        }

        [Fact]
        internal async Task When_method_contains_seven_local_function_invocations_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void M()
                        {
                            void L()
                            {
                            }

                            L();
                            L();
                            L();
                            L();

                            L();
                            L();
                            L();
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source);
        }

        [Fact]
        internal async Task When_method_contains_eight_switch_statements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void [|M|](int i)
                        {
                            switch (i)
                            {
                            }
                            switch (i)
                            {
                            }
                            switch (i)
                            {
                            }
                            switch (i)
                            {
                            }

                            switch (i)
                            {
                            }
                            switch (i)
                            {
                            }
                            switch (i)
                            {
                            }
                            switch (i)
                            {
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Method 'C.M(int)' contains 8 statements, which exceeds the maximum of 7 statements");
        }

        [Fact]
        internal async Task When_method_contains_seven_switch_statements_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void M(int i)
                        {
                            switch (i)
                            {
                            }
                            switch (i)
                            {
                            }
                            switch (i)
                            {
                            }
                            switch (i)
                            {
                            }

                            switch (i)
                            {
                            }
                            switch (i)
                            {
                            }
                            switch (i)
                            {
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source);
        }

        [Fact]
        internal async Task When_method_contains_eight_statements_in_switch_case_blocks_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void [|M|](int i)
                        {
                            switch (i)
                            {
                                case 1:
                                {
                                    ; ; ;
                                    goto default;
                                }
                                default:
                                    ; ;
                                    break;
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Method 'C.M(int)' contains 8 statements, which exceeds the maximum of 7 statements");
        }

        [Fact]
        internal async Task When_method_contains_seven_statements_in_switch_case_blocks_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void M(int i)
                        {
                            switch (i)
                            {
                                case 1:
                                {
                                    ; ;
                                    goto default;
                                }
                                default:
                                    ; ;
                                    break;
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source);
        }

        [Fact]
        internal async Task When_method_contains_eight_throw_statements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void [|M|](string s)
                        {
                            throw new NotImplementedException();
                            throw new NotImplementedException();
                            throw new NotImplementedException();
                            throw new NotImplementedException();

                            throw new NotImplementedException();
                            throw new NotImplementedException();
                            throw new NotImplementedException();
                            throw new NotImplementedException();
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Method 'C.M(string)' contains 8 statements, which exceeds the maximum of 7 statements");
        }

        [Fact]
        internal async Task When_method_contains_seven_throw_statements_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void M(string s)
                        {
                            throw new NotImplementedException();
                            throw new NotImplementedException();
                            throw new NotImplementedException();
                            throw new NotImplementedException();

                            throw new NotImplementedException();
                            throw new NotImplementedException();
                            throw new NotImplementedException();
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source);
        }

        [Fact]
        internal async Task When_method_contains_eight_try_statements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void [|M|]()
                        {
                            try
                            {
                                try
                                {
                                }
                                finally
                                {
                                }
                            }
                            catch (Exception)
                            {
                                try
                                {
                                    try
                                    {
                                    }
                                    finally
                                    {
                                    }
                                }
                                catch (Exception)
                                {
                                }
                            }

                            try
                            {
                                try
                                {
                                }
                                finally
                                {
                                }
                            }
                            catch (Exception)
                            {
                                try
                                {
                                    try
                                    {
                                    }
                                    finally
                                    {
                                    }
                                }
                                catch (Exception)
                                {
                                }
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Method 'C.M()' contains 8 statements, which exceeds the maximum of 7 statements");
        }

        [Fact]
        internal async Task When_method_contains_seven_try_statements_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void M()
                        {
                            try
                            {
                                try
                                {
                                }
                                finally
                                {
                                }
                            }
                            catch (Exception)
                            {
                                try
                                {
                                    try
                                    {
                                    }
                                    finally
                                    {
                                    }
                                }
                                catch (Exception)
                                {
                                }
                            }

                            try
                            {
                                try
                                {
                                }
                                finally
                                {
                                }
                            }
                            catch (Exception)
                            {
                                try
                                {
                                }
                                catch (Exception)
                                {
                                }
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source);
        }

        [Fact]
        internal async Task When_method_contains_eight_using_statements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        private IDisposable d;

                        void [|M|]()
                        {
                            using (d)
                                using (d)
                                    using (d)
                                        using (d)
                                        {
                                        }

                            using (d)
                                using (d)
                                    using (d)
                                        using (IDisposable x = null, d = null)
                                        {
                                        }
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Method 'C.M()' contains 8 statements, which exceeds the maximum of 7 statements");
        }

        [Fact]
        internal async Task When_method_contains_seven_using_statements_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        private IDisposable d;

                        void M()
                        {
                            using (d)
                                using (d)
                                    using (d)
                                        using (d)
                                        {
                                        }

                            using (d)
                                using (d)
                                    using (IDisposable x = null, d = null)
                                    {
                                    }
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source);
        }

        [Fact]
        internal async Task When_method_contains_eight_return_statements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .Using(typeof(IEnumerable).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        public int [|M|]()
                        {
                            return 1;
                            return 2;
                            return 3;
                            return 4;

                            return 5;
                            return 6;
                            return 7;
                            return 8;
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Method 'C.M()' contains 8 statements, which exceeds the maximum of 7 statements");
        }

        [Fact]
        internal async Task When_method_contains_seven_return_statements_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .Using(typeof(IEnumerable).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        public int P
                        {
                            get
                            {
                                return 1;
                                return 2;
                                return 3;
                                return 4;

                                return 5;
                                return 6;
                                return 7;
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source);
        }

        [Fact]
        internal async Task When_method_contains_eight_yield_statements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .Using(typeof(IEnumerable).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        IEnumerable [|M|]()
                        {
                            yield break;
                            yield return new object();
                            yield break;
                            yield return new object();

                            yield break;
                            yield return new object();
                            yield break;
                            yield return new object();
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Method 'C.M()' contains 8 statements, which exceeds the maximum of 7 statements");
        }

        [Fact]
        internal async Task When_method_contains_seven_yield_statements_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .Using(typeof(IEnumerable).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        IEnumerable M()
                        {
                            yield break;
                            yield return new object();
                            yield break;
                            yield return new object();

                            yield break;
                            yield return new object();
                            yield break;
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source);
        }

        [Fact]
        internal async Task When_method_contains_eight_lock_statements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        private static readonly object guard = new object();

                        void [|M|]()
                        {
                            lock (guard)
                                lock (guard)
                                    lock (guard)
                                        lock (guard)
                                        {

                                            lock (guard)
                                            {
                                                lock (guard)
                                                {
                                                    lock (guard)
                                                    {
                                                        lock (guard)
                                                        {
                                                        }
                                                    }
                                                }
                                            }
                                        }
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Method 'C.M()' contains 8 statements, which exceeds the maximum of 7 statements");
        }

        [Fact]
        internal async Task When_method_contains_seven_lock_statements_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        private static readonly object guard = new object();

                        void M()
                        {
                            lock (guard)
                                lock (guard)
                                    lock (guard)
                                        lock (guard)
                                        {

                                            lock (guard)
                                            {
                                                lock (guard)
                                                {
                                                    lock (guard)
                                                    {
                                                    }
                                                }
                                            }
                                        }
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source);
        }

        [Fact]
        internal async Task When_method_contains_eight_fixed_statements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .Using(typeof(IEnumerable).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        unsafe void [|M|]()
                        {
                            fixed (char* p1 = Environment.MachineName, p2 = Environment.MachineName)
                                fixed (char* p3 = Environment.MachineName, p4 = Environment.MachineName)
                                    fixed (char* p5 = Environment.MachineName, p6 = Environment.MachineName)
                                        fixed (char* p7 = Environment.MachineName, p8 = Environment.MachineName)
                                        {
                                        }

                            fixed (char* p9 = Environment.MachineName, p10 = Environment.MachineName)
                            {
                                fixed (char* p11 = Environment.MachineName, p12 = Environment.MachineName)
                                {
                                    fixed (char* p13 = Environment.MachineName, p14 = Environment.MachineName)
                                    {
                                        fixed (char* p15 = Environment.MachineName, p16 = Environment.MachineName)
                                        {
                                        }
                                    }
                                }
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Method 'C.M()' contains 8 statements, which exceeds the maximum of 7 statements");
        }

        [Fact]
        internal async Task When_method_contains_seven_fixed_statements_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .Using(typeof(IEnumerable).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        unsafe void M()
                        {
                            fixed (char* p1 = Environment.MachineName, p2 = Environment.MachineName)
                                fixed (char* p3 = Environment.MachineName, p4 = Environment.MachineName)
                                    fixed (char* p5 = Environment.MachineName, p6 = Environment.MachineName)
                                        fixed (char* p7 = Environment.MachineName, p8 = Environment.MachineName)
                                        {
                                        }

                            fixed (char* p9 = Environment.MachineName, p10 = Environment.MachineName)
                            {
                                fixed (char* p11 = Environment.MachineName, p12 = Environment.MachineName)
                                {
                                    fixed (char* p13 = Environment.MachineName, p14 = Environment.MachineName)
                                    {
                                    }
                                }
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source);
        }

        [Fact]
        internal async Task When_method_contains_eight_unsafe_statements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .Using(typeof(IEnumerable).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        void [|M|]()
                        {
                            unsafe
                            {
                            }
                            unsafe
                            {
                            }
                            unsafe
                            {
                            }
                            unsafe
                            {
                            }
                            unsafe
                            {
                            }
                            unsafe
                            {
                            }
                            unsafe
                            {
                            }
                            unsafe
                            {
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Method 'C.M()' contains 8 statements, which exceeds the maximum of 7 statements");
        }

        [Fact]
        internal async Task When_method_contains_seven_unsafe_statements_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .Using(typeof(IEnumerable).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        void M()
                        {
                            unsafe
                            {
                            }
                            unsafe
                            {
                            }
                            unsafe
                            {
                            }
                            unsafe
                            {
                            }
                            unsafe
                            {
                            }
                            unsafe
                            {
                            }
                            unsafe
                            {
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source);
        }

        [Fact]
        internal async Task When_method_contains_eight_checked_statements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .Using(typeof(IEnumerable).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        void [|M|]()
                        {
                            checked
                            {
                                unchecked
                                {
                                }
                            }

                            unchecked
                            {
                                checked
                                {
                                }
                            }

                            checked
                            {
                            }

                            unchecked
                            {
                            }

                            checked
                            {
                            }

                            unchecked
                            {
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Method 'C.M()' contains 8 statements, which exceeds the maximum of 7 statements");
        }

        [Fact]
        internal async Task When_method_contains_seven_checked_statements_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .Using(typeof(IEnumerable).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        void M()
                        {
                            checked
                            {
                                unchecked
                                {
                                }
                            }

                            unchecked
                            {
                                checked
                                {
                                }
                            }

                            checked
                            {
                            }

                            unchecked
                            {
                            }

                            checked
                            {
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source);
        }

        [Fact]
        internal async Task When_method_contains_labels_and_eight_goto_statements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void [|M|]()
                        {
                            Label1:
                                goto Label2;
                            Label2:
                                goto Label3;
                            Label3:
                                goto Label4;
                            Label4:
                                goto Label5;

                            Label5:
                                goto Label6;
                            Label6:
                                goto Label7;
                            Label7:
                                goto Label8;
                            Label8:
                                goto Label1;
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Method 'C.M()' contains 8 statements, which exceeds the maximum of 7 statements");
        }

        [Fact]
        internal async Task When_method_contains_labels_and_seven_goto_statements_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void M()
                        {
                            Label1:
                                goto Label2;
                            Label2:
                                goto Label3;
                            Label3:
                                goto Label4;
                            Label4:
                                goto Label5;

                            Label5:
                                goto Label6;
                            Label6:
                                goto Label7;
                            Label7:
                                goto Label1;
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source);
        }

        [Fact]
        internal async Task When_method_contains_eight_continue_statements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void [|M|]()
                        {
                            while (true)
                            {
                                continue;
                                continue;
                                continue;
                                continue;

                                continue;
                                continue;
                                continue;
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Method 'C.M()' contains 8 statements, which exceeds the maximum of 7 statements");
        }

        [Fact]
        internal async Task When_method_contains_seven_continue_statements_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void M()
                        {
                            while (true)
                            {
                                continue;
                                continue;
                                continue;
                                continue;

                                continue;
                                continue;
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source);
        }

        [Fact]
        internal async Task When_method_contains_eight_break_statements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void [|M|]()
                        {
                            while (true)
                            {
                                break;
                                break;
                                break;
                                break;

                                break;
                                break;
                                break;
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Method 'C.M()' contains 8 statements, which exceeds the maximum of 7 statements");
        }

        [Fact]
        internal async Task When_method_contains_seven_break_statements_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void M()
                        {
                            while (true)
                            {
                                break;
                                break;
                                break;
                                break;

                                break;
                                break;
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source);
        }

        #endregion

        #region Reading through nested constructs

        [Fact]
        internal async Task When_method_contains_eight_statements_with_local_function_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void [|M|]()
                        {
                            void L()
                            {
                                ; ;
                                ; ;
                            }

                            ; ;
                            ; ;
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Method 'C.M()' contains 8 statements, which exceeds the maximum of 7 statements");
        }

        [Fact]
        internal async Task When_method_contains_seven_statements_with_local_function_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void M()
                        {
                            void L()
                            {
                                ; ;
                                ; ;
                            }

                            ; ;
                            ;
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source);
        }

        [Fact]
        internal async Task When_method_contains_eight_statements_with_lambda_block_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void [|M|]()
                        {
                            ; ;
                            ; ;

                            Action<int> action = i =>
                            {
                                ; ;
                                ;
                            };
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Method 'C.M()' contains 8 statements, which exceeds the maximum of 7 statements");
        }

        [Fact]
        internal async Task When_method_contains_seven_statements_with_lambda_block_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void M()
                        {
                            ; ;
                            ; ;

                            Action<int> action = i =>
                            {
                                ; ;
                            };
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source);
        }

        [Fact]
        internal async Task When_method_contains_eight_statements_with_parenthesized_lambda_block_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void [|M|]()
                        {
                            ; ;
                            ; ;

                            Action action = () =>
                            {
                                ; ;
                                ;
                            };
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Method 'C.M()' contains 8 statements, which exceeds the maximum of 7 statements");
        }

        [Fact]
        internal async Task When_method_contains_seven_statements_with_parenthesized_lambda_block_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void M()
                        {
                            ; ;
                            ; ;

                            Action action = () =>
                            {
                                ; ;
                            };
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source);
        }

        [Fact]
        internal async Task When_method_contains_eight_statements_with_anonymous_method_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void [|M|]()
                        {
                            ; ;
                            ; ;

                            Action action = delegate
                            {
                                ; ;
                                ;
                            };
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Method 'C.M()' contains 8 statements, which exceeds the maximum of 7 statements");
        }

        [Fact]
        internal async Task When_method_contains_seven_statements_with_anonymous_method_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void M()
                        {
                            ; ;
                            ; ;

                            Action action = delegate
                            {
                                ; ;
                            };
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source);
        }

        #endregion

        [Fact]
        internal async Task When_method_contains_mixed_set_of_statements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .Using(typeof(IDisposable).Namespace)
                .Using(typeof(CLSCompliantAttribute).Namespace)
                .Using(typeof(CallerMemberNameAttribute).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        IDisposable AcquireResource() => throw null; 
                        int field;

                        [CLSCompliant(isCompliant: false)]
                        [Obsolete(null, false)]
                        void [|M1|]()
                        {
                            using (var resource = AcquireResource())                    // 1
                            {
                                int i = true ? 1 : throw new NotSupportedException();   // 2

                                try                                                     // 3
                                {
                                    int j = checked(i++);                               // 4
                                    Action action = () => throw null;                   // 5
                                }
                                catch (Exception ex)
                                {
                                    return;                                             // 6
                                    throw;                                              // 7
                                }
                                finally
                                {
                                    ;                                                   // 8
                                }
                            }
                        }

                        void [|M2|](int i, [CallerMemberName] string memberName = null)
                        {
                            switch (i)                                      // 1
                            {
                                case 1:
                                case 2:
                                case 3:
                                    goto case 4;                            // 2
                                case 4:
                                    goto default;                           // 3
                                default:
                                {
                                    throw null;                             // 4
                                }
                            }

                            unsafe                                          // 5
                            {
                                fixed (int* p = &field)                     // 6
                                {
                                    var x = stackalloc int[] { 1, 2, 3 };   // 7
                                    int y = *x;                             // 8
                                }
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Method 'C.M1()' contains 8 statements, which exceeds the maximum of 7 statements",
                "Method 'C.M2(int, string)' contains 8 statements, which exceeds the maximum of 7 statements");
        }

        [Fact]
        internal async Task When_async_method_contains_mixed_set_of_statements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .Using(typeof(Task).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        async Task [|M|](bool b = default(bool))
                        {
                            if (b)                      // 1
                            {
                                throw null;             // 2
                            }

                            await Task.Yield();         // 3

                            if (!b)                     // 4
                            {
                                await Task.Yield();     // 5
                                await Task.Yield();     // 6
                            }
                            else
                                throw null;             // 7

                            return;                     // 8
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Method 'C.M(bool)' contains 8 statements, which exceeds the maximum of 7 statements");
        }

        #region Non-default configuration

        [Fact]
        internal async Task When_using_editor_config_setting_it_must_be_applied()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithOptions(new AnalyzerOptionsBuilder()
                    .ForEditorConfig(new EditorConfigSettingsBuilder()
                        .Including(DiagnosticId, "max_statement_count", "16")))
                .InGlobalScope(@"
                    class C
                    {
                        int [|M|](string s)
                        {
                            ; ; ; ;
                            ; ; ; ;
                            ; ; ; ;
                            ; ; ; ;
                            throw null;
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Method 'C.M(string)' contains 17 statements, which exceeds the maximum of 16 statements");
        }

        [Fact]
        internal async Task When_using_xml_config_setting_it_must_be_applied()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithOptions(new AnalyzerOptionsBuilder()
                    .ForXmlSettings(new XmlSettingsBuilder()
                        .Including(DiagnosticId, "MaxStatementCount", "16")))
                .InGlobalScope(@"
                    class C
                    {
                        int [|M|](string s)
                        {
                            ; ; ; ;
                            ; ; ; ;
                            ; ; ; ;
                            ; ; ; ;
                            throw null;
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Method 'C.M(string)' contains 17 statements, which exceeds the maximum of 16 statements");
        }

        [Fact]
        internal async Task When_using_xml_and_editor_config_setting_it_must_use_editor_config_value()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithOptions(new AnalyzerOptionsBuilder()
                    .ForXmlSettings(new XmlSettingsBuilder()
                        .Including(DiagnosticId, "MaxStatementCount", "12"))
                    .ForEditorConfig(new EditorConfigSettingsBuilder()
                        .Including(DiagnosticId, "max_statement_count", "16")))
                .InGlobalScope(@"
                    class C
                    {
                        int [|M|](string s)
                        {
                            ; ; ; ;
                            ; ; ; ;
                            ; ; ; ;
                            ; ; ; ;
                            throw null;
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Method 'C.M(string)' contains 17 statements, which exceeds the maximum of 16 statements");
        }

        [Fact]
        internal async Task When_xml_file_is_corrupt_it_must_use_default_value()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithOptions(new AnalyzerOptionsBuilder()
                    .ForXmlText("*** BAD XML ***"))
                .InGlobalScope(@"
                    class C
                    {
                        int [|M|](string s)
                        {
                            ; ; ; ;
                            ; ; ;
                            throw null;
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Method 'C.M(string)' contains 8 statements, which exceeds the maximum of 7 statements");
        }

        [Fact]
        internal async Task When_settings_are_missing_it_must_use_default_value()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithOptions(new AnalyzerOptionsBuilder()
                    .ForEditorConfig(new EditorConfigSettingsBuilder()
                        .Including(DiagnosticId, "other-unused-setting", "some-value"))
                    .ForXmlSettings(new XmlSettingsBuilder()
                        .Including(DiagnosticId, "OtherUnusedSetting", "SomeValue")))
                .InGlobalScope(@"
                    class C
                    {
                        int [|M|](string s)
                        {
                            ; ; ; ;
                            ; ; ;
                            throw null;
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Method 'C.M(string)' contains 8 statements, which exceeds the maximum of 7 statements");
        }

        [Fact]
        internal async Task When_xml_setting_value_is_missing_it_must_use_default_value()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithOptions(new AnalyzerOptionsBuilder()
                    .ForXmlSettings(new XmlSettingsBuilder()
                        .Including(DiagnosticId, "MaxStatementCount", null)))
                .InGlobalScope(@"
                    class C
                    {
                        int [|M|](string s)
                        {
                            ; ; ; ;
                            ; ; ;
                            throw null;
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Method 'C.M(string)' contains 8 statements, which exceeds the maximum of 7 statements");
        }

        [Fact]
        internal async Task When_editor_config_setting_value_is_missing_it_must_use_default_value()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithOptions(new AnalyzerOptionsBuilder()
                    .ForEditorConfig(new EditorConfigSettingsBuilder()
                        .Including(DiagnosticId, "max_statement_count", null)))
                .InGlobalScope(@"
                    class C
                    {
                        int [|M|](string s)
                        {
                            ; ; ; ;
                            ; ; ;
                            throw null;
                        }
                    }
                ")
                .Build();

            // Act and assert
            await VerifyGuidelineDiagnosticAsync(source,
                "Method 'C.M(string)' contains 8 statements, which exceeds the maximum of 7 statements");
        }

        [Fact]
        internal async Task When_xml_setting_value_is_invalid_it_must_fail()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithOptions(new AnalyzerOptionsBuilder()
                    .ForXmlSettings(new XmlSettingsBuilder()
                        .Including(DiagnosticId, "MaxStatementCount", "bad")))
                .InGlobalScope(@"
                    class C
                    {
                        int [|M|](string s)
                        {
                            ; ; ; ;
                            ; ; ;
                            throw null;
                        }
                    }
                ")
                .Build();

            // Act
            Func<Task> action = async () => await VerifyGuidelineDiagnosticAsync(source);

            // Assert
            await action.Should().ThrowAsync<Exception>()
                .WithMessage("*Value for 'AV1500:MaxStatementCount' in 'CSharpGuidelinesAnalyzer.config' must be in range 0-255.*");
        }

        [Fact]
        internal async Task When_editor_config_setting_value_is_invalid_it_must_fail()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithOptions(new AnalyzerOptionsBuilder()
                    .ForEditorConfig(new EditorConfigSettingsBuilder()
                        .Including(DiagnosticId, "max_statement_count", "bad")))
                .InGlobalScope(@"
                    class C
                    {
                        int [|M|](string s)
                        {
                            ; ; ; ;
                            ; ; ;
                            throw null;
                        }
                    }
                ")
                .Build();

            // Act
            Func<Task> action = async () => await VerifyGuidelineDiagnosticAsync(source);

            // Assert
            await action.Should().ThrowAsync<Exception>()
                .WithMessage("*Value for 'dotnet_diagnostic.av1500.max_statement_count' in '.editorconfig' must be in range 0-255.*");
        }

        [Fact]
        internal async Task When_xml_setting_value_is_out_of_range_it_must_fail()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithOptions(new AnalyzerOptionsBuilder()
                    .ForXmlSettings(new XmlSettingsBuilder()
                        .Including(DiagnosticId, "MaxStatementCount", "-1")))
                .InGlobalScope(@"
                    class C
                    {
                        int [|M|](string s)
                        {
                            ; ; ; ;
                            ; ; ;
                            throw null;
                        }
                    }
                ")
                .Build();

            // Act
            Func<Task> action = async () => await VerifyGuidelineDiagnosticAsync(source);

            // Assert
            await action.Should().ThrowAsync<Exception>()
                .WithMessage("*Value for 'AV1500:MaxStatementCount' in 'CSharpGuidelinesAnalyzer.config' must be in range 0-255.*");
        }

        [Fact]
        internal async Task When_editor_config_setting_value_is_out_of_range_it_must_fail()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithOptions(new AnalyzerOptionsBuilder()
                    .ForEditorConfig(new EditorConfigSettingsBuilder()
                        .Including(DiagnosticId, "max_statement_count", "-1")))
                .InGlobalScope(@"
                    class C
                    {
                        int [|M|](string s)
                        {
                            ; ; ; ;
                            ; ; ;
                            throw null;
                        }
                    }
                ")
                .Build();

            // Act
            Func<Task> action = async () => await VerifyGuidelineDiagnosticAsync(source);

            // Assert
            await action.Should().ThrowAsync<Exception>()
                .WithMessage("*Value for 'dotnet_diagnostic.av1500.max_statement_count' in '.editorconfig' must be in range 0-255.*");
        }

        #endregion

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new AvoidMemberWithManyStatementsAnalyzer();
        }
    }
}
