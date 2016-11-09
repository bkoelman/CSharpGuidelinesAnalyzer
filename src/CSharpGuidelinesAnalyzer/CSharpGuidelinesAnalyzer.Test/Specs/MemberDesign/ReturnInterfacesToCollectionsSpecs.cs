using System.Collections;
using System.Collections.Generic;
using CSharpGuidelinesAnalyzer.Rules.MemberDesign;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.MemberDesign
{
    public class ReturnInterfacesToCollectionsSpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => ReturnInterfacesToCollectionsAnalyzer.DiagnosticId;

        [Fact]
        public void When_method_returns_void_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
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
        public void When_method_returns_string_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        string M()
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
        public void When_method_returns_generic_List_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .Using(typeof (List<>).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        List<string> [|M|]()
                        {
                            throw new NotImplementedException();
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Return type in signature for 'C.M()' should be a collection interface instead of a concrete type.");
        }

        [Fact]
        public void When_method_returns_array_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        int[] [|M|]()
                        {
                            throw new NotImplementedException();
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Return type in signature for 'C.M()' should be a collection interface instead of a concrete type.");
        }

        [Fact]
        public void When_method_returns_generic_ICollection_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .Using(typeof (ICollection<>).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        ICollection<string> M()
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
        public void When_method_returns_IEnumerable_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .Using(typeof (IEnumerable).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        IEnumerable M()
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
        public void When_inherited_method_returns_generic_List_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .Using(typeof (List<>).Namespace)
                .InGlobalScope(@"
                    public abstract class B
                    {
                        public abstract List<string> [|M|]();
                    }

                    public class C : B
                    {
                        public override List<string> M()
                        {
                            throw new NotImplementedException();
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Return type in signature for 'B.M()' should be a collection interface instead of a concrete type.");
        }

        [Fact]
        public void When_hidden_method_returns_generic_List_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .Using(typeof (List<>).Namespace)
                .InGlobalScope(@"
                    public class B
                    {
                        public virtual List<string> [|M|]()
                        {
                            throw new NotImplementedException();
                        }
                    }

                    public class C : B
                    {
                        public new List<string> M()
                        {
                            throw new NotImplementedException();
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Return type in signature for 'B.M()' should be a collection interface instead of a concrete type.");
        }

        [Fact]
        public void When_implicitly_implemented_method_returns_generic_List_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .Using(typeof (List<>).Namespace)
                .InGlobalScope(@"
                    public interface I
                    {
                        List<string> [|M|]();
                    }

                    public class C : I
                    {
                        public List<string> M()
                        {
                            throw new NotImplementedException();
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Return type in signature for 'I.M()' should be a collection interface instead of a concrete type.");
        }

        [Fact]
        public void When_explicitly_implemented_method_returns_generic_List_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .Using(typeof (List<>).Namespace)
                .InGlobalScope(@"
                    public interface I
                    {
                        List<string> [|M|]();
                    }

                    public class C : I
                    {
                        List<string> I.M()
                        {
                            throw new NotImplementedException();
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Return type in signature for 'I.M()' should be a collection interface instead of a concrete type.");
        }

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new ReturnInterfacesToCollectionsAnalyzer();
        }
    }
}