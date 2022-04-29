using CSharpGuidelinesAnalyzer.Rules.Naming;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

// @formatter:keep_existing_linebreaks true

namespace CSharpGuidelinesAnalyzer.Test.Specs.Naming;

public sealed class DoNotUseNumberInIdentifierNameSpecs : CSharpGuidelinesAnalysisTestFixture
{
    protected override string DiagnosticId => DoNotUseNumberInIdentifierNameAnalyzer.DiagnosticId;

    [Fact]
    internal async Task When_class_name_contains_no_digits_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class NoDigits
                {
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_class_name_contains_a_digit_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class [|C5|]
                {
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Class 'C5' contains one or more digits in its name");
    }

    [Fact]
    internal async Task When_struct_name_contains_no_digits_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                struct NoDigits
                {
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_struct_name_contains_a_digit_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                struct [|S22|]
                {
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Struct 'S22' contains one or more digits in its name");
    }

    [Fact]
    internal async Task When_enum_name_contains_no_digits_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                enum NoDigits
                {
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_enum_name_contains_a_digit_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                enum [|E3|]
                {
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Enum 'E3' contains one or more digits in its name");
    }

    [Fact]
    internal async Task When_interface_name_contains_no_digits_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                interface INoDigits
                {
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_interface_name_contains_a_digit_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                interface [|I9|]
                {
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Interface 'I9' contains one or more digits in its name");
    }

    [Fact]
    internal async Task When_delegate_name_contains_no_digits_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                delegate void NoDigits();
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_delegate_name_contains_a_digit_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                delegate void [|D5|]();
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Delegate 'D5' contains one or more digits in its name");
    }

    [Fact]
    internal async Task When_field_name_contains_no_digits_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                private int f;
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_field_name_contains_a_digit_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                private int [|f3|];
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Field 'f3' contains one or more digits in its name");
    }

    [Fact]
    internal async Task When_property_name_contains_no_digits_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                public int P { get; } = 123;
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_property_name_contains_a_digit_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                public int [|P6|] { get; } = 123;
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Property 'P6' contains one or more digits in its name");
    }

    [Fact]
    internal async Task When_property_name_contains_no_digits_in_anonymous_type_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                private int B;

                void Method()
                {
                    var instance = new
                    {
                        A = string.Empty,
                        B
                    };
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_property_name_contains_a_digit_in_anonymous_type_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                private int [|B1|];

                void Method()
                {
                    var instance = new
                    {
                        [|A1|] = string.Empty,
                        [|B1|]
                    };
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Field 'B1' contains one or more digits in its name",
            "Property 'A1' contains one or more digits in its name",
            "Property 'B1' contains one or more digits in its name");
    }

    [Fact]
    internal async Task When_inherited_property_name_contains_a_digit_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                public abstract class B
                {
                    public abstract int [|P6|] { get; }
                }

                public class C : B
                {
                    public override int P6 => 123;
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Property 'P6' contains one or more digits in its name");
    }

    [Fact]
    internal async Task When_implemented_property_name_contains_a_digit_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                public interface I
                {
                    int [|P6|] { get; }
                }

                public class C : I
                {
                    public int P6 => 123;
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Property 'P6' contains one or more digits in its name");
    }

    [Fact]
    internal async Task When_event_name_contains_no_digits_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                public event EventHandler ValueChanged;
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_event_name_contains_a_digit_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                public event EventHandler [|Value9Changed|];
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Event 'Value9Changed' contains one or more digits in its name");
    }

