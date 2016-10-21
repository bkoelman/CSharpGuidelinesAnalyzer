using CSharpGuidelinesAnalyzer.ClassDesign;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.ClassDesign
{
    public class DoNotHideInheritedMembersSpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => DoNotHideInheritedMembersAnalyzer.DiagnosticId;

        [Fact]
        public void When_base_property_is_overridden_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .InGlobalScope(@"
                    class B
                    {
                        public virtual string P
                        {
                            get
                            {
                                throw new NotImplementedException();
                            }
                            set
                            {
                                throw new NotImplementedException();
                            }
                        }
                    }

                    class C : B
                    {
                        public override string P
                        {
                            get
                            {
                                throw new NotImplementedException();
                            }
                            set
                            {
                                throw new NotImplementedException();
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        public void When_base_property_is_hidden_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .InGlobalScope(@"
                    class B
                    {
                        public virtual string P
                        {
                            get
                            {
                                throw new NotImplementedException();
                            }
                            set
                            {
                                throw new NotImplementedException();
                            }
                        }
                    }

                    class C : B
                    {
                        public new string [|P|]
                        {
                            get
                            {
                                throw new NotImplementedException();
                            }
                            set
                            {
                                throw new NotImplementedException();
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "'C.P' hides inherited member.");
        }

        [Fact]
        public void When_base_method_is_overridden_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .InGlobalScope(@"
                    class B
                    {
                        public virtual void M(int i)
                        {
                        }
                    }

                    class C : B
                    {
                        public override void M(int i)
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        public void When_base_method_is_hidden_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .InGlobalScope(@"
                    class B
                    {
                        public virtual void M(int i)
                        {
                        }
                    }

                    class C : B
                    {
                        public new void [|M|](int i)
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "'C.M(int)' hides inherited member.");
        }

        [Fact]
        public void When_base_event_is_overridden_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .InGlobalScope(@"
                    class B
                    {
                        public virtual event EventHandler Changed
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

                    class C : B
                    {
                        public override event EventHandler Changed
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
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        public void When_base_event_is_hidden_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .InGlobalScope(@"
                    class B
                    {
                        public virtual event EventHandler Changed
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

                    class C : B
                    {
                        public new event EventHandler [|Changed|]
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
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "'C.Changed' hides inherited member.");
        }

        [Fact]
        public void When_default_base_event_is_hidden_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .InGlobalScope(@"
                    class B
                    {
                        public virtual event EventHandler Changed;
                    }

                    class C : B
                    {
                        public new event EventHandler [|Changed|];
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "'C.Changed' hides inherited member.");
        }

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new DoNotHideInheritedMembersAnalyzer();
        }
    }
}