using System.Collections.Generic;
using CSharpGuidelinesAnalyzer.Rules.Framework;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.Framework
{
    public sealed class PreferLanguageSyntaxOverCallingImplementationSpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => PreferLanguageSyntaxOverCallingImplementationAnalyzer.DiagnosticId;

        [Fact]
        internal void When_nullable_value_type_is_checked_for_null_using_null_check_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M(int? i)
                    {
                        if (i == null)
                        {
                        }

                        if (i != null)
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_nullable_value_type_is_checked_for_null_using_Nullable_HasValue_property_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M(int? i)
                    {
                        if ([|i.HasValue|])
                        {
                        }

                        if (![|i.HasValue|])
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Replace call to Nullable<T>.HasValue with null check.",
                "Replace call to Nullable<T>.HasValue with null check.");
        }

        [Fact]
        internal void When_nullable_value_type_is_compared_to_numeric_constant_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M(int? i)
                    {
                        if (i > 5)
                        {
                        }

                        if (i < 5.0)
                        {
                        }

                        if (i <= 3.0f)
                        {
                        }

                        if (i >= 3.0m)
                        {
                        }

                        if (i == 8u)
                        {
                        }

                        if (i != 8L)
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_nullable_value_type_is_compared_to_numeric_type_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M(int? i)
                    {
                        if (i > GetNumber())
                        {
                        }

                        if (i < GetNumber())
                        {
                        }

                        if (i <= GetNumber())
                        {
                        }

                        if (i >= GetNumber())
                        {
                        }

                        if (i == GetNumber())
                        {
                        }

                        if (i != GetNumber())
                        {
                        }
                    }

                    int GetNumber() => throw null;
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_nullable_value_type_is_checked_for_null_in_various_ways_and_compared_to_number_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .Using(typeof(EqualityComparer<>).Namespace)
                .InDefaultClass(@"
                    void M(int? i)
                    {
                        if ([|i != null && i > 5|])
                        {
                        }

                        if ([|i.HasValue && i > 5|])
                        {
                        }

                        if ([|!(i is null) && i < 5|])
                        {
                        }

                        if ([|!i.Equals(null) && i <= 3|])
                        {
                        }

                        if ([|!object.Equals(i, null) && i >= 3|])
                        {
                        }

                        if ([|!object.ReferenceEquals(i, null) && i > 8|])
                        {
                        }

                        if ([|!EqualityComparer<int?>.Default.Equals(i, null) && i < 8|])
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Remove null check in numeric comparison.",
                "Remove null check in numeric comparison.",
                "Remove null check in numeric comparison.",
                "Remove null check in numeric comparison.",
                "Remove null check in numeric comparison.",
                "Remove null check in numeric comparison.",
                "Remove null check in numeric comparison.");
        }

        [Fact]
        internal void
            When_nullable_value_type_is_checked_for_null_in_various_ways_and_compared_to_number_value_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .Using(typeof(EqualityComparer<>).Namespace)
                .InDefaultClass(@"
                    void M(int? i)
                    {
                        if ([|i != null && i.Value > 5|])
                        {
                        }

                        if ([|!(i is null) && i.Value < 5|])
                        {
                        }

                        if ([|!i.Equals(null) && i.Value <= 3|])
                        {
                        }

                        if ([|!object.Equals(i, null) && i.Value >= 3|])
                        {
                        }

                        if ([|!object.ReferenceEquals(i, null) && i.Value == 8|])
                        {
                        }

                        if ([|!EqualityComparer<int?>.Default.Equals(i, null) && i.Value == 8|])
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Remove null check in numeric comparison.",
                "Remove null check in numeric comparison.",
                "Remove null check in numeric comparison.",
                "Remove null check in numeric comparison.",
                "Remove null check in numeric comparison.",
                "Remove null check in numeric comparison.");
        }

        [Fact]
        internal void When_nullable_value_type_is_checked_for_null_and_same_argument_is_compared_to_number_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        int? P { get; set; }

                        int? F;

                        void M(int? i, int?[] j, int?[] k, dynamic d)
                        {
                            if ([|i != null && i > 5|])
                            {
                            }

                            if ([|j[0] != null && j[0] > 5|])
                            {
                            }

                            if ([|k[i.Value + 1] != null && k[i.Value + 1] > 5|])
                            {
                            }

                            if ([|!(P is null) && P > 5|])
                            {
                            }

                            if ([|F != null && F > 5|])
                            {
                            }

                            int? l = i;
                            if ([|l != null && l > 5|])
                            {
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Remove null check in numeric comparison.",
                "Remove null check in numeric comparison.",
                "Remove null check in numeric comparison.",
                "Remove null check in numeric comparison.",
                "Remove null check in numeric comparison.",
                "Remove null check in numeric comparison.");
        }

        [Fact]
        internal void
            When_nullable_value_type_is_checked_for_null_and_same_argument_is_compared_to_number_value_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        int? P { get; set; }

                        int? F;

                        void M(int? i, int?[] j, int?[] k, dynamic d)
                        {
                            if ([|i != null && i.Value > 5|])
                            {
                            }

                            if ([|j[0] != null && j[0].Value > 5|])
                            {
                            }

                            if ([|k[i.Value + 1] != null && k[i.Value + 1].Value > 5|])
                            {
                            }

                            if ([|!(P is null) && P.Value > 5|])
                            {
                            }

                            if ([|F != null && F.Value > 5|])
                            {
                            }

                            int? l = i;
                            if ([|l != null && l.Value > 5|])
                            {
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Remove null check in numeric comparison.",
                "Remove null check in numeric comparison.",
                "Remove null check in numeric comparison.",
                "Remove null check in numeric comparison.",
                "Remove null check in numeric comparison.",
                "Remove null check in numeric comparison.");
        }

        [Fact]
        internal void
            When_nullable_value_type_is_checked_for_null_and_different_argument_is_compared_to_number_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void M(int? i, int? other, int?[] array, dynamic k)
                        {
                            if (i != null && other > 5)
                            {
                            }

                            if (array[0] != null && array[1] > 5)
                            {
                            }

                            if (k.M() != null && k.M() > 5)
                            {
                            }

                            if (N() != null && N() > 5)
                            {
                            }

                            if (this != null && this > 5)
                            {
                            }
                        }

                        int? N() => throw null;

                        public static bool operator >(C left, int right) => throw null;
                        public static bool operator <(C left, int right) => throw null;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void
            When_nullable_value_type_is_checked_for_null_and_same_argument_is_compared_for_non_equality_to_nullable_value_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void M(int? i, int? j)
                        {
                            if (i != null && i == j)
                            {
                            }

                            if (i != null && i.Value == j)
                            {
                            }

                            if (i != null && i != j)
                            {
                            }

                            if (i != null && i.Value != j)
                            {
                            }

                            if (i != null && i != 123)
                            {
                            }

                            if (i != null && i.Value != 123)
                            {
                            }

                            if ([|i != null && i > j|])
                            {
                            }

                            if ([|i != null && i.Value > j|])
                            {
                            }

                            if ([|i != null && i >= j|])
                            {
                            }

                            if ([|i != null && i.Value >= j|])
                            {
                            }

                            if ([|i != null && i < j|])
                            {
                            }

                            if ([|i != null && i.Value < j|])
                            {
                            }

                            if ([|i != null && i <= j|])
                            {
                            }

                            if ([|i != null && i.Value <= j|])
                            {
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Remove null check in numeric comparison.",
                "Remove null check in numeric comparison.",
                "Remove null check in numeric comparison.",
                "Remove null check in numeric comparison.",
                "Remove null check in numeric comparison.",
                "Remove null check in numeric comparison.",
                "Remove null check in numeric comparison.",
                "Remove null check in numeric comparison.");
        }

        [Fact]
        internal void
            When_nullable_type_is_used_in_nameof_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void M(string s)
                        {
                            if (s != nameof(Nullable<int>.HasValue))
                            {
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new PreferLanguageSyntaxOverCallingImplementationAnalyzer();
        }
    }
}
