using Microsoft.CodeAnalysis;

namespace CSharpGuidelinesAnalyzer.Extensions;

internal static class SemanticModelExtensions
{
    public static DataFlowAnalysis? SafeAnalyzeDataFlow(this SemanticModel model, SyntaxNode bodySyntax)
    {
        DataFlowAnalysis dataFlowAnalysis = model.AnalyzeDataFlow(bodySyntax);
        return dataFlowAnalysis.Succeeded ? dataFlowAnalysis : null;
    }
}
