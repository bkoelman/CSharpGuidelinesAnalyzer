using System.Threading.Tasks;
using CSharpGuidelinesAnalyzer.Rules.Naming;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.Naming
{
    public class NameAsyncMethodsCorrectlySpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => NameAsyncMethodsCorrectlyAnalyzer.DiagnosticId;

        [Fact]
        public void When_async_method_name_ends_with_Async_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .Using(typeof (Task).Namespace)
                .InDefaultClass(@"
                    async Task SomeAsync()
                    {
                        throw new NotImplementedException();
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        public void When_async_method_name_ends_with_TaskAsync_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .Using(typeof (Task).Namespace)
                .InDefaultClass(@"
                    async Task SomeTaskAsync()
                    {
                        throw new NotImplementedException();
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        public void When_async_method_name_does_not_end_with_Async_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .Using(typeof (Task).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        async Task [|Some|]()
                        {
                            throw new NotImplementedException();
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Name of async method 'C.Some()' should end with Async or TaskAsync.");
        }

        [Fact]
        public void When_regular_method_name_does_not_end_with_Async_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .Using(typeof (Task).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        Task Some()
                        {
                            throw new NotImplementedException();
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new NameAsyncMethodsCorrectlyAnalyzer();
        }
    }
}