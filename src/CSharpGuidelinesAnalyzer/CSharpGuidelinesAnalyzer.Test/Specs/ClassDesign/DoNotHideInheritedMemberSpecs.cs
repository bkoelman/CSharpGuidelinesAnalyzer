using CSharpGuidelinesAnalyzer.Rules.ClassDesign;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.ClassDesign;

public sealed class DoNotHideInheritedMemberSpecs : CSharpGuidelinesAnalysisTestFixture
{
    protected override string DiagnosticId => DoNotHideInheritedMemberAnalyzer.DiagnosticId;

    [Fact]
    internal async Task When_field_hides_base_field_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class B
                {
                    public string F;
                }

                class C : B
                {
                    public new string [|F|];
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "'C.F' hides inherited member");
    }

    [Fact]
    internal async Task When_property_overrides_base_property_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class B
                {
                    public virtual string P
                    {
                        get
                        {
                            throw new NotImplementedException();
                        }
                        set
                        {
                            throw new NotImplementedException();
                        }
                    }
                }

                class C : B
                {
                    public override string P
                    {
                        get
                        {
                            throw new NotImplementedException();
                        }
                        set
                        {
                            throw new NotImplementedException();
                        }
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_property_hides_base_property_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class B
                {
                    public virtual string P
                    {
                        get
                        {
                            throw new NotImplementedException();
                        }
                        set
                        {
                            throw new NotImplementedException();
                        }
                    }
                }

                class C : B
                {
                    public new string [|P|]
                    {
                        get
                        {
                            throw new NotImplementedException();
                        }
                        set
                        {
                            throw new NotImplementedException();
                        }
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "'C.P' hides inherited member");
    }

    [Fact]
    internal async Task When_method_overrides_base_method_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class B
                {
                    public virtual void M(int i)
                    {
                    }
                }

                class C : B
                {
                    public override void M(int i)
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_hides_base_method_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class B
                {
                    public virtual void M(int i)
                    {
                    }
                }

                class C : B
                {
                    public new void [|M|](int i)
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "'C.M(int)' hides inherited member");
    }

    [Fact]
    internal async Task When_event_overrides_base_event_with_accessors_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class B
                {
                    public virtual event EventHandler Changed
                    {
                        add
                        {
                            throw new NotImplementedException();
                        }
                        remove
                        {
                            throw new NotImplementedException();
                        }
                    }
                }

                class C : B
                {
                    public override event EventHandler Changed
                    {
                        add
                        {
                            throw new NotImplementedException();
                        }
                        remove
                        {
                            throw new NotImplementedException();
                        }
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_event_hides_base_event_with_accessors_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class B
                {
                    public virtual event EventHandler Changed
                    {
                        add
                        {
                            throw new NotImplementedException();
                        }
                        remove
                        {
                            throw new NotImplementedException();
                        }
                    }
                }

                class C : B
                {
                    public new event EventHandler [|Changed|]
                    {
                        add
                        {
                            throw new NotImplementedException();
                        }
                        remove
                        {
                            throw new NotImplementedException();
                        }
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "'C.Changed' hides inherited member");
    }

    [Fact]
    internal async Task When_event_overrides_base_event_without_accessors_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class B
                {
                    public virtual event EventHandler Changed;
                }

                class C : B
                {
                    public override event EventHandler Changed;
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_event_hides_base_event_without_accessors_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class B
                {
                    public virtual event EventHandler Changed;
                }

                class C : B
                {
                    public new event EventHandler [|Changed|];
                }

            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "'C.Changed' hides inherited member");
    }

    [Fact]
    internal async Task When_nested_class_hides_base_nested_struct_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class B
                {
                    public struct S
                    {
                    }
                }

                class C : B
                {
                    public new class [|S|]
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "'C.S' hides inherited member");
    }

    [Fact]
    internal async Task When_nested_struct_hides_base_nested_class_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class B
                {
                    public class S
                    {
                    }
                }

                class C : B
                {
                    public new struct [|S|]
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "'C.S' hides inherited member");
    }

    [Fact]
    internal async Task When_delegate_hides_base_property_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class B
                {
                    public int P
                    {
                        get; set;
                    }
                }

                class C : B
                {
                    public new delegate void [|P|](string s);
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "'C.P' hides inherited member");
    }

    [Fact]
    internal async Task When_property_hides_base_delegate_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class B
                {
                    public delegate void P(string s);
                }

                class C : B
                {
                    public new int [|P|]
                    {
                        get; set;
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "'C.P' hides inherited member");
    }

    [Fact]
    internal async Task When_nested_enum_hides_base_field_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class B
                {
                    public int F;
                }

                class C : B
                {
                    public new enum [|F|]
                    {
                        A, B
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "'C.F' hides inherited member");
    }

    [Fact]
    internal async Task When_field_hides_base_nested_enum_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class B
                {
                    public enum F
                    {
                        A, B
                    }
                }

                class C : B
                {
                    public new int [|F|];
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "'C.F' hides inherited member");
    }

    [Fact]
    internal async Task When_nested_interface_hides_base_event_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class B
                {
                    public event EventHandler Changed;
                }

                class C : B
                {
                    public new interface [|Changed|]
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "'C.Changed' hides inherited member");
    }

    [Fact]
    internal async Task When_event_hides_base_nested_interface_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class B
                {
                    public interface Changed
                    {
                    }
                }

                class C : B
                {
                    public new event EventHandler [|Changed|];
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "'C.Changed' hides inherited member");
    }

    protected override DiagnosticAnalyzer CreateAnalyzer()
    {
        return new DoNotHideInheritedMemberAnalyzer();
    }
}
