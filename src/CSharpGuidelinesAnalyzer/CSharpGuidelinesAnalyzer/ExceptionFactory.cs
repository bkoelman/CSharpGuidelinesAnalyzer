using System;
using JetBrains.Annotations;

namespace CSharpGuidelinesAnalyzer
{
    /// <summary />
    internal static class ExceptionFactory
    {
        [NotNull]
        public static Exception Unreachable() => new Exception("This program location is thought to be unreachable.");
    }
}