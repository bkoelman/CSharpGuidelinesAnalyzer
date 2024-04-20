using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using FluentAssertions;
using Xunit;

// @formatter:keep_existing_linebreaks true

namespace CSharpGuidelinesAnalyzer.Test.Specs.Maintainability;

public partial class AvoidMemberWithManyStatementsSpecs
{
    [Fact]
    internal async Task When_using_editor_config_setting_it_must_be_applied()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .WithOptions(new AnalyzerOptionsBuilder()
                .ForEditorConfig(new EditorConfigSettingsBuilder()
                    .Including(DiagnosticId, "max_statement_count", "16")))
            .InGlobalScope("""
                class C
                {
                    int [|M|](string s)
                    {
                        ; ; ; ;
                        ; ; ; ;
                        ; ; ; ;
                        ; ; ; ;
                        throw null;
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'C.M(string)' contains 17 statements, which exceeds the maximum of 16 statements");
    }

    [Fact]
    internal async Task When_using_xml_config_setting_it_must_be_applied()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .WithOptions(new AnalyzerOptionsBuilder()
                .ForXmlSettings(new XmlSettingsBuilder()
                    .Including(DiagnosticId, "MaxStatementCount", "16")))
            .InGlobalScope("""
                class C
                {
                    int [|M|](string s)
                    {
                        ; ; ; ;
                        ; ; ; ;
                        ; ; ; ;
                        ; ; ; ;
                        throw null;
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'C.M(string)' contains 17 statements, which exceeds the maximum of 16 statements");
    }

    [Fact]
    internal async Task When_using_xml_and_editor_config_setting_it_must_use_editor_config_value()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .WithOptions(new AnalyzerOptionsBuilder()
                .ForXmlSettings(new XmlSettingsBuilder()
                    .Including(DiagnosticId, "MaxStatementCount", "12"))
                .ForEditorConfig(new EditorConfigSettingsBuilder()
                    .Including(DiagnosticId, "max_statement_count", "16")))
            .InGlobalScope("""
                class C
                {
                    int [|M|](string s)
                    {
                        ; ; ; ;
                        ; ; ; ;
                        ; ; ; ;
                        ; ; ; ;
                        throw null;
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'C.M(string)' contains 17 statements, which exceeds the maximum of 16 statements");
    }

    [Fact]
    internal async Task When_xml_file_is_corrupt_it_must_use_default_value()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .WithOptions(new AnalyzerOptionsBuilder()
                .ForXmlText("*** BAD XML ***"))
            .InGlobalScope("""
                class C
                {
                    int [|M|](string s)
                    {
                        ; ; ; ;
                        ; ; ;
                        throw null;
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'C.M(string)' contains 8 statements, which exceeds the maximum of 7 statements");
    }

    [Fact]
    internal async Task When_settings_are_missing_it_must_use_default_value()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .WithOptions(new AnalyzerOptionsBuilder()
                .ForEditorConfig(new EditorConfigSettingsBuilder()
                    .Including(DiagnosticId, "other-unused-setting", "some-value"))
                .ForXmlSettings(new XmlSettingsBuilder()
                    .Including(DiagnosticId, "OtherUnusedSetting", "SomeValue")))
            .InGlobalScope("""
                class C
                {
                    int [|M|](string s)
                    {
                        ; ; ; ;
                        ; ; ;
                        throw null;
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'C.M(string)' contains 8 statements, which exceeds the maximum of 7 statements");
    }

    [Fact]
    internal async Task When_xml_setting_value_is_missing_it_must_use_default_value()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .WithOptions(new AnalyzerOptionsBuilder()
                .ForXmlSettings(new XmlSettingsBuilder()
                    .Including(DiagnosticId, "MaxStatementCount", null)))
            .InGlobalScope("""
                class C
                {
                    int [|M|](string s)
                    {
                        ; ; ; ;
                        ; ; ;
                        throw null;
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'C.M(string)' contains 8 statements, which exceeds the maximum of 7 statements");
    }

    [Fact]
    internal async Task When_editor_config_setting_value_is_missing_it_must_use_default_value()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .WithOptions(new AnalyzerOptionsBuilder()
                .ForEditorConfig(new EditorConfigSettingsBuilder()
                    .Including(DiagnosticId, "max_statement_count", null)))
            .InGlobalScope("""
                class C
                {
                    int [|M|](string s)
                    {
                        ; ; ; ;
                        ; ; ;
                        throw null;
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'C.M(string)' contains 8 statements, which exceeds the maximum of 7 statements");
    }

    [Fact]
    internal async Task When_xml_setting_value_is_invalid_it_must_fail()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .WithOptions(new AnalyzerOptionsBuilder()
                .ForXmlSettings(new XmlSettingsBuilder()
                    .Including(DiagnosticId, "MaxStatementCount", "bad")))
            .InGlobalScope("""
                class C
                {
                    int [|M|](string s)
                    {
                        ; ; ; ;
                        ; ; ;
                        throw null;
                    }
                }
                """)
            .Build();

        // Act
        Func<Task> action = async () => await VerifyGuidelineDiagnosticAsync(source);

        // Assert
        await action.Should().ThrowAsync<Exception>()
            .WithMessage("*Value for 'AV1500:MaxStatementCount' in 'CSharpGuidelinesAnalyzer.config' must be in range 0-255.*");
    }

    [Fact]
    internal async Task When_editor_config_setting_value_is_invalid_it_must_fail()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .WithOptions(new AnalyzerOptionsBuilder()
                .ForEditorConfig(new EditorConfigSettingsBuilder()
                    .Including(DiagnosticId, "max_statement_count", "bad")))
            .InGlobalScope("""
                class C
                {
                    int [|M|](string s)
                    {
                        ; ; ; ;
                        ; ; ;
                        throw null;
                    }
                }
                """)
            .Build();

        // Act
        Func<Task> action = async () => await VerifyGuidelineDiagnosticAsync(source);

        // Assert
        await action.Should().ThrowAsync<Exception>()
            .WithMessage("*Value for 'dotnet_diagnostic.av1500.max_statement_count' in '.editorconfig' must be in range 0-255.*");
    }

    [Fact]
    internal async Task When_xml_setting_value_is_out_of_range_it_must_fail()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .WithOptions(new AnalyzerOptionsBuilder()
                .ForXmlSettings(new XmlSettingsBuilder()
                    .Including(DiagnosticId, "MaxStatementCount", "-1")))
            .InGlobalScope("""
                class C
                {
                    int [|M|](string s)
                    {
                        ; ; ; ;
                        ; ; ;
                        throw null;
                    }
                }
                """)
            .Build();

        // Act
        Func<Task> action = async () => await VerifyGuidelineDiagnosticAsync(source);

        // Assert
        await action.Should().ThrowAsync<Exception>()
            .WithMessage("*Value for 'AV1500:MaxStatementCount' in 'CSharpGuidelinesAnalyzer.config' must be in range 0-255.*");
    }

    [Fact]
    internal async Task When_editor_config_setting_value_is_out_of_range_it_must_fail()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .WithOptions(new AnalyzerOptionsBuilder()
                .ForEditorConfig(new EditorConfigSettingsBuilder()
                    .Including(DiagnosticId, "max_statement_count", "-1")))
            .InGlobalScope("""
                class C
                {
                    int [|M|](string s)
                    {
                        ; ; ; ;
                        ; ; ;
                        throw null;
                    }
                }
                """)
            .Build();

        // Act
        Func<Task> action = async () => await VerifyGuidelineDiagnosticAsync(source);

        // Assert
        await action.Should().ThrowAsync<Exception>()
            .WithMessage("*Value for 'dotnet_diagnostic.av1500.max_statement_count' in '.editorconfig' must be in range 0-255.*");
    }
}
