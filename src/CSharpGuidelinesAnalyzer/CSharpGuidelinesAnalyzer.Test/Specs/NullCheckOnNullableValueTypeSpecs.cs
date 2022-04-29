#if DEBUG
using CSharpGuidelinesAnalyzer.Rules;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

// @formatter:keep_existing_linebreaks true

namespace CSharpGuidelinesAnalyzer.Test.Specs;

public sealed class NullCheckOnNullableValueTypeSpecs : CSharpGuidelinesAnalysisTestFixture
{
    protected override string DiagnosticId => NullCheckOnNullableValueTypeAnalyzer.DiagnosticId;

    [Fact]
    internal async Task When_enum_type_is_checked_for_null_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                enum E { A, B, C }

                void M(E e)
                {
                    if (e == null)
                    {
                    }

                    if (null == e)
                    {
                    }

                    if (e == default)
                    {
                    }

                    if (e == default(E))
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_enum_type_is_checked_for_not_null_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                enum E { A, B, C }

                void M(E e)
                {
                    if (e != null)
                    {
                    }

                    if (null != e)
                    {
                    }

                    if (e != default)
                    {
                    }

                    if (e != default(E))
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_value_type_is_checked_for_null_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M(int i)
                {
                    if (i == null)
                    {
                    }

                    if (null == i)
                    {
                    }

                    if (i == default)
                    {
                    }

                    if (i == default(int))
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_value_type_is_checked_for_not_null_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M(int i)
                {
                    if (i != null)
                    {
                    }

                    if (null != i)
                    {
                    }

                    if (i != default)
                    {
                    }

                    if (i != default(int))
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_reference_type_is_checked_for_null_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M(string s)
                {
                    if (s == null)
                    {
                    }

                    if (null == s)
                    {
                    }

                    if (s is null)
                    {
                    }

                    if (s == default)
                    {
                    }

                    if (s == default(string))
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_reference_type_is_checked_for_not_null_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M(string s)
                {
                    if (s != null)
                    {
                    }

                    if (null != s)
                    {
                    }

                    if (!(s is null))
                    {
                    }

                    if (s != default)
                    {
                    }

                    if (s != default(string))
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_interface_type_is_checked_for_null_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                interface I<T>
                {
                }

                class X : I<int?>
                {
                }

                void M(I<int?> i)
                {
                    if (i == null)
                    {
                    }

                    if (null == i)
                    {
                    }

                    if (i is null)
                    {
                    }

                    if (i == default)
                    {
                    }

                    if (i == default(I<int?>))
                    {
                    }

                    if (i is X x)
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_interface_type_is_checked_for_not_null_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                interface I<T>
                {
                }

                void M(I<int?> i)
                {
                    if (i != null)
                    {
                    }

                    if (null != i)
                    {
                    }

                    if (!(i is null))
                    {
                    }

                    if (i != default)
                    {
                    }

                    if (i != default(I<int?>))
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_delegate_type_is_checked_for_null_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                delegate void D();

                void M(D d)
                {
                    if (d == null)
                    {
                    }

                    if (null == d)
                    {
                    }

                    if (d is null)
                    {
                    }

                    if (d == default)
                    {
                    }

                    if (d == default(D))
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_delegate_type_is_checked_for_not_null_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                delegate void D();

                void M(D d)
                {
                    if (d != null)
                    {
                    }

                    if (null != d)
                    {
                    }

                    if (!(d is null))
                    {
                    }

                    if (d != default)
                    {
                    }

                    if (d != default(D))
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_nullable_value_type_is_checked_for_null_using_equality_operator_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M(int? i, int? j, int? k, int? l, int? m, int? n)
                {
                    if ([|i|] == null)
                    {
                    }

                    if (null == [|j|])
                    {
                    }

                    if ([|k|] == default)
                    {
                    }

                    if ([|l|] == default(int?))
                    {
                    }

                    if (!([|m|] != null))
                    {
                    }

                    if (!(([|n|] == null) && j > 0))
                    {
                    }

                    if (i == 0 || j >= 0)
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Expression of nullable value type 'i' is checked for null using EqualityOperator",
            "Expression of nullable value type 'j' is checked for null using EqualityOperator",
            "Expression of nullable value type 'k' is checked for null using EqualityOperator",
            "Expression of nullable value type 'l' is checked for null using EqualityOperator",
            "Expression of nullable value type 'm' is checked for null using EqualityOperator",
            "Expression of nullable value type 'n' is checked for null using EqualityOperator");
    }

    [Fact]
    internal async Task When_nullable_value_type_is_checked_for_not_null_using_equality_operator_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M(int? i, int? j, int? k, int? l, int? m)
                {
                    if ([|i|] != null)
                    {
                    }

                    if (null != [|j|])
                    {
                    }

                    if ([|k|] != default)
                    {
                    }

                    if ([|l|] != default(int?))
                    {
                    }

                    if (!([|m|] == null))
                    {
                    }

                    if (i != 0 || j < 0)
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Expression of nullable value type 'i' is checked for not-null using EqualityOperator",
            "Expression of nullable value type 'j' is checked for not-null using EqualityOperator",
            "Expression of nullable value type 'k' is checked for not-null using EqualityOperator",
            "Expression of nullable value type 'l' is checked for not-null using EqualityOperator",
            "Expression of nullable value type 'm' is checked for not-null using EqualityOperator");
    }

    [Fact]
    internal async Task When_nullable_value_type_is_checked_for_null_using_is_operator_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M(int? i)
                {
                    if ([|i|] is null)
                    {
                    }

                    if (i is 0)
                    {
                    }

                    if (i is int x)
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Expression of nullable value type 'i' is checked for null using IsPattern");
    }

    [Fact]
    internal async Task When_nullable_value_type_is_checked_for_not_null_using_is_operator_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M(int? i)
                {
                    if (!([|i|] is null))
                    {
                    }

                    if (!(i is 0))
                    {
                    }

                    if (!(i is int x))
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Expression of nullable value type 'i' is checked for not-null using IsPattern");
    }

    [Fact]
    internal async Task When_nullable_value_type_is_checked_for_null_using_HasValue_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M(int? i)
                {
                    if ([|i|].HasValue)
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Expression of nullable value type 'i' is checked for not-null using NullableHasValueMethod");
    }

    [Fact]
    internal async Task When_nullable_value_type_is_checked_for_not_null_using_HasValue_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M(int? i)
                {
                    if (![|i|].HasValue)
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Expression of nullable value type 'i' is checked for null using NullableHasValueMethod");
    }

    [Fact]
    internal async Task When_nullable_value_type_is_checked_for_null_using_Equals_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M(int? i, int? j, int? k)
                {
                    if ([|i|].Equals(null))
                    {
                    }

                    if ([|j|].Equals(default))
                    {
                    }

                    if ([|k|].Equals(default(int?)))
                    {
                    }

                    if (i.Equals(0))
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Expression of nullable value type 'i' is checked for null using NullableEqualsMethod",
            "Expression of nullable value type 'j' is checked for null using NullableEqualsMethod",
            "Expression of nullable value type 'k' is checked for null using NullableEqualsMethod");
    }

    [Fact]
    internal async Task When_nullable_value_type_is_checked_for_not_null_using_Equals_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M(int? i, int? j, int? k)
                {
                    if (![|i|].Equals(null))
                    {
                    }

                    if (![|j|].Equals(default))
                    {
                    }

                    if (![|k|].Equals(default(int?)))
                    {
                    }

                    if (!i.Equals(0))
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Expression of nullable value type 'i' is checked for not-null using NullableEqualsMethod",
            "Expression of nullable value type 'j' is checked for not-null using NullableEqualsMethod",
            "Expression of nullable value type 'k' is checked for not-null using NullableEqualsMethod");
    }

    [Fact]
    internal async Task When_nullable_value_type_is_checked_for_null_using_ObjectEquals_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M(int? i, int? j, int? k, int? l)
                {
                    if (object.Equals([|i|], null))
                    {
                    }

                    if (object.Equals(null, [|j|]))
                    {
                    }

                    if (object.Equals([|k|], default))
                    {
                    }

                    if (object.Equals([|l|], default(int?)))
                    {
                    }

                    if (object.Equals(i, 0) || object.Equals(0, j))
                    {
                    }

                    if (object.Equals(null, null) || object.Equals(0, 0))
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Expression of nullable value type 'i' is checked for null using StaticObjectEqualsMethod",
            "Expression of nullable value type 'j' is checked for null using StaticObjectEqualsMethod",
            "Expression of nullable value type 'k' is checked for null using StaticObjectEqualsMethod",
            "Expression of nullable value type 'l' is checked for null using StaticObjectEqualsMethod");
    }

    [Fact]
    internal async Task When_nullable_value_type_is_checked_for_not_null_using_ObjectEquals_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M(int? i, int? j, int? k, int? l)
                {
                    if (!object.Equals([|i|], null))
                    {
                    }

                    if (!object.Equals(null, [|j|]))
                    {
                    }

                    if (!object.Equals([|k|], default))
                    {
                    }

                    if (!object.Equals([|l|], default(int?)))
                    {
                    }

                    if (!object.Equals(i, 0) || !object.Equals(0, j))
                    {
                    }

                    if (!object.Equals(null, null) || !object.Equals(0, 0))
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Expression of nullable value type 'i' is checked for not-null using StaticObjectEqualsMethod",
            "Expression of nullable value type 'j' is checked for not-null using StaticObjectEqualsMethod",
            "Expression of nullable value type 'k' is checked for not-null using StaticObjectEqualsMethod",
            "Expression of nullable value type 'l' is checked for not-null using StaticObjectEqualsMethod");
    }

    [Fact]
    internal async Task When_nullable_value_type_is_checked_for_null_using_ReferenceEquals_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M(int? i, int? j, int? k, int? l)
                {
                    if (ReferenceEquals([|i|], null))
                    {
                    }

                    if (ReferenceEquals(null, [|j|]))
                    {
                    }

                    if (ReferenceEquals([|k|], default))
                    {
                    }

                    if (ReferenceEquals([|l|], default(int?)))
                    {
                    }

                    if (ReferenceEquals(i, 0) || ReferenceEquals(j, 0))
                    {
                    }

                    if (ReferenceEquals(null, null))
                    {
                    }

                    if (ReferenceEquals(0, 0))
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Expression of nullable value type 'i' is checked for null using StaticObjectReferenceEqualsMethod",
            "Expression of nullable value type 'j' is checked for null using StaticObjectReferenceEqualsMethod",
            "Expression of nullable value type 'k' is checked for null using StaticObjectReferenceEqualsMethod",
            "Expression of nullable value type 'l' is checked for null using StaticObjectReferenceEqualsMethod");
    }

    [Fact]
    internal async Task When_nullable_value_type_is_checked_for_not_null_using_ReferenceEquals_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M(int? i, int? j, int? k, int? l)
                {
                    if (!ReferenceEquals([|i|], null))
                    {
                    }

                    if (!ReferenceEquals(null, [|j|]))
                    {
                    }

                    if (!ReferenceEquals([|k|], default))
                    {
                    }

                    if (!ReferenceEquals([|l|], default(int?)))
                    {
                    }

                    if (!ReferenceEquals(i, 0) || !ReferenceEquals(j, 0))
                    {
                    }

                    if (!ReferenceEquals(null, null))
                    {
                    }

                    if (!ReferenceEquals(0, 0))
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Expression of nullable value type 'i' is checked for not-null using StaticObjectReferenceEqualsMethod",
            "Expression of nullable value type 'j' is checked for not-null using StaticObjectReferenceEqualsMethod",
            "Expression of nullable value type 'k' is checked for not-null using StaticObjectReferenceEqualsMethod",
            "Expression of nullable value type 'l' is checked for not-null using StaticObjectReferenceEqualsMethod");
    }

    [Fact]
    internal async Task When_nullable_value_type_is_checked_for_null_using_ObjectReferenceEquals_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M(int? i, int? j, int? k, int? l)
                {
                    if (object.ReferenceEquals([|i|], null))
                    {
                    }

                    if (object.ReferenceEquals(null, [|j|]))
                    {
                    }

                    if (!!object.ReferenceEquals([|k|], default))
                    {
                    }

                    if (object.ReferenceEquals([|l|], default(int?)))
                    {
                    }

                    if (object.ReferenceEquals(i, 0) || object.ReferenceEquals(j, 0))
                    {
                    }

                    if (object.ReferenceEquals(null, null))
                    {
                    }

                    if (object.ReferenceEquals(0, 0))
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Expression of nullable value type 'i' is checked for null using StaticObjectReferenceEqualsMethod",
            "Expression of nullable value type 'j' is checked for null using StaticObjectReferenceEqualsMethod",
            "Expression of nullable value type 'k' is checked for null using StaticObjectReferenceEqualsMethod",
            "Expression of nullable value type 'l' is checked for null using StaticObjectReferenceEqualsMethod");
    }

    [Fact]
    internal async Task When_nullable_value_type_is_checked_for_not_null_using_ObjectReferenceEquals_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M(int? i, int? j, int? k, int? l)
                {
                    if (!object.ReferenceEquals([|i|], null))
                    {
                    }

                    if (!!!object.ReferenceEquals(null, [|j|]))
                    {
                    }

                    if (!object.ReferenceEquals([|k|], default))
                    {
                    }

                    if (!object.ReferenceEquals([|l|], default(int?)))
                    {
                    }

                    if (!object.ReferenceEquals(i, 0) || !object.ReferenceEquals(j, 0))
                    {
                    }

                    if (!object.ReferenceEquals(null, null))
                    {
                    }

                    if (!object.ReferenceEquals(0, 0))
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Expression of nullable value type 'i' is checked for not-null using StaticObjectReferenceEqualsMethod",
            "Expression of nullable value type 'j' is checked for not-null using StaticObjectReferenceEqualsMethod",
            "Expression of nullable value type 'k' is checked for not-null using StaticObjectReferenceEqualsMethod",
            "Expression of nullable value type 'l' is checked for not-null using StaticObjectReferenceEqualsMethod");
    }

