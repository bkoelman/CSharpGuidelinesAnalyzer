using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using FluentAssertions;
using Xunit;

// @formatter:keep_existing_linebreaks true

namespace CSharpGuidelinesAnalyzer.Test.Specs.Maintainability;

public partial class AvoidSignatureWithManyParametersSpecs
{
    [Fact]
    internal async Task When_using_editor_config_setting_it_must_be_applied()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .WithOptions(new AnalyzerOptionsBuilder()
                .ForEditorConfig(new EditorConfigSettingsBuilder()
                    .Including(DiagnosticId, "max_parameter_count", "8")))
            .InGlobalScope(@"
                class C
                {
                    public [|C|](int first, string second, double third, float fourth, byte fifth, char sixth, DateTime seventh, TimeSpan eighth, ushort ninth)
                    {
                    }

                    void [|M|](int first, string second, double third, float fourth, byte fifth, char sixth, DateTime seventh, TimeSpan eighth, ushort ninth)
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Constructor for 'C' contains 9 parameters, which exceeds the maximum of 8 parameters",
            "Method 'M' contains 9 parameters, which exceeds the maximum of 8 parameters");
    }

    [Fact]
    internal async Task When_using_xml_config_setting_it_must_be_applied()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .WithOptions(new AnalyzerOptionsBuilder()
                .ForXmlSettings(new XmlSettingsBuilder()
                    .Including(DiagnosticId, "MaxParameterCount", "8")))
            .InGlobalScope(@"
                class C
                {
                    public [|C|](int first, string second, double third, float fourth, byte fifth, char sixth, DateTime seventh, TimeSpan eighth, ushort ninth)
                    {
                    }

                    void [|M|](int first, string second, double third, float fourth, byte fifth, char sixth, DateTime seventh, TimeSpan eighth, ushort ninth)
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Constructor for 'C' contains 9 parameters, which exceeds the maximum of 8 parameters",
            "Method 'M' contains 9 parameters, which exceeds the maximum of 8 parameters");
    }

    [Fact]
    internal async Task When_using_editor_config_setting_with_constructor_override_it_must_be_applied()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .WithOptions(new AnalyzerOptionsBuilder()
                .ForEditorConfig(new EditorConfigSettingsBuilder()
                    .Including(DiagnosticId, "max_parameter_count", "6")
                    .Including(DiagnosticId, "max_constructor_parameter_count", "8")))
            .InGlobalScope(@"
                class C
                {
                    public [|C|](int first, string second, double third, float fourth, byte fifth, char sixth, DateTime seventh, TimeSpan eighth, ushort ninth)
                    {
                    }

                    void [|M|](int first, string second, double third, float fourth, byte fifth, char sixth, DateTime seventh, TimeSpan eighth, ushort ninth)
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Constructor for 'C' contains 9 parameters, which exceeds the maximum of 8 parameters",
            "Method 'M' contains 9 parameters, which exceeds the maximum of 6 parameters");
    }

    [Fact]
    internal async Task When_using_xml_config_setting_with_constructor_override_it_must_be_applied()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .WithOptions(new AnalyzerOptionsBuilder()
                .ForXmlSettings(new XmlSettingsBuilder()
                    .Including(DiagnosticId, "MaxParameterCount", "6")
                    .Including(DiagnosticId, "MaxConstructorParameterCount", "8")))
            .InGlobalScope(@"
                class C
                {
                    public [|C|](int first, string second, double third, float fourth, byte fifth, char sixth, DateTime seventh, TimeSpan eighth, ushort ninth)
                    {
                    }

                    void [|M|](int first, string second, double third, float fourth, byte fifth, char sixth, DateTime seventh, TimeSpan eighth, ushort ninth)
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Constructor for 'C' contains 9 parameters, which exceeds the maximum of 8 parameters",
            "Method 'M' contains 9 parameters, which exceeds the maximum of 6 parameters");
    }

    [Fact]
    internal async Task When_using_xml_and_editor_config_settings_it_must_use_editor_config_values()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .WithOptions(new AnalyzerOptionsBuilder()
                .ForEditorConfig(new EditorConfigSettingsBuilder()
                    .Including(DiagnosticId, "max_parameter_count", "4")
                    .Including(DiagnosticId, "max_constructor_parameter_count", "3"))
                .ForXmlSettings(new XmlSettingsBuilder()
                    .Including(DiagnosticId, "MaxParameterCount", "8")
                    .Including(DiagnosticId, "MaxConstructorParameterCount", "7")))
            .InGlobalScope(@"
                class C
                {
                    public [|C|](int first, string second, double third, float fourth, byte fifth, char sixth, DateTime seventh, TimeSpan eighth, ushort ninth)
                    {
                    }

                    void [|M|](int first, string second, double third, float fourth, byte fifth, char sixth, DateTime seventh, TimeSpan eighth, ushort ninth)
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Constructor for 'C' contains 9 parameters, which exceeds the maximum of 3 parameters",
            "Method 'M' contains 9 parameters, which exceeds the maximum of 4 parameters");
    }

    [Fact]
    internal async Task When_xml_setting_value_is_out_of_range_it_must_fail()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .WithOptions(new AnalyzerOptionsBuilder()
                .ForXmlSettings(new XmlSettingsBuilder()
                    .Including(DiagnosticId, "MaxParameterCount", "-1")))
            .InDefaultClass(@"
                void [|M|](int first, string second, double third, float fourth, byte fifth, char sixth, DateTime seventh, TimeSpan eighth, ushort ninth)
                {
                }
            ")
            .Build();

        // Act
        Func<Task> action = async () => await VerifyGuidelineDiagnosticAsync(source);

        // Assert
        await action.Should().ThrowAsync<Exception>()
            .WithMessage("*Value for 'AV1561:MaxParameterCount' in 'CSharpGuidelinesAnalyzer.config' must be in range 0-255.*");
    }

    [Fact]
    internal async Task When_editor_config_setting_value_is_out_of_range_it_must_fail()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .WithOptions(new AnalyzerOptionsBuilder()
                .ForEditorConfig(new EditorConfigSettingsBuilder()
                    .Including(DiagnosticId, "max_parameter_count", "-1")))
            .InDefaultClass(@"
                void [|M|](int first, string second, double third, float fourth, byte fifth, char sixth, DateTime seventh, TimeSpan eighth, ushort ninth)
                {
                }
            ")
            .Build();

        // Act
        Func<Task> action = async () => await VerifyGuidelineDiagnosticAsync(source);

        // Assert
        await action.Should().ThrowAsync<Exception>()
            .WithMessage("*Value for 'dotnet_diagnostic.av1561.max_parameter_count' in '.editorconfig' must be in range 0-255.*");
    }

    [Fact]
    internal async Task When_xml_constructor_setting_value_is_out_of_range_it_must_fail()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .WithOptions(new AnalyzerOptionsBuilder()
                .ForXmlSettings(new XmlSettingsBuilder()
                    .Including(DiagnosticId, "MaxConstructorParameterCount", "-1")))
            .InDefaultClass(@"
                void [|M|](int first, string second, double third, float fourth, byte fifth, char sixth, DateTime seventh, TimeSpan eighth, ushort ninth)
                {
                }
            ")
            .Build();

        // Act
        Func<Task> action = async () => await VerifyGuidelineDiagnosticAsync(source);

        // Assert
        await action.Should().ThrowAsync<Exception>()
            .WithMessage("*Value for 'AV1561:MaxConstructorParameterCount' in 'CSharpGuidelinesAnalyzer.config' must be in range 0-255.*");
    }

    [Fact]
    internal async Task When_editor_config_constructor_setting_value_is_out_of_range_it_must_fail()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .WithOptions(new AnalyzerOptionsBuilder()
                .ForEditorConfig(new EditorConfigSettingsBuilder()
                    .Including(DiagnosticId, "max_constructor_parameter_count", "-1")))
            .InDefaultClass(@"
                void [|M|](int first, string second, double third, float fourth, byte fifth, char sixth, DateTime seventh, TimeSpan eighth, ushort ninth)
                {
                }
            ")
            .Build();

        // Act
        Func<Task> action = async () => await VerifyGuidelineDiagnosticAsync(source);

        // Assert
        await action.Should().ThrowAsync<Exception>()
            .WithMessage("*Value for 'dotnet_diagnostic.av1561.max_constructor_parameter_count' in '.editorconfig' must be in range 0-255.*");
    }
}
