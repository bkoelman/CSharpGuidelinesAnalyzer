using CSharpGuidelinesAnalyzer.Rules.Maintainability;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

// @formatter:keep_existing_linebreaks true

namespace CSharpGuidelinesAnalyzer.Test.Specs.Maintainability;

public sealed class DoNotAssignToParameterSpecs : CSharpGuidelinesAnalysisTestFixture
{
    protected override string DiagnosticId => DoNotAssignToParameterAnalyzer.DiagnosticId;

    [Fact]
    internal async Task When_method_is_abstract_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                public abstract class C
                {
                    public abstract void M(string p);
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_is_partial_without_body_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                partial class C
                {
                    partial void M(string p);
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_bool_parameter_is_read_from_in_method_body_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                void M(bool p)
                {
                    var x = p.ToString(null);
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_bool_parameter_is_written_to_in_method_body_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                void M(bool [|p|])
                {
                    p = true;
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "The value of parameter 'p' is overwritten in its method body");
    }

    [Fact]
    internal async Task When_int_parameter_is_read_from_in_method_body_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                void M(int p)
                {
                    var x = p.GetHashCode();
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_int_parameter_is_written_to_in_method_body_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                void M(int [|p|])
                {
                    p += 5;
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "The value of parameter 'p' is overwritten in its method body");
    }

    [Fact]
    internal async Task When_unsigned_short_parameter_is_read_from_in_method_body_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                void M(ushort p)
                {
                    var x = p.GetHashCode();
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_unsigned_short_parameter_is_written_to_in_method_body_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                void M(ushort [|p|])
                {
                    p += 5;
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "The value of parameter 'p' is overwritten in its method body");
    }

    [Fact]
    internal async Task When_decimal_parameter_is_read_from_in_method_body_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                void M(decimal p)
                {
                    var x = p.ToString(null, null);
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_decimal_parameter_is_written_to_in_method_body_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                void M(decimal [|p|])
                {
                    p += 5m;
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "The value of parameter 'p' is overwritten in its method body");
    }

    [Fact]
    internal async Task When_double_parameter_is_read_from_in_method_body_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                void M(double p)
                {
                    var x = p.ToString(null, null);
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_double_parameter_is_written_to_in_method_body_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                void M(double [|p|])
                {
                    p += 5.0;
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "The value of parameter 'p' is overwritten in its method body");
    }

    [Fact]
    internal async Task When_DateTime_parameter_is_read_from_in_method_body_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                void M(DateTime p)
                {
                    var x = p.Year;
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_DateTime_parameter_is_written_to_in_method_body_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                void M(DateTime [|p|])
                {
                    p = DateTime.UtcNow;
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "The value of parameter 'p' is overwritten in its method body");
    }

    [Fact]
    internal async Task When_nullable_parameter_is_read_from_in_method_body_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                void M(int? p)
                {
                    var x = p.Value.ToString(null, null);
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_nullable_parameter_is_written_to_in_method_body_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                void M(int? [|p|])
                {
                    p += 5;
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "The value of parameter 'p' is overwritten in its method body");
    }

    [Fact]
    internal async Task When_enum_parameter_is_read_from_in_method_body_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                public enum E
                {
                    A, B
                }

                void M(E p)
                {
                    var x = p.HasFlag(E.A);
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_enum_parameter_is_written_to_in_method_body_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                public enum E
                {
                    A, B
                }

                void M(E [|p|])
                {
                    p = E.B;
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "The value of parameter 'p' is overwritten in its method body");
    }

    [Fact]
    internal async Task When_struct_parameter_is_read_from_in_method_body_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                public struct S
                {
                    public int Value;
                }

                void M(S p)
                {
                    var x = p.Value;
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_struct_parameter_is_invoked_on_in_method_body_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                public struct S
                {
                    public void M()
                    {
                    }
                }

                class C
                {
                    public C(S s = default(S))
                    {
                        s.M();
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_struct_parameter_is_reassigned_in_method_body_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                public struct S
                {
                }

                void M(S [|p|])
                {
                    p = new S();
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "The value of parameter 'p' is overwritten in its method body");
    }

    [Fact]
    internal async Task When_struct_parameter_is_compound_reassigned_in_method_body_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                public struct S
                {
                    public static S operator +(S p, int i)
                    {
                        throw null;
                    }
                }

                void M(S [|p|])
                {
                    p += 5;
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "The value of parameter 'p' is overwritten in its method body");
    }

    [Fact]
    internal async Task When_struct_parameter_is_pre_incremented_in_method_body_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                public struct S
                {
                    public static S operator ++(S p)
                    {
                        throw null;
                    }
                }

                void M(S [|p|])
                {
                    ++p;
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "The value of parameter 'p' is overwritten in its method body");
    }

    [Fact]
    internal async Task When_struct_parameter_is_post_incremented_in_method_body_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                public struct S
                {
                    public static S operator ++(S p)
                    {
                        throw null;
                    }
                }

                void M(S [|p|])
                {
                    p++;
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "The value of parameter 'p' is overwritten in its method body");
    }

    [Fact]
    internal async Task When_struct_parameter_is_pre_decremented_in_method_body_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                public struct S
                {
                    public static S operator --(S p)
                    {
                        throw null;
                    }
                }

                void M(S [|p|])
                {
                    --p;
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "The value of parameter 'p' is overwritten in its method body");
    }

