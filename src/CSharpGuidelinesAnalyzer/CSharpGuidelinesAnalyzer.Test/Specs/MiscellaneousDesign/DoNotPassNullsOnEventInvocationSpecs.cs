using CSharpGuidelinesAnalyzer.Rules.MiscellaneousDesign;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.MiscellaneousDesign
{
    public sealed class DoNotPassNullsOnEventInvocationSpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => DoNotPassNullsOnEventInvocationAnalyzer.DiagnosticId;

        [Fact]
        internal void When_sender_argument_is_null_in_nonstatic_event_invocation_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        event EventHandler ValueChanged;

                        void M()
                        {
                            ValueChanged([|null|], EventArgs.Empty);
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "'sender' argument is null in non-static event invocation.");
        }

        [Fact]
        internal void When_sender_argument_is_folded_null_in_nonstatic_event_invocation_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        private const object ConstantForNull = null;

                        event EventHandler ValueChanged;

                        void M()
                        {
                            ValueChanged([|ConstantForNull|], EventArgs.Empty);
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "'sender' argument is null in non-static event invocation.");
        }

        [Fact]
        internal void When_named_sender_argument_is_folded_null_in_nonstatic_event_invocation_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        private const object ConstantForNull = null;

                        event EventHandler ValueChanged;

                        void M()
                        {
                            ValueChanged(e: EventArgs.Empty, [|sender: ConstantForNull|]);
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "'sender' argument is null in non-static event invocation.");
        }

        [Fact]
        internal void When_sender_argument_is_null_in_static_event_invocation_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        static event EventHandler ValueChanged;

                        void M()
                        {
                            ValueChanged(null, EventArgs.Empty);
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_sender_argument_is_null_in_method_invocation_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void ValueChanged(object sender, EventArgs e)
                        {
                            throw new NotImplementedException();
                        }

                        void M()
                        {
                            ValueChanged(null, EventArgs.Empty);
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_sender_parameter_is_missing_in_event_type_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        public delegate void OtherEventHandler(string senderName, EventArgs e);

                        event OtherEventHandler ValueChanged;

                        void M()
                        {
                            ValueChanged(null, EventArgs.Empty);
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_sender_argument_is_null_in_null_conditional_nonstatic_event_invocation_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        event EventHandler ValueChanged;

                        void M()
                        {
                            ValueChanged?.Invoke([|null|], EventArgs.Empty);
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "'sender' argument is null in non-static event invocation.");
        }

        [Fact]
        internal void When_sender_argument_is_null_in_null_conditional_static_event_invocation_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        static event EventHandler ValueChanged;

                        void M()
                        {
                            ValueChanged?.Invoke(null, EventArgs.Empty);
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_both_arguments_are_provided_in_event_invocation_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        event EventHandler ValueChanged;

                        void M()
                        {
                            ValueChanged(this, EventArgs.Empty);
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_second_argument_is_null_in_nonstatic_event_invocation_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        event EventHandler ValueChanged;

                        void M()
                        {
                            ValueChanged(this, [|null|]);
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "'e' argument is null in event invocation.");
        }

        [Fact]
        internal void When_second_argument_is_folded_null_in_nonstatic_event_invocation_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        private const EventArgs ConstantForNull = null;

                        event EventHandler ValueChanged;

                        void M()
                        {
                            ValueChanged(this, [|ConstantForNull|]);
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "'e' argument is null in event invocation.");
        }

        [Fact]
        internal void When_named_second_argument_is_folded_null_in_nonstatic_event_invocation_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        private const EventArgs ConstantForNull = null;

                        event EventHandler ValueChanged;

                        void M()
                        {
                            ValueChanged([|e: ConstantForNull|], sender: this);
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "'e' argument is null in event invocation.");
        }

        [Fact]
        internal void When_second_argument_is_null_in_static_event_invocation_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        static event EventHandler ValueChanged;

                        void M()
                        {
                            ValueChanged(this, [|null|]);
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "'e' argument is null in event invocation.");
        }

        [Fact]
        internal void When_second_argument_is_null_in_method_invocation_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void ValueChanged(object sender, EventArgs e)
                        {
                            throw new NotImplementedException();
                        }

                        void M()
                        {
                            ValueChanged(this, null);
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_second_argument_of_derived_type_is_null_in_event_invocation_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public class OtherEventArgs : EventArgs
                    {
                    }

                    public delegate void OtherEventHandler(object sender, OtherEventArgs eventArguments);

                    class C
                    {
                        event OtherEventHandler ValueChanged;

                        void M()
                        {
                            ValueChanged(this, [|null|]);
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "'eventArguments' argument is null in event invocation.");
        }

        [Fact]
        internal void When_second_parameter_has_wrong_type_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        public delegate void OtherEventHandler(object sender, string data);

                        event OtherEventHandler ValueChanged;

                        void M()
                        {
                            ValueChanged(this, null);
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_second_argument_is_null_in_null_conditional_nonstatic_event_invocation_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        event EventHandler ValueChanged;

                        void M()
                        {
                            ValueChanged?.Invoke(this, [|null|]);
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "'e' argument is null in event invocation.");
        }

        [Fact]
        internal void When_second_argument_is_null_in_null_conditional_static_event_invocation_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        static event EventHandler ValueChanged;

                        void M()
                        {
                            ValueChanged?.Invoke(this, [|null|]);
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "'e' argument is null in event invocation.");
        }

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new DoNotPassNullsOnEventInvocationAnalyzer();
        }
    }
}
