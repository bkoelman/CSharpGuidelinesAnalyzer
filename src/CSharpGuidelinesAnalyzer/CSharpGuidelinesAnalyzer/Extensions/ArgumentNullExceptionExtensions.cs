using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace CSharpGuidelinesAnalyzer.Extensions;

internal static class ArgumentNullExceptionExtensions
{
    extension(ArgumentNullException)
    {
        public static void ThrowIfNullOrEmpty<T>([NotNull] IEnumerable<T?>? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
        {
            ArgumentNullException.ThrowIfNull(argument, paramName);

            if (!argument.Any())
            {
                throw new ArgumentException("The value cannot be empty.", paramName);
            }
        }
    }
}
