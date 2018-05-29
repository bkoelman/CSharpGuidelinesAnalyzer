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
        internal void When_nullable_value_type_is_compared_to_number_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M(int? i)
                    {
                        if (i > 5)
                        {
                        }

                        if (i < 5)
                        {
                        }

                        if (i <= 3)
                        {
                        }

                        if (i >= 3)
                        {
                        }

                        if (i == 8)
                        {
                        }

                        if (i != 8)
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_nullable_value_type_is_checked_for_null_and_compared_to_number_it_must_be_reported()
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

                        if ([|!(i is null) && i < 5|])
                        {
                        }

                        if ([|!i.Equals(null) && i <= 3|])
                        {
                        }

                        if ([|!object.Equals(i, null) && i >= 3|])
                        {
                        }

                        if ([|!object.ReferenceEquals(i, null) && i == 8|])
                        {
                        }

                        if ([|!EqualityComparer<int?>.Default.Equals(i, null) && i != 8|])
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

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new PreferLanguageSyntaxOverCallingImplementationAnalyzer();
        }
    }
}