    [Fact]
    internal async Task When_struct_parameter_is_post_decremented_in_method_body_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                public struct S
                {
                    public static S operator --(S p)
                    {
                        throw null;
                    }
                }

                void M(S [|p|])
                {
                    p--;
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "The value of parameter 'p' is overwritten in its method body");
    }

    [Fact]
    internal async Task When_struct_parameter_is_overwritten_by_deconstruction_in_method_body_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                class C
                {
                    public void Deconstruct(out int i, out S s) => throw null;
                
                    public struct S
                    {
                    }
                
                    void M(S [|s|], C c)
                    {
                        int i;
                        (i, s) = c;
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "The value of parameter 's' is overwritten in its method body");
    }

    [Fact]
    internal async Task When_struct_parameter_is_passed_by_ref_in_method_body_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                public struct S
                {
                }

                void M(S [|p|])
                {
                    N(ref p);
                }

                void N(ref S s) => throw null;
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "The value of parameter 'p' is overwritten in its method body");
    }

    [Fact]
    internal async Task When_struct_parameter_is_passed_by_out_in_method_body_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                public struct S
                {
                }

                void M(S [|p|])
                {
                    N(out p);
                }

                void N(out S s) => throw null;
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "The value of parameter 'p' is overwritten in its method body");
    }

    [Fact]
    internal async Task When_struct_parameter_is_passed_by_in_in_method_body_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                public struct S
                {
                }

                void M(S p)
                {
                    N(in p);
                }

                void N(in S s) => throw null;
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_ref_struct_parameter_is_reassigned_in_method_body_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                public struct S
                {
                }

                void M(ref S p)
                {
                    p = new S();
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_out_struct_parameter_is_reassigned_in_method_body_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                public struct S
                {
                }

                void M(out S p)
                {
                    p = new S();
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_in_struct_parameter_is_invoked_in_method_body_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                struct S
                {
                    public void M() => throw null;
                }

                void M(in S p)
                {
                    p.M();
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_string_parameter_is_read_from_in_method_body_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                void M(string p)
                {
                    string x = p;
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_string_parameter_is_written_to_in_method_body_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                void M(string [|p|])
                {
                    p += "X";
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "The value of parameter 'p' is overwritten in its method body");
    }

    [Fact]
    internal async Task When_ref_string_parameter_is_written_to_in_method_body_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                void M(ref string p)
                {
                    p += "X";
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_out_string_parameter_is_written_to_in_method_body_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                void M(out string p)
                {
                    p = "X";
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_in_string_parameter_is_invoked_in_method_body_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                void M(in string p)
                {
                    p.ToString();
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_string_parameter_is_read_from_in_method_expression_body_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                void M(string p) => N(p);

                void N(string s)
                {
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_string_parameter_is_written_to_in_method_expression_body_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                void M(string [|p|]) => N(ref p);

                void N(ref string s)
                {
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "The value of parameter 'p' is overwritten in its method body");
    }

    [Fact]
    internal async Task When_string_parameter_is_read_from_in_lambda_expression_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                void M()
                {
                    N(p =>
                    {
                        Console.WriteLine(p);
                        return true;
                    });
                }

                void N(Func<string, bool> f)
                {
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_string_parameter_is_written_to_in_lambda_expression_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                void M()
                {
                    N([|p|] =>
                    {
                        p = "A";
                        return true;
                    });
                }

                void N(Func<string, bool> f)
                {
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "The value of parameter 'p' is overwritten in its method body");
    }

    [Fact]
    internal async Task When_string_parameter_is_read_from_in_anonymous_method_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                void M()
                {
                    N(delegate(string p)
                    {
                        Console.WriteLine(p);
                        return true;
                    });
                }

                void N(Func<string, bool> f)
                {
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_string_parameter_is_written_to_in_anonymous_method_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                void M()
                {
                    N(delegate(string [|p|])
                    {
                        p = "A";
                        return true;
                    });
                }

                void N(Func<string, bool> f)
                {
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "The value of parameter 'p' is overwritten in its method body");
    }

    [Fact]
    internal async Task When_string_parameter_is_read_from_in_constructor_body_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                class C
                {
                    public C(string p)
                    {
                        string s = p;
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_string_parameter_is_written_to_in_constructor_body_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                class C
                {
                    public C(string [|p|])
                    {
                        p += "X";
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "The value of parameter 'p' is overwritten in its method body");
    }

    [Fact]
    internal async Task When_string_parameter_is_read_from_in_constructor_expression_body_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                class C
                {
                    public C(string p) => N(p);
                
                    void N(string s)
                    {
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_string_parameter_is_written_to_in_constructor_expression_body_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                class C
                {
                    public C(string [|p|]) => N(ref p);
                
                    void N(ref string s)
                    {
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "The value of parameter 'p' is overwritten in its method body");
    }

    [Fact]
    internal async Task When_string_parameter_is_read_from_in_conversion_operator_body_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                class C
                {
                    int Value;
                
                    public static implicit operator int(C p)
                    {
                        return p.Value;
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_string_parameter_is_written_to_in_conversion_operator_body_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                class C
                {
                    int Value;
                
                    public static implicit operator int(C [|p|])
                    {
                        p = new C();
                        throw null;
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "The value of parameter 'p' is overwritten in its method body");
    }

    [Fact]
    internal async Task When_string_parameter_is_read_from_in_conversion_operator_expression_body_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                class C
                {
                    int Value;
                
                    public static implicit operator int(C p) => p.Value;
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_string_parameter_is_written_to_in_conversion_operator_expression_body_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                class C
                {
                    int Value;
                
                    public static implicit operator int(C [|p|]) => M(ref p);
                
                    static int M(ref C p) => throw null;
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "The value of parameter 'p' is overwritten in its method body");
    }

    [Fact]
    internal async Task When_value_parameter_is_read_from_in_property_setter_body_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                public int Value
                {
                    set
                    {
                        var str = value.ToString();
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_value_parameter_is_written_to_in_property_setter_body_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                public int Value
                {
                    [|set|]
                    {
                        value += 3;
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "The value of parameter 'value' is overwritten in its method body");
    }

    [Fact]
    internal async Task When_value_parameter_is_read_from_in_property_setter_expression_body_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                public int Value
                {
                    set => value.ToString();
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_value_parameter_is_written_to_in_property_setter_expression_body_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                public int Value
                {
                    [|set|] => Math.Min(value += 4, 1);
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "The value of parameter 'value' is overwritten in its method body");
    }

    [Fact]
    internal async Task When_string_parameter_is_read_from_in_indexer_getter_setter_body_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                public int this[string key]
                {
                    get
                    {
                        return int.Parse(key);
                    }
                    set
                    {
                        string copy = key;
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_string_parameter_is_written_to_in_indexer_getter_setter_body_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                public int this[string [|key|]]
                {
                    get
                    {
                        key = string.Empty;
                        throw null;
                    }
                    set
                    {
                        key += new string('x', 10);
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "The value of parameter 'key' is overwritten in its method body");
    }

    [Fact]
    internal async Task When_string_parameters_are_written_to_in_indexer_getter_setter_body_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                public int this[string [|key1|], string [|key2|]]
                {
                    get
                    {
                        key1 = string.Empty;
                        throw null;
                    }
                    set
                    {
                        key2 += new string('x', 10);
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "The value of parameter 'key1' is overwritten in its method body",
            "The value of parameter 'key2' is overwritten in its method body");
    }

    [Fact]
    internal async Task When_string_parameter_is_read_from_in_indexer_getter_setter_expression_body_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                public int this[string key]
                {
                    get => int.Parse(key);
                    set => key.GetHashCode();
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_string_parameter_is_written_to_in_indexer_getter_setter_expression_body_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                public int this[string [|key|]]
                {
                    get => int.Parse(key = string.Empty);
                    set => int.Parse(key = string.Empty);
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "The value of parameter 'key' is overwritten in its method body");
    }

    [Fact]
    internal async Task When_string_parameter_is_read_from_in_event_adder_remover_body_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                public event EventHandler Started
                {
                    add
                    {
                        var x = value;
                    }
                    remove
                    {
                        var x = value;
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_string_parameter_is_written_to_in_event_adder_remover_body_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                public event EventHandler Started
                {
                    [|add|]
                    {
                        value = null;
                        throw null;
                    }
                    [|remove|]
                    {
                        value = null;
                        throw null;
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "The value of parameter 'value' is overwritten in its method body",
            "The value of parameter 'value' is overwritten in its method body");
    }

    [Fact]
    internal async Task When_string_parameter_is_read_from_in_event_adder_remover_expression_body_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                public event EventHandler Started
                {
                    add => value.ToString();
                    remove => value.ToString();
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_string_parameter_is_written_to_in_event_adder_remover_expression_body_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                public event EventHandler Started
                {
                    [|add|] => value = null;
                    [|remove|] => value = null;
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "The value of parameter 'value' is overwritten in its method body",
            "The value of parameter 'value' is overwritten in its method body");
    }

    [Fact]
    internal async Task When_string_parameter_is_read_from_in_local_function_body_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                void M()
                {
                    void L(string p)
                    {
                        string x = p;
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_string_parameter_is_written_to_in_local_function_body_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                void M()
                {
                    void L(string [|p|])
                    {
                        p += "X";
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "The value of parameter 'p' is overwritten in its method body");
    }

    [Fact]
    internal async Task When_string_parameter_is_read_from_in_local_function_expression_body_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                void M()
                {
                    void L(string p) => N(p);
                }

                void N(string s)
                {
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_string_parameter_is_written_to_in_local_function_expression_body_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                void M()
                {
                    void L(string [|p|]) => N(ref p);
                }

                void N(ref string s)
                {
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "The value of parameter 'p' is overwritten in its method body");
    }

    protected override DiagnosticAnalyzer CreateAnalyzer()
    {
        return new DoNotAssignToParameterAnalyzer();
    }
}
