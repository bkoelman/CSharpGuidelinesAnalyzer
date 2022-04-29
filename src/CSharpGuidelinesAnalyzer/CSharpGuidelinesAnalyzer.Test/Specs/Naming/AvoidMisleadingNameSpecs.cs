using CSharpGuidelinesAnalyzer.Rules.Naming;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.Naming;

public sealed class AvoidMisleadingNameSpecs : CSharpGuidelinesAnalysisTestFixture
{
    protected override string DiagnosticId => AvoidMisleadingNameAnalyzer.DiagnosticId;

    [Fact]
    internal async Task When_variable_is_named_b001_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M()
                {
                    for (int [|b001|] = 5; b001 < 10; b001++)
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Variable 'b001' has a name that is difficult to read");
    }

    [Fact]
    internal async Task When_method_parameter_is_named_b001_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M(int [|b001|])
                {
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Parameter 'b001' has a name that is difficult to read");
    }

    [Fact]
    internal async Task When_local_function_parameter_is_named_b001_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M()
                {
                    void L(int [|b001|])
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Parameter 'b001' has a name that is difficult to read");
    }

    [Fact]
    internal async Task When_variable_is_named_lo_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M()
                {
                    int [|lo|];
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Variable 'lo' has a name that is difficult to read");
    }

    [Fact]
    internal async Task When_method_parameter_is_named_lo_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M(int [|lo|])
                {
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Parameter 'lo' has a name that is difficult to read");
    }

    [Fact]
    internal async Task When_local_function_parameter_is_named_lo_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M()
                {
                    void L(int [|lo|])
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Parameter 'lo' has a name that is difficult to read");
    }

    [Fact]
    internal async Task When_variable_is_named_I1_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M()
                {
                    string [|I1|] = ""X"";
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Variable 'I1' has a name that is difficult to read");
    }

    [Fact]
    internal async Task When_method_parameter_is_named_I1_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M(string [|I1|] = ""X"")
                {
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Parameter 'I1' has a name that is difficult to read");
    }

    [Fact]
    internal async Task When_local_function_parameter_is_named_I1_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M()
                {
                    void L(string [|I1|] = ""X"")
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Parameter 'I1' has a name that is difficult to read");
    }

    [Fact]
    internal async Task When_variable_is_named_lOl_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M()
                {
                    bool [|lOl|] = true;
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Variable 'lOl' has a name that is difficult to read");
    }

    [Fact]
    internal async Task When_method_parameter_is_named_lOl_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M(bool [|lOl|] = true)
                {
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Parameter 'lOl' has a name that is difficult to read");
    }

    [Fact]
    internal async Task When_local_function_parameter_is_named_lOl_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M()
                {
                    void L(string [|lOl|] = ""X"")
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Parameter 'lOl' has a name that is difficult to read");
    }

    protected override DiagnosticAnalyzer CreateAnalyzer()
    {
        return new AvoidMisleadingNameAnalyzer();
    }
}
