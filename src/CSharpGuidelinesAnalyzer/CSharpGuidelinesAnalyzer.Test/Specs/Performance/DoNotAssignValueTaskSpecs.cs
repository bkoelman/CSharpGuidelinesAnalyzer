using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CSharpGuidelinesAnalyzer.Rules.Performance;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.Performance
{
    public sealed class DoNotAssignValueTaskSpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => DoNotAssignValueTaskAnalyzer.DiagnosticId;

        [Fact]
        internal void When_awaiting_ValueTask_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .Using(typeof(ValueTask).Namespace)
                .InDefaultClass(@"
                    async Task M()
                    {
                        await GetAsync();
                        await GetAsync().ConfigureAwait(false);

                        string result1 = await GetStringAsync();
                        string result2 = await GetStringAsync().ConfigureAwait(false);
                    }

                    ValueTask GetAsync() => throw null;
                    ValueTask<string> GetStringAsync() => throw null;
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        [Fact]
        internal void When_initializing_variable_from_ValueTask_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .Using(typeof(ValueTask).Namespace)
                .Using(typeof(ConfiguredValueTaskAwaitable).Namespace)
                .InDefaultClass(@"
                    void M()
                    {
                        ValueTask task [|=|] GetAsync();
                        ConfiguredValueTaskAwaitable configuredTask [|=|] GetAsync().ConfigureAwait(false);

                        ValueTask<string> stringTask [|=|] GetStringAsync();
                        ConfiguredValueTaskAwaitable<string> configuredStringTask [|=|] GetStringAsync().ConfigureAwait(false);

                        string result = GetStringAsync().Result;
                    }

                    ValueTask GetAsync() => throw null;
                    ValueTask<string> GetStringAsync() => throw null;
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Assignment of ValueTask without await to 'task'",
                "Assignment of ValueTask without await to 'configuredTask'",
                "Assignment of ValueTask without await to 'stringTask'",
                "Assignment of ValueTask without await to 'configuredStringTask'");
        }

        [Fact]
        internal void When_conditionally_initializing_variable_from_ValueTask_using_null_conditional_operator_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .Using(typeof(ValueTask).Namespace)
                .Using(typeof(ConfiguredValueTaskAwaitable).Namespace)
                .InDefaultClass(@"
                    C TheC { get; set; }

                    void M()
                    {
                        ValueTask task [|=|] TheC?.GetAsync() ?? default;
                        ConfiguredValueTaskAwaitable configuredTask [|=|] TheC?.GetAsync().ConfigureAwait(false) ?? default;

                        ValueTask<string> stringTask [|=|] TheC?.GetStringAsync() ?? default;
                        ConfiguredValueTaskAwaitable<string> configuredStringTask [|=|] TheC?.GetStringAsync().ConfigureAwait(false) ?? default;
                    }

                    class C
                    {
                        public ValueTask GetAsync() => throw null;
                        public ValueTask<string> GetStringAsync() => throw null;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Assignment of ValueTask without await to 'task'",
                "Assignment of ValueTask without await to 'configuredTask'",
                "Assignment of ValueTask without await to 'stringTask'",
                "Assignment of ValueTask without await to 'configuredStringTask'");
        }

        [Fact]
        internal void When_initializing_member_from_ValueTask_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .Using(typeof(ValueTask).Namespace)
                .Using(typeof(ConfiguredValueTaskAwaitable).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        ValueTask task [|=|] GetAsync();
                        ConfiguredValueTaskAwaitable ConfiguredTask {get; set; } [|=|] GetAsync().ConfigureAwait(false);
                        ValueTask<string> stringTask [|=|] GetStringAsync();
                        ConfiguredValueTaskAwaitable<string> ConfiguredStringTask {get; set; } [|=|] GetStringAsync().ConfigureAwait(false);

                        static ValueTask GetAsync() => throw null;
                        static ValueTask<string> GetStringAsync() => throw null;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Assignment of ValueTask without await to 'C.task'",
                "Assignment of ValueTask without await to 'C.ConfiguredTask'",
                "Assignment of ValueTask without await to 'C.stringTask'",
                "Assignment of ValueTask without await to 'C.ConfiguredStringTask'");
        }

        [Fact]
        internal void When_assigning_variable_from_ValueTask_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .Using(typeof(ValueTask).Namespace)
                .Using(typeof(ConfiguredValueTaskAwaitable).Namespace)
                .InDefaultClass(@"
                    void M()
                    {
                        ValueTask task;
                        task [|=|] GetAsync();

                        ConfiguredValueTaskAwaitable configuredTask;
                        configuredTask [|=|] GetAsync().ConfigureAwait(false);

                        ValueTask<string> stringTask;
                        stringTask [|=|] GetStringAsync();

                        ConfiguredValueTaskAwaitable<string> configuredStringTask;
                        configuredStringTask [|=|] GetStringAsync().ConfigureAwait(false);

                        string result;
                        result = GetStringAsync().Result;
                    }

                    ValueTask GetAsync() => throw null;
                    ValueTask<string> GetStringAsync() => throw null;
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Assignment of ValueTask without await to 'task'",
                "Assignment of ValueTask without await to 'configuredTask'",
                "Assignment of ValueTask without await to 'stringTask'",
                "Assignment of ValueTask without await to 'configuredStringTask'");
        }

        [Fact]
        internal void When_conditionally_assigning_variable_from_ValueTask_using_null_coalescing_operator_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .Using(typeof(ValueTask).Namespace)
                .Using(typeof(ConfiguredValueTaskAwaitable).Namespace)
                .InDefaultClass(@"
                    void M()
                    {
                        ValueTask? task = null;
                        task [|??=|] GetAsync();

                        ConfiguredValueTaskAwaitable? configuredTask = null;
                        configuredTask [|??=|] GetAsync().ConfigureAwait(false);

                        ValueTask<string>? stringTask = null;
                        stringTask [|??=|] GetStringAsync();

                        ConfiguredValueTaskAwaitable<string>? configuredStringTask = null;
                        configuredStringTask [|??=|] GetStringAsync().ConfigureAwait(false);
                    }

                    ValueTask GetAsync() => throw null;
                    ValueTask<string> GetStringAsync() => throw null;
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Assignment of ValueTask without await to 'task'",
                "Assignment of ValueTask without await to 'configuredTask'",
                "Assignment of ValueTask without await to 'stringTask'",
                "Assignment of ValueTask without await to 'configuredStringTask'");
        }

        [Fact]
        internal void When_assigning_member_from_ValueTask_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .Using(typeof(ValueTask).Namespace)
                .Using(typeof(ConfiguredValueTaskAwaitable).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        ValueTask task;
                        ConfiguredValueTaskAwaitable ConfiguredTask { get; set; }
                        ValueTask<string> stringTask;
                        ConfiguredValueTaskAwaitable<string> ConfiguredStringTask { get; set; }

                        void M()
                        {
                            task [|=|] GetAsync();
                            ConfiguredTask [|=|] GetAsync().ConfigureAwait(false);
                            stringTask [|=|] GetStringAsync();
                            ConfiguredStringTask [|=|] GetStringAsync().ConfigureAwait(false);
                        }

                        ValueTask GetAsync() => throw null;
                        ValueTask<string> GetStringAsync() => throw null;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Assignment of ValueTask without await to 'C.task'",
                "Assignment of ValueTask without await to 'C.ConfiguredTask'",
                "Assignment of ValueTask without await to 'C.stringTask'",
                "Assignment of ValueTask without await to 'C.ConfiguredStringTask'");
        }

        [Fact]
        internal void When_using_argument_of_type_ValueTask_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .Using(typeof(ValueTask).Namespace)
                .Using(typeof(ConfiguredValueTaskAwaitable).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        void M()
                        {
                            Target([|default(ValueTask)|]);
                            Target([|default(ConfiguredValueTaskAwaitable)|]);
                            Target([|default(ValueTask<string>)|]);
                            Target([|default(ConfiguredValueTaskAwaitable<string>)|]);
                        }

                        void Target(object value) => throw null;
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "Usage of ValueTask without await on parameter 'value' of 'C.Target(object)'",
                "Usage of ValueTask without await on parameter 'value' of 'C.Target(object)'",
                "Usage of ValueTask without await on parameter 'value' of 'C.Target(object)'",
                "Usage of ValueTask without await on parameter 'value' of 'C.Target(object)'");
        }

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new DoNotAssignValueTaskAnalyzer();
        }
    }
}
