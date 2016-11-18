using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;

namespace CSharpGuidelinesAnalyzer.Extensions
{
    /// <summary />
    internal static class CompilationExtensions
    {
        public static bool SupportsOperations([NotNull] this Compilation compilation)
        {
            Guard.NotNull(compilation, nameof(compilation));

            IReadOnlyDictionary<string, string> features = compilation.SyntaxTrees.FirstOrDefault()?.Options.Features;
            return features != null && features.ContainsKey("IOperation") && features["IOperation"] == "true";
        }
    }
}