    [Fact]
    internal async Task When_nullable_value_type_is_checked_for_null_using_DefaultEqualityComparer_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(EqualityComparer<>).Namespace)
            .InDefaultClass(@"
                void M(int? i, int? j, int? k, int? l)
                {
                    if (EqualityComparer<int?>.Default.Equals([|i|], null))
                    {
                    }

                    if (EqualityComparer<int?>.Default.Equals(null, [|j|]))
                    {
                    }

                    if (EqualityComparer<int?>.Default.Equals([|k|], default))
                    {
                    }

                    if (EqualityComparer<int?>.Default.Equals([|l|], default(int?)))
                    {
                    }

                    if (EqualityComparer<int?>.Default.Equals(i, 0) || EqualityComparer<int?>.Default.Equals(j, 0))
                    {
                    }

                    if (EqualityComparer<int?>.Default.Equals(null, null))
                    {
                    }

                    if (EqualityComparer<int?>.Default.Equals(0, 0))
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Expression of nullable value type 'i' is checked for null using EqualityComparerEqualsMethod",
            "Expression of nullable value type 'j' is checked for null using EqualityComparerEqualsMethod",
            "Expression of nullable value type 'k' is checked for null using EqualityComparerEqualsMethod",
            "Expression of nullable value type 'l' is checked for null using EqualityComparerEqualsMethod");
    }

    [Fact]
    internal async Task When_nullable_value_type_is_checked_for_not_null_using_DefaultEqualityComparer_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(EqualityComparer<>).Namespace)
            .InDefaultClass(@"
                void M(int? i, int? j, int? k, int? l)
                {
                    if (!EqualityComparer<int?>.Default.Equals([|i|], null))
                    {
                    }

                    if (!EqualityComparer<int?>.Default.Equals(null, [|j|]))
                    {
                    }

                    if (!EqualityComparer<int?>.Default.Equals([|k|], default))
                    {
                    }

                    if (!EqualityComparer<int?>.Default.Equals([|l|], default(int?)))
                    {
                    }

                    if (!EqualityComparer<int?>.Default.Equals(i, 0) || !EqualityComparer<int?>.Default.Equals(j, 0))
                    {
                    }

                    if (!EqualityComparer<int?>.Default.Equals(null, null))
                    {
                    }

                    if (!EqualityComparer<int?>.Default.Equals(0, 0))
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Expression of nullable value type 'i' is checked for not-null using EqualityComparerEqualsMethod",
            "Expression of nullable value type 'j' is checked for not-null using EqualityComparerEqualsMethod",
            "Expression of nullable value type 'k' is checked for not-null using EqualityComparerEqualsMethod",
            "Expression of nullable value type 'l' is checked for not-null using EqualityComparerEqualsMethod");
    }

    [Fact]
    internal async Task When_nullable_value_type_is_checked_for_null_using_cast_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M(int? i, int? j, int? k)
                {
                    if ((object)[|i|] == null)
                    {
                    }

                    if (null == (object)[|j|])
                    {
                    }

                    if ((object)[|k|] == default)
                    {
                    }

                    if ((double)i == 0 || (decimal)j == 0)
                    {
                    }

                    if ((object)null == (object)null)
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Expression of nullable value type 'i' is checked for null using EqualityOperator",
            "Expression of nullable value type 'j' is checked for null using EqualityOperator",
            "Expression of nullable value type 'k' is checked for null using EqualityOperator");
    }

    [Fact]
    internal async Task When_nullable_value_type_is_checked_for_not_null_using_cast_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M(int? i, int? j, int? k)
                {
                    if ((object)[|i|] != null)
                    {
                    }

                    if (null != (object)[|j|])
                    {
                    }

                    if ((object)[|k|] != default)
                    {
                    }

                    if ((double)i != 0 || (decimal)j != 0)
                    {
                    }

                    if ((object)null != (object)null)
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Expression of nullable value type 'i' is checked for not-null using EqualityOperator",
            "Expression of nullable value type 'j' is checked for not-null using EqualityOperator",
            "Expression of nullable value type 'k' is checked for not-null using EqualityOperator");
    }

    [Fact]
    internal async Task When_nullable_value_type_is_checked_for_null_using_case_statement_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M(int? i, int? j, int? k)
                {
                    switch (i)
                    {
                        case null:
                        {
                            break;
                        }
                    }

                    switch (j)
                    {
                        case int x when x != null:
                        {
                            break;
                        }
                    }

                    switch (k)
                    {
                        case 0:
                        {
                            break;
                        }
                        default:
                        {
                            break;
                        }
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_variable_of_nullable_value_type_is_checked_for_null_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M(int? i)
                {
                    int? v = i;

                    if ([|v|] == null)
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Expression of nullable value type 'v' is checked for null using EqualityOperator");
    }

    [Fact]
    internal async Task When_tuple_element_of_nullable_value_type_is_checked_for_null_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M(int? i)
                {
                    (int? v, int? w) tuple = (i, i);

                    if ([|tuple.v|] == null)
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Expression of nullable value type 'tuple.v' is checked for null using EqualityOperator");
    }

    [Fact]
    internal async Task When_range_variable_of_nullable_value_type_is_checked_for_null_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .InDefaultClass(@"
                void M()
                {
                    var query =
                        from item in Enumerable.Empty<int?>()
                        let rangeVar = item
                        where [|rangeVar|] == null
                        select item;
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Expression of nullable value type 'rangeVar' is checked for null using EqualityOperator");
    }

    [Fact]
    internal async Task When_field_of_nullable_value_type_is_checked_for_null_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                int? f;

                void M()
                {
                    if ([|f|] == null)
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Expression of nullable value type 'f' is checked for null using EqualityOperator");
    }

    [Fact]
    internal async Task When_property_of_nullable_value_type_is_checked_for_null_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                int? P { get; set; }

                void M()
                {
                    if ([|P|] == null)
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Expression of nullable value type 'P' is checked for null using EqualityOperator");
    }

    [Fact]
    internal async Task When_local_function_parameter_of_nullable_value_type_is_checked_for_null_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M()
                {
                    void L(int? i)
                    {
                        if ([|i|] == null)
                        {
                        }
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Expression of nullable value type 'i' is checked for null using EqualityOperator");
    }

    [Fact]
    internal async Task When_local_function_return_value_of_nullable_value_type_is_checked_for_null_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M()
                {
                    if ([|L()|] == null)
                    {
                    }

                    int? L() => throw null;
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Expression of nullable value type 'L()' is checked for null using EqualityOperator");
    }

    [Fact]
    internal async Task When_method_return_value_of_nullable_value_type_is_checked_for_null_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .InDefaultClass(@"
                void M()
                {
                    if ([|N(string.Empty)|] == null)
                    {
                    }
                }

                int? N(string s) => throw null;

            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Expression of nullable value type 'N(string.Empty)' is checked for null using EqualityOperator");
    }

    protected override DiagnosticAnalyzer CreateAnalyzer()
    {
        return new NullCheckOnNullableValueTypeAnalyzer();
    }
}
#endif
