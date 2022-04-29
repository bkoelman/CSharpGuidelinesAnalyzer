using CSharpGuidelinesAnalyzer.Rules.Maintainability;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

// @formatter:keep_existing_linebreaks true

namespace CSharpGuidelinesAnalyzer.Test.Specs.Maintainability;

public sealed class AvoidConditionWithDoubleNegationSpecs : CSharpGuidelinesAnalysisTestFixture
{
    protected override string DiagnosticId => AvoidConditionWithDoubleNegationAnalyzer.DiagnosticId;

    [Fact]
    internal async Task When_logical_not_operator_is_applied_on_a_method_with_Not_in_its_name_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class C
                {
                    public bool IsNotVisible()
                    {
                        return true;
                    }

                    public void M()
                    {
                        if ([|!IsNotVisible()|])
                        {
                        }
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Logical not operator is applied on method 'IsNotVisible', which has a negation in its name");
    }

    [Fact]
    internal async Task When_logical_not_operator_is_applied_on_a_method_with_No_in_its_name_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class C
                {
                    public bool IsNoActiveCustomer()
                    {
                        return true;
                    }

                    public void M()
                    {
                        if ([|!IsNoActiveCustomer()|])
                        {
                        }
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Logical not operator is applied on method 'IsNoActiveCustomer', which has a negation in its name");
    }

    [Fact]
    internal async Task When_logical_not_operator_is_applied_on_a_chained_method_with_No_in_its_name_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .Using(typeof(IEnumerable<>).Namespace)
            .InGlobalScope(@"
                class C
                {
                    public IEnumerable<string> GetNotActiveCustomerNames()
                    {
                        throw new NotImplementedException();
                    }

                    public void M()
                    {
                        if (!GetNotActiveCustomerNames().Any())
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
    internal async Task When_logical_not_operator_is_applied_on_a_normal_method_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class C
                {
                    public bool IsNotVisible()
                    {
                        return true;
                    }

                    public bool IsHidden() => IsNotVisible();

                    public void M()
                    {
                        if (!IsHidden())
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
    internal async Task When_logical_not_operator_is_applied_on_a_local_function_with_Not_in_its_name_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class C
                {
                    public void M()
                    {
                        bool IsNotVisible()
                        {
                            return true;
                        }

                        void L()
                        {
                            if ([|!IsNotVisible()|])
                            {
                            }
                        }
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Logical not operator is applied on local function 'IsNotVisible', which has a negation in its name");
    }

    [Fact]
    internal async Task When_logical_not_operator_is_applied_on_a_local_function_with_No_in_its_name_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class C
                {
                    public void M()
                    {
                        bool IsNoActiveCustomer()
                        {
                            return true;
                        }

                        void L()
                        {
                            if ([|!IsNoActiveCustomer()|])
                            {
                            }
                        }
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Logical not operator is applied on local function 'IsNoActiveCustomer', which has a negation in its name");
    }

    [Fact]
    internal async Task When_logical_not_operator_is_applied_on_a_chained_local_function_with_No_in_its_name_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .Using(typeof(IEnumerable<>).Namespace)
            .InGlobalScope(@"
                class C
                {
                    public void M()
                    {
                        IEnumerable<string> GetNotActiveCustomerNames()
                        {
                            throw new NotImplementedException();
                        }

                        void L()
                        {
                            if (!GetNotActiveCustomerNames().Any())
                            {
                            }
                        }
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_logical_not_operator_is_applied_on_a_normal_local_function_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class C
                {
                    public void M()
                    {
                        bool IsNotVisible()
                        {
                            return true;
                        }

                        bool IsHidden() => IsNotVisible();

                        void L()
                        {
                            if (!IsHidden())
                            {
                            }
                        }
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_logical_not_operator_is_applied_on_a_property_with_Not_in_its_name_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class C
                {
                    public bool IsNotVisible => true;

                    public void M()
                    {
                        if ([|!IsNotVisible|])
                        {
                        }
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Logical not operator is applied on property 'IsNotVisible', which has a negation in its name");
    }

    [Fact]
    internal async Task When_logical_not_operator_is_applied_on_a_property_with_No_in_its_name_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class C
                {
                    public bool IsNoActiveCustomer => true;

                    public void M()
                    {
                        if ([|!IsNoActiveCustomer|])
                        {
                        }
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Logical not operator is applied on property 'IsNoActiveCustomer', which has a negation in its name");
    }

    [Fact]
    internal async Task When_logical_not_operator_is_applied_on_a_normal_property_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class C
                {
                    public bool IsNotVisible => true;

                    public bool IsHidden => IsNotVisible;

                    public void M()
                    {
                        if (!IsHidden)
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
    internal async Task When_logical_not_operator_is_applied_on_a_field_with_Not_in_its_name_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class C
                {
                    public bool isNotVisible;

                    public void M()
                    {
                        if ([|!isNotVisible|])
                        {
                        }
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Logical not operator is applied on field 'isNotVisible', which has a negation in its name");
    }

    [Fact]
    internal async Task When_logical_not_operator_is_applied_on_a_field_with_No_in_its_name_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class C
                {
                    public bool isNoActiveCustomer;

                    public void M()
                    {
                        if ([|!isNoActiveCustomer|])
                        {
                        }
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Logical not operator is applied on field 'isNoActiveCustomer', which has a negation in its name");
    }

    [Fact]
    internal async Task When_logical_not_operator_is_applied_on_a_normal_field_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class C
                {
                    public bool isHidden;

                    public void M()
                    {
                        if (!isHidden)
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
    internal async Task When_logical_not_operator_is_applied_on_a_parameter_with_Not_in_its_name_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class C
                {
                    public void M(bool isNotVisible)
                    {
                        if ([|!isNotVisible|])
                        {
                        }
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Logical not operator is applied on parameter 'isNotVisible', which has a negation in its name");
    }

    [Fact]
    internal async Task When_logical_not_operator_is_applied_on_a_parameter_with_No_in_its_name_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class C
                {
                    public void M(bool isNoActiveCustomer)
                    {
                        if ([|!isNoActiveCustomer|])
                        {
                        }
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Logical not operator is applied on parameter 'isNoActiveCustomer', which has a negation in its name");
    }

    [Fact]
    internal async Task When_logical_not_operator_is_applied_on_a_normal_parameter_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class C
                {
                    public void M(bool isHidden)
                    {
                        if (!isHidden)
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
    internal async Task When_logical_not_operator_is_applied_on_a_variable_with_Not_in_its_name_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class C
                {
                    public void M()
                    {
                        bool isNotVisible = false;

                        if ([|!isNotVisible|])
                        {
                        }
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Logical not operator is applied on variable 'isNotVisible', which has a negation in its name");
    }

    [Fact]
    internal async Task When_logical_not_operator_is_applied_on_a_variable_with_No_in_its_name_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class C
                {
                    public void M()
                    {
                        bool isNoActiveCustomer = false;

                        if ([|!isNoActiveCustomer|])
                        {
                        }
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Logical not operator is applied on variable 'isNoActiveCustomer', which has a negation in its name");
    }

    [Fact]
    internal async Task When_logical_not_operator_is_applied_on_a_normal_variable_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class C
                {
                    public void M()
                    {
                        bool isHidden = false;

                        if (!isHidden)
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
    internal async Task When_logical_not_operator_is_applied_in_a_field_initializer_on_a_variable_with_No_in_its_name_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class C
                {
                    public bool IsHidden => [|!IsNotVisible()|];

                    bool IsNotVisible() => throw null;
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Logical not operator is applied on method 'IsNotVisible', which has a negation in its name");
    }

    [Fact]
    internal async Task When_logical_not_operator_is_applied_in_a_constructor_initializer_on_a_variable_with_No_in_its_name_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class C
                {
                    protected C(bool b)
                    {
                    }

                }

                class D : C
                {
                    public D()
                        : base([|!IsNotVisible()|])
                    {
                    }

                    static bool IsNotVisible() => throw null;
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Logical not operator is applied on method 'IsNotVisible', which has a negation in its name");
    }

    protected override DiagnosticAnalyzer CreateAnalyzer()
    {
        return new AvoidConditionWithDoubleNegationAnalyzer();
    }
}
