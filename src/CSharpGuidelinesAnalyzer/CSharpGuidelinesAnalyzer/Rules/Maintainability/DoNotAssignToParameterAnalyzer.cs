using System.Collections.Immutable;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Rules.Maintainability
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DoNotAssignToParameterAnalyzer : GuidelineAnalyzer
    {
        public const string DiagnosticId = "AV1568";

        private const string Title = "Parameter value should not be overwritten in method body";
        private const string MessageFormat = "The value of parameter '{0}' is overwritten in its method body.";
        private const string Description = "Don't use parameters as temporary variables.";

        [NotNull]
        private static readonly AnalyzerCategory Category = AnalyzerCategory.Maintainability;

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category.Name, DiagnosticSeverity.Info, true, Description, Category.GetHelpLinkUri(DiagnosticId));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        private static readonly ImmutableArray<SpecialType> IntegralValueTypes = ImmutableArray.Create(SpecialType.System_Boolean,
            SpecialType.System_Char, SpecialType.System_SByte, SpecialType.System_Byte, SpecialType.System_Int16,
            SpecialType.System_UInt16, SpecialType.System_Int32, SpecialType.System_UInt32, SpecialType.System_Int64,
            SpecialType.System_UInt64, SpecialType.System_Decimal, SpecialType.System_Single, SpecialType.System_Double,
            SpecialType.System_IntPtr, SpecialType.System_UIntPtr, SpecialType.System_DateTime);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSyntaxNodeAction(c => c.SkipEmptyName(AnalyzeParameter), SyntaxKind.Parameter);
        }

        private void AnalyzeParameter(SymbolAnalysisContext context)
        {
            var parameter = (IParameterSymbol)context.Symbol;

            if (parameter.RefKind != RefKind.None || IsNonIntegralStruct(parameter) || parameter.IsSynthesized())
            {
                return;
            }

            if (!(parameter.ContainingSymbol is IMethodSymbol method) || method.IsAbstract)
            {
                return;
            }

            AnalyzeParameterUsageInMethod(parameter, method, context);
        }

        private static void AnalyzeParameterUsageInMethod([NotNull] IParameterSymbol parameter, [NotNull] IMethodSymbol method,
            SymbolAnalysisContext context)
        {
            SyntaxNode body = method.TryGetBodySyntaxForMethod(context.CancellationToken);
            if (body != null)
            {
                SemanticModel model = context.Compilation.GetSemanticModel(body.SyntaxTree);
                DataFlowAnalysis dataFlowAnalysis = model.AnalyzeDataFlow(body);
                if (dataFlowAnalysis.Succeeded)
                {
                    if (dataFlowAnalysis.WrittenInside.Contains(parameter))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Rule, parameter.Locations[0], parameter.Name));
                    }
                }
            }
        }

        private bool IsNonIntegralStruct([NotNull] IParameterSymbol parameter)
        {
            return parameter.Type.TypeKind == TypeKind.Struct && !IsIntegralType(parameter.Type);
        }

        private bool IsIntegralType([NotNull] ITypeSymbol type)
        {
            return type.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T ||
                IntegralValueTypes.Contains(type.SpecialType);
        }
    }
}