    [Fact]
    internal async Task When_method_name_contains_no_digits_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M()
                {
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_name_contains_a_digit_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void [|M6|]()
                {
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'M6' contains one or more digits in its name");
    }

    [Fact]
    internal async Task When_inherited_method_name_contains_a_digit_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                public abstract class B
                {
                    public abstract void [|M6|]();
                }

                public class C : B
                {
                    public override void M6()
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'M6' contains one or more digits in its name");
    }

    [Fact]
    internal async Task When_implemented_method_name_contains_a_digit_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                public interface I
                {
                    void [|M6|]();
                }

                public class C : I
                {
                    public void M6()
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'M6' contains one or more digits in its name");
    }

    [Fact]
    internal async Task When_local_function_name_contains_no_digits_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M()
                {
                    void L()
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_local_function_name_contains_a_digit_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M()
                {
                    void [|L6|]()
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Local function 'L6' contains one or more digits in its name");
    }

    [Fact]
    internal async Task When_method_parameter_name_contains_no_digits_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M(int p)
                {
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_parameter_name_contains_a_digit_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M(int [|p1|])
                {
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Parameter 'p1' contains one or more digits in its name");
    }

    [Fact]
    internal async Task When_lambda_parameter_name_contains_no_digits_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M()
                {
                    Func<int, bool> f = p => true;
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_lambda_parameter_name_contains_a_digit_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M()
                {
                    Func<int, bool> f = [|p1|] => true;
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Parameter 'p1' contains one or more digits in its name");
    }

    [Fact]
    internal async Task When_local_function_parameter_name_contains_no_digits_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M()
                {
                    void L(int p)
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_local_function_parameter_name_contains_a_digit_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M()
                {
                    void L(int [|p1|])
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Parameter 'p1' contains one or more digits in its name");
    }

    [Fact]
    internal async Task When_inherited_parameter_name_contains_a_digit_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                public abstract class B
                {
                    public abstract void M(int [|p7|]);
                }

                public class C : B
                {
                    public override void M(int p7)
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Parameter 'p7' contains one or more digits in its name");
    }

    [Fact]
    internal async Task When_implemented_parameter_name_contains_a_digit_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                public interface I
                {
                    void M(int [|p7|]);
                }

                public class C : I
                {
                    public void M(int p7)
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Parameter 'p7' contains one or more digits in its name");
    }

    [Fact]
    internal async Task When_variable_name_contains_no_digits_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M()
                {
                    string str = ""A"";
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_variable_name_contains_a_digit_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M()
                {
                    string [|str12|] = ""A"";
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Variable 'str12' contains one or more digits in its name");
    }

    [Fact]
    internal async Task When_range_variable_names_contain_no_digits_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .InDefaultClass(@"
                void M()
                {
                        var query =
                            from a in Enumerable.Empty<string>()
                            join b in Enumerable.Empty<string>() on a.GetHashCode() equals b.GetHashCode() into c
                            group c by c.ToString() into d
                            let e = d.GetHashCode()
                            where true
                            let f = e.ToString()
                            select string.Empty;
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_range_variable_names_contain_digits_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .InDefaultClass(@"
                void M()
                {
                        var query =
                            from [|a1|] in Enumerable.Empty<string>()
                            join [|b2|] in Enumerable.Empty<string>() on a1.GetHashCode() equals b2.GetHashCode() into [|c3|]
                            group c3 by c3.ToString() into [|d4|]
                            let [|e5|] = d4.GetHashCode()
                            where true
                            let [|f6|] = e5.ToString()
                            select string.Empty;
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Range variable 'a1' contains one or more digits in its name",
            "Range variable 'b2' contains one or more digits in its name",
            "Range variable 'c3' contains one or more digits in its name",
            "Range variable 'd4' contains one or more digits in its name",
            "Range variable 'e5' contains one or more digits in its name",
            "Range variable 'f6' contains one or more digits in its name");
    }

    [Fact]
    internal async Task When_tuple_element_names_contain_no_digits_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .InDefaultClass(@"
                void M()
                {
                    (var aa, var bb) = GetTuple();
                    (int cc, string dd) tt = GetTuple();
                    var (ee, ff) = GetTuple();
                }

                (int, string) GetTuple() => throw null;
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_tuple_element_names_contain_digits_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .InDefaultClass(@"
                void M((int [|p1|], int [|p2|]) pp)
                {
                    (var [|e1|], var [|e2|]) = GetTuple();
                    (int [|e3|], string [|e4|]) tt = GetTuple();
                    var ([|e5|], [|e6|]) = GetTuple();

                    int [|e7|];
                    string [|e8|];

                    (e7, e8) = GetTuple();
                    
                    (int [|l1|], string [|l2|]) LocalGetTuple() => throw null;
                }

                (int [|m1|], string [|m2|]) GetTuple() => throw null;
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Tuple element 'p1' contains one or more digits in its name",
            "Tuple element 'p2' contains one or more digits in its name",
            "Tuple element 'e1' contains one or more digits in its name",
            "Tuple element 'e2' contains one or more digits in its name",
            "Tuple element 'e3' contains one or more digits in its name",
            "Tuple element 'e4' contains one or more digits in its name",
            "Tuple element 'e5' contains one or more digits in its name",
            "Tuple element 'e6' contains one or more digits in its name",
            "Variable 'e7' contains one or more digits in its name",
            "Variable 'e8' contains one or more digits in its name",
            "Tuple element 'l1' contains one or more digits in its name",
            "Tuple element 'l2' contains one or more digits in its name",
            "Tuple element 'm1' contains one or more digits in its name",
            "Tuple element 'm2' contains one or more digits in its name");
    }

    [Fact]
    internal async Task When_inferred_tuple_element_names_contain_digits_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .InDefaultClass(@"
                void Method()
                {
                    var anon = new
                    {
                        [|A1|] = string.Empty,
                        [|B1|] = 1
                    };

                    var tuple = ([|anon.A1|], [|anon.B1|]);
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Property 'A1' contains one or more digits in its name",
            "Property 'B1' contains one or more digits in its name",
            "Tuple element 'A1' contains one or more digits in its name",
            "Tuple element 'B1' contains one or more digits in its name");
    }

    [Fact]
    internal async Task When_MSTest_method_name_contains_a_digit_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                namespace Microsoft.VisualStudio.TestTools.UnitTesting
                {
                    public class TestMethodAttribute : Attribute
                    {
                    }
                }

                namespace App
                {
                    using Microsoft.VisualStudio.TestTools.UnitTesting;

                    class UnitTests
                    {
                        [TestMethod]
                        void When_running_10_times_it_must_work()
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
    internal async Task When_XUnit_method_name_contains_a_digit_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                namespace Xunit
                {
                    public class FactAttribute : Attribute
                    {
                    }
                }

                namespace App
                {
                    using Xunit;

                    class UnitTests
                    {
                        [Fact]
                        void When_running_10_times_it_must_work()
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
    internal async Task When_NUnit_method_name_contains_a_digit_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                namespace NUnit.Framework
                {
                    public class TestAttribute : Attribute
                    {
                    }
                }

                namespace App
                {
                    using NUnit.Framework;

                    class UnitTests
                    {
                        [Test]
                        void When_running_10_times_it_must_work()
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
    internal async Task When_MbUnit_method_name_contains_a_digit_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                namespace MbUnit.Framework
                {
                    public class TestAttribute : Attribute
                    {
                    }
                }

                namespace App
                {
                    using MbUnit.Framework;

                    class UnitTests
                    {
                        [Test]
                        void When_running_10_times_it_must_work()
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
    internal async Task When_method_name_contains_only_whitelisted_words_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void Int16SaveInt32Or3DIntoInt64WithUtf7InWin32()
                {
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_name_contains_whitelisted_words_and_digits_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void [|ConvertDigit9IntoInt32|]()
                {
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'ConvertDigit9IntoInt32' contains one or more digits in its name");
    }

    [Fact]
    internal async Task When_method_name_contains_part_of_whitelisted_word_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void [|GetMatrix3Demo|]()
                {
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'GetMatrix3Demo' contains one or more digits in its name");
    }

    protected override DiagnosticAnalyzer CreateAnalyzer()
    {
        return new DoNotUseNumberInIdentifierNameAnalyzer();
    }
}
