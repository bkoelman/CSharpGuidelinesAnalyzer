using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace CSharpGuidelinesAnalyzer.Test.RoslynTestFramework
{
    /// <summary>
    /// Parameter validations, intended to be used for member precondition checks.
    /// </summary>
    internal static class FrameworkGuard
    {
        [AssertionMethod]
        [ContractAnnotation("value: null => halt")]
        public static void NotNull<T>([CanBeNull] [NoEnumeration] T value, [NotNull] [InvokerParameterName] string name)
            where T : class
        {
            if (value is null)
            {
                throw new ArgumentNullException(name);
            }
        }

        [AssertionMethod]
        [ContractAnnotation("value: null => halt")]
        private static void NotNullNorEmpty<T>([CanBeNull] [ItemCanBeNull] IEnumerable<T> value,
            [NotNull] [InvokerParameterName] string name)
        {
            NotNull(value, name);

            if (!value.Any())
            {
                throw new ArgumentException($"'{name}' cannot be empty.", name);
            }
        }

        [AssertionMethod]
        [ContractAnnotation("value: null => halt")]
        public static void NotNullNorWhiteSpace([CanBeNull] string value, [NotNull] [InvokerParameterName] string name)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                NotNullNorEmpty(value, name);

                throw new ArgumentException($"'{name}' cannot contain only whitespace.", name);
            }
        }
    }
}
