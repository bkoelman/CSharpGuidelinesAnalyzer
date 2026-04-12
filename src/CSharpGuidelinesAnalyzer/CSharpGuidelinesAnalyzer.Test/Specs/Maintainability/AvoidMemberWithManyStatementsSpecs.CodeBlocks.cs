using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Xunit;

// @formatter:keep_existing_linebreaks true

namespace CSharpGuidelinesAnalyzer.Test.Specs.Maintainability;

public partial class AvoidMemberWithManyStatementsSpecs
{
    [Fact]
    internal async Task When_method_body_contains_eight_statements_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                class C
                {
                    int [|M|](string s)
                    {
                        ; ; ; ;
                        ; ; ;
                        throw null;
                    }
                }
                """)
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
            .InGlobalScope("""
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
                """)
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
            .InGlobalScope("""
                class C
                {
                    public static C operator[|++|](C c)
                    {
                        ; ; ; ;
                        ; ; ;
                        throw null;
                    }
                }
                """)
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
            .InGlobalScope("""
                class C
                {
                    public static implicit operator [|C|](string s)
                    {
                        ; ; ; ;
                        ; ; ;
                        throw null;
                    }
                }
                """)
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
            .InGlobalScope("""
                class C
                {
                    public static explicit operator [|C|](string s)
                    {
                        ; ; ; ;
                        ; ; ;
                        throw null;
                    }
                }
                """)
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
            .InGlobalScope("""
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
                """)
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
            .InGlobalScope("""
                class C
                {
                    public [|C|](string s)
                    {
                        ; ; ; ;
                        ; ; ;
                        throw null;
                    }
                }
                """)
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
            .InGlobalScope("""
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
                """)
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
            .InGlobalScope("""
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
                """)
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
            .InGlobalScope("""
                class C
                {
                    static [|C|]()
                    {
                        ; ; ; ;
                        ; ; ;
                        throw null;
                    }
                }
                """)
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
            .InGlobalScope("""
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
                """)
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
            .InGlobalScope("""
                class C
                {
                    ~[|C|]()
                    {
                        ; ; ; ;
                        ; ; ;
                        throw null;
                    }
                }
                """)
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
            .InGlobalScope("""
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
                """)
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
            .InGlobalScope("""
                class C
                {
                    Func<int> [|P|] { get; } = () =>
                    {
                        ; ; ; ;
                        ; ; ;
                        throw null;
                    };
                }
                """)
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
            .InGlobalScope("""
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
                """)
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
            .InGlobalScope("""
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
                """)
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
            .InGlobalScope("""
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
                """)
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
            .InGlobalScope("""
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
                """)
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
            .InGlobalScope("""
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
                """)
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
            .InGlobalScope("""
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
                """)
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
            .InGlobalScope("""
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
                """)
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
            .InGlobalScope("""
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
                """)
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
            .InGlobalScope("""
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
                """)
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
            .InGlobalScope("""
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
                """)
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
            .InGlobalScope("""
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
                """)
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
            .InGlobalScope("""
                class C
                {
                    Action [|f|] = () =>
                    {
                        ; ; ; ;
                        ; ; ;
                        throw null;
                    };
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Initializer for 'C.f' contains 8 statements, which exceeds the maximum of 7 statements");
    }
}
