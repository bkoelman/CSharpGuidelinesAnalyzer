using CSharpGuidelinesAnalyzer.Rules.Naming;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

// @formatter:keep_existing_linebreaks true

namespace CSharpGuidelinesAnalyzer.Test.Specs.Naming;

public sealed class DoNotIncludeContainingTypeNameInMemberNameSpecs : CSharpGuidelinesAnalysisTestFixture
{
    protected override string DiagnosticId => DoNotIncludeContainingTypeNameInMemberNameAnalyzer.DiagnosticId;

    [Fact]
    internal async Task When_method_name_contains_class_name_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                namespace N.M
                {
                    class Employee
                    {
                        static void [|GetEmployee|]()
                        {
                        }
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'GetEmployee' contains the name of its containing type 'Employee'");
    }

    [Fact]
    internal async Task When_method_name_does_not_contain_class_name_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                namespace N.M
                {
                    class Employee
                    {
                        static void Activate()
                        {
                        }
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_explicitly_implemented_method_name_contains_class_name_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                namespace N.M
                {
                    interface IEmployee
                    {
                        string GetEmployee();
                    }
                
                    class Employee : IEmployee
                    {
                        string IEmployee.[|GetEmployee|]() => throw null;
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'N.M.IEmployee.GetEmployee' contains the name of its containing type 'Employee'");
    }

    [Fact]
    internal async Task When_explicitly_implemented_method_name_does_not_contain_class_name_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                namespace N.M
                {
                    interface IEmployee
                    {
                        string GetName();
                    }
                
                    class Employee : IEmployee
                    {
                        string IEmployee.GetName() => throw null;
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_class_name_consists_of_a_single_letter_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                class C
                {
                    void IsC()
                    {
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_field_name_contains_struct_name_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                struct Customer
                {
                    bool [|IsCustomerActive|];
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Field 'IsCustomerActive' contains the name of its containing type 'Customer'");
    }

    [Fact]
    internal async Task When_field_name_does_not_contain_struct_name_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                struct Customer
                {
                    bool IsActive;
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_property_name_contains_class_name_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                class Order
                {
                    bool [|IsOrderDeleted|] { get; set; }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Property 'IsOrderDeleted' contains the name of its containing type 'Order'");
    }

    [Fact]
    internal async Task When_property_name_contains_generic_class_name_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                class Order<TCategory>
                {
                    bool [|IsOrderDeleted|] { get; set; }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Property 'IsOrderDeleted' contains the name of its containing type 'Order'");
    }

    [Fact]
    internal async Task When_property_name_does_not_contain_class_name_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                class Order
                {
                    bool IsDeleted { get; set; }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_User_class_contains_UserName_property_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                class User
                {
                    string UserName { get; set; }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_event_name_contains_class_name_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                class Registration
                {
                    event EventHandler [|RegistrationCompleted|]
                    {
                        add
                        {
                            throw new NotImplementedException();
                        }
                        remove
                        {
                            throw new NotImplementedException();
                        }
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Event 'RegistrationCompleted' contains the name of its containing type 'Registration'");
    }

    [Fact]
    internal async Task When_event_name_does_not_contain_class_name_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(NotImplementedException).Namespace)
            .InGlobalScope("""
                class Registration
                {
                    event EventHandler Completed
                    {
                        add
                        {
                            throw new NotImplementedException();
                        }
                        remove
                        {
                            throw new NotImplementedException();
                        }
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_enum_member_contains_enum_name_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                enum WindowState
                {
                    [|WindowStateVisible|]
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Field 'WindowStateVisible' contains the name of its containing type 'WindowState'");
    }

    [Fact]
    internal async Task When_enum_member_does_not_contain_enum_name_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                enum WindowState
                {
                    WindowVisible
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_class_contains_constructor_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                class Employee
                {
                    Employee()
                    {
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_class_contains_static_constructor_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                class Employee
                {
                    static Employee()
                    {
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_class_contains_destructor_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope("""
                class Employee
                {
                    ~Employee()
                    {
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    protected override DiagnosticAnalyzer CreateAnalyzer()
    {
        return new DoNotIncludeContainingTypeNameInMemberNameAnalyzer();
    }
}
