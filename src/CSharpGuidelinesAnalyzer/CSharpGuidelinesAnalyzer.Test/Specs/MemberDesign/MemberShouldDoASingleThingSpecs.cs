using CSharpGuidelinesAnalyzer.Rules.MemberDesign;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.MemberDesign
{
    public sealed class MemberShouldDoASingleThingSpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => MemberShouldDoASingleThingAnalyzer.DiagnosticId;

        [Fact]
        internal void When_property_name_contains_the_word_And_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        object [|CustomerAndOrder|] { get; set; }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Property 'CustomerAndOrder' contains the word 'and', which suggests doing multiple things.");
        }

        [Fact]
        internal void When_method_name_contains_the_word_And_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    struct C
                    {
                        void [|SaveCustomerAndOrder|]()
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'SaveCustomerAndOrder' contains the word 'and', which suggests doing multiple things.");
        }

        [Fact]
        internal void When_local_function_name_contains_the_word_And_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    struct C
                    {
                        void M()
                        {
                            void [|SaveCustomerAndOrder|]()
                            {
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Local function 'SaveCustomerAndOrder' contains the word 'and', which suggests doing multiple things.");
        }

        [Fact]
        internal void When_nested_local_function_name_contains_the_word_And_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    struct C
                    {
                        void M()
                        {
                            void N()
                            {
                                void O()
                                {
                                    void [|SaveCustomerAndOrder|]()
                                    {
                                    }
                                }
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Local function 'SaveCustomerAndOrder' contains the word 'and', which suggests doing multiple things.");
        }

        [Fact]
        internal void When_field_name_contains_the_word_And_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        object [|_customerAndOrder|];
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Field '_customerAndOrder' contains the word 'and', which suggests doing multiple things.");
        }

        [Fact]
        internal void When_event_name_contains_the_word_And_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        public event EventHandler [|CustomerAndOrderSaved|]
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
            VerifyGuidelineDiagnostic(source,
                "Event 'CustomerAndOrderSaved' contains the word 'and', which suggests doing multiple things.");
        }

        [Fact]
        internal void When_method_name_contains_the_word_and_with_underscores_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void [|save_customer_and_order|]()
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'save_customer_and_order' contains the word 'and', which suggests doing multiple things.");
        }

        [Fact]
        internal void When_method_name_contains_the_word_and_with_digits_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void [|Match1And2Again3|]()
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'Match1And2Again3' contains the word 'and', which suggests doing multiple things.");
        }

        [Fact]
        internal void When_method_name_contains_the_word_Land_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void ExploreTheBigLandOutHere()
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_name_contains_the_word_Andy_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void VisitAndyWithJohn()
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_MSTest_method_name_contains_the_word_And_it_must_be_skipped()
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
                            void When_true_and_false_it_must_work()
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
        internal void When_XUnit_method_name_contains_the_word_And_it_must_be_skipped()
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
                            void When_true_and_false_it_must_work()
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
        internal void When_NUnit_method_name_contains_the_word_And_it_must_be_skipped()
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
                            void When_true_and_false_it_must_work()
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
        internal void When_MbUnit_method_name_contains_the_word_And_it_must_be_skipped()
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
                            void When_true_and_false_it_must_work()
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
        internal void When_method_name_consists_of_the_word_And_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    struct C
                    {
                        void And()
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_name_starts_with_the_word_And_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    struct C
                    {
                        void AndComputer()
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_name_ends_with_the_word_And_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    struct C
                    {
                        void ParseLogicalAnd()
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new MemberShouldDoASingleThingAnalyzer();
        }
    }
}
