using CSharpGuidelinesAnalyzer.Rules.Naming;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

// @formatter:keep_existing_linebreaks true

namespace CSharpGuidelinesAnalyzer.Test.Specs.Naming;

public sealed class DoNotUseAbbreviationInIdentifierNameSpecs : CSharpGuidelinesAnalysisTestFixture
{
    protected override string DiagnosticId => DoNotUseAbbreviationInIdentifierNameAnalyzer.DiagnosticId;

    [Fact]
    internal async Task When_class_name_contains_no_abbreviation_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                class OkButtonContainer
                {
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_class_name_contains_an_abbreviation_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                class [|OkBtnContainer|]
                {
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Class 'OkBtnContainer' should have a more descriptive name");
    }

    [Fact]
    internal async Task When_class_name_consists_of_single_letter_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                class [|C|]
                {
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Class 'C' should have a more descriptive name");
    }

    [Fact]
    internal async Task When_struct_name_contains_no_abbreviation_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                struct OkButtonContainer
                {
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_struct_name_contains_an_abbreviation_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                struct [|OkBtnContainer|]
                {
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Struct 'OkBtnContainer' should have a more descriptive name");
    }

    [Fact]
    internal async Task When_struct_name_consists_of_single_letter_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                struct [|S|]
                {
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Struct 'S' should have a more descriptive name");
    }

    [Fact]
    internal async Task When_enum_name_contains_no_abbreviation_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                enum NextButtonAction
                {
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_enum_name_contains_an_abbreviation_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                enum [|NextBtnAction|]
                {
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Enum 'NextBtnAction' should have a more descriptive name");
    }

    [Fact]
    internal async Task When_enum_name_consists_of_single_letter_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                enum [|E|]
                {
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Enum 'E' should have a more descriptive name");
    }

    [Fact]
    internal async Task When_interface_name_contains_no_abbreviation_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                interface IOkButtonContainer
                {
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_interface_name_contains_an_abbreviation_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                interface [|IOkBtnContainer|]
                {
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Interface 'IOkBtnContainer' should have a more descriptive name");
    }

    [Fact]
    internal async Task When_interface_name_consists_of_single_letter_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                interface [|I|]
                {
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Interface 'I' should have a more descriptive name");
    }

    [Fact]
    internal async Task When_delegate_name_contains_no_abbreviation_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("delegate void ButtonClick();")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_delegate_name_contains_an_abbreviation_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("delegate void [|BtnClick|]();")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Delegate 'BtnClick' should have a more descriptive name");
    }

    [Fact]
    internal async Task When_delegate_name_consists_of_single_letter_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("delegate void [|D|]();")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Delegate 'D' should have a more descriptive name");
    }

    [Fact]
    internal async Task When_field_name_contains_no_abbreviation_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("private int some;")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_field_name_contains_an_abbreviation_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("private int [|txtData|];")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Field 'txtData' should have a more descriptive name");
    }

    [Fact]
    internal async Task When_field_name_consists_of_single_letter_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("private int [|i|];")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Field 'i' should have a more descriptive name");
    }

