using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Semantics;

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

            context.RegisterSymbolAction(AnalyzeMethod, SymbolKind.Method);

            context.RegisterCompilationStartAction(startContext =>
            {
                if (startContext.Compilation.SupportsOperations())
                {
                    startContext.RegisterOperationBlockAction(
                        c => c.SkipInvalid(_ => AnalyzeCodeBlock(c, startContext.Compilation)));
                }
            });
        }

        private void AnalyzeMethod(SymbolAnalysisContext context)
        {
            var method = (IMethodSymbol) context.Symbol;

            MethodAnalysisContext methodContext = MethodAnalysisContext.FromSymbolAnalysisContext(context, method);
            AnalyzeParametersInMethod(methodContext);
        }

        private static void AnalyzeParametersInMethod(MethodAnalysisContext context)
        {
            if (context.Method.IsAbstract || !context.Method.Parameters.Any())
            {
                return;
            }

            SyntaxNode body = context.Method.TryGetBodySyntaxForMethod(context.CancellationToken);
            if (body != null)
            {
                SemanticModel model = context.Compilation.GetSemanticModel(body.SyntaxTree);
                DataFlowAnalysis dataFlowAnalysis = model.AnalyzeDataFlow(body);
                if (dataFlowAnalysis.Succeeded)
                {
                    foreach (IParameterSymbol parameter in context.Method.Parameters)
                    {
                        context.CancellationToken.ThrowIfCancellationRequested();

                        AnalyzeParameterInMethod(parameter, dataFlowAnalysis, context);
                    }
                }
            }
        }

        private static void AnalyzeParameterInMethod([NotNull] IParameterSymbol parameter,
            [NotNull] DataFlowAnalysis dataFlowAnalysis, MethodAnalysisContext context)
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

        private void AnalyzeCodeBlock(OperationBlockAnalysisContext context, [NotNull] Compilation compilation)
        {
            var walker = new OperationBlockWalker(context, compilation);

            foreach (IOperation operation in context.OperationBlocks)
            {
                walker.Visit(operation);
            }
        }

        private sealed class OperationBlockWalker : OperationWalker
        {
            private readonly OperationBlockAnalysisContext context;

            [NotNull]
            private readonly Compilation compilation;

            public OperationBlockWalker(OperationBlockAnalysisContext context, [NotNull] Compilation compilation)
            {
                Guard.NotNull(compilation, nameof(compilation));

                this.context = context;
                this.compilation = compilation;
            }

            public override void VisitLambdaExpression([NotNull] ILambdaExpression operation)
            {
                AnalyzeLambdaExpression(operation);

                base.VisitLambdaExpression(operation);
            }

            private void AnalyzeLambdaExpression([NotNull] ILambdaExpression operation)
            {
                MethodAnalysisContext methodContext = MethodAnalysisContext.FromOperationBlockAnalysisContext(context,
                    operation.Signature, compilation);

                AnalyzeParametersInMethod(methodContext);
            }
        }

        private struct MethodAnalysisContext
        {
            [NotNull]
            private readonly Action<Diagnostic> reportDiagnostic;

            [NotNull]
            public IMethodSymbol Method { get; }

            [NotNull]
            public Compilation Compilation { get; }

            public CancellationToken CancellationToken { get; }

            public MethodAnalysisContext([NotNull] IMethodSymbol method, [NotNull] Compilation compilation,
                CancellationToken cancellationToken, [NotNull] Action<Diagnostic> reportDiagnostic)
            {
                Guard.NotNull(method, nameof(method));
                Guard.NotNull(compilation, nameof(compilation));
                Guard.NotNull(reportDiagnostic, nameof(reportDiagnostic));

                Method = method;
                Compilation = compilation;
                CancellationToken = cancellationToken;
                this.reportDiagnostic = reportDiagnostic;
            }

            public void ReportDiagnostic([NotNull] Diagnostic diagnostic)
            {
                reportDiagnostic(diagnostic);
            }

            public static MethodAnalysisContext FromSymbolAnalysisContext(SymbolAnalysisContext context,
                [NotNull] IMethodSymbol method)
            {
                Guard.NotNull(method, nameof(method));

                return new MethodAnalysisContext(method, context.Compilation, context.CancellationToken,
                    context.ReportDiagnostic);
            }

            public static MethodAnalysisContext FromOperationBlockAnalysisContext(OperationBlockAnalysisContext context,
                [NotNull] IMethodSymbol method, [NotNull] Compilation compilation)
            {
                Guard.NotNull(method, nameof(method));
                Guard.NotNull(compilation, nameof(compilation));

                return new MethodAnalysisContext(method, compilation, context.CancellationToken,
                    context.ReportDiagnostic);
            }
        }
    }
}