using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using CSharpGuidelinesAnalyzer.Rules.MemberDesign;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.MemberDesign
{
    public sealed class ReturnInterfaceToCollectionSpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => ReturnInterfaceToCollectionAnalyzer.DiagnosticId;

        [Fact]
        internal void When_method_returns_void_it_must_be_skipped()
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
        internal void When_method_returns_string_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
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
        internal void When_method_returns_generic_List_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .Using(typeof(List<>).Namespace)
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
        internal void When_method_returns_array_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
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
        internal void When_method_returns_generic_ICollection_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .Using(typeof(ICollection<>).Namespace)
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
        internal void When_method_returns_IEnumerable_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .Using(typeof(IEnumerable).Namespace)
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
        internal void When_method_returns_ImmutableArray_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithReference(typeof(ImmutableArray<>).Assembly)
                .Using(typeof(ImmutableArray<>).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        ImmutableArray<int> M()
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
        internal void When_method_returns_IImmutableList_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithReference(typeof(IImmutableList<>).Assembly)
                .Using(typeof(IImmutableList<>).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        IImmutableList<int> M()
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
        internal void When_method_returns_ImmutableStack_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithReference(typeof(ImmutableStack<>).Assembly)
                .Using(typeof(ImmutableStack<>).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        ImmutableStack<int> M()
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
        internal void When_method_returns_ImmutableDictionary_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithReference(typeof(ImmutableDictionary<,>).Assembly)
                .Using(typeof(ImmutableDictionary<,>).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        ImmutableDictionary<int, string> M()
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
        internal void When_method_returns_aliased_ImmutableHashSet_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithReference(typeof(ImmutableHashSet<>).Assembly)
                .Using(typeof(ImmutableHashSet<>).Namespace)
                .InGlobalScope(@"
                    using HS = System.Collections.Immutable.ImmutableHashSet<int>;

                    class C
                    {
                        HS M()
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
        internal void When_inherited_method_returns_generic_List_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .Using(typeof(List<>).Namespace)
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
        internal void When_hidden_method_returns_generic_List_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .Using(typeof(List<>).Namespace)
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
        internal void When_implicitly_implemented_method_returns_generic_List_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .Using(typeof(List<>).Namespace)
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
        internal void When_explicitly_implemented_method_returns_generic_List_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .Using(typeof(List<>).Namespace)
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

        [Fact]
        internal void When_property_type_is_generic_List_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .Using(typeof(List<>).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        List<string> P { get; set; }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }


        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new ReturnInterfaceToCollectionAnalyzer();
        }
    }
}