    [Fact]
    internal async Task When_property_name_contains_no_abbreviation_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("public int Some { get; } = 123;")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_property_name_contains_an_abbreviation_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("public bool [|ChkActive|] { get; } = true;")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Property 'ChkActive' should have a more descriptive name");
    }

    [Fact]
    internal async Task When_property_name_consists_of_single_letter_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("public bool [|X|] { get; } = true;")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Property 'X' should have a more descriptive name");
    }

    [Fact]
    internal async Task When_property_name_consists_of_single_letter_in_anonymous_type_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                private int [|B|];

                void Method()
                {
                    var instance = new
                    {
                        [|A|] = string.Empty,
                        [|B|]
                    };
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Field 'B' should have a more descriptive name",
            "Property 'A' should have a more descriptive name",
            "Property 'B' should have a more descriptive name");
    }

    [Fact]
    internal async Task When_inherited_property_name_contains_an_abbreviation_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                public abstract class Bb
                {
                    public abstract int [|CmbItemCount|] { get; }
                }

                public class Cc : Bb
                {
                    public override int CmbItemCount => 123;
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Property 'CmbItemCount' should have a more descriptive name");
    }

    [Fact]
    internal async Task When_inherited_property_name_consists_of_single_letter_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                public abstract class Bb
                {
                    public abstract int [|X|] { get; }
                }

                public class Cc : Bb
                {
                    public override int X => 123;
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Property 'X' should have a more descriptive name");
    }

    [Fact]
    internal async Task When_implemented_property_name_contains_an_abbreviation_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                public interface Ii
                {
                    int [|PrgValue|] { get; }
                }

                public class Cc : Ii
                {
                    public int PrgValue => 123;
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Property 'PrgValue' should have a more descriptive name");
    }

    [Fact]
    internal async Task When_implemented_property_name_consists_of_single_letter_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                public interface Ii
                {
                    int [|P|] { get; }
                }

                public class Cc : Ii
                {
                    public int P => 123;
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Property 'P' should have a more descriptive name");
    }

    [Fact]
    internal async Task When_event_name_contains_no_abbreviation_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("public event EventHandler ValueChanged;")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_event_name_contains_an_abbreviation_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("public event EventHandler [|TxtChanged|];")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Event 'TxtChanged' should have a more descriptive name");
    }

    [Fact]
    internal async Task When_event_name_consists_of_single_letter_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("public event EventHandler [|E|];")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Event 'E' should have a more descriptive name");
    }

    [Fact]
    internal async Task When_method_name_contains_no_abbreviation_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                void More()
                {
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_name_contains_an_abbreviation_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                void [|MakeRptVisible|]()
                {
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'MakeRptVisible' should have a more descriptive name");
    }

    [Fact]
    internal async Task When_method_name_consists_of_single_letter_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                void [|M|]()
                {
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'M' should have a more descriptive name");
    }

    [Fact]
    internal async Task When_inherited_method_name_contains_an_abbreviation_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                public abstract class Bb
                {
                    public abstract void [|MakeRptVisible|]();
                }

                public class Cc : Bb
                {
                    public override void MakeRptVisible()
                    {
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'MakeRptVisible' should have a more descriptive name");
    }

    [Fact]
    internal async Task When_inherited_method_name_consists_of_single_letter_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                public abstract class Bb
                {
                    public abstract void [|M|]();
                }

                public class Cc : Bb
                {
                    public override void M()
                    {
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'M' should have a more descriptive name");
    }

    [Fact]
    internal async Task When_implemented_method_name_contains_an_abbreviation_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                public interface Ii
                {
                    void [|FldCount|]();
                }

                public class Cc : Ii
                {
                    public void FldCount()
                    {
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'FldCount' should have a more descriptive name");
    }

    [Fact]
    internal async Task When_implemented_method_name_consists_of_single_letter_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                public interface Ii
                {
                    void [|M|]();
                }

                public class Cc : Ii
                {
                    public void M()
                    {
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'M' should have a more descriptive name");
    }

    [Fact]
    internal async Task When_local_function_name_contains_no_abbreviation_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                void Some()
                {
                    void More()
                    {
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_local_function_name_contains_an_abbreviation_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                void Some()
                {
                    void [|MakeRptVisible|]()
                    {
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Local function 'MakeRptVisible' should have a more descriptive name");
    }

    [Fact]
    internal async Task When_local_function_name_consists_of_single_letter_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                void Some()
                {
                    void [|L|]()
                    {
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Local function 'L' should have a more descriptive name");
    }

    [Fact]
    internal async Task When_method_parameter_name_contains_no_abbreviation_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                void Method(int someParameter)
                {
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_parameter_name_contains_an_abbreviation_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                void Method(object [|tvHistory|])
                {
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Parameter 'tvHistory' should have a more descriptive name");
    }

    [Fact]
    internal async Task When_method_parameter_name_consists_of_single_letter_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                void Method(object [|x|])
                {
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Parameter 'x' should have a more descriptive name");
    }

    [Fact]
    internal async Task When_lambda_parameter_name_contains_no_abbreviation_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                void Method()
                {
                    Func<int, bool> func = someParameter => true;
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_lambda_parameter_name_contains_an_abbreviation_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                void Method()
                {
                    Func<int, bool> func = [|chkAgree|] => true;
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Parameter 'chkAgree' should have a more descriptive name");
    }

    [Fact]
    internal async Task When_lambda_parameter_name_consists_of_single_letter_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                void Method()
                {
                    Func<int, bool> func = [|x|] => true;
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Parameter 'x' should have a more descriptive name");
    }

    [Fact]
    internal async Task When_inherited_parameter_name_contains_an_abbreviation_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                public abstract class Bb
                {
                    public abstract void Method(int [|btnCount|]);
                }

                public class Cc : Bb
                {
                    public override void Method(int btnCount)
                    {
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Parameter 'btnCount' should have a more descriptive name");
    }

    [Fact]
    internal async Task When_inherited_parameter_name_consists_of_single_letter_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                public abstract class Bb
                {
                    public abstract void Method(int [|x|]);
                }

                public class Cc : Bb
                {
                    public override void Method(int x)
                    {
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Parameter 'x' should have a more descriptive name");
    }

    [Fact]
    internal async Task When_implemented_parameter_name_contains_an_abbreviation_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                public interface Ii
                {
                    void Method(int [|btnCount|]);
                }

                public class Cc : Ii
                {
                    public void Method(int btnCount)
                    {
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Parameter 'btnCount' should have a more descriptive name");
    }

    [Fact]
    internal async Task When_implemented_parameter_name_consists_of_single_letter_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                public interface Ii
                {
                    void Method(int [|x|]);
                }

                public class Cc : Ii
                {
                    public void Method(int x)
                    {
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Parameter 'x' should have a more descriptive name");
    }

    [Fact]
    internal async Task When_local_function_parameter_name_contains_no_abbreviation_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                void Method()
                {
                    void LocalFunction(int someParameter)
                    {
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_local_function_parameter_name_contains_an_abbreviation_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                void Method()
                {
                    void LocalFunction(object [|tvHistory|])
                    {
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Parameter 'tvHistory' should have a more descriptive name");
    }

    [Fact]
    internal async Task When_local_function_parameter_name_consists_of_single_letter_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                void Method()
                {
                    void LocalFunction(object [|x|])
                    {
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Parameter 'x' should have a more descriptive name");
    }

    [Fact]
    internal async Task When_variable_name_contains_no_abbreviation_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                void Method()
                {
                    string lineString = "A";
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_variable_name_contains_an_abbreviation_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                void Method()
                {
                    string [|str|] = "A";
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Variable 'str' should have a more descriptive name");
    }

    [Fact]
    internal async Task When_variable_name_consists_of_single_letter_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                void Method()
                {
                    string [|s|] = "A";
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Variable 's' should have a more descriptive name");
    }

    [Fact]
    internal async Task When_variable_name_consists_of_single_underscore_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                void Method()
                {
                    string _ = "A";
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_range_variable_names_consist_of_single_letter_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .InDefaultClass("""
                void Method()
                {
                    var query =
                        from [|a|] in Enumerable.Empty<string>()
                        join [|b|] in Enumerable.Empty<string>() on a.GetHashCode() equals b.GetHashCode() into [|c|]
                        group c by c.ToString() into [|d|]
                        let [|e|] = d.GetHashCode()
                        where true
                        let [|f|] = e.ToString()
                        select string.Empty;
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Range variable 'a' should have a more descriptive name",
            "Range variable 'b' should have a more descriptive name",
            "Range variable 'c' should have a more descriptive name",
            "Range variable 'd' should have a more descriptive name",
            "Range variable 'e' should have a more descriptive name",
            "Range variable 'f' should have a more descriptive name");
    }

    [Fact]
    internal async Task When_tuple_element_names_consist_of_single_letter_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .InDefaultClass("""
                void Method((int [|p|], int [|q|]) pp)
                {
                    (var [|a|], var [|b|]) = GetTuple();
                    (int [|c|], string [|d|]) tt = GetTuple();
                    var ([|e|], [|f|]) = GetTuple();

                    int [|g|];
                    string [|h|];

                    (g, h) = GetTuple();

                    (int [|l|], string [|m|]) LocalGetTuple() => throw null;
                }

                (int [|x|], string [|y|]) GetTuple() => throw null;
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Tuple element 'p' should have a more descriptive name",
            "Tuple element 'q' should have a more descriptive name",
            "Tuple element 'a' should have a more descriptive name",
            "Tuple element 'b' should have a more descriptive name",
            "Tuple element 'c' should have a more descriptive name",
            "Tuple element 'd' should have a more descriptive name",
            "Tuple element 'e' should have a more descriptive name",
            "Tuple element 'f' should have a more descriptive name",
            "Variable 'g' should have a more descriptive name",
            "Variable 'h' should have a more descriptive name",
            "Tuple element 'l' should have a more descriptive name",
            "Tuple element 'm' should have a more descriptive name",
            "Tuple element 'x' should have a more descriptive name",
            "Tuple element 'y' should have a more descriptive name");
    }

    [Fact]
    internal async Task When_inferred_tuple_element_names_consist_of_single_letter_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .InDefaultClass("""
                void Method()
                {
                    var anon = new
                    {
                        [|A|] = string.Empty,
                        [|B|] = 1
                    };

                    var tuple = ([|anon.A|], [|anon.B|]);
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Property 'A' should have a more descriptive name",
            "Property 'B' should have a more descriptive name",
            "Tuple element 'A' should have a more descriptive name",
            "Tuple element 'B' should have a more descriptive name");
    }

    [Fact]
    internal async Task When_variable_name_contains_an_abbreviation_in_catch_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass("""
                void Method()
                {
                    try
                    {
                    }
                    catch (Exception [|ex|])
                    {
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Variable 'ex' should have a more descriptive name");
    }

    protected override DiagnosticAnalyzer CreateAnalyzer()
    {
        return new DoNotUseAbbreviationInIdentifierNameAnalyzer();
    }
}
