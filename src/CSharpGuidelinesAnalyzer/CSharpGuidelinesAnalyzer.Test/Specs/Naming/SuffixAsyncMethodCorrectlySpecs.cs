using CSharpGuidelinesAnalyzer.Rules.Naming;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.Naming;

public sealed class SuffixAsyncMethodCorrectlySpecs : CSharpGuidelinesAnalysisTestFixture
{
    protected override string DiagnosticId => SuffixAsyncMethodCorrectlyAnalyzer.DiagnosticId;

    [Fact]
    internal async Task When_async_method_name_ends_with_Async_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(Task).Namespace)
            .InDefaultClass(@"
                async Task SomeAsync()
                {
                    throw new NotImplementedException();
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_async_method_name_ends_with_TaskAsync_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(Task).Namespace)
            .InDefaultClass(@"
                async Task SomeTaskAsync()
                {
                    throw new NotImplementedException();
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_async_method_name_does_not_end_with_Async_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(Task).Namespace)
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
        await VerifyGuidelineDiagnosticAsync(source,
            "Name of async method 'C.Some()' should end with Async or TaskAsync");
    }

    [Fact]
    internal async Task When_regular_method_name_does_not_end_with_Async_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(Task).Namespace)
            .InDefaultClass(@"
                Task Some()
                {
                    throw new NotImplementedException();
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_async_local_function_name_ends_with_Async_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(Task).Namespace)
            .InDefaultClass(@"
                void M()
                {
                    async Task SomeAsync()
                    {
                        throw new NotImplementedException();
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_async_local_function_name_ends_with_TaskAsync_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(Task).Namespace)
            .InDefaultClass(@"
                void M()
                {
                    async Task SomeTaskAsync()
                    {
                        throw new NotImplementedException();
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_async_local_function_name_does_not_end_with_Async_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(Task).Namespace)
            .InDefaultClass(@"
                void M()
                {
                    async Task [|Some|]()
                    {
                        throw new NotImplementedException();
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Name of async local function 'Some()' should end with Async or TaskAsync");
    }

    [Fact]
    internal async Task When_regular_local_function_name_does_not_end_with_Async_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(Task).Namespace)
            .InDefaultClass(@"
                void M()
                {
                    Task Some()
                    {
                        throw new NotImplementedException();
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_async_entry_point_method_name_does_not_end_with_Async_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(Task).Namespace)
            .WithOutputKind(OutputKind.WindowsApplication)
            .InGlobalScope(@"
                class Program
                {
                    static async Task Main(string[] args)
                    {
                        throw new NotImplementedException();
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_test_method_name_does_not_end_with_Async_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(Task).Namespace)
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
                        public async Task When_some_condition_it_must_work()
                        {
                        }
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_parameterized_test_method_name_does_not_end_with_Async_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(Task).Namespace)
            .InGlobalScope(@"
                namespace Xunit
                {
                    public class TheoryAttribute : Attribute
                    {
                    }

                    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
                    public class InlineDataAttribute : Attribute
                    {
                        public InlineDataAttribute(params object[] data)
                        {
                        }
                    }
                }

                namespace App
                {
                    using Xunit;

                    class UnitTests
                    {
                        [InlineData(""A"")]
                        [InlineData(""B"")]
                        [Theory]
                        public async Task When_some_condition_it_must_work(string value)
                        {
                            _ = value;
                        }
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    protected override DiagnosticAnalyzer CreateAnalyzer()
    {
        return new SuffixAsyncMethodCorrectlyAnalyzer();
    }
}
