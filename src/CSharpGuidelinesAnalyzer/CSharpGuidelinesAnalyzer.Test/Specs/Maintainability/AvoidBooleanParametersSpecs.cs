using System;
using CSharpGuidelinesAnalyzer.Maintainability;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.Maintainability
{
    public class AvoidBooleanParametersSpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => AvoidBooleanParametersAnalyzer.DiagnosticId;

        [Fact]
        public void When_method_parameter_type_is_bool_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M(bool [|b|])
                    {
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Parameter b is of type bool, which should be avoided.");
        }

        [Fact]
        public void When_method_parameter_type_is_nullable_bool_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M(bool? [|b|])
                    {
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Parameter b is of type bool?, which should be avoided.");
        }

        [Fact]
        public void When_method_parameter_type_is_string_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    void M(string s)
                    {
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        public void When_property_is_bool_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    bool B { get; set; }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        public void When_property_is_nullable_bool_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .InDefaultClass(@"
                    bool? B { get; set; }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        public void When_indexer_parameter_type_is_bool_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .Using(typeof (NotImplementedException).Namespace)
                .InDefaultClass(@"
                    int this[bool [|b|]]
                    {
                        get { throw new NotImplementedException(); }
                        set { throw new NotImplementedException(); }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Parameter b is of type bool, which should be avoided.");
        }

        [Fact]
        public void When_indexer_parameter_type_is_nullable_bool_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .Using(typeof (NotImplementedException).Namespace)
                .InDefaultClass(@"
                    int this[bool? [|b|]]
                    {
                        get { throw new NotImplementedException(); }
                        set { throw new NotImplementedException(); }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Parameter b is of type bool?, which should be avoided.");
        }

        [Fact]
        public void When_method_parameter_type_is_bool_in_overridden_method_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .InGlobalScope(@"
                    public class C
                    {
                        protected virtual void M(bool [|b|])
                        {
                        }
                    }

                    public class D : C
                    {
                        protected override void M(bool b)
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Parameter b is of type bool, which should be avoided.");
        }

        [Fact]
        public void When_method_parameter_type_is_nullable_bool_in_overridden_method_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .InGlobalScope(@"
                    public class C
                    {
                        protected virtual void M(bool? [|b|])
                        {
                        }
                    }

                    public class D : C
                    {
                        protected override void M(bool? b)
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Parameter b is of type bool?, which should be avoided.");
        }

        [Fact]
        public void When_method_parameter_type_is_bool_in_hidden_method_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .InGlobalScope(@"
                    public class C
                    {
                        protected virtual void M(bool [|b|])
                        {
                        }
                    }

                    public class D : C
                    {
                        protected new void M(bool b)
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Parameter b is of type bool, which should be avoided.");
        }

        [Fact]
        public void When_method_parameter_type_is_nullable_bool_in_hidden_method_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .InGlobalScope(@"
                    public class C
                    {
                        protected virtual void M(bool? [|b|])
                        {
                        }
                    }

                    public class D : C
                    {
                        protected new void M(bool? b)
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Parameter b is of type bool?, which should be avoided.");
        }

        [Fact]
        public void When_indexer_parameter_type_is_bool_in_overridden_indexer_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .Using(typeof (NotImplementedException).Namespace)
                .InGlobalScope(@"
                    public abstract class C
                    {
                        public abstract int this[bool [|b|]] 
                        { 
                            get; set; 
                        }
                    }

                    public class D : C
                    {
                        public override int this[bool b]
                        {
                            get { throw new NotImplementedException(); }
                            set { throw new NotImplementedException(); }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Parameter b is of type bool, which should be avoided.");
        }

        [Fact]
        public void When_indexer_parameter_type_is_nullable_bool_in_overridden_indexer_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .Using(typeof (NotImplementedException).Namespace)
                .InGlobalScope(@"
                    public abstract class C
                    {
                        public abstract int this[bool? [|b|]] 
                        { 
                            get; set; 
                        }
                    }

                    public class D : C
                    {
                        public override int this[bool? b]
                        {
                            get { throw new NotImplementedException(); }
                            set { throw new NotImplementedException(); }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Parameter b is of type bool?, which should be avoided.");
        }

        [Fact]
        public void When_indexer_parameter_type_is_bool_in_hidden_indexer_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .Using(typeof (NotImplementedException).Namespace)
                .InGlobalScope(@"
                    public class C
                    {
                        public virtual int this[bool [|b|]] 
                        { 
                            get { throw new NotImplementedException(); }
                            set { throw new NotImplementedException(); }
                        }
                    }

                    public class D : C
                    {
                        public new int this[bool b]
                        {
                            get { throw new NotImplementedException(); }
                            set { throw new NotImplementedException(); }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Parameter b is of type bool, which should be avoided.");
        }

        [Fact]
        public void When_indexer_parameter_type_is_nullable_bool_in_hidden_indexer_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .Using(typeof (NotImplementedException).Namespace)
                .InGlobalScope(@"
                    public class C
                    {
                        public virtual int this[bool? [|b|]] 
                        { 
                            get { throw new NotImplementedException(); }
                            set { throw new NotImplementedException(); }
                        }
                    }

                    public class D : C
                    {
                        public new int this[bool? b]
                        {
                            get { throw new NotImplementedException(); }
                            set { throw new NotImplementedException(); }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Parameter b is of type bool?, which should be avoided.");
        }

        [Fact]
        public void When_method_parameter_type_is_bool_in_implicit_interface_implementation_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .InGlobalScope(@"
                    public interface I
                    {
                        void M(bool [|b|]);
                    }

                    public class C : I
                    {
                        public void M(bool b)
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Parameter b is of type bool, which should be avoided.");
        }

        [Fact]
        public void When_method_parameter_type_is_nullable_bool_in_implicit_interface_implementation_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .InGlobalScope(@"
                    public interface I
                    {
                        void M(bool? [|b|]);
                    }

                    public class C : I
                    {
                        public void M(bool? b)
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Parameter b is of type bool?, which should be avoided.");
        }

        [Fact]
        public void When_method_parameter_type_is_bool_in_explicit_interface_implementation_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .InGlobalScope(@"
                    public interface I
                    {
                        void M(bool [|b|]);
                    }

                    public class C : I
                    {
                        void I.M(bool b)
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Parameter b is of type bool, which should be avoided.");
        }

        [Fact]
        public void When_method_parameter_type_is_nullable_bool_in_explicit_interface_implementation_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .InGlobalScope(@"
                    public interface I
                    {
                        void M(bool? [|b|]);
                    }

                    public class C : I
                    {
                        void I.M(bool? b)
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Parameter b is of type bool?, which should be avoided.");
        }

        [Fact]
        public void When_indexer_parameter_type_is_bool_in_implicit_interface_implementation_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .Using(typeof (NotImplementedException).Namespace)
                .InGlobalScope(@"
                    public interface I
                    {
                        int this[bool [|b|]] 
                        { 
                            get; set; 
                        }
                    }

                    public class C : I
                    {
                        public int this[bool b]
                        {
                            get { throw new NotImplementedException(); }
                            set { throw new NotImplementedException(); }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Parameter b is of type bool, which should be avoided.");
        }

        [Fact]
        public void When_indexer_parameter_type_is_nullable_bool_in_implicit_interface_implementation_it_must_be_skipped
            ()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .Using(typeof (NotImplementedException).Namespace)
                .InGlobalScope(@"
                    public interface I
                    {
                        int this[bool? [|b|]] 
                        { 
                            get; set; 
                        }
                    }

                    public class C : I
                    {
                        public int this[bool? b]
                        {
                            get { throw new NotImplementedException(); }
                            set { throw new NotImplementedException(); }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Parameter b is of type bool?, which should be avoided.");
        }

        [Fact]
        public void When_indexer_parameter_type_is_bool_in_explicit_interface_implementation_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .Using(typeof (NotImplementedException).Namespace)
                .InGlobalScope(@"
                    public interface I
                    {
                        int this[bool [|b|]] 
                        { 
                            get; set; 
                        }
                    }

                    public class C : I
                    {
                        int I.this[bool b]
                        {
                            get { throw new NotImplementedException(); }
                            set { throw new NotImplementedException(); }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Parameter b is of type bool, which should be avoided.");
        }

        [Fact]
        public void When_indexer_parameter_type_is_nullable_bool_in_explicit_interface_implementation_it_must_be_skipped
            ()
        {
            // Arrange
            ParsedSourceCode source = new ClassSourceCodeBuilder()
                .Using(typeof (NotImplementedException).Namespace)
                .InGlobalScope(@"
                    public interface I
                    {
                        int this[bool? [|b|]] 
                        { 
                            get; set; 
                        }
                    }

                    public class C : I
                    {
                        int I.this[bool? b]
                        {
                            get { throw new NotImplementedException(); }
                            set { throw new NotImplementedException(); }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Parameter b is of type bool?, which should be avoided.");
        }

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new AvoidBooleanParametersAnalyzer();
        }

        protected override CodeFixProvider CreateFixProvider()
        {
            throw new NotImplementedException();
        }
    }
}