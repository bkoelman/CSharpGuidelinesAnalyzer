using CSharpGuidelinesAnalyzer.Rules.Maintainability;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

// @formatter:keep_existing_linebreaks true

namespace CSharpGuidelinesAnalyzer.Test.Specs.Maintainability;

public sealed class AvoidBooleanParameterSpecs : CSharpGuidelinesAnalysisTestFixture
{
    protected override string DiagnosticId => AvoidBooleanParameterAnalyzer.DiagnosticId;

    [Fact]
    internal async Task When_public_method_parameter_type_is_bool_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                public void M(bool [|b|])
                {
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Parameter 'b' is of type 'bool'");
    }

    [Fact]
    internal async Task When_public_method_parameter_type_is_nullable_bool_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                public void M(bool? [|b|])
                {
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Parameter 'b' is of type 'bool?'");
    }

    [Fact]
    internal async Task When_public_method_parameter_type_is_string_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                public void M(string s)
                {
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_public_method_parameter_type_is_bool_in_Deconstruct_method_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                public struct S
                {
                    public string Text;
                    public bool IsEnabled;

                    public void Deconstruct(out string text, out bool isEnabled)
                    {
                        text = Text;
                        isEnabled = IsEnabled;
                    }

                    static void Test()
                    {
                        S s = new S
                        {
                            Text = string.Empty,
                            IsEnabled = true
                        };

                        (string a, bool b) = s;
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_public_constructor_parameter_type_is_bool_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                public class C
                {
                    public C(bool [|b|])
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Parameter 'b' is of type 'bool'");
    }

    [Fact]
    internal async Task When_public_delegate_parameter_type_is_bool_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                public delegate void D(bool [|b|]);
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Parameter 'b' is of type 'bool'");
    }

    [Fact]
    internal async Task When_public_property_is_bool_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                public bool B
                {
                    get { throw new NotImplementedException(); }
                    set
                    {
                        value = true;
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_public_property_is_nullable_bool_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                public bool? B
                {
                    get { throw new NotImplementedException(); }
                    set
                    {
                        value = true;
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_public_indexer_parameter_type_is_bool_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                public int this[bool [|b|]]
                {
                    get { throw new NotImplementedException(); }
                    set { throw new NotImplementedException(); }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Parameter 'b' is of type 'bool'");
    }

    [Fact]
    internal async Task When_public_indexer_parameter_type_is_nullable_bool_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                public int this[bool? [|b|]]
                {
                    get { throw new NotImplementedException(); }
                    set { throw new NotImplementedException(); }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Parameter 'b' is of type 'bool?'");
    }

    [Fact]
    internal async Task When_protected_method_parameter_type_is_bool_in_overridden_method_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                public class C
                {
                    protected virtual void M(bool [|b|])
                    {
                    }
                }

                public class D : C
                {
                    protected override void M(bool b)
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Parameter 'b' is of type 'bool'");
    }

    [Fact]
    internal async Task When_protected_method_parameter_type_is_nullable_bool_in_overridden_method_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                public class C
                {
                    protected virtual void M(bool? [|b|])
                    {
                    }
                }

                public class D : C
                {
                    protected override void M(bool? b)
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Parameter 'b' is of type 'bool?'");
    }

    [Fact]
    internal async Task When_protected_method_parameter_type_is_bool_in_hidden_method_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                public class C
                {
                    protected virtual void M(bool [|b|])
                    {
                    }
                }

                public class D : C
                {
                    protected new void M(bool b)
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Parameter 'b' is of type 'bool'");
    }

    [Fact]
    internal async Task When_protected_method_parameter_type_is_nullable_bool_in_hidden_method_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                public class C
                {
                    protected virtual void M(bool? [|b|])
                    {
                    }
                }

                public class D : C
                {
                    protected new void M(bool? b)
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Parameter 'b' is of type 'bool?'");
    }

    [Fact]
    internal async Task When_public_indexer_parameter_type_is_bool_in_overridden_indexer_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                public abstract class C
                {
                    public abstract int this[bool [|b|]]
                    {
                        get; set;
                    }
                }

                public class D : C
                {
                    public override int this[bool b]
                    {
                        get { throw new NotImplementedException(); }
                        set { throw new NotImplementedException(); }
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Parameter 'b' is of type 'bool'");
    }

    [Fact]
    internal async Task When_public_indexer_parameter_type_is_nullable_bool_in_overridden_indexer_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                public abstract class C
                {
                    public abstract int this[bool? [|b|]]
                    {
                        get; set;
                    }
                }

                public class D : C
                {
                    public override int this[bool? b]
                    {
                        get { throw new NotImplementedException(); }
                        set { throw new NotImplementedException(); }
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Parameter 'b' is of type 'bool?'");
    }

    [Fact]
    internal async Task When_public_indexer_parameter_type_is_bool_in_hidden_indexer_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                public class C
                {
                    public virtual int this[bool [|b|]]
                    {
                        get { throw new NotImplementedException(); }
                        set { throw new NotImplementedException(); }
                    }
                }

                public class D : C
                {
                    public new int this[bool b]
                    {
                        get { throw new NotImplementedException(); }
                        set { throw new NotImplementedException(); }
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Parameter 'b' is of type 'bool'");
    }

    [Fact]
    internal async Task When_public_indexer_parameter_type_is_nullable_bool_in_hidden_indexer_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                public class C
                {
                    public virtual int this[bool? [|b|]]
                    {
                        get { throw new NotImplementedException(); }
                        set { throw new NotImplementedException(); }
                    }
                }

                public class D : C
                {
                    public new int this[bool? b]
                    {
                        get { throw new NotImplementedException(); }
                        set { throw new NotImplementedException(); }
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Parameter 'b' is of type 'bool?'");
    }

    [Fact]
    internal async Task When_public_method_parameter_type_is_bool_in_implicit_interface_implementation_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                public interface I
                {
                    void M(bool [|b|]);
                }

                public class C : I
                {
                    public void M(bool b)
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Parameter 'b' is of type 'bool'");
    }

    [Fact]
    internal async Task When_public_method_parameter_type_is_nullable_bool_in_implicit_interface_implementation_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                public interface I
                {
                    void M(bool? [|b|]);
                }

                public class C : I
                {
                    public void M(bool? b)
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Parameter 'b' is of type 'bool?'");
    }

    [Fact]
    internal async Task When_public_method_parameter_type_is_bool_in_explicit_interface_implementation_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                public interface I
                {
                    void M(bool [|b|]);
                }

                public class C : I
                {
                    void I.M(bool b)
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Parameter 'b' is of type 'bool'");
    }

    [Fact]
    internal async Task When_public_method_parameter_type_is_nullable_bool_in_explicit_interface_implementation_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                public interface I
                {
                    void M(bool? [|b|]);
                }

                public class C : I
                {
                    void I.M(bool? b)
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Parameter 'b' is of type 'bool?'");
    }

    [Fact]
    internal async Task When_public_indexer_parameter_type_is_bool_in_implicit_interface_implementation_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                public interface I
                {
                    int this[bool [|b|]]
                    {
                        get; set;
                    }
                }

                public class C : I
                {
                    public int this[bool b]
                    {
                        get { throw new NotImplementedException(); }
                        set { throw new NotImplementedException(); }
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Parameter 'b' is of type 'bool'");
    }

    [Fact]
    internal async Task When_public_indexer_parameter_type_is_nullable_bool_in_implicit_interface_implementation_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                public interface I
                {
                    int this[bool? [|b|]]
                    {
                        get; set;
                    }
                }

                public class C : I
                {
                    public int this[bool? b]
                    {
                        get { throw new NotImplementedException(); }
                        set { throw new NotImplementedException(); }
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Parameter 'b' is of type 'bool?'");
    }

    [Fact]
    internal async Task When_public_indexer_parameter_type_is_bool_in_explicit_interface_implementation_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                public interface I
                {
                    int this[bool [|b|]]
                    {
                        get; set;
                    }
                }

                public class C : I
                {
                    int I.this[bool b]
                    {
                        get { throw new NotImplementedException(); }
                        set { throw new NotImplementedException(); }
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Parameter 'b' is of type 'bool'");
    }

    [Fact]
    internal async Task When_public_indexer_parameter_type_is_nullable_bool_in_explicit_interface_implementation_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                public interface I
                {
                    int this[bool? [|b|]]
                    {
                        get; set;
                    }
                }

                public class C : I
                {
                    int I.this[bool? b]
                    {
                        get { throw new NotImplementedException(); }
                        set { throw new NotImplementedException(); }
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Parameter 'b' is of type 'bool?'");
    }

    [Fact]
    internal async Task When_method_parameter_type_is_bool_in_public_class_hierarchy_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                public class C
                {
                    public class D
                    {
                        public void M1(bool [|b1|])
                        {
                        }

                        protected void M2(bool [|b2|])
                        {
                        }

                        internal void M3(bool [|b3|])
                        {
                        }

                        protected internal void M4(bool [|b4|])
                        {
                        }

                        private protected void M5(bool [|b5|])
                        {
                        }

                        private void M6(bool b6)
                        {
                        }
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Parameter 'b1' is of type 'bool'",
            "Parameter 'b2' is of type 'bool'",
            "Parameter 'b3' is of type 'bool'",
            "Parameter 'b4' is of type 'bool'",
            "Parameter 'b5' is of type 'bool'");
    }

    [Fact]
    internal async Task When_method_parameter_type_is_bool_in_protected_class_hierarchy_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                public class C
                {
                    protected class D
                    {
                        public void M1(bool [|b1|])
                        {
                        }

                        protected void M2(bool [|b2|])
                        {
                        }

                        internal void M3(bool [|b3|])
                        {
                        }

                        protected internal void M4(bool [|b4|])
                        {
                        }

                        private protected void M5(bool [|b5|])
                        {
                        }

                        private void M6(bool b6)
                        {
                        }
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Parameter 'b1' is of type 'bool'",
            "Parameter 'b2' is of type 'bool'",
            "Parameter 'b3' is of type 'bool'",
            "Parameter 'b4' is of type 'bool'",
            "Parameter 'b5' is of type 'bool'");
    }

    [Fact]
    internal async Task When_method_parameter_type_is_bool_in_internal_class_hierarchy_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                internal class C
                {
                    internal class D
                    {
                        public void M1(bool [|b1|])
                        {
                        }

                        protected void M2(bool [|b2|])
                        {
                        }

                        internal void M3(bool [|b3|])
                        {
                        }

                        protected internal void M4(bool [|b4|])
                        {
                        }

                        private protected void M5(bool [|b5|])
                        {
                        }

                        private void M6(bool b6)
                        {
                        }
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Parameter 'b1' is of type 'bool'",
            "Parameter 'b2' is of type 'bool'",
            "Parameter 'b3' is of type 'bool'",
            "Parameter 'b4' is of type 'bool'",
            "Parameter 'b5' is of type 'bool'");
    }

    [Fact]
    internal async Task When_method_parameter_type_is_bool_in_protected_internal_class_hierarchy_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                internal class C
                {
                    protected internal class D
                    {
                        public void M1(bool [|b1|])
                        {
                        }

                        protected void M2(bool [|b2|])
                        {
                        }

                        internal void M3(bool [|b3|])
                        {
                        }

                        protected internal void M4(bool [|b4|])
                        {
                        }

                        private protected void M5(bool [|b5|])
                        {
                        }

                        private void M6(bool b6)
                        {
                        }
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Parameter 'b1' is of type 'bool'",
            "Parameter 'b2' is of type 'bool'",
            "Parameter 'b3' is of type 'bool'",
            "Parameter 'b4' is of type 'bool'",
            "Parameter 'b5' is of type 'bool'");
    }

    [Fact]
    internal async Task When_method_parameter_type_is_bool_in_private_class_hierarchy_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                public class C
                {
                    private class D
                    {
                        public void M1(bool b1)
                        {
                        }

                        protected void M2(bool b2)
                        {
                        }

                        internal void M3(bool b3)
                        {
                        }

                        protected internal void M4(bool b4)
                        {
                        }

                        private protected void M5(bool b5)
                        {
                        }

                        private void M6(bool b6)
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
    internal async Task When_parameter_type_is_bool_in_IDisposable_pattern_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                public class C
                {
                    protected virtual void Dispose(bool disposing)
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    protected override DiagnosticAnalyzer CreateAnalyzer()
    {
        return new AvoidBooleanParameterAnalyzer();
    }
}
