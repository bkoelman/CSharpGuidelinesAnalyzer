using CSharpGuidelinesAnalyzer.Rules.MiscellaneousDesign;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

// @formatter:keep_existing_linebreaks true

namespace CSharpGuidelinesAnalyzer.Test.Specs.MiscellaneousDesign;

public sealed class CatchSpecificExceptionSpecs : CSharpGuidelinesAnalysisTestFixture
{
    protected override string DiagnosticId => CatchSpecificExceptionAnalyzer.DiagnosticId;

    [Fact]
    internal async Task When_catching_unfiltered_exception_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M()
                {
                    try
                    {
                        M();
                    }
                    [|catch|] (Exception)
                    {
                        throw null;
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Catch a specific exception instead of Exception, SystemException or ApplicationException");
    }

    [Fact]
    internal async Task When_catching_filtered_exception_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M()
                {
                    try
                    {
                        M();
                    }
                    catch (Exception ex) when (ex.Message.Length > 0)
                    {
                        throw null;
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_catching_custom_exception_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                class OtherException : Exception
                {
                }

                void M()
                {
                    try
                    {
                        M();
                    }
                    catch (OtherException)
                    {
                        throw null;
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_catching_argument_exception_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                class OtherException : Exception
                {
                }

                void M()
                {
                    try
                    {
                        M();
                    }
                    catch (ArgumentException)
                    {
                        throw null;
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_catching_aggregate_exception_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                class OtherException : Exception
                {
                }

                void M()
                {
                    try
                    {
                        M();
                    }
                    catch (AggregateException)
                    {
                        throw null;
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_catching_unfiltered_system_exception_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M()
                {
                    try
                    {
                        M();
                    }
                    [|catch|] (SystemException)
                    {
                        throw null;
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Catch a specific exception instead of Exception, SystemException or ApplicationException");
    }

    [Fact]
    internal async Task When_catching_filtered_system_exception_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M()
                {
                    try
                    {
                        M();
                    }
                    catch (SystemException ex) when (ex.Message.Length > 0)
                    {
                        throw null;
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_catching_unfiltered_application_exception_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M()
                {
                    try
                    {
                        M();
                    }
                    [|catch|] (ApplicationException)
                    {
                        throw null;
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Catch a specific exception instead of Exception, SystemException or ApplicationException");
    }

    [Fact]
    internal async Task When_catching_filtered_application_exception_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M()
                {
                    try
                    {
                        M();
                    }
                    catch (ApplicationException ex) when (ex.Message.Length > 0)
                    {
                        throw null;
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_catching_unmanaged_exception_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M()
                {
                    try
                    {
                        M();
                    }
                    [|catch|]
                    {
                        throw null;
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Catch a specific exception instead of Exception, SystemException or ApplicationException");
    }

    protected override DiagnosticAnalyzer CreateAnalyzer()
    {
        return new CatchSpecificExceptionAnalyzer();
    }
}
