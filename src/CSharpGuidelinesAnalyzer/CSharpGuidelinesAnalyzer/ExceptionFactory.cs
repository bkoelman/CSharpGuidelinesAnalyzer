using System;
using JetBrains.Annotations;

namespace CSharpGuidelinesAnalyzer
{
    /// <summary />
#pragma warning disable AV1008 // Class should not be static
    internal static class ExceptionFactory
#pragma warning restore AV1008 // Class should not be static
    {
        [NotNull]
        public static Exception Unreachable() => new Exception("This program location is thought to be unreachable.");
    }
}
