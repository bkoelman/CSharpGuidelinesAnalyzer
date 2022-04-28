using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CSharpGuidelinesAnalyzer.Rules.MemberDesign;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.MemberDesign
{
    public sealed class ReturnInterfaceToUnchangeableCollectionSpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => ReturnInterfaceToUnchangeableCollectionAnalyzer.DiagnosticId;

        [Fact]
        internal void When_method_returns_void_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public class C
                    {
                        public void M()
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
                    public class C
                    {
                        public string M()
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
        internal void When_method_returns_array_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public class C
                    {
                        public int[] [|M|]()
                        {
                            throw new NotImplementedException();
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Return type in signature for 'C.M()' should be an interface to an unchangeable collection");
        }

        [Fact]
        internal void When_private_method_returns_array_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public class C
                    {
                        private int[] M()
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
                    public class C
                    {
                        public List<string> [|M|]()
                        {
                            throw new NotImplementedException();
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Return type in signature for 'C.M()' should be an interface to an unchangeable collection");
        }

        [Fact]
        internal void When_public_method_in_private_class_returns_generic_List_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .Using(typeof(List<>).Namespace)
                .InGlobalScope(@"
                    public class Outer
                    {
                        private class C
                        {
                            public List<string> M()
                            {
                                throw new NotImplementedException();
                            }
                        }
                    }

                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_returns_generic_IList_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .Using(typeof(IList<>).Namespace)
                .InGlobalScope(@"
                    public class C
                    {
                        public IList<string> [|M|]()
                        {
                            throw new NotImplementedException();
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Return type in signature for 'C.M()' should be an interface to an unchangeable collection");
        }

        [Fact]
        internal void When_method_returns_generic_ICollection_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .Using(typeof(ICollection<>).Namespace)
                .InGlobalScope(@"
                    public class C
                    {
                        public ICollection<string> [|M|]()
                        {
                            throw new NotImplementedException();
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Return type in signature for 'C.M()' should be an interface to an unchangeable collection");
        }

        [Fact]
        internal void When_method_returns_custom_collection_type_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .Using(typeof(List<>).Namespace)
                .InGlobalScope(@"
                    public class CustomCollection : List<string>
                    {
                    }

                    public class C
                    {
                        public CustomCollection [|M|]()
                        {
                            throw new NotImplementedException();
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Return type in signature for 'C.M()' should be an interface to an unchangeable collection");
        }

        [Fact]
        internal void When_method_returns_IEnumerable_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .Using(typeof(IEnumerable).Namespace)
                .InGlobalScope(@"
                    public class C
                    {
                        public IEnumerable M()
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
        internal void When_method_returns_generic_IEnumerable_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .Using(typeof(IEnumerable<>).Namespace)
                .InGlobalScope(@"
                    public class C
                    {
                        public IEnumerable<string> M()
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
        internal void When_method_returns_IQueryable_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithReference(typeof(IQueryable).Assembly)
                .Using(typeof(IQueryable).Namespace)
                .InGlobalScope(@"
                    public class C
                    {
                        public IQueryable M()
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
        internal void When_method_returns_generic_IAsyncEnumerable_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithReference(typeof(IAsyncEnumerable<>).Assembly)
                .Using(typeof(IAsyncEnumerable<>).Namespace)
                .InGlobalScope(@"
                    public class C
                    {
                        public IAsyncEnumerable<string> M()
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
        internal void When_method_returns_generic_IQueryable_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithReference(typeof(IQueryable<>).Assembly)
                .Using(typeof(IQueryable<>).Namespace)
                .InGlobalScope(@"
                    public class C
                    {
                        public IQueryable<string> M()
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
        internal void When_method_returns_generic_IReadOnlyCollection_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .Using(typeof(IReadOnlyCollection<>).Namespace)
                .InGlobalScope(@"
                    public class C
                    {
                        public IReadOnlyCollection<string> M()
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
        internal void When_method_returns_generic_IReadOnlyList_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .Using(typeof(IReadOnlyList<>).Namespace)
                .InGlobalScope(@"
                    public class C
                    {
                        public IReadOnlyList<string> M()
                        {
                            throw new NotImplementedException();
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

#if NET5_0_OR_GREATER
        [Fact]
        internal void When_method_returns_generic_IReadOnlySet_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .Using(typeof(IReadOnlySet<>).Namespace)
                .InGlobalScope(@"
                    public class C
                    {
                        public IReadOnlySet<string> M()
                        {
                            throw new NotImplementedException();
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }
#endif

        [Fact]
        internal void When_method_returns_generic_IReadOnlyDictionary_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .Using(typeof(IReadOnlyDictionary<,>).Namespace)
                .InGlobalScope(@"
                    public class C
                    {
                        public IReadOnlyDictionary<string, int> M()
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
                    public class C
                    {
                        public ImmutableArray<int> M()
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
                    public class C
                    {
                        public IImmutableList<int> M()
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
                    public class C
                    {
                        public ImmutableStack<int> M()
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
                    public class C
                    {
                        public ImmutableDictionary<int, string> M()
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

                    public class C
                    {
                        public HS M()
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
        internal void When_inherited_method_returns_generic_List_it_must_be_reported()
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
                "Return type in signature for 'B.M()' should be an interface to an unchangeable collection");
        }

        [Fact]
        internal void When_hidden_method_returns_generic_List_it_must_be_reported()
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
                "Return type in signature for 'B.M()' should be an interface to an unchangeable collection");
        }

        [Fact]
        internal void When_implicitly_implemented_method_returns_generic_List_it_must_be_reported()
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
                "Return type in signature for 'I.M()' should be an interface to an unchangeable collection");
        }

        [Fact]
        internal void When_explicitly_implemented_method_returns_generic_List_it_must_be_reported()
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
                "Return type in signature for 'I.M()' should be an interface to an unchangeable collection");
        }

        [Fact]
        internal void When_property_type_is_generic_List_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .Using(typeof(List<>).Namespace)
                .InGlobalScope(@"
                    public class C
                    {
                        public List<string> P { get; set; }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new ReturnInterfaceToUnchangeableCollectionAnalyzer();
        }
    }
}
