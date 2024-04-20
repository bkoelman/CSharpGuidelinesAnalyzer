using System.Runtime.CompilerServices;
using CSharpGuidelinesAnalyzer.Rules.Maintainability;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

// @formatter:keep_existing_linebreaks true

namespace CSharpGuidelinesAnalyzer.Test.Specs.Maintainability;

public sealed partial class AvoidMemberWithManyStatementsSpecs : CSharpGuidelinesAnalysisTestFixture
{
    protected override string DiagnosticId => AvoidMemberWithManyStatementsAnalyzer.DiagnosticId;

    [Fact]
    internal async Task When_method_contains_mixed_set_of_statements_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(IDisposable).Namespace)
            .Using(typeof(CLSCompliantAttribute).Namespace)
            .Using(typeof(CallerMemberNameAttribute).Namespace)
            .InGlobalScope("""
                class C
                {
                    IDisposable AcquireResource() => throw null;
                    int field;
                
                    [CLSCompliant(isCompliant: false)]
                    [Obsolete(null, false)]
                    void [|M1|]()
                    {
                        using (var resource = AcquireResource())                    // 1
                        {
                            int i = true ? 1 : throw new NotSupportedException();   // 2
                
                            try                                                     // 3
                            {
                                int j = checked(i++);                               // 4
                                Action action = () => throw null;                   // 5
                            }
                            catch (Exception ex)
                            {
                                return;                                             // 6
                                throw;                                              // 7
                            }
                            finally
                            {
                                ;                                                   // 8
                            }
                        }
                    }
                
                    void [|M2|](int i, [CallerMemberName] string memberName = null)
                    {
                        switch (i)                                      // 1
                        {
                            case 1:
                            case 2:
                            case 3:
                                goto case 4;                            // 2
                            case 4:
                                goto default;                           // 3
                            default:
                            {
                                throw null;                             // 4
                            }
                        }
                
                        unsafe                                          // 5
                        {
                            fixed (int* p = &field)                     // 6
                            {
                                var x = stackalloc int[] { 1, 2, 3 };   // 7
                                int y = *x;                             // 8
                            }
                        }
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'C.M1()' contains 8 statements, which exceeds the maximum of 7 statements",
            "Method 'C.M2(int, string)' contains 8 statements, which exceeds the maximum of 7 statements");
    }

    [Fact]
    internal async Task When_async_method_contains_mixed_set_of_statements_it_must_be_reported()
    {
        // Arrange
        ParsedSourceCode source = new TypeSourceCodeBuilder()
            .Using(typeof(Task).Namespace)
            .InGlobalScope("""
                class C
                {
                    async Task [|M|](bool b = default(bool))
                    {
                        if (b)                      // 1
                        {
                            throw null;             // 2
                        }
                
                        await Task.Yield();         // 3
                
                        if (!b)                     // 4
                        {
                            await Task.Yield();     // 5
                            await Task.Yield();     // 6
                        }
                        else
                            throw null;             // 7
                
                        return;                     // 8
                    }
                }
                """)
            .Build();

        // Act and assert
        await VerifyGuidelineDiagnosticAsync(source,
            "Method 'C.M(bool)' contains 8 statements, which exceeds the maximum of 7 statements");
    }

    protected override DiagnosticAnalyzer CreateAnalyzer()
    {
        return new AvoidMemberWithManyStatementsAnalyzer();
    }
}
