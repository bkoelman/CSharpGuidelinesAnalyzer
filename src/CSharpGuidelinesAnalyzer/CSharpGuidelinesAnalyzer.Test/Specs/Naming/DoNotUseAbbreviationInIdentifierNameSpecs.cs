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
        internal void When_field_name_contains_no_abbreviation_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        private int some;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_field_name_contains_an_abbreviation_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        private int [|txtData|];
                    }
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
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        private int [|i|];
                    }
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
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        public int Some { get; } = 123;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_property_name_contains_an_abbreviation_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        public bool [|ChkActive|] { get; } = true;
                    }
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
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        public bool [|X|] { get; } = true;
                    }
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
                    public abstract class B
                    {
                        public abstract int [|CmbItemCount|] { get; }
                    }

                    public class C : B
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
                    public abstract class B
                    {
                        public abstract int [|X|] { get; }
                    }

                    public class C : B
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
                    public interface I
                    {
                        int [|PrgValue|] { get; }
                    }

                    public class C : I
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
                    public interface I
                    {
                        int [|P|] { get; }
                    }

                    public class C : I
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
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        public event EventHandler ValueChanged;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_event_name_contains_an_abbreviation_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        public event EventHandler [|TxtChanged|];
                    }
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
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        public event EventHandler [|E|];
                    }
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
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
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
        internal void When_method_name_contains_an_abbreviation_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void [|MakeRptVisible|]()
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
        internal void When_method_name_consists_of_single_letter_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void [|M|]()
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
        internal void When_inherited_method_name_contains_an_abbreviation_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public abstract class B
                    {
                        public abstract void [|MakeRptVisible|]();
                    }

                    public class C : B
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
                    public abstract class B
                    {
                        public abstract void [|M|]();
                    }

                    public class C : B
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
                    public interface I
                    {
                        void [|FldCount|]();
                    }

                    public class C : I
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
                    public interface I
                    {
                        void [|M|]();
                    }

                    public class C : I
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
        internal void When_parameter_name_contains_no_abbreviation_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void Method(int someParameter)
                        {
                        }
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
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void Method(object [|tvHistory|])
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
        internal void When_parameter_name_consists_of_single_letter_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void Method(object [|x|])
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
        internal void When_inherited_parameter_name_contains_an_abbreviation_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public abstract class B
                    {
                        public abstract void Method(int [|btnCount|]);
                    }

                    public class C : B
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
                    public abstract class B
                    {
                        public abstract void Method(int [|x|]);
                    }

                    public class C : B
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
                    public interface I
                    {
                        void Method(int [|btnCount|]);
                    }

                    public class C : I
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
                    public interface I
                    {
                        void Method(int [|x|]);
                    }

                    public class C : I
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
        internal void When_variable_name_contains_no_abbreviation_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void Method()
                        {
                            string lineString = ""A"";
                        }
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
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void Method()
                        {
                            string [|str|] = ""A"";
                        }
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
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void Method()
                        {
                            string [|s|] = ""A"";
                        }
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
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithReference(typeof(Enumerable).Assembly)
                .Using(typeof(Enumerable).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        int[] Method(int[] items)
                        {
                            return items.Where(i => i > 5).ToArray();
                        }
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
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void Method()
                        {
                            string _ = ""A"";
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new DoNotUseAbbreviationInIdentifierNameAnalyzer();
        }
    }
}
