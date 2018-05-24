using System.Linq;
using CSharpGuidelinesAnalyzer.Rules.Naming;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.Naming
{
    public sealed class DoNotUseAbbreviationInIdentifierNameSpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => DoNotUseAbbreviationInIdentifierNameAnalyzer.DiagnosticId;

        [Fact]
        internal void When_class_name_contains_no_abbreviation_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class OkButtonContainer
                    {
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_class_name_contains_an_abbreviation_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class [|OkBtnContainer|]
                    {
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Class 'OkBtnContainer' should have a more descriptive name.");
        }

        [Fact]
        internal void When_class_name_consists_of_single_letter_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class [|C|]
                    {
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Class 'C' should have a more descriptive name.");
        }

        [Fact]
        internal void When_struct_name_contains_no_abbreviation_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    struct OkButtonContainer
                    {
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_struct_name_contains_an_abbreviation_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    struct [|OkBtnContainer|]
                    {
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Struct 'OkBtnContainer' should have a more descriptive name.");
        }

        [Fact]
        internal void When_struct_name_consists_of_single_letter_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    struct [|S|]
                    {
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Struct 'S' should have a more descriptive name.");
        }

        [Fact]
        internal void When_enum_name_contains_no_abbreviation_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    enum NextButtonAction
                    {
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_enum_name_contains_an_abbreviation_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    enum [|NextBtnAction|]
                    {
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Enum 'NextBtnAction' should have a more descriptive name.");
        }

        [Fact]
        internal void When_enum_name_consists_of_single_letter_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    enum [|E|]
                    {
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Enum 'E' should have a more descriptive name.");
        }

        [Fact]
        internal void When_interface_name_contains_no_abbreviation_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    interface IOkButtonContainer
                    {
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_interface_name_contains_an_abbreviation_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    interface [|IOkBtnContainer|]
                    {
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Interface 'IOkBtnContainer' should have a more descriptive name.");
        }

        [Fact]
        internal void When_interface_name_consists_of_single_letter_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    interface [|I|]
                    {
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Interface 'I' should have a more descriptive name.");
        }

        [Fact]
        internal void When_delegate_name_contains_no_abbreviation_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    delegate void ButtonClick();
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_delegate_name_contains_an_abbreviation_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    delegate void [|BtnClick|]();
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Delegate 'BtnClick' should have a more descriptive name.");
        }

        [Fact]
        internal void When_delegate_name_consists_of_single_letter_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    delegate void [|D|]();
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Delegate 'D' should have a more descriptive name.");
        }

        [Fact]
        internal void When_field_name_contains_no_abbreviation_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    private int some;
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_field_name_contains_an_abbreviation_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    private int [|txtData|];
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Field 'txtData' should have a more descriptive name.");
        }

        [Fact]
        internal void When_field_name_consists_of_single_letter_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    private int [|i|];
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Field 'i' should have a more descriptive name.");
        }

        [Fact]
        internal void When_property_name_contains_no_abbreviation_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    public int Some { get; } = 123;
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_property_name_contains_an_abbreviation_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    public bool [|ChkActive|] { get; } = true;
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Property 'ChkActive' should have a more descriptive name.");
        }

        [Fact]
        internal void When_property_name_consists_of_single_letter_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    public bool [|X|] { get; } = true;
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Property 'X' should have a more descriptive name.");
        }

        [Fact]
        internal void When_inherited_property_name_contains_an_abbreviation_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public abstract class Bb
                    {
                        public abstract int [|CmbItemCount|] { get; }
                    }

                    public class Cc : Bb
                    {
                        public override int CmbItemCount => 123;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Property 'CmbItemCount' should have a more descriptive name.");
        }

        [Fact]
        internal void When_inherited_property_name_consists_of_single_letter_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public abstract class Bb
                    {
                        public abstract int [|X|] { get; }
                    }

                    public class Cc : Bb
                    {
                        public override int X => 123;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Property 'X' should have a more descriptive name.");
        }

        [Fact]
        internal void When_implemented_property_name_contains_an_abbreviation_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public interface Ii
                    {
                        int [|PrgValue|] { get; }
                    }

                    public class Cc : Ii
                    {
                        public int PrgValue => 123;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Property 'PrgValue' should have a more descriptive name.");
        }

        [Fact]
        internal void When_implemented_property_name_consists_of_single_letter_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public interface Ii
                    {
                        int [|P|] { get; }
                    }

                    public class Cc : Ii
                    {
                        public int P => 123;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Property 'P' should have a more descriptive name.");
        }

        [Fact]
        internal void When_event_name_contains_no_abbreviation_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    public event EventHandler ValueChanged;
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_event_name_contains_an_abbreviation_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    public event EventHandler [|TxtChanged|];
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Event 'TxtChanged' should have a more descriptive name.");
        }

        [Fact]
        internal void When_event_name_consists_of_single_letter_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    public event EventHandler [|E|];
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Event 'E' should have a more descriptive name.");
        }

        [Fact]
        internal void When_method_name_contains_no_abbreviation_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void More()
                    {
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_name_contains_an_abbreviation_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void [|MakeRptVisible|]()
                    {
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'MakeRptVisible' should have a more descriptive name.");
        }

        [Fact]
        internal void When_method_name_consists_of_single_letter_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void [|M|]()
                    {
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'M' should have a more descriptive name.");
        }

        [Fact]
        internal void When_inherited_method_name_contains_an_abbreviation_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
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
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'MakeRptVisible' should have a more descriptive name.");
        }

        [Fact]
        internal void When_inherited_method_name_consists_of_single_letter_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
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
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'M' should have a more descriptive name.");
        }

        [Fact]
        internal void When_implemented_method_name_contains_an_abbreviation_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
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
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'FldCount' should have a more descriptive name.");
        }

        [Fact]
        internal void When_implemented_method_name_consists_of_single_letter_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
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
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'M' should have a more descriptive name.");
        }

        [Fact]
        internal void When_local_function_name_contains_no_abbreviation_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void Some()
                    {
                        void More()
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_local_function_name_contains_an_abbreviation_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void Some()
                    {
                        void [|MakeRptVisible|]()
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Local function 'MakeRptVisible' should have a more descriptive name.");
        }

        [Fact]
        internal void When_local_function_name_consists_of_single_letter_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void Some()
                    {
                        void [|L|]()
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Local function 'L' should have a more descriptive name.");
        }

        [Fact]
        internal void When_parameter_name_contains_no_abbreviation_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void Method(int someParameter)
                    {
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_parameter_name_contains_an_abbreviation_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void Method(object [|tvHistory|])
                    {
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Parameter 'tvHistory' should have a more descriptive name.");
        }

        [Fact]
        internal void When_parameter_name_consists_of_single_letter_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void Method(object [|x|])
                    {
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Parameter 'x' should have a more descriptive name.");
        }

        [Fact]
        internal void When_inherited_parameter_name_contains_an_abbreviation_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
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
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Parameter 'btnCount' should have a more descriptive name.");
        }

        [Fact]
        internal void When_inherited_parameter_name_consists_of_single_letter_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
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
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Parameter 'x' should have a more descriptive name.");
        }

        [Fact]
        internal void When_implemented_parameter_name_contains_an_abbreviation_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
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
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Parameter 'btnCount' should have a more descriptive name.");
        }

        [Fact]
        internal void When_implemented_parameter_name_consists_of_single_letter_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
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
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Parameter 'x' should have a more descriptive name.");
        }

        [Fact]
        internal void When_local_function_parameter_name_contains_no_abbreviation_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void Method()
                    {
                        void LocalFunction(int someParameter)
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_local_function_parameter_name_contains_an_abbreviation_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void Method()
                    {
                        void LocalFunction(object [|tvHistory|])
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Parameter 'tvHistory' should have a more descriptive name.");
        }

        [Fact]
        internal void When_local_function_parameter_name_consists_of_single_letter_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void Method()
                    {
                        void LocalFunction(object [|x|])
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Parameter 'x' should have a more descriptive name.");
        }

        [Fact]
        internal void When_variable_name_contains_no_abbreviation_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void Method()
                    {
                        string lineString = ""A"";
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_variable_name_contains_an_abbreviation_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void Method()
                    {
                        string [|str|] = ""A"";
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Variable 'str' should have a more descriptive name.");
        }

        [Fact]
        internal void When_variable_name_consists_of_single_letter_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void Method()
                    {
                        string [|s|] = ""A"";
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Variable 's' should have a more descriptive name.");
        }

        [Fact]
        internal void When_variable_name_consists_of_single_letter_inside_lambda_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .WithReference(typeof(Enumerable).Assembly)
                .Using(typeof(Enumerable).Namespace)
                .InDefaultClass(@"
                    int[] Method(int[] items)
                    {
                        return items.Where(i => i > 5).ToArray();
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_variable_name_consists_of_single_underscore_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void Method()
                    {
                        string _ = ""A"";
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_range_variable_name_consists_of_single_letter_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .Using(typeof(Enumerable).Namespace)
                .InDefaultClass(@"
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
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Range variable 'a' should have a more descriptive name.",
                "Range variable 'b' should have a more descriptive name.",
                "Range variable 'c' should have a more descriptive name.",
                "Range variable 'd' should have a more descriptive name.",
                "Range variable 'e' should have a more descriptive name.",
                "Range variable 'f' should have a more descriptive name.");
        }

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new DoNotUseAbbreviationInIdentifierNameAnalyzer();
        }
    }
}
