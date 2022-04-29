using CSharpGuidelinesAnalyzer.Rules.Layout;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

// @formatter:keep_existing_linebreaks true

namespace CSharpGuidelinesAnalyzer.Test.Specs.Layout;

public sealed class PlaceMethodsInCallingOrderSpecs : CSharpGuidelinesAnalysisTestFixture
{
    protected override string DiagnosticId => PlaceMembersInCallingOrderAnalyzer.DiagnosticId;

    [Fact]
    internal async Task When_file_contains_types_in_invalid_order_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                namespace NS
                {
                    public abstract class [|Worker|]
                    {
                        public void Init() => throw null;
                    }

                    public sealed class Worker<T> : Worker
                    {
                        public void Run()
                        {
                            base.Init();
                        }
                    }
                }")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            @"Type 'Worker' should be moved down. Valid file order:
Worker<T>
Worker<T>.Run()
Worker
Worker.Init()");
    }

    [Fact]
    internal async Task When_type_contains_no_dependencies_in_valid_order_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class C
                {
                    const int Cn = 1;
                    static readonly int fsr;
                    static int fs;
                    int fr;
                    int f;

                    static string Ps { get; set; }
                    string P { get; set; }

                    public string this[int index]
                    {
                        get => throw null;
                        set => throw null;
                    }

                    static event EventHandler Es;
                    event EventHandler E;

                    static C()
                    {
                    }

                    public C()
                    {
                    }

                    void M()
                    {
                    }

                    public static bool operator==(C left, C right) => throw null;
                    public static bool operator!=(C left, C right) => throw null;
                    public static explicit operator string(C c) => throw null;

                    ~C()
                    {
                    }

                    class N
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_type_contains_nameof_reference_in_invalid_order_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class C
                {
                    void X() => throw null;

                    void M()
                    {
                        _ = nameof(X); // does not affect ordering
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_type_contains_typeof_reference_in_invalid_order_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class [|D|]
                {
                }

                class C
                {
                    void M()
                    {
                        _ = typeof(D);
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            @"Type 'D' should be moved down. Valid file order:
C
C.M()
D");
    }

    [Fact]
    internal async Task When_type_contains_instantiation_reference_in_invalid_order_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class [|C|]
                {
                }

                class D
                {
                    public D()
                    {
                        _ = new C();
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            @"Type 'C' should be moved down. Valid file order:
D
D.D()
C");
    }

    [Fact]
    internal async Task When_nested_member_contains_reference_to_parent_member_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class Z
                {
                    void E()
                    {
                        var x = new X();
                        x.A();
                    }
                }

                class X
                {
                    internal void A()
                    {
                    }

                    internal void B()
                    {
                    }

                    internal void C()
                    {
                    }

                    class Y
                    {
                        void D()
                        {
                            var x = new X();
                            x.C(); // does not affect ordering
                        }
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_partial_type_contains_members_in_invalid_order_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
partial class [|C|]
{
public void N()
{
}
}

partial class C
{
public void M()
{
    N();
}
}")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            @"Type 'C' should be moved down. Valid file order:
C
C.M()
C
C.N()");
    }

    [Fact]
    internal async Task When_type_contains_partial_methods_in_invalid_order_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
partial class C
{
partial void M();

void X() => throw null;
}

partial class C
{
partial void M()
{
    X();
}
}")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_type_contains_usages_of_nested_type_in_invalid_order_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class Top
                {
                    void X()
                    {
                        var nested = new Nested();
                        nested.B();
                    }

                    static void Y()
                    {
                    }

                    private class Nested
                    {
                        public void [|A|]()
                        {
                            B();
                        }

                        public void B()
                        {
                            Top.Y();
                        }
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            @"Member 'Top.Nested.A()' should be moved down. Valid file order:
Top
Top.X()
Top.Y()
Top.Nested
Top.Nested.B()
Top.Nested.A()");
    }

    [Fact]
    internal async Task When_type_contains_field_initializer_methods_in_invalid_order_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class C
                {
                    private int _f1 = Init1();
                    private string _f2 = Init2();

                    private static string [|Init2|]() => throw null;
                    private static int Init1() => throw null;
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            @"Member 'C.Init2()' should be moved down. Valid file order:
C
C._f1
C._f2
C.Init1()
C.Init2()");
    }

    [Fact]
    internal async Task When_type_contains_properties_in_invalid_order_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class C
                {
                    public int [|P|]
                    {
                        get => throw null;
                        set => throw null;
                    }

                    public string Q
                    {
                        get => P.ToString();
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            @"Member 'C.P' should be moved down. Valid file order:
C
C.Q
C.P");
    }

    [Fact]
    internal async Task When_type_contains_event_in_invalid_order_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class C
                {
                    public static event EventHandler Ev
                    {
                        add
                        {
                            _ = value;
                            A();
                        }
                        remove
                        {
                            _ = value;
                            R();
                        }
                    }

                    void [|M|]() => throw null;

                    static void R() => throw null;

                    static void A() => throw null;
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            @"Member 'C.M()' should be moved down. Valid file order:
C
C.Ev
C.A()
C.R()
C.M()");
    }

    [Fact]
    internal async Task When_type_contains_recursive_method_chain_in_invalid_order_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class C
                {
                    void X()
                    {
                        Y();
                    }

                    int [|Rec1|](string s)
                    {
                        if (true)
                        {
                            return Rec2(bool.Parse(s));
                        }
                    }

                    int Rec2(bool b)
                    {
                        if (true)
                        {
                            return Rec1(b.ToString());
                        }
                    }

                    void Y()
                    {
                        Rec2(true);
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            @"Member 'C.Rec1(string)' should be moved down. Valid file order:
C
C.X()
C.Y()
C.Rec2(bool)
C.Rec1(string)");
    }

    [Fact]
    internal async Task When_type_contains_self_recursive_method_in_invalid_order_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class C
                {
                    void [|X|]()
                    {
                    }

                    int SelfRecursive(int index)
                    {
                        X();

                        index--;
                        return index > 0 ? SelfRecursive(index) : index;
                    }

                    void Y()
                    {
                        SelfRecursive(5);
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            @"Member 'C.X()' should be moved down. Valid file order:
C
C.Y()
C.SelfRecursive(int)
C.X()");
    }

    [Fact]
    internal async Task When_type_contains_method_overloads_in_invalid_order_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class C
                {
                    int [|M|](bool b)
                    {
                        return b ? 1 : -1;
                    }

                    int M()
                    {
                        return M(true);
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            @"Member 'C.M(bool)' should be moved down. Valid file order:
C
C.M()
C.M(bool)");
    }

    [Fact]
    internal async Task When_type_contains_method_group_in_invalid_order_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .InGlobalScope(@"
                class C
                {
                    bool [|Filter|](string element) => true;

                    void M(string[] source)
                    {
                        _ = source.Where(Filter);
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            @"Member 'C.Filter(string)' should be moved down. Valid file order:
C
C.M(string[])
C.Filter(string)");
    }

    [Fact]
    internal async Task When_type_contains_user_defined_conversion_operator_in_invalid_order_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(Enumerable).Namespace)
            .InGlobalScope(@"
                class C
                {
                    public static explicit [|operator|] string(C c) => null;

                    public C()
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            @"Member 'C.explicit operator string(C)' should be moved down. Valid file order:
C
C.C()
C.explicit operator string(C)");
    }

    [Fact]
    internal async Task When_type_contains_constructor_overloads_in_invalid_order_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class C
                {
                    private [|C|]()
                    {
                    }

                    public C(string value) : this()
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            @"Member 'C.C()' should be moved down. Valid file order:
C
C.C(string)
C.C()");
    }

    [Fact]
    internal async Task When_type_contains_test_methods_in_invalid_order_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .WithReference(typeof(FactAttribute).Assembly)
            .Using(typeof(FactAttribute).Namespace)
            .InGlobalScope(@"
                public class C
                {
                    [Fact]
                    public void M1()
                    {
                        A();
                    }

                    private void [|A|]() => throw null;

                    [Fact]
                    public void M2()
                    {
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            @"Member 'C.A()' should be moved down. Valid file order:
C
C.M1()
C.M2()
C.A()");
    }

    [Fact]
    internal async Task When_method_contains_local_functions_in_invalid_order_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                public class C
                {
                    public void M()
                    {
                        B();

                        void A() => throw null;
                        void B() => A();
                        void C() => B();
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    [Fact]
    internal async Task When_type_contains_mixed_member_kinds_in_invalid_order_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .InGlobalScope(@"
                class C
                {
                    string _f = GetF();

                    static string [|GetF|]() => string.Empty;

                    public string P
                    {
                        get => GetP();
                    }

                    string GetP() => throw null;

                    public C(int i)
                    {
                        if (i > 0)
                        {
                            _f = i.ToString();
                        }
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            @"Member 'C.GetF()' should be moved down. Valid file order:
C
C._f
C.P
C.C(int)
C.GetF()
C.GetP()");
    }

    [Fact]
    internal async Task When_type_contains_missing_methods_in_invalid_order_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .AllowingCompileErrors()
            .InGlobalScope(@"
                public class C
                {
                    public void [|Y|]()
                    {
                        Missing();
                    }

                    public void X()
                    {
                        Y();
                    }
                }
            ")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            @"Member 'C.Y()' should be moved down. Valid file order:
C
C.X()
C.Y()");
    }

    [Fact]
    internal async Task When_file_contains_top_level_statements_in_invalid_order_it_must_be_skipped()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .WithOutputKind(OutputKind.ConsoleApplication)
            .InGlobalScope(@"
                B();

                void A()
                {
                    C();
                }

                void B()
                {
                }

                void C()
                {
                }")
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source);
    }

    protected override DiagnosticAnalyzer CreateAnalyzer()
    {
        return new PlaceMembersInCallingOrderAnalyzer();
    }
}
