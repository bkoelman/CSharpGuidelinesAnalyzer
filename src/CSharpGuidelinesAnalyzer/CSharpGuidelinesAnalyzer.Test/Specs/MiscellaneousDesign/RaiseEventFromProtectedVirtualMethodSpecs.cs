using CSharpGuidelinesAnalyzer.Rules.MiscellaneousDesign;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

// @formatter:keep_existing_linebreaks true

namespace CSharpGuidelinesAnalyzer.Test.Specs.MiscellaneousDesign;

public sealed class RaiseEventFromProtectedVirtualMethodSpecs : CSharpGuidelinesAnalysisTestFixture
{
    protected override string DiagnosticId => RaiseEventFromProtectedVirtualMethodAnalyzer.DiagnosticId;

    [Fact]
    internal async Task When_event_invocation_method_is_protected_virtual_and_named_On_followed_by_event_name_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class C
                {
                    public event EventHandler ValueChanged;

                    protected virtual void OnValueChanged(EventArgs args)
                    {
                        ValueChanged?.Invoke(this, args);
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_event_invocation_method_is_private_protected_virtual_and_named_On_followed_by_event_name_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class C
                {
                    public event EventHandler ValueChanged;

                    private protected virtual void OnValueChanged(EventArgs args) => ValueChanged?.Invoke(this, args);
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_event_invocation_method_is_not_protected_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class C
                {
                    public event EventHandler ValueChanged;

                    private void [|OnValueChanged|](EventArgs args)
                    {
                        ValueChanged?.Invoke(this, args);
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'OnValueChanged' raises event 'ValueChanged', so it should be protected and virtual");
    }

    [Fact]
    internal async Task When_event_invocation_method_is_not_virtual_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class C
                {
                    public event EventHandler ValueChanged;

                    protected void [|OnValueChanged|](EventArgs args)
                    {
                        var snapshot = ValueChanged;
                        if (snapshot != null)
                        {
                            snapshot(this, args);
                        }
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'OnValueChanged' raises event 'ValueChanged', so it should be protected and virtual");
    }

    [Fact]
    internal async Task When_event_invocation_method_is_private_non_virtual_in_sealed_class_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                public sealed class C
                {
                    public event EventHandler ValueChanged;

                    private void OnValueChanged(EventArgs args)
                    {
                        ValueChanged?.Invoke(this, args);
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_event_invocation_method_is_static_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class C
                {
                    public static event EventHandler ValueChanged;

                    private static void OnValueChanged(EventArgs args)
                    {
                        ValueChanged?.Invoke(null, args);
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_event_invocation_method_name_does_not_follow_pattern_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class C
                {
                    public event EventHandler ValueChanged;

                    protected virtual void [|RaiseEvent|](EventArgs args)
                    {
                        ValueChanged(this, args);
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'RaiseEvent' raises event 'ValueChanged', so it should be named 'OnValueChanged'");
    }

    [Fact]
    internal async Task When_event_invocation_method_name_does_not_follow_pattern_and_is_not_protected_and_not_virtual_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class C
                {
                    public event EventHandler ValueChanged;

                    private void [|RaiseEvent|](EventArgs args)
                    {
                        ValueChanged(this, args);
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'RaiseEvent' raises event 'ValueChanged', so it should be named 'OnValueChanged'");
    }

    [Fact]
    internal async Task When_event_invocation_method_name_has_extra_words_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class C
                {
                    public event EventHandler ValueChanged;

                    protected virtual void [|RaiseOnValueChanged|](EventArgs args)
                    {
                        EventHandler snapshot;
                        snapshot = ValueChanged;

                        if (snapshot != null)
                        {
                            snapshot(this, args);
                        }
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'RaiseOnValueChanged' raises event 'ValueChanged', so it should be named 'OnValueChanged'");
    }

    [Fact]
    internal async Task When_event_invocation_method_is_lambda_expression_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class C
                {
                    public event EventHandler ValueChanged;

                    private void M()
                    {
                        Action action = [|() => ValueChanged?.Invoke(this, EventArgs.Empty)|];
                        action();
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Event 'ValueChanged' should be raised from a regular method");
    }

    [Fact]
    internal async Task When_event_invocation_method_is_local_function_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class C
                {
                    public event EventHandler ValueChanged;

                    public void RaiseEvent(EventArgs args)
                    {
                        void [|OnValueChanged|]()
                        {
                            ValueChanged?.Invoke(this, args);
                        }
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Event 'ValueChanged' should be raised from a regular method");
    }

    [Fact]
    internal async Task When_event_invocation_method_is_explicit_interface_implementation_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                namespace N
                {
                    interface I
                    {
                        event EventHandler ValueChanged;

                        void OnValueChanged();
                    }

                    class C : I
                    {
                        public event EventHandler ValueChanged;

                        void I.OnValueChanged()
                        {
                            ValueChanged?.Invoke(this, EventArgs.Empty);
                        }
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    protected override DiagnosticAnalyzer CreateAnalyzer()
    {
        return new RaiseEventFromProtectedVirtualMethodAnalyzer();
    }
}
