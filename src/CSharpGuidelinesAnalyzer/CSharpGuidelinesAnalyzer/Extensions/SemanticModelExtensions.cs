using System;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;

namespace CSharpGuidelinesAnalyzer.Extensions
{
    internal static class SemanticModelExtensions
    {
        [CanBeNull]
        public static DataFlowAnalysis SafeAnalyzeDataFlow([NotNull] this SemanticModel model, [NotNull] SyntaxNode bodySyntax)
        {
            try
            {
                DataFlowAnalysis dataFlowAnalysis = model.AnalyzeDataFlow(bodySyntax);
                return dataFlowAnalysis.Succeeded ? dataFlowAnalysis : null;
            }
            catch (NullReferenceException)
            {
                // Bug workaround for https://github.com/dotnet/roslyn/issues/27969
                return null;
            }
        }
    }
}
