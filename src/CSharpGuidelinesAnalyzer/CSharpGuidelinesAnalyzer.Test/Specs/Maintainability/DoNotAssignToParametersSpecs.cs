using CSharpGuidelinesAnalyzer.Rules.Maintainability;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.Maintainability
{
    public sealed class DoNotAssignToParametersSpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => DoNotAssignToParametersAnalyzer.DiagnosticId;

        [Fact]
        internal void When_method_is_abstract_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public abstract class C
                    {
                        public abstract void M(string p);
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_is_partial_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    partial class C
                    {
                        partial void M(string p);
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_string_parameter_is_read_from_in_method_body_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M(string p)
                    {
                        string x = p;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_string_parameter_is_written_to_in_method_body_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M(string [|p|])
                    {
                        p += ""X"";
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "The value of parameter 'p' is overwritten in its method body.");
        }

        [Fact]
        internal void When_bool_parameter_is_read_from_in_method_body_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M(bool p)
                    {
                        var x = p.ToString(null);
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_bool_parameter_is_written_to_in_method_body_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M(bool [|p|])
                    {
                        p = true;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "The value of parameter 'p' is overwritten in its method body.");
        }

        [Fact]
        internal void When_int_parameter_is_read_from_in_method_body_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M(int p)
                    {
                        var x = p.GetHashCode();
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_int_parameter_is_written_to_in_method_body_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M(int [|p|])
                    {
                        p += 5;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "The value of parameter 'p' is overwritten in its method body.");
        }

        [Fact]
        internal void When_unsigned_short_parameter_is_read_from_in_method_body_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M(ushort p)
                    {
                        var x = p.GetHashCode();
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_unsigned_short_parameter_is_written_to_in_method_body_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M(ushort [|p|])
                    {
                        p += 5;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "The value of parameter 'p' is overwritten in its method body.");
        }

        [Fact]
        internal void When_decimal_parameter_is_read_from_in_method_body_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M(decimal p)
                    {
                        var x = p.ToString(null, null);
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_decimal_parameter_is_written_to_in_method_body_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M(decimal [|p|])
                    {
                        p += 5m;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "The value of parameter 'p' is overwritten in its method body.");
        }

        [Fact]
        internal void When_double_parameter_is_read_from_in_method_body_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M(double p)
                    {
                        var x = p.ToString(null, null);
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_double_parameter_is_written_to_in_method_body_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M(double [|p|])
                    {
                        p += 5.0;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "The value of parameter 'p' is overwritten in its method body.");
        }

        [Fact]
        internal void When_DateTime_parameter_is_read_from_in_method_body_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M(DateTime p)
                    {
                        var x = p.Year;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_DateTime_parameter_is_written_to_in_method_body_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M(DateTime [|p|])
                    {
                        p = DateTime.UtcNow;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "The value of parameter 'p' is overwritten in its method body.");
        }

        [Fact]
        internal void When_nullable_parameter_is_read_from_in_method_body_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M(int? p)
                    {
                        var x = p.Value.ToString(null, null);
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_nullable_parameter_is_written_to_in_method_body_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M(int? [|p|])
                    {
                        p += 5;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "The value of parameter 'p' is overwritten in its method body.");
        }

        [Fact]
        internal void When_enum_parameter_is_read_from_in_method_body_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    public enum E
                    {
                        A, B
                    }

                    void M(E p)
                    {
                        var x = p.HasFlag(E.A);
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_enum_parameter_is_written_to_in_method_body_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    public enum E
                    {
                        A, B
                    }

                    void M(E [|p|])
                    {
                        p = E.B;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "The value of parameter 'p' is overwritten in its method body.");
        }

        [Fact]
        internal void When_struct_parameter_is_read_from_in_method_body_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    public struct S
                    {
                        public int Value { get; set; }
                    }

                    void M(S p)
                    {
                        var x = p.Value;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_struct_parameter_is_invoked_on_in_method_body_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    public struct S
                    {
                        public void M()
                        {
                        }
                    }

                    class C
                    {
                        public C(S s)
                        {
                            s.M();
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact(Skip = "https://github.com/bkoelman/CSharpGuidelinesAnalyzer/issues/85")]
        public void When_struct_parameter_is_written_to_in_method_body_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    public struct S
                    {
                    }

                    void M(S [|p|])
                    {
                        p = new S();
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "The value of parameter 'p' is overwritten in its method body.");
        }

        [Fact]
        internal void When_ref_parameter_is_written_to_in_method_body_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M(ref string p)
                    {
                        p += ""X"";
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_out_parameter_is_written_to_in_method_body_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M(out string p)
                    {
                        p = ""X"";
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_parameter_is_read_from_in_method_expression_body_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M(string p) => N(p);

                    void N(string s)
                    {
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_parameter_is_written_to_in_method_expression_body_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M(string [|p|]) => N(ref p);

                    void N(ref string s)
                    {
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "The value of parameter 'p' is overwritten in its method body.");
        }

        [Fact]
        internal void When_parameter_is_read_from_in_lambda_expression_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
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
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_parameter_is_written_to_in_lambda_expression_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        N([|p|] => 
                        {
                            p = ""A"";
                            return true;
                        });
                    }

                    void N(Func<string, bool> f)
                    {
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "The value of parameter 'p' is overwritten in its method body.");
        }

        [Fact]
        internal void When_string_parameter_is_read_from_in_constructor_body_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        public C(string p)
                        {
                            string s = p;
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_string_parameter_is_written_to_in_constructor_body_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        public C(string [|p|])
                        {
                            p += ""X"";
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "The value of parameter 'p' is overwritten in its method body.");
        }

        [Fact]
        internal void When_ref_parameter_is_written_to_in_constructor_body_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        public C(ref string p)
                        {
                            p += ""X"";
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_out_parameter_is_written_to_in_constructor_body_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    class C
                    {
                        public C(ref string p)
                        {
                            p = ""X"";
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new DoNotAssignToParametersAnalyzer();
        }
    }
}