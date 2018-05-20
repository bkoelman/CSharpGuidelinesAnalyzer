using System.Collections.Generic;
using CSharpGuidelinesAnalyzer.Rules.Maintainability;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.Maintainability
{
    public sealed class AvoidSignatureWithManyParametersSpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => AvoidSignatureWithManyParametersAnalyzer.DiagnosticId;

        [Fact]
        internal void When_method_contains_three_parameters_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M(int first, string second, double third)
                    {
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_contains_four_parameters_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void [|M|](int first, string second, double third, object fourth)
                    {
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'M' contains more than 3 parameters.");
        }

        [Fact]
        internal void When_method_contains_tuple_parameter_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M((int a, int b) [|p|])
                    {
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'M' contains tuple parameter 'p'.");
        }

        [Fact]
        internal void When_method_contains_array_of_tuple_parameter_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M((int a, int b)[] p)
                    {
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_contains_collection_of_tuple_parameter_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .Using(typeof(IList<>).Namespace)
                .InDefaultClass(@"
                    void M(IList<(int a, int b)> p)
                    {
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_returns_tuple_of_two_elements_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    (int, string) M()
                    {
                        throw new NotImplementedException();
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_returns_tuple_of_three_elements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    (int, string, bool) [|M|]()
                    {
                        throw new NotImplementedException();
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'M' returns a tuple with more than 2 elements.");
        }

        [Fact]
        internal void When_instance_constructor_contains_three_parameters_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        public C(int first, string second, double third)
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_instance_constructor_contains_four_parameters_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        public [|C|](int first, string second, double third, object fourth)
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Constructor for 'C' contains more than 3 parameters.");
        }

        [Fact]
        internal void When_instance_constructor_contains_tuple_parameter_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        public C((int a, int b) [|p|])
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Constructor for 'C' contains tuple parameter 'p'.");
        }

        [Fact]
        internal void When_indexer_contains_three_parameters_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    public string this[int first, string second, double third]
                    {
                        get { throw new NotImplementedException(); }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_indexer_contains_four_parameters_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    public string [|this|][int first, string second, double third, object fourth]
                    {
                        get { throw new NotImplementedException(); }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Indexer contains more than 3 parameters.");
        }

        [Fact]
        internal void When_indexer_contains_tuple_parameter_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    public string this[(int a, int b) [|p|]]
                    {
                        get { throw new NotImplementedException(); }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Indexer contains tuple parameter 'p'.");
        }

        [Fact]
        internal void When_delegate_contains_three_parameters_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public delegate void D(int first, string second, double third);
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_delegate_contains_four_parameters_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public delegate void [|D|](int first, string second, double third, object fourth);
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Delegate 'D' contains more than 3 parameters.");
        }

        [Fact]
        internal void When_delegate_contains_tuple_parameter_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public delegate void D((int a, int b) [|p|]);
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Delegate 'D' contains tuple parameter 'p'.");
        }

        [Fact]
        internal void When_delegate_returns_tuple_of_two_elements_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public delegate (int, bool) D();
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_delegate_returns_tuple_of_three_elements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public delegate (int, string, int) [|D|]();
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Delegate 'D' returns a tuple with more than 2 elements.");
        }

        [Fact]
        internal void When_local_function_contains_three_parameters_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        void L(int first, string second, double third)
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_local_function_contains_four_parameters_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        void [|L|](int first, string second, double third, object fourth)
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Local function 'L' contains more than 3 parameters.");
        }

        [Fact]
        internal void When_local_function_contains_tuple_parameter_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        void L((int a, int b) [|p|])
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Local function 'L' contains tuple parameter 'p'.");
        }

        [Fact]
        internal void When_local_function_returns_tuple_of_two_elements_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        (int, string) L()
                        {
                            throw new NotImplementedException();
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_local_function_returns_tuple_of_three_elements_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        (int, string, double) [|L|]()
                        {
                            throw new NotImplementedException();
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Local function 'L' returns a tuple with more than 2 elements.");
        }

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new AvoidSignatureWithManyParametersAnalyzer();
        }
    }
}
