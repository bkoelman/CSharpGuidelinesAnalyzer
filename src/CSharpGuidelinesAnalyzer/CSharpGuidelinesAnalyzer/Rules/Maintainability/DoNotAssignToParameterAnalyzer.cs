using System.Collections.Immutable;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace CSharpGuidelinesAnalyzer.Rules.Maintainability
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DoNotAssignToParameterAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV1568";

        private const string Title = "Parameter value should not be overwritten in method body";
        private const string MessageFormat = "The value of parameter '{0}' is overwritten in its method body.";
        private const string Description = "Don't use parameters as temporary variables.";

        [NotNull]
        private static readonly AnalyzerCategory Category = AnalyzerCategory.Maintainability;

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category.DisplayName, DiagnosticSeverity.Info, true, Description, Category.GetHelpLinkUri(DiagnosticId));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        private static readonly ImmutableArray<SpecialType> SimpleTypes = ImmutableArray.Create(SpecialType.System_Boolean,
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

            if (parameter.RefKind != RefKind.None || parameter.IsSynthesized())
            {
                return;
            }

            if (!(parameter.ContainingSymbol is IMethodSymbol method) || method.IsAbstract)
            {
                return;
            }

            if (IsUserDefinedStruct(parameter))
            {
                // A user-defined struct can reassign its 'this' parameter on invocation. That's why the compiler dataflow
                // analysis reports all access as writes. Because that's not very practical, we run our own assignment analysis.

                AnalyzeParameterUsageInMethodSlow(parameter, method, context);
            }
            else
            {
                AnalyzeParameterUsageInMethod(parameter, method, context);
            }
        }

        private bool IsUserDefinedStruct([NotNull] IParameterSymbol parameter)
        {
            return parameter.Type.TypeKind == TypeKind.Struct && !IsSimpleType(parameter.Type);
        }

        private bool IsSimpleType([NotNull] ITypeSymbol type)
        {
            return type.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T || SimpleTypes.Contains(type.SpecialType);
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

        private void AnalyzeParameterUsageInMethodSlow([NotNull] IParameterSymbol parameter, [NotNull] IMethodSymbol method,
            SymbolAnalysisContext context)
        {
            IOperation body = method.TryGetOperationBlockForMethod(context.Compilation, context.CancellationToken);
            if (body != null)
            {
                var walker = new AssignmentWalker(parameter);
                walker.Visit(body);

                if (walker.SeenAssignment)
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, parameter.Locations[0], parameter.Name));
                }
            }
        }

        private sealed class AssignmentWalker : OperationWalker
        {
            [NotNull]
            private readonly IParameterSymbol parameter;

            public bool SeenAssignment { get; private set; }

            public AssignmentWalker([NotNull] IParameterSymbol parameter)
            {
                Guard.NotNull(parameter, nameof(parameter));
                this.parameter = parameter;
            }

            public override void VisitSimpleAssignment([NotNull] ISimpleAssignmentOperation operation)
            {
                if (IsReferenceToCurrentParameter(operation.Target))
                {
                    SeenAssignment = true;
                    return;
                }

                base.VisitSimpleAssignment(operation);
            }

            public override void VisitCompoundAssignment([NotNull] ICompoundAssignmentOperation operation)
            {
                if (IsReferenceToCurrentParameter(operation.Target))
                {
                    SeenAssignment = true;
                    return;
                }

                base.VisitCompoundAssignment(operation);
            }

            public override void VisitIncrementOrDecrement([NotNull] IIncrementOrDecrementOperation operation)
            {
                if (IsReferenceToCurrentParameter(operation.Target))
                {
                    SeenAssignment = true;
                    return;
                }

                base.VisitIncrementOrDecrement(operation);
            }

            public override void VisitDeconstructionAssignment([NotNull] IDeconstructionAssignmentOperation operation)
            {
                if (operation.Target is ITupleOperation tuple)
                {
                    foreach (IOperation element in tuple.Elements)
                    {
                        if (IsReferenceToCurrentParameter(element))
                        {
                            SeenAssignment = true;
                            return;
                        }
                    }
                }

                base.VisitDeconstructionAssignment(operation);
            }

            public override void VisitArgument([NotNull] IArgumentOperation operation)
            {
                if (IsReferenceToCurrentParameter(operation.Value))
                {
                    if (operation.Parameter.RefKind == RefKind.Ref || operation.Parameter.RefKind == RefKind.Out)
                    {
                        SeenAssignment = true;
                        return;
                    }
                }

                base.VisitArgument(operation);
            }

            private bool IsReferenceToCurrentParameter([NotNull] IOperation operation)
            {
                return operation is IParameterReferenceOperation parameterReference &&
                    parameter.Equals(parameterReference.Parameter);
            }
        }
    }
}
