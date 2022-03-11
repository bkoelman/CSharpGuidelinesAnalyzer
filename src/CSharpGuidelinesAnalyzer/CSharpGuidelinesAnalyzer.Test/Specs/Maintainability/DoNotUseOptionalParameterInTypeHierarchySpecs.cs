using CSharpGuidelinesAnalyzer.Rules.Maintainability;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.Maintainability
{
    public sealed class DoNotUseOptionalParameterInTypeHierarchySpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => DoNotUseOptionalParameterInTypeHierarchyAnalyzer.DiagnosticId;

        [Fact]
        internal void When_using_optional_parameter_in_regular_method_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void M(int p = 5) => throw null;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_using_required_parameter_in_interface_method_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    interface I
                    {
                        void M(int p);
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_using_optional_parameter_in_interface_method_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    interface I
                    {
                        void M([|int p = 5|]);
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'I.M(int)' contains optional parameter 'p'");
        }

        [Fact]
        internal void When_using_optional_parameter_in_implicitly_implemented_interface_method_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    interface I
                    {
                        void M([|int p = 5|]);
                    }

                    class C : I
                    {
                        public void M([|int q = 8|]) => throw null;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'I.M(int)' contains optional parameter 'p'",
                "Method 'C.M(int)' contains optional parameter 'q'");
        }

        [Fact]
        internal void When_using_optional_parameter_in_explicitly_implemented_interface_method_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    interface I
                    {
                        void M([|int p = 5|]);
                    }

                    class C : I
                    {
                        void I.M([|int q = 8|]) => throw null;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'I.M(int)' contains optional parameter 'p'",
                "Method 'C.I.M(int)' contains optional parameter 'q'");
        }

        [Fact]
        internal void When_using_optional_parameter_in_abstract_virtual_or_overridden_method_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    abstract class AbstractBase
                    {
                        protected abstract void M([|int p = 5|]);
                    }

                    class ConcreteBase
                    {
                        protected virtual void M([|int q = 8|]) => throw null;
                    }

                    class ConcreteDerived : ConcreteBase
                    {
                        protected override void M([|int r = 12|]) => throw null;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method 'AbstractBase.M(int)' contains optional parameter 'p'",
                "Method 'ConcreteBase.M(int)' contains optional parameter 'q'",
                "Method 'ConcreteDerived.M(int)' contains optional parameter 'r'");
        }

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new DoNotUseOptionalParameterInTypeHierarchyAnalyzer();
        }
    }
}
