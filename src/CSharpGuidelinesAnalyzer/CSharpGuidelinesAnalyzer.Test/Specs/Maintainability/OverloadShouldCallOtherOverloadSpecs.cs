using CSharpGuidelinesAnalyzer.Rules.Maintainability;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.Maintainability
{
    public sealed class OverloadShouldCallOtherOverloadSpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => OverloadShouldCallOtherOverloadAnalyzer.DiagnosticId;

        [Fact]
        internal void When_type_is_enum_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    enum E
                    {
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_type_is_interface_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    interface I
                    {
                        void M();

                        void M(string s);
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_differently_named_methods_exist_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        public void M()
                        {
                        }

                        public void M2(string s)
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_no_single_longest_method_overload_exists_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void M()
                        {
                        }

                        public void M(string s)
                        {
                        }

                        public void M(int i)
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_longest_method_overload_is_not_virtual_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void M()
                        {
                            M(string.Empty);
                        }

                        public void [|M|](string s)
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method overload with the most parameters should be virtual.");
        }

        [Fact]
        internal void When_longest_method_overload_in_base_type_is_not_virtual_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class B
                    {
                        void M()
                        {
                            M(string.Empty);
                        }

                        public void [|M|](string s)
                        {
                        }
                    }

                    class C : B
                    {
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Method overload with the most parameters should be virtual.");
        }

        [Fact]
        internal void When_longest_method_overload_is_private_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void M()
                        {
                            M(string.Empty);
                        }

                        private void M(string s)
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_longest_method_overload_is_not_virtual_in_struct_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    struct C
                    {
                        void M()
                        {
                            M(string.Empty);
                        }

                        public void M(string s)
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_longest_method_overload_is_not_virtual_in_sealed_class_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    sealed class C
                    {
                        void M()
                        {
                            M(string.Empty);
                        }

                        public void M(string s)
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_longest_method_overload_is_static_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void M()
                        {
                            M(string.Empty);
                        }

                        public static void M(string s)
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_shorter_method_overload_does_not_invoke_another_overload_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void [|M|]()
                        {
                        }

                        protected virtual void M(string s)
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Overloaded method 'C.M()' should call another overload.");
        }

        [Fact]
        internal void When_shorter_method_overload_does_not_invoke_another_overload_in_base_type_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class B
                    {
                        void [|M|]()
                        {
                        }

                        protected virtual void M(string s)
                        {
                        }
                    }

                    class C : B
                    {
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Overloaded method 'B.M()' should call another overload.");
        }

        [Fact]
        internal void When_shorter_method_overload_invokes_another_overload_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void M()
                        {
                            M(string.Empty);
                        }

                        protected virtual void M(string s)
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_shorter_method_overload_invokes_another_generic_overload_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void M()
                        {
                            M(string.Empty, 0);
                        }

                        protected virtual void M<T>(string s, T t)
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_shorter_method_overload_invokes_itself_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        void [|M|]()
                        {
                            M();
                        }

                        protected virtual void M(string s)
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Overloaded method 'C.M()' should call another overload.");
        }

        [Fact]
        internal void When_shorter_method_overload_invokes_another_overload_in_base_type_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class B
                    {
                        protected virtual void M(string s, int i)
                        {
                        }
                    }

                    class C : B
                    {
                        void M()
                        {
                            base.M(string.Empty, 0);
                        }

                        void M(string s)
                        {
                            base.M(s, 0);
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_shorter_method_overload_invokes_same_overload_in_generic_base_type_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class B<T>
                    {
                        protected virtual void M(T t)
                        {
                            M(t, 0);
                        }

                        protected virtual void M(T t, int i)
                        {
                        }
                    }

                    class C : B<string>
                    {
                        protected override void M(string t)
                        {
                            base.M(t);
                        }

                        protected override void M(string t, int i)
                        {
                            base.M(t, i);
                        }

                        protected virtual void M(string t, int i, object o)
                        {
                            base.M(t, i);
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_shorter_method_overload_is_abstract_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    abstract class C
                    {
                        protected abstract void M();

                        protected virtual void M(string s)
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_shorter_expression_bodied_method_overload_does_not_invoke_another_overload_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        string [|M|]() => ""X"";

                        protected virtual string M(string s) => s;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Overloaded method 'C.M()' should call another overload.");
        }

        [Fact]
        internal void When_shorter_expression_bodied_method_overload_invokes_another_overload_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        string M() => M(""X"");

                        protected virtual string M(string s) => s;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_shorter_expression_bodied_method_overload_invokes_itself_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    struct C
                    {
                        string [|M|]() => M();

                        string M(string s) => s;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Overloaded method 'C.M()' should call another overload.");
        }

        [Fact]
        internal void When_partial_method_overload_invokes_another_overload_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    partial class C
                    {
                        partial void M();

                        protected virtual void M(string s)
                        {
                        }
                    }

                    partial class C
                    {
                        partial void M()
                        {
                            M(string.Empty);
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_partial_method_overload_does_not_invoke_another_overload_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    partial class C
                    {
                        partial void M();

                        protected virtual void M(string s)
                        {
                        }
                    }

                    partial class C
                    {
                        partial void [|M|]()
                        {
                        }
                    }
                ")
                .Build();

            VerifyGuidelineDiagnostic(source,
                "Overloaded method 'C.M()' should call another overload.");
        }

        [Fact]
        internal void When_partial_method_implementation_is_missing_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    partial class C
                    {
                        partial void M();

                        protected virtual void M(string s)
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_shorter_constructor_overload_does_not_invoke_another_overload_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    class C
                    {
                        public C()
                        {
                        }

                        public C(string s)
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_parameter_order_in_overloads_is_consistent_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public class C
                    {
                        public bool M()
                        {
                            return M(string.Empty);
                        }

                        public bool M(string value)
                        {
                            return M(value, -1);
                        }

                        public bool M(string value, int index)
                        {
                            return M(value, index, false);
                        }

                        protected virtual bool M(string value, int index, bool flag)
                        {
                            throw new NotImplementedException();
                        }
                    }
                ")
                .Build();

            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_parameter_order_in_overloads_is_consistent_with_extra_parameter_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public class C
                    {
                        public bool M(string value, string other)
                        {
                            return M(value, -1, false);
                        }

                        protected virtual bool M(string value, int index, bool flag)
                        {
                            throw new NotImplementedException();
                        }
                    }
                ")
                .Build();

            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_parameter_order_in_overloads_is_not_consistent_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public class C
                    {
                        public bool M()
                        {
                            return M(string.Empty);
                        }

                        public bool M(string value)
                        {
                            return M(-1, value);
                        }

                        public bool [|M|](int index, string value)
                        {
                            return M(value, index, false);
                        }

                        protected virtual bool M(string value, int index, bool flag)
                        {
                            throw new NotImplementedException();
                        }
                    }
                ")
                .Build();

            VerifyGuidelineDiagnostic(source,
                "Parameter order in 'C.M(int, string)' does not match with the parameter order of the longest overload.");
        }

        [Fact]
        internal void When_parameter_order_in_overloads_is_not_consistent_in_base_type_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public class B
                    {
                        public bool M()
                        {
                            return M(string.Empty);
                        }

                        public bool M(string value)
                        {
                            return M(-1, value);
                        }

                        public bool [|M|](int index, string value)
                        {
                            return M(value, index, false);
                        }

                        protected virtual bool M(string value, int index, bool flag)
                        {
                            throw new NotImplementedException();
                        }
                    }

                    public class C : B
                    {
                    }
                ")
                .Build();

            VerifyGuidelineDiagnostic(source,
                "Parameter order in 'B.M(int, string)' does not match with the parameter order of the longest overload.");
        }

        [Fact]
        internal void When_parameter_order_in_overloads_is_consistent_with_params_array_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public class C
                    {
                        public bool M()
                        {
                            return M(Array.Empty<object>());
                        }

                        public bool M(params object[] args)
                        {
                            return M(string.Empty, args);
                        }

                        public bool M(string value, params object[] args)
                        {
                            return M(value, -1, args);
                        }

                        public bool M(string value, int index, params object[] args)
                        {
                            return M(value, index, false, args);
                        }

                        protected virtual bool M(string value, int index, bool flag, params object[] args)
                        {
                            throw new NotImplementedException();
                        }
                    }
                ")
                .Build();

            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_parameter_order_in_overloads_is_not_consistent_with_params_array_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public class C
                    {
                        public bool M()
                        {
                            return M(Array.Empty<object>());
                        }

                        public bool M(params object[] args)
                        {
                            return M(string.Empty, args);
                        }

                        public bool M(string value, params object[] args)
                        {
                            return M(-1, value, args);
                        }

                        public bool [|M|](int index, string value, params object[] args)
                        {
                            return M(value, index, false, args);
                        }

                        protected virtual bool M(string value, int index, bool flag, params object[] args)
                        {
                            throw new NotImplementedException();
                        }
                    }
                ")
                .Build();

            VerifyGuidelineDiagnostic(source,
                "Parameter order in 'C.M(int, string, params object[])' does not match with the parameter order of the longest overload.");
        }

        [Fact]
        internal void When_parameter_order_in_overloads_is_consistent_with_optional_parameters_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public class C
                    {
                        public bool M()
                        {
                            return M(string.Empty);
                        }

                        public bool M(string value = null)
                        {
                            return M(value, -1);
                        }

                        public bool M(string value = null, int index = -1)
                        {
                            return M(value, index, false);
                        }

                        protected virtual bool M(string value = null, int index = -1, bool flag = true)
                        {
                            throw new NotImplementedException();
                        }
                    }
                ")
                .Build();

            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_parameter_order_in_overloads_is_not_consistent_with_optional_parameters_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public class C
                    {
                        public bool M()
                        {
                            return M(string.Empty);
                        }

                        public bool M(string value = null)
                        {
                            return M(value, -1);
                        }

                        public bool [|M|](int index = -1, string value = null)
                        {
                            return M(value, index, false);
                        }

                        protected virtual bool M(string value = null, int index = -1, bool flag = true)
                        {
                            throw new NotImplementedException();
                        }
                    }
                ")
                .Build();

            VerifyGuidelineDiagnostic(source,
                "Parameter order in 'C.M(int, string)' does not match with the parameter order of the longest overload.");
        }

        [Fact]
        internal void
            When_parameter_order_in_overloads_is_consistent_with_regular_and_optional_and_extra_parameters_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public class C
                    {
                        public bool M(DateTime time = default(DateTime))
                        {
                            return M(1.0, index: 0, DateTime.MinValue);
                        }

                        public bool M(double first, int index = -1, DateTime time = default(DateTime))
                        {
                            return M(first, value: string.Empty, index: 0, new object());
                        }

                        public bool M(double first, string value, int index = -1, object other = null)
                        {
                            return M(first, value, index, false, Array.Empty<object>());
                        }

                        protected virtual bool M(double first, string value, int index = -1, bool flag = true, params object[] data)
                        {
                            throw new NotImplementedException();
                        }
                    }
                ")
                .Build();

            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_parameter_order_in_overridden_overloads_is_not_consistent_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public abstract class B
                    {
                        public abstract bool M();
                        public abstract bool M(string value);
                        public abstract bool M(int index, string value);
                        protected abstract bool M(string value, int index, bool flag);
                    }

                    public class C : B
                    {
                        public override bool M()
                        {
                            return M(string.Empty);
                        }

                        public override bool M(string value)
                        {
                            return M(-1, value);
                        }

                        public override bool M(int index, string value)
                        {
                            return M(value, index, false);
                        }

                        protected override bool M(string value, int index, bool flag)
                        {
                            throw new NotImplementedException();
                        }
                    }
                ")
                .Build();

            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_parameter_order_in_hidden_overloads_is_not_consistent_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public abstract class B
                    {
                        public virtual bool M()
                        {
                            return M(string.Empty);
                        }

                        public virtual bool M(string value)
                        {
                            return M(-1, value);
                        }

                        public virtual bool [|M|](int index, string value)
                        {
                            return M(value, index, false);
                        }

                        protected virtual bool M(string value, int index, bool flag)
                        {
                            throw new NotImplementedException();
                        }
                    }

                    public class C : B
                    {
                        public override bool M()
                        {
                            return M(string.Empty);
                        }

                        public override bool M(string value)
                        {
                            return M(-1, value);
                        }

                        public new bool M(int index, string value)
                        {
                            return M(value, index, false);
                        }

                        protected override bool M(string value, int index, bool flag)
                        {
                            throw new NotImplementedException();
                        }
                    }
                ")
                .Build();

            VerifyGuidelineDiagnostic(source,
                "Parameter order in 'B.M(int, string)' does not match with the parameter order of the longest overload.");
        }

        [Fact]
        internal void When_parameter_order_in_implicitly_implemented_overloads_is_not_consistent_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public interface I
                    {
                        bool M();
                        bool M(string value);
                        bool M(int index, string value);
                        bool M(string value, int index, bool flag);
                    }

                    public class C : I
                    {
                        public bool M()
                        {
                            return M(string.Empty);
                        }

                        public bool M(string value)
                        {
                            return M(-1, value);
                        }

                        public bool M(int index, string value)
                        {
                            return M(value, index, false);
                        }

                        public virtual bool M(string value, int index, bool flag)
                        {
                            throw new NotImplementedException();
                        }
                    }
                ")
                .Build();

            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_parameter_order_in_explicitly_implemented_overloads_is_not_consistent_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public interface I
                    {
                        bool M();
                        bool M(string value);
                        bool M(int index, string value);
                        bool M(string value, int index, bool flag);
                    }

                    public class C : I
                    {
                        bool I.M()
                        {
                            var @interface = (I)this;
                            return @interface.M(string.Empty);
                        }

                        bool I.M(string value)
                        {
                            var @interface = (I)this;
                            return @interface.M(-1, value);
                        }

                        bool I.M(int index, string value)
                        {
                            var @interface = (I)this;
                            return @interface.M(value, index, false);
                        }

                        bool I.M(string value, int index, bool flag)
                        {
                            throw new NotImplementedException();
                        }
                    }
                ")
                .Build();

            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_parameter_order_in_generic_explicitly_implemented_overloads_is_not_consistent_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .InGlobalScope(@"
                    public interface I<T>
                    {
                        T M();
                        T M(string value);
                        T M(int index, string value);
                        T M<U>(string value, int index, U extra);
                    }

                    public class C : I<bool>
                    {
                        bool I<bool>.M()
                        {
                            var @interface = (I<bool>)this;
                            return @interface.M(string.Empty);
                        }

                        bool I<bool>.M(string value)
                        {
                            var @interface = (I<bool>)this;
                            return @interface.M(-1, value);
                        }

                        bool I<bool>.M(int index, string value)
                        {
                            var @interface = (I<bool>)this;
                            return @interface.M(value, index, 0);
                        }

                        bool I<bool>.M<U>(string value, int index, U extra)
                        {
                            throw new NotImplementedException();
                        }
                    }
                ")
                .Build();

            VerifyGuidelineDiagnostic(source);
        }

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new OverloadShouldCallOtherOverloadAnalyzer();
        }
    }
}
