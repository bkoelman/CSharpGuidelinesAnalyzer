using CSharpGuidelinesAnalyzer.Rules.Naming;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

// @formatter:keep_existing_linebreaks true

namespace CSharpGuidelinesAnalyzer.Test.Specs.Naming;

public sealed class PrefixEventHandlersWithOnSpecs : CSharpGuidelinesAnalysisTestFixture
{
    protected override string DiagnosticId => PrefixEventHandlersWithOnAnalyzer.DiagnosticId;

    [Fact]
    internal async Task When_event_is_bound_to_an_anonymous_method_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                public class X
                {
                    public event EventHandler ValueChanged;
                }

                public class C
                {
                    public void M()
                    {
                        X x = new X();
                        x.ValueChanged += new EventHandler(delegate(object s, EventArgs e)
                        {
                        });
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_event_is_bound_to_a_lambda_body_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                public class X
                {
                    public event EventHandler ValueChanged;
                }

                public class C
                {
                    public void M()
                    {
                        X x = new X();
                        x.ValueChanged += (s, e) =>
                        {
                        };
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_event_is_bound_to_a_lambda_expression_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                public class X
                {
                    public event EventHandler ValueChanged;
                }

                public class C
                {
                    public void M()
                    {
                        X x = new X();
                        x.ValueChanged += (s, e) => M();
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_event_is_unbound_from_a_misnamed_method_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                public class X
                {
                    public event EventHandler ValueChanged;
                }

                public class C
                {
                    public void M()
                    {
                        X x = new X();
                        x.ValueChanged -= HandleValueChangedOfX;
                    }
                
                    public void HandleValueChangedOfX(object sender, EventArgs e)
                    {
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_remote_event_is_bound_to_a_method_that_matches_pattern_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                public class Coordinate
                {
                    public event EventHandler ValueChanged;
                }

                public class C
                {
                    private Coordinate _topLeft = new Coordinate();
                
                    public void M()
                    {
                        _topLeft.ValueChanged += TopLeftOnValueChanged;
                    }
                
                    public void TopLeftOnValueChanged(object sender, EventArgs e)
                    {
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_remote_static_event_is_bound_to_a_method_that_matches_pattern_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                public static class Coordinate
                {
                    public static event EventHandler ValueChanged;
                }

                public class C
                {
                    public void M()
                    {
                        Coordinate.ValueChanged += CoordinateOnValueChanged;
                    }
                
                    public void CoordinateOnValueChanged(object sender, EventArgs e)
                    {
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_remote_event_is_bound_to_a_method_that_does_not_match_pattern_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                public class Coordinate
                {
                    public event EventHandler ValueChanged;
                }

                public class C
                {
                    private Coordinate _topLeft = new Coordinate();
                
                    public void M()
                    {
                        _topLeft.ValueChanged += [|HandleTopLeftValueChanged|];
                    }
                
                    public void HandleTopLeftValueChanged(object sender, EventArgs e)
                    {
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'HandleTopLeftValueChanged' that handles event 'ValueChanged' should be renamed to 'TopLeftOnValueChanged'");
    }

    [Fact]
    internal async Task When_remote_event_is_bound_to_a_method_on_a_field_named_underscore_and_matches_pattern_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                public class Coordinate
                {
                    public event EventHandler ValueChanged;
                }

                public class C
                {
                    private Coordinate _ = new Coordinate();
                
                    public void M()
                    {
                        _.ValueChanged += OnValueChanged;
                    }
                
                    public void OnValueChanged(object sender, EventArgs e)
                    {
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_local_event_is_bound_to_a_method_that_matches_pattern_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                public class C
                {
                    public event EventHandler ValueChanged;
                
                    public void M()
                    {
                        ValueChanged += OnValueChanged;
                    }
                
                    public void OnValueChanged(object sender, EventArgs e)
                    {
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_local_static_event_is_bound_to_a_method_that_matches_pattern_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                public class C
                {
                    public static event EventHandler ValueChanged;
                
                    public void M()
                    {
                        ValueChanged += OnValueChanged;
                    }
                
                    public void OnValueChanged(object sender, EventArgs e)
                    {
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_local_event_is_bound_to_a_method_that_does_not_match_pattern_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                public class C
                {
                    public event EventHandler ValueChanged;
                
                    public void M()
                    {
                        ValueChanged += [|HandleValueChanged|];
                    }
                
                    public void HandleValueChanged(object sender, EventArgs e)
                    {
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'HandleValueChanged' that handles event 'ValueChanged' should be renamed to 'OnValueChanged'");
    }

    [Fact]
    internal async Task When_local_event_is_bound_to_a_local_function_that_matches_pattern_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                public class C
                {
                    public event EventHandler ValueChanged;
                
                    public void M()
                    {
                        void L()
                        {
                            ValueChanged += OnValueChanged;
                        }
                
                        void OnValueChanged(object sender, EventArgs e)
                        {
                        }
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_local_event_is_bound_to_a_local_function_that_does_not_match_pattern_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                public class C
                {
                    public event EventHandler ValueChanged;
                
                    public void M()
                    {
                        void L()
                        {
                            ValueChanged += [|HandleValueChanged|];
                        }
                
                        void HandleValueChanged(object sender, EventArgs e)
                        {
                        }
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Local function 'HandleValueChanged' that handles event 'ValueChanged' should be renamed to 'OnValueChanged'");
    }

    protected override DiagnosticAnalyzer CreateAnalyzer()
    {
        return new PrefixEventHandlersWithOnAnalyzer();
    }
}
