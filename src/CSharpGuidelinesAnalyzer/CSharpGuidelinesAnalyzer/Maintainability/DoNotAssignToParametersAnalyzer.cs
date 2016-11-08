using System.Collections.Immutable;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Maintainability
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DoNotAssignToParametersAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV1568";

        private const string Title = "The value of a parameter is overwritten in its method body.";
        private const string MessageFormat = "The value of parameter '{0}' is overwritten in its method body.";
        private const string Description = "Don't use parameters as temporary variables.";
        private const string Category = "Maintainability";

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category, DiagnosticSeverity.Warning, true, Description, HelpLinkUris.GetForCategory(Category, DiagnosticId));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        private static readonly ImmutableArray<SpecialType> IntegralValueTypes =
            new[]
            {
                SpecialType.System_Boolean,
                SpecialType.System_Char,
                SpecialType.System_SByte,
                SpecialType.System_Byte,
                SpecialType.System_Int16,
                SpecialType.System_UInt16,
                SpecialType.System_Int32,
                SpecialType.System_UInt32,
                SpecialType.System_Int64,
                SpecialType.System_UInt64,
                SpecialType.System_Decimal,
                SpecialType.System_Single,
                SpecialType.System_Double,
                SpecialType.System_IntPtr,
                SpecialType.System_UIntPtr,
                SpecialType.System_DateTime
            }.ToImmutableArray();

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSyntaxNodeAction(c => AnalyzeParameter(AnalysisUtilities.SyntaxToSymbolContext(c)),
                SyntaxKind.Parameter);
        }

        private void AnalyzeParameter(SymbolAnalysisContext context)
        {
            var parameter = (IParameterSymbol) context.Symbol;

            if (parameter.RefKind != RefKind.None)
            {
                return;
            }

            if (parameter.Type.TypeKind == TypeKind.Struct && !IsIntegralType(parameter.Type))
            {
                return;
            }

            var method = parameter.ContainingSymbol as IMethodSymbol;
            if (method == null || method.IsAbstract)
            {
                return;
            }

            SyntaxNode body = AnalysisUtilities.TryGetBodySyntaxForMethod(method, context.CancellationToken);
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

        private bool IsIntegralType([NotNull] ITypeSymbol type)
        {
            return type.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T ||
                IntegralValueTypes.Contains(type.SpecialType);
        }
    }
}