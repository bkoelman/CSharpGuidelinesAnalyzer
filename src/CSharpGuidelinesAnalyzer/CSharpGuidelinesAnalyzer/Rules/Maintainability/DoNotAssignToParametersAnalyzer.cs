using System.Collections.Immutable;
using System.Linq;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Rules.Maintainability
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
            ImmutableArray.Create(SpecialType.System_Boolean, SpecialType.System_Char, SpecialType.System_SByte,
                SpecialType.System_Byte, SpecialType.System_Int16, SpecialType.System_UInt16, SpecialType.System_Int32,
                SpecialType.System_UInt32, SpecialType.System_Int64, SpecialType.System_UInt64,
                SpecialType.System_Decimal, SpecialType.System_Single, SpecialType.System_Double,
                SpecialType.System_IntPtr, SpecialType.System_UIntPtr, SpecialType.System_DateTime);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSyntaxNodeAction(AnalyzeSyntax, SyntaxKind.MethodDeclaration,
                SyntaxKind.ConstructorDeclaration, SyntaxKind.OperatorDeclaration,
                SyntaxKind.ConversionOperatorDeclaration, SyntaxKind.GetAccessorDeclaration,
                SyntaxKind.SetAccessorDeclaration, SyntaxKind.AddAccessorDeclaration,
                SyntaxKind.RemoveAccessorDeclaration, SyntaxKind.ParenthesizedLambdaExpression,
                SyntaxKind.SimpleLambdaExpression, SyntaxKind.AnonymousMethodExpression, SyntaxKind.LetClause,
                SyntaxKind.WhereClause, SyntaxKind.AscendingOrdering, SyntaxKind.DescendingOrdering,
                SyntaxKind.JoinClause, SyntaxKind.GroupClause, SyntaxKind.SelectClause, SyntaxKind.FromClause);
        }

        private void AnalyzeSyntax(SyntaxNodeAnalysisContext context)
        {
            IMethodSymbol method = TryGetMethod(context.Node, context.SemanticModel);

            if (method != null)
            {
                AnalyzeMethod(context, method);
            }
        }

        [CanBeNull]
        private IMethodSymbol TryGetMethod([NotNull] SyntaxNode syntax, [NotNull] SemanticModel semanticModel)
        {
            return semanticModel.GetDeclaredSymbol(syntax) as IMethodSymbol ??
                semanticModel.GetSymbolInfo(syntax).Symbol as IMethodSymbol;
        }

        private static void AnalyzeMethod(SyntaxNodeAnalysisContext context, [NotNull] IMethodSymbol method)
        {
            if (method.IsAbstract || !method.Parameters.Any())
            {
                return;
            }

            SyntaxNode body = method.TryGetBodySyntaxForMethod(context.CancellationToken);
            if (body != null)
            {
                SemanticModel model = context.Compilation.GetSemanticModel(body.SyntaxTree);
                DataFlowAnalysis dataFlowAnalysis = model.AnalyzeDataFlow(body);
                if (dataFlowAnalysis.Succeeded)
                {
                    foreach (IParameterSymbol parameter in method.Parameters)
                    {
                        context.CancellationToken.ThrowIfCancellationRequested();

                        AnalyzeParameter(parameter, dataFlowAnalysis, context);
                    }
                }
            }
        }

        private static void AnalyzeParameter([NotNull] IParameterSymbol parameter,
            [NotNull] DataFlowAnalysis dataFlowAnalysis, SyntaxNodeAnalysisContext context)
        {
            if (parameter.Name.Length == 0)
            {
                return;
            }

            if (parameter.RefKind != RefKind.None)
            {
                return;
            }

            if (parameter.Type.TypeKind == TypeKind.Struct && !IsIntegralType(parameter.Type))
            {
                return;
            }

            if (dataFlowAnalysis.WrittenInside.Contains(parameter))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, parameter.Locations[0], parameter.Name));
            }
        }

        private static bool IsIntegralType([NotNull] ITypeSymbol type)
        {
            return type.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T ||
                IntegralValueTypes.Contains(type.SpecialType);
        }
    }
}