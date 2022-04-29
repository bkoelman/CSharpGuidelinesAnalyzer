using CSharpGuidelinesAnalyzer.Rules.Framework;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

// @formatter:keep_existing_linebreaks true

namespace CSharpGuidelinesAnalyzer.Test.Specs.Framework;

public sealed class DoNotImplicitlyConvertToDynamicSpecs : CSharpGuidelinesAnalysisTestFixture
{
    protected override string DiagnosticId => DoNotImplicitlyConvertToDynamicAnalyzer.DiagnosticId;

    [Fact]
    internal async Task When_dynamic_identifier_is_assigned_from_dynamic_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class C
                {
                    dynamic field = GetDynamic();

                    dynamic Property
                    {
                        get => field;
                        set => value = field;
                    }

                    public C()
                    {
                        dynamic d = GetDynamicLocal();

                        dynamic GetDynamicLocal() => throw null;
                    }

                    void M(ref dynamic p)
                    {
                        SetDynamic(p);

                        p += field;
                    }

                    static dynamic GetDynamic() => throw null;
                    static void SetDynamic(dynamic d) => throw null;
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_dynamic_identifier_is_assigned_from_object_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class B
                {
                    protected B(dynamic d)
                    {
                    }
                }

                class C : B
                {
                    dynamic field = GetObject();

                    dynamic Property
                    {
                        get => field;
                        set => value = field;
                    }

                    public C()
                        : base(new object())
                    {
                        dynamic d = GetObjectLocal();

                        object GetObjectLocal() => throw null;
                    }

                    void M(ref dynamic p)
                    {
                        SetDynamic(new object());

                        p += field;
                    }

                    static object GetObject() => throw null;
                    static void SetDynamic(dynamic d) => throw null;
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_dynamic_identifier_is_assigned_from_null_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class B
                {
                    protected B(dynamic d)
                    {
                    }
                }

                class C : B
                {
                    dynamic field = null;

                    dynamic Property
                    {
                        get => null;
                        set => value = null;
                    }

                    public C()
                        : base(null)
                    {
                        dynamic d = null;
                    }

                    void M(ref dynamic p)
                    {
                        SetDynamic(null);

                        p += null;
                    }

                    static void SetDynamic(dynamic d) => throw null;
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_dynamic_identifier_is_assigned_from_constant_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class B
                {
                    protected B(dynamic d)
                    {
                    }
                }

                class C : B
                {
                    dynamic field = [|""A""|];

                    dynamic Property
                    {
                        get => [|1.5m|];
                        set => value = [|1|];
                    }

                    public C()
                        : base([|'X'|])
                    {
                        dynamic d = [|2.8D|];
                    }

                    void M(ref dynamic p)
                    {
                        const byte b = (byte)0;
                        SetDynamic([|b|]);

                        p += [|2.9F|];
                    }

                    static void SetDynamic(dynamic d) => throw null;
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "An expression of type 'String' is implicitly converted to dynamic",
            "An expression of type 'Decimal' is implicitly converted to dynamic",
            "An expression of type 'Int32' is implicitly converted to dynamic",
            "An expression of type 'Char' is implicitly converted to dynamic",
            "An expression of type 'Double' is implicitly converted to dynamic",
            "An expression of type 'Byte' is implicitly converted to dynamic",
            "An expression of type 'Single' is implicitly converted to dynamic");
    }

    [Fact]
    internal async Task When_dynamic_identifier_is_assigned_from_cast_to_dynamic_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class C
                {
                    dynamic field = (dynamic)""A"";

                    dynamic Property
                    {
                        get => (dynamic)1.5m;
                        set => value = (dynamic)1;
                    }

                    public C()
                    {
                        dynamic d = (dynamic)2.8D;
                    }

                    void M(ref dynamic p)
                    {
                        const byte b = (byte)0;
                        SetDynamic((dynamic)b);

                        p += (dynamic)2.9F;

                        dynamic d = (dynamic)new { A = p };
                    }

                    static void SetDynamic(dynamic d) => throw null;
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_dynamic_identifier_is_assigned_from_invocation_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class B
                {
                    protected B(dynamic d)
                    {
                    }
                }

                class C : B
                {
                    dynamic field = [|GetString()|];

                    dynamic Property
                    {
                        get => [|GetDecimal()|];
                        set => value = [|GetInt32()|];
                    }

                    public C()
                        : base([|GetChar()|])
                    {
                        dynamic d = [|GetDouble()|];

                        double GetDouble() => throw null;
                    }

                    void M(ref dynamic p)
                    {
                        SetDynamic([|GetByte()|]);

                        p += [|GetDateTime()|];

                        dynamic d = [|new { A = p }|];

                        DateTime GetDateTime() => throw null;
                    }

                    static string GetString() => throw null;
                    static decimal GetDecimal() => throw null;
                    static int GetInt32() => throw null;
                    static char GetChar() => throw null;
                    static byte GetByte() => throw null;
                    static void SetDynamic(dynamic d) => throw null;
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "An expression of type 'String' is implicitly converted to dynamic",
            "An expression of type 'Decimal' is implicitly converted to dynamic",
            "An expression of type 'Int32' is implicitly converted to dynamic",
            "An expression of type 'Char' is implicitly converted to dynamic",
            "An expression of type 'Double' is implicitly converted to dynamic",
            "An expression of type 'Byte' is implicitly converted to dynamic",
            "An expression of type 'DateTime' is implicitly converted to dynamic",
            "An expression of type '(anonymous)' is implicitly converted to dynamic");
    }

    [Fact]
    internal async Task When_dynamic_identifier_is_assigned_from_Activator_invocation_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new MemberSourceCodeBuilder()
            .Using(typeof(Activator).Namespace)
            .InDefaultClass(@"
                void M(ref dynamic p)
                {
                    dynamic instance1 = Activator.CreateInstance(new int[0].GetType());
                    dynamic instance2 = Activator.CreateInstance(null, string.Empty, string.Empty);
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    protected override DiagnosticAnalyzer CreateAnalyzer()
    {
        return new DoNotImplicitlyConvertToDynamicAnalyzer();
    }
}
