using CSharpGuidelinesAnalyzer.Rules.Naming;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.Naming
{
    public sealed class DoNotUseNumbersInIdentifiersSpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => DoNotUseNumbersInIdentifiersAnalyzer.DiagnosticId;

        [Fact]
        internal void When_class_name_contains_no_digits_it_must_be_skipped()
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
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_class_name_contains_a_digit_it_must_be_reported()
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
            VerifyGuidelineDiagnostic(source,
                "Class 'C5' contains one or more digits in its name.");
        }

        [Fact]
        internal void When_struct_name_contains_no_digits_it_must_be_skipped()
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
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_struct_name_contains_a_digit_it_must_be_reported()
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
            VerifyGuidelineDiagnostic(source,
                "Struct 'S22' contains one or more digits in its name.");
        }

        [Fact]
        internal void When_enum_name_contains_no_digits_it_must_be_skipped()
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
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_enum_name_contains_a_digit_it_must_be_reported()
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
            VerifyGuidelineDiagnostic(source,
                "Enum 'E3' contains one or more digits in its name.");
        }

        [Fact]
        internal void When_interface_name_contains_no_digits_it_must_be_skipped()
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
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_interface_name_contains_a_digit_it_must_be_reported()
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
            VerifyGuidelineDiagnostic(source,
                "Interface 'I9' contains one or more digits in its name.");
        }

        [Fact]
        internal void When_field_name_contains_no_digits_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        private int f;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_field_name_contains_a_digit_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        private int [|f3|];
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Field 'f3' contains one or more digits in its name.");
        }

        [Fact]
        internal void When_property_name_contains_no_digits_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        public int P { get; } = 123;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_property_name_contains_a_digit_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        public int [|P6|] { get; } = 123;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Property 'P6' contains one or more digits in its name.");
        }

        [Fact]
        internal void When_inherited_property_name_contains_a_digit_it_must_be_skipped()
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
            VerifyGuidelineDiagnostic(source,
                "Property 'P6' contains one or more digits in its name.");
        }

        [Fact]
        internal void When_implemented_property_name_contains_a_digit_it_must_be_skipped()
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
            VerifyGuidelineDiagnostic(source,
                "Property 'P6' contains one or more digits in its name.");
        }

        [Fact]
        internal void When_event_name_contains_no_digits_it_must_be_skipped()
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
        internal void When_event_name_contains_a_digit_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        public event EventHandler [|Value9Changed|];
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Event 'Value9Changed' contains one or more digits in its name.");
        }

        [Fact]
        internal void When_method_name_contains_no_digits_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void M()
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_name_contains_a_digit_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void [|M6|]()
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'M6' contains one or more digits in its name.");
        }

        [Fact]
        internal void When_inherited_method_name_contains_a_digit_it_must_be_skipped()
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
            VerifyGuidelineDiagnostic(source,
                "Method 'M6' contains one or more digits in its name.");
        }

        [Fact]
        internal void When_implemented_method_name_contains_a_digit_it_must_be_skipped()
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
            VerifyGuidelineDiagnostic(source,
                "Method 'M6' contains one or more digits in its name.");
        }

        [Fact]
        internal void When_parameter_name_contains_no_digits_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void M(int p)
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_parameter_name_contains_a_digit_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void M(int [|p1|])
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Parameter 'p1' contains one or more digits in its name.");
        }

        [Fact]
        internal void When_inherited_parameter_name_contains_a_digit_it_must_be_skipped()
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
            VerifyGuidelineDiagnostic(source,
                "Parameter 'p7' contains one or more digits in its name.");
        }

        [Fact]
        internal void When_implemented_parameter_name_contains_a_digit_it_must_be_skipped()
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
            VerifyGuidelineDiagnostic(source,
                "Parameter 'p7' contains one or more digits in its name.");
        }

        [Fact]
        internal void When_variable_name_contains_no_digits_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void M()
                        {
                            string str = ""A"";
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_variable_name_contains_a_digit_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void M()
                        {
                            string [|str12|] = ""A"";
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Variable 'str12' contains one or more digits in its name.");
        }

        [Fact]
        internal void When_MSTest_method_name_contains_a_digit_it_must_be_skipped()
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
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_XUnit_method_name_contains_a_digit_it_must_be_skipped()
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
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_NUnit_method_name_contains_a_digit_it_must_be_skipped()
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
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_MbUnit_method_name_contains_a_digit_it_must_be_skipped()
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
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_name_contains_only_whitelisted_words_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void Int16SaveInt32Or3DIntoInt64()
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_name_contains_whitelisted_words_and_digits_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void [|ConvertDigit9IntoInt32|]()
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'ConvertDigit9IntoInt32' contains one or more digits in its name.");
        }

        [Fact]
        internal void When_method_name_contains_part_of_whitelisted_word_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void [|GetMatrix3Demo|]()
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'GetMatrix3Demo' contains one or more digits in its name.");
        }

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new DoNotUseNumbersInIdentifiersAnalyzer();
        }
    }
}