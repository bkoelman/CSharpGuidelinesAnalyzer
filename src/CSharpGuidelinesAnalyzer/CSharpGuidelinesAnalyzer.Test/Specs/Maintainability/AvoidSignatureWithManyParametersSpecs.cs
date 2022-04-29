using CSharpGuidelinesAnalyzer.Rules.Maintainability;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using FluentAssertions;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

// @formatter:keep_existing_linebreaks true

namespace CSharpGuidelinesAnalyzer.Test.Specs.Maintainability;

public sealed class AvoidSignatureWithManyParametersSpecs : CSharpGuidelinesAnalysisTestFixture
{
    protected override string DiagnosticId => AvoidSignatureWithManyParametersAnalyzer.DiagnosticId;

    [Fact]
    internal async Task When_method_contains_three_parameters_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M(int first, string second, double third)
                {
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_contains_four_parameters_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void [|M|](int first, string second, double third, object fourth)
                {
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'M' contains 4 parameters, which exceeds the maximum of 3 parameters");
    }

    [Fact]
    internal async Task When_method_contains_tuple_parameter_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M((int a, int b) [|p|])
                {
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'M' contains tuple parameter 'p'");
    }

    [Fact]
    internal async Task When_method_contains_system_tuple_parameter_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M(Tuple<int, int> [|p|])
                {
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'M' contains tuple parameter 'p'");
    }

    [Fact]
    internal async Task When_method_contains_array_of_tuple_parameter_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M((int a, int b)[] p)
                {
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_contains_collection_of_tuple_parameter_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(IList<>).Namespace)
            .InDefaultClass(@"
                void M(IList<(int a, int b)> p)
                {
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_returns_tuple_of_two_elements_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                (int, string) M()
                {
                    throw new NotImplementedException();
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_returns_tuple_of_three_elements_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                (int, string, bool) [|M|]()
                {
                    throw new NotImplementedException();
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'M' returns a tuple with 3 elements, which exceeds the maximum of 2 elements");
    }

    [Fact]
    internal async Task When_method_returns_system_tuple_of_two_elements_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                Tuple<int, string> M()
                {
                    throw new NotImplementedException();
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_returns_system_tuple_of_three_elements_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                Tuple<int, string, bool> [|M|]()
                {
                    throw new NotImplementedException();
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'M' returns a tuple with 3 elements, which exceeds the maximum of 2 elements");
    }

    [Fact]
    internal async Task When_method_is_extern_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                extern (int, string, object) M(int first, string second, double third, object fourth);
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_overrides_base_method_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class B
                {
                    protected virtual extern (int, string, object) M(int first, string second, double third, object fourth);
                }

                class C : B
                {
                    protected override (int, string, object) M(int first, string second, double third, object fourth) => throw null;
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_hides_base_method_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class B
                {
                    protected virtual extern (int, string, object) M(int first, string second, double third, object fourth);
                }

                class C : B
                {
                    protected new (int, string, object) M(int first, string second, double third, object fourth) => throw null;
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_implicitly_implements_interface_method_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                interface I
                {
#pragma warning disable AV1561
                    (int, string, object) M(int first, string second, double third, object fourth);
#pragma warning restore AV1561
                }

                class C : I
                {
                    public (int, string, object) M(int first, string second, double third, object fourth) => throw null;
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_method_explicitly_implements_interface_method_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                interface I
                {
#pragma warning disable AV1561
                    (int, string, object) M(int first, string second, double third, object fourth);
#pragma warning restore AV1561
                }

                class C : I
                {
                    (int, string, object) I.M(int first, string second, double third, object fourth) => throw null;
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_instance_constructor_contains_three_parameters_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class C
                {
                    public C(int first, string second, double third)
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_instance_constructor_contains_four_parameters_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class C
                {
                    public [|C|](int first, string second, double third, object fourth)
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Constructor for 'C' contains 4 parameters, which exceeds the maximum of 3 parameters");
    }

    [Fact]
    internal async Task When_instance_constructor_contains_tuple_parameter_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class C
                {
                    public C((int a, int b) [|p|])
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Constructor for 'C' contains tuple parameter 'p'");
    }

    [Fact]
    internal async Task When_instance_constructor_contains_system_tuple_parameter_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class C
                {
                    public C(Tuple<int, int> [|p|])
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Constructor for 'C' contains tuple parameter 'p'");
    }

    [Fact]
    internal async Task When_indexer_contains_three_parameters_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                public string this[int first, string second, double third]
                {
                    get { throw new NotImplementedException(); }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_indexer_contains_four_parameters_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                public string [|this|][int first, string second, double third, object fourth]
                {
                    get { throw new NotImplementedException(); }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Indexer contains 4 parameters, which exceeds the maximum of 3 parameters");
    }

    [Fact]
    internal async Task When_indexer_contains_tuple_parameter_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                public string this[(int a, int b) [|p|]]
                {
                    get { throw new NotImplementedException(); }
                    set { throw new NotImplementedException(); }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Indexer contains tuple parameter 'p'");
    }

    [Fact]
    internal async Task When_indexer_contains_system_tuple_parameter_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                public string this[Tuple<int, int> [|p|]]
                {
                    get { throw new NotImplementedException(); }
                    set { throw new NotImplementedException(); }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Indexer contains tuple parameter 'p'");
    }

    [Fact]
    internal async Task When_delegate_contains_three_parameters_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                public delegate void D(int first, string second, double third);
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_delegate_contains_four_parameters_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                public delegate void [|D|](int first, string second, double third, object fourth);
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Delegate 'D' contains 4 parameters, which exceeds the maximum of 3 parameters");
    }

    [Fact]
    internal async Task When_delegate_contains_tuple_parameter_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                public delegate void D((int a, int b) [|p|]);
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Delegate 'D' contains tuple parameter 'p'");
    }

    [Fact]
    internal async Task When_delegate_contains_system_tuple_parameter_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                public delegate void D(Tuple<int, int> [|p|]);
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Delegate 'D' contains tuple parameter 'p'");
    }

    [Fact]
    internal async Task When_delegate_returns_tuple_of_two_elements_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                public delegate (int, bool) D();
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_delegate_returns_tuple_of_three_elements_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                public delegate (int, string, int) [|D|]();
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Delegate 'D' returns a tuple with 3 elements, which exceeds the maximum of 2 elements");
    }

    [Fact]
    internal async Task When_delegate_returns_system_tuple_of_two_elements_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                public delegate Tuple<int, bool> D();
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_delegate_returns_system_tuple_of_three_elements_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                public delegate Tuple<int, string, int> [|D|]();
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Delegate 'D' returns a tuple with 3 elements, which exceeds the maximum of 2 elements");
    }

    [Fact]
    internal async Task When_local_function_contains_three_parameters_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M()
                {
                    void L(int first, string second, double third)
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_local_function_contains_four_parameters_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M()
                {
                    void [|L|](int first, string second, double third, object fourth)
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Local function 'L' contains 4 parameters, which exceeds the maximum of 3 parameters");
    }

    [Fact]
    internal async Task When_local_function_contains_tuple_parameter_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M()
                {
                    void L((int a, int b) [|p|])
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Local function 'L' contains tuple parameter 'p'");
    }

    [Fact]
    internal async Task When_local_function_contains_system_tuple_parameter_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M()
                {
                    void L(Tuple<int, int> [|p|])
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Local function 'L' contains tuple parameter 'p'");
    }

    [Fact]
    internal async Task When_local_function_returns_tuple_of_two_elements_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M()
                {
                    (int, string) L()
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
    internal async Task When_local_function_returns_tuple_of_three_elements_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M()
                {
                    (int, string, double) [|L|]()
                    {
                        throw new NotImplementedException();
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Local function 'L' returns a tuple with 3 elements, which exceeds the maximum of 2 elements");
    }

    [Fact]
    internal async Task When_local_function_returns_system_tuple_of_two_elements_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M()
                {
                    Tuple<int, string> L()
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
    internal async Task When_local_function_returns_system_tuple_of_three_elements_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M()
                {
                    Tuple<int, string, double> [|L|]()
                    {
                        throw new NotImplementedException();
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Local function 'L' returns a tuple with 3 elements, which exceeds the maximum of 2 elements");
    }

    #region Non-default configuration

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

    #endregion

    protected override DiagnosticAnalyzer CreateAnalyzer()
    {
        return new AvoidSignatureWithManyParametersAnalyzer();
    }
}
