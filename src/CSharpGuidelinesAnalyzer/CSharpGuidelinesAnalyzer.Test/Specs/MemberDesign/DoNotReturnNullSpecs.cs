using System.Collections;
using CSharpGuidelinesAnalyzer.Rules.MemberDesign;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

// @formatter:keep_existing_linebreaks true

namespace CSharpGuidelinesAnalyzer.Test.Specs.MemberDesign;

public sealed class DoNotReturnNullSpecs : CSharpGuidelinesAnalysisTestFixture
{
    protected override string DiagnosticId => DoNotReturnNullAnalyzer.DiagnosticId;

    [Fact]
    internal async Task When_returning_from_void_method_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                public void M()
                {
                    return;
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_returning_constant_from_method_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                public int M()
                {
                    return 3;
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_returning_null_for_return_type_Exception_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                public Exception M()
                {
                    return null;
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_returning_null_for_return_type_string_it_must_be_reported()
    {
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class C
                {
                    public string M()
                    {
                        return [|null|];
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "null is returned from method 'C.M()' which has return type of string, collection or task");
    }

    [Fact]
    internal async Task When_returning_null_for_return_type_List_of_int_it_must_be_reported()
    {
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(List<>).Namespace)
            .InGlobalScope(@"
                class C
                {
                    public List<int> M()
                    {
                        return [|null|];
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "null is returned from method 'C.M()' which has return type of string, collection or task");
    }

    [Fact]
    internal async Task When_returning_null_for_return_type_IEnumerable_it_must_be_reported()
    {
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(IEnumerable).Namespace)
            .InGlobalScope(@"
                class C
                {
                    public IEnumerable M()
                    {
                        return [|null|];
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "null is returned from method 'C.M()' which has return type of string, collection or task");
    }

    [Fact]
    internal async Task When_returning_null_for_return_type_int_array_it_must_be_reported()
    {
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class C
                {
                    public int[] M()
                    {
                        return [|null|];
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "null is returned from method 'C.M()' which has return type of string, collection or task");
    }

    [Fact]
    internal async Task When_returning_null_for_return_type_Task_it_must_be_reported()
    {
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(Task).Namespace)
            .InGlobalScope(@"
                class C
                {
                    public Task M()
                    {
                        return [|null|];
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "null is returned from method 'C.M()' which has return type of string, collection or task");
    }

    [Fact]
    internal async Task When_returning_null_for_return_type_Task_of_int_it_must_be_reported()
    {
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(Task<>).Namespace)
            .InGlobalScope(@"
                class C
                {
                    public Task<int> M()
                    {
                        return [|null|];
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "null is returned from method 'C.M()' which has return type of string, collection or task");
    }

    [Fact]
    internal async Task When_returning_from_async_Task_method_it_must_be_skipped()
    {
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(Task).Namespace)
            .InGlobalScope(@"
                class C
                {
                    public async Task M()
                    {
                        return;
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_returning_from_async_ValueTask_method_it_must_be_skipped()
    {
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(ValueTask).Namespace)
            .InGlobalScope(@"
                class C
                {
                    public async ValueTask M()
                    {
                        return;
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_returning_null_for_async_return_type_Task_of_Exception_it_must_be_skipped()
    {
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(Task<>).Namespace)
            .InGlobalScope(@"
                class C
                {
                    public async Task<Exception> M()
                    {
                        return null;
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_returning_null_for_async_return_type_ValueTask_of_Exception_it_must_be_skipped()
    {
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(ValueTask<>).Namespace)
            .InGlobalScope(@"
                class C
                {
                    public async ValueTask<Exception> M()
                    {
                        return null;
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_returning_null_for_async_return_type_Task_of_string_it_must_be_reported()
    {
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(Task<>).Namespace)
            .InGlobalScope(@"
                class C
                {
                    public async Task<string> M()
                    {
                        return [|null|];
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "null is returned from method 'C.M()' which has return type of string, collection or task");
    }

    [Fact]
    internal async Task When_returning_null_for_async_return_type_ValueTask_of_string_it_must_be_reported()
    {
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(ValueTask<>).Namespace)
            .InGlobalScope(@"
                class C
                {
                    public async ValueTask<string> M()
                    {
                        return [|null|];
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "null is returned from method 'C.M()' which has return type of string, collection or task");
    }

    [Fact]
    internal async Task When_yielding_break_it_must_be_skipped()
    {
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(IEnumerable<>).Namespace)
            .InGlobalScope(@"
                class C
                {
                    public IEnumerable<string> M()
                    {
                        yield break;
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_yielding_null_for_return_type_IEnumerable_of_Exception_it_must_be_skipped()
    {
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(IEnumerable<>).Namespace)
            .InGlobalScope(@"
                class C
                {
                    public IEnumerable<Exception> M()
                    {
                        yield return null;
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_yielding_null_for_return_type_IEnumerable_of_string_it_must_be_reported()
    {
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(IEnumerable<>).Namespace)
            .InGlobalScope(@"
                class C
                {
                    public IEnumerable<string> M()
                    {
                        yield return [|null|];
                        yield break;
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "null is returned from method 'C.M()' which has return type of string, collection or task");
    }

    [Fact]
    internal async Task When_returning_null_constant_for_return_type_string_it_must_be_reported()
    {
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class C
                {
                    private const string NullConstant = null;

                    public string M()
                    {
                        return [|NullConstant|];
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "null is returned from method 'C.M()' which has return type of string, collection or task");
    }

    [Fact]
    internal async Task When_returning_cast_to_null_for_return_type_string_it_must_be_reported()
    {
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class C
                {
                    public string M()
                    {
                        return [|(string)(object)null|];
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "null is returned from method 'C.M()' which has return type of string, collection or task");
    }

    [Fact]
    internal async Task When_returning_null_for_return_type_string_from_property_getter_it_must_be_reported()
    {
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class C
                {
                    public string P
                    {
                        get
                        {
                            return [|null|];
                        }
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "null is returned from property accessor 'C.P.get' which has return type of string, collection or task");
    }

    [Fact]
    internal async Task When_returning_null_for_return_type_string_from_indexer_getter_it_must_be_reported()
    {
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class C
                {
                    public string this[int index]
                    {
                        get
                        {
                            return [|null|];
                        }
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "null is returned from property accessor 'C.this[int].get' which has return type of string, collection or task");
    }

    [Fact]
    internal async Task When_returning_null_for_return_type_string_from_local_function_it_must_be_reported()
    {
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class C
                {
                    public string M()
                    {
                        return L();

                        string L()
                        {
                            return [|null|];
                        }
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "null is returned from local function 'L()' which has return type of string, collection or task");
    }

    [Fact]
    internal async Task When_returning_expression_bodied_null_for_return_type_string_it_must_be_reported()
    {
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class C
                {
                    public string M() => [|null|];
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "null is returned from method 'C.M()' which has return type of string, collection or task");
    }

    [Fact]
    internal async Task When_returning_null_for_return_type_string_from_lambda_expression_it_must_be_reported()
    {
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class C
                {
                    public void M()
                    {
                        Func<int, string> callback = input => [|null|];
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "null is returned from method 'lambda expression' which has return type of string, collection or task");
    }

    [Fact]
    internal async Task When_returning_null_for_return_type_string_from_lambda_statement_it_must_be_reported()
    {
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class C
                {
                    public void M()
                    {
                        Func<int, string> callback = input => 
                        {
                            return [|null|];
                        };
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "null is returned from method 'lambda expression' which has return type of string, collection or task");
    }

    [Fact]
    internal async Task When_returning_null_for_return_type_string_from_anonymous_method_it_must_be_reported()
    {
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class C
                {
                    public void M()
                    {
                        Func<int, string> callback = delegate(int input)
                        {
                            return [|null|];
                        };
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "null is returned from method 'lambda expression' which has return type of string, collection or task");
    }

    protected override DiagnosticAnalyzer CreateAnalyzer()
    {
        return new DoNotReturnNullAnalyzer();
    }
}
