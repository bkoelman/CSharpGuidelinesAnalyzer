using JetBrains.Annotations;
using SysNotNull = System.Diagnostics.CodeAnalysis.NotNullAttribute;

namespace CSharpGuidelinesAnalyzer.Test.RoslynTestFramework;

/// <summary>
/// Parameter validations, intended to be used for member precondition checks.
/// </summary>
internal static class FrameworkGuard
{
    [AssertionMethod]
    public static void NotNull<T>([NoEnumeration] [SysNotNull] T? value, [InvokerParameterName] string name)
        where T : class
    {
        if (value is null)
        {
            throw new ArgumentNullException(name);
        }
    }

    [AssertionMethod]
    private static void NotNullNorEmpty<T>([SysNotNull] IEnumerable<T>? value, [InvokerParameterName] string name)
    {
        NotNull(value, name);

        if (!value.Any())
        {
            throw new ArgumentException($"'{name}' cannot be empty.", name);
        }
    }

    [AssertionMethod]
    public static void NotNullNorWhiteSpace([SysNotNull] string? value, [InvokerParameterName] string name)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            NotNullNorEmpty(value, name);

            throw new ArgumentException($"'{name}' cannot contain only whitespace.", name);
        }
    }
}
