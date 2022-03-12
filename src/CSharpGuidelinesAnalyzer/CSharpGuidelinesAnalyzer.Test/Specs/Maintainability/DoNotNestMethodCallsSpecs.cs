using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpGuidelinesAnalyzer.Rules.Maintainability;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.Maintainability
{
    public sealed class DoNotNestMethodCallsSpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => DoNotNestMethodCallsAnalyzer.DiagnosticId;

        [Fact]
        internal void When_method_call_is_not_nested_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        A(null);
                    }

                    object A(object inner) => throw null;
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_calls_are_nested_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class Example
                    {
                        void M()
                        {
                            A([|B(null)|]);
                        }

                        object A(object outer) => throw null;
                        object B(object inner) => throw null;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Argument for parameter 'outer' in method call to 'Example.A(object)' calls nested method 'Example.B(object)'");
        }

        [Fact]
        internal void When_method_calls_are_nested_from_constructor_this_initializer_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class Example
                    {
                        public Example()
                            : this(GetDefaultText())
                        {
                        }

                        public Example(string text) => throw null;

                        static string GetDefaultText() => throw null;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_calls_are_nested_from_constructor_base_initializer_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class Base
                    {
                        public Base(string text) => throw null;
                    }

                    class Derived : Base
                    {
                        public Derived()
                            : base(GetDefaultText())
                        {
                        }

                        static string GetDefaultText() => throw null;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_calls_are_nested_from_field_initializer_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        string _value = Convert(GetDefaultText());

                        static string Convert(string source) => throw null;
                        static string GetDefaultText() => throw null;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_extension_method_calls_are_nested_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .Using(typeof(Enumerable).Namespace)
                .Using(typeof(IEnumerable<>).Namespace)
                .InDefaultClass(@"
                    void M()
                    {
                        var query = GetItems().Skip(1).Take(1).Select(item => item.First()).ToArray();
                    }

                    IEnumerable<string> GetItems() => throw null;
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_calls_are_chained_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M()
                    {
                        A(null).ToString();
                    }

                    object A(object inner) => throw null;
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_call_contains_method_group_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .Using(typeof(Enumerable).Namespace)
                .InDefaultClass(@"
                    void M()
                    {
                        string[] items = new[] { string.Empty };
                        items.Where(Filter);
                    }

                    bool Filter(string item) => throw null;
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_call_contains_property_accessor_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    string Value { get; set; }

                    void M()
                    {
                        A(Value);
                    }

                    object A(object inner) => throw null;
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_method_call_contains_lambda_body_with_method_call_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .Using(typeof(Enumerable).Namespace)
                .InDefaultClass(@"
                    void M()
                    {
                        string[] items = new[] { string.Empty };
                        items.Where(item => item.Any());
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_constructor_call_contains_nested_constructor_call_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .Using(typeof(StringBuilder).Namespace)
                .InGlobalScope(@"
                    class Example
                    {
                        public Example(StringBuilder builder) => throw null;

                        void M()
                        {
                            new Example([|new StringBuilder()|]);
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Argument for parameter 'builder' in method call to 'Example.Example(StringBuilder)' calls nested method 'StringBuilder.StringBuilder()'");
        }

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new DoNotNestMethodCallsAnalyzer();
        }
    }
}
