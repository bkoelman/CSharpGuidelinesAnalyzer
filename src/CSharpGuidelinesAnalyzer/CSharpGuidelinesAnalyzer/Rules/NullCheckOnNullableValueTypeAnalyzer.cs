using System;
using System.Collections.Immutable;
using System.Linq;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace CSharpGuidelinesAnalyzer.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class NullCheckOnNullableValueTypeAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV0000";

        private const string Title = "Expression of nullable value type is checked for null or not-null";
        private const string TypeMessageFormat = "Expression of nullable value type '{0}' is checked for {1}";

        private const string Description =
            "Internal analyzer that reports when an expression of type nullable value type is checked for null or not-null.";

        [NotNull]
        private static readonly AnalyzerCategory Category = AnalyzerCategory.Framework;

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, TypeMessageFormat,
            Category.DisplayName, DiagnosticSeverity.Hidden, false, Description, Category.GetHelpLinkUri(DiagnosticId));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(startContext =>
            {
                var knownSymbols = new KnownSymbols(startContext.Compilation);

                if (knownSymbols.NullableHasValueProperty != null)
                {
                    startContext.RegisterOperationAction(
                        c => c.SkipInvalid(_ => AnalyzePropertyReference(c, knownSymbols.NullableHasValueProperty)),
                        OperationKind.PropertyReference);
                }

                if (knownSymbols.HasMethods)
                {
                    startContext.RegisterOperationAction(c => c.SkipInvalid(_ => AnalyzeInvocation(c, knownSymbols)),
                        OperationKind.Invocation);
                }

                startContext.RegisterOperationAction(c => c.SkipInvalid(AnalyzeIsPattern), OperationKind.IsPattern);
                startContext.RegisterOperationAction(c => c.SkipInvalid(AnalyzeBinaryOperator), OperationKind.BinaryOperator);
            });
        }

        private void AnalyzePropertyReference(OperationAnalysisContext context,
            [NotNull] IPropertySymbol nullableHasValueProperty)
        {
            var propertyReference = (IPropertyReferenceOperation)context.Operation;

            if (propertyReference.Property.OriginalDefinition.Equals(nullableHasValueProperty) &&
                IsNullableValueType(propertyReference.Instance))
            {
                ReportAtOperation(propertyReference.Instance, context.ReportDiagnostic, false);
            }
        }

        private void AnalyzeInvocation(OperationAnalysisContext context, [NotNull] KnownSymbols knownSymbols)
        {
            var invocation = (IInvocationOperation)context.Operation;

            if (invocation.TargetMethod == null)
            {
                return;
            }

            if (invocation.Arguments.Length == 1)
            {
                AnalyzeSingleArgumentInvocation(invocation, knownSymbols, context);
            }
            else if (invocation.Arguments.Length == 2)
            {
                AnalyzeDoubleArgumentInvocation(invocation, knownSymbols, context);
            }
        }

        private void AnalyzeSingleArgumentInvocation([NotNull] IInvocationOperation invocation,
            [NotNull] KnownSymbols knownSymbols, OperationAnalysisContext context)
        {
            if (invocation.Instance == null)
            {
                return;
            }

            bool isNullableEquals = invocation.TargetMethod.OriginalDefinition.Equals(knownSymbols.NullableEqualsMethod);
            if (isNullableEquals)
            {
                bool isInverted = IsInverted(invocation);

                AnalyzeArguments(invocation.Instance, invocation.Arguments[0].Value, isInverted, context.ReportDiagnostic);
            }
        }

        private void AnalyzeDoubleArgumentInvocation([NotNull] IInvocationOperation invocation,
            [NotNull] KnownSymbols knownSymbols, OperationAnalysisContext context)
        {
            bool isObjectReferenceEquals = invocation.TargetMethod.Equals(knownSymbols.ObjectReferenceEqualsMethod);
            bool isStaticObjectEquals = invocation.TargetMethod.Equals(knownSymbols.StaticObjectEqualsMethod);
            bool isEqualityComparerEquals =
                invocation.TargetMethod.OriginalDefinition.Equals(knownSymbols.EqualityComparerEqualsMethod);

            if (isObjectReferenceEquals || isStaticObjectEquals || isEqualityComparerEquals)
            {
                IArgumentOperation leftArgument = invocation.Arguments[0];
                IArgumentOperation rightArgument = invocation.Arguments[1];

                bool isInverted = IsInverted(invocation);

                AnalyzeArguments(leftArgument.Value, rightArgument.Value, isInverted, context.ReportDiagnostic);
            }
        }

        private void AnalyzeIsPattern(OperationAnalysisContext context)
        {
            var isPattern = (IIsPatternOperation)context.Operation;

            if (isPattern.Pattern is IConstantPatternOperation constantPattern)
            {
                if (IsConstantNullOrDefault(constantPattern.Value) && IsNullableValueType(isPattern.Value))
                {
                    bool isInverted = IsInverted(isPattern);

                    ReportAtOperation(isPattern.Value, context.ReportDiagnostic, isInverted);
                }
            }
        }

        private void AnalyzeBinaryOperator(OperationAnalysisContext context)
        {
            var binaryOperator = (IBinaryOperation)context.Operation;

            bool isInverted;
            if (binaryOperator.OperatorKind == BinaryOperatorKind.Equals)
            {
                isInverted = false;
            }
            else if (binaryOperator.OperatorKind == BinaryOperatorKind.NotEquals)
            {
                isInverted = true;
            }
            else
            {
                return;
            }

            AnalyzeArguments(binaryOperator.LeftOperand, binaryOperator.RightOperand, isInverted, context.ReportDiagnostic);
        }

        private bool IsInverted([NotNull] IOperation operation)
        {
            bool isInverted = false;

            IOperation currentOperation = operation.Parent;
            while (currentOperation is IUnaryOperation unaryOperation && unaryOperation.OperatorKind == UnaryOperatorKind.Not)
            {
                isInverted = !isInverted;
                currentOperation = currentOperation.Parent;
            }

            return isInverted;
        }

        private void AnalyzeArguments([NotNull] IOperation leftArgument, [NotNull] IOperation rightArgument, bool isInverted,
            [NotNull] Action<Diagnostic> reportDiagnostic)
        {
            bool leftIsNull = IsConstantNullOrDefault(leftArgument);
            bool rightIsNull = IsConstantNullOrDefault(rightArgument);

            if (rightIsNull)
            {
                if (leftIsNull)
                {
                    return;
                }

                if (IsNullableValueType(leftArgument))
                {
                    ReportAtOperation(leftArgument, reportDiagnostic, isInverted);
                }
            }
            else
            {
                if (!leftIsNull)
                {
                    return;
                }

                if (IsNullableValueType(rightArgument))
                {
                    ReportAtOperation(rightArgument, reportDiagnostic, isInverted);
                }
            }
        }

        private bool IsConstantNullOrDefault([NotNull] IOperation operation)
        {
            IOperation source = operation is IConversionOperation conversion ? conversion.Operand : operation;

            if (source.ConstantValue.HasValue && source.ConstantValue.Value == null)
            {
                return true;
            }

            return source is IDefaultValueOperation;
        }

        private bool IsNullableValueType([NotNull] IOperation operation)
        {
            IOperation source = operation is IConversionOperation conversion ? conversion.Operand : operation;

            return source.Type.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T;
        }

        private static void ReportAtOperation([NotNull] IOperation operation, [NotNull] Action<Diagnostic> reportDiagnostic,
            bool isInverted)
        {
            reportDiagnostic(Diagnostic.Create(Rule, operation.Syntax.GetLocation(), operation.Syntax.ToString(),
                isInverted ? "not-null" : "null"));
        }

        private sealed class KnownSymbols
        {
            [CanBeNull]
            public IPropertySymbol NullableHasValueProperty { get; }

            [CanBeNull]
            public IMethodSymbol ObjectReferenceEqualsMethod { get; }

            [CanBeNull]
            public IMethodSymbol StaticObjectEqualsMethod { get; }

            [CanBeNull]
            public IMethodSymbol NullableEqualsMethod { get; }

            [CanBeNull]
            public IMethodSymbol EqualityComparerEqualsMethod { get; }

            public bool HasMethods =>
                ObjectReferenceEqualsMethod != null || StaticObjectEqualsMethod != null || NullableEqualsMethod != null ||
                EqualityComparerEqualsMethod != null;

            public KnownSymbols([NotNull] Compilation compilation)
            {
                Guard.NotNull(compilation, nameof(compilation));

                NullableHasValueProperty = ResolveNullableHasValueProperty(compilation);
                ObjectReferenceEqualsMethod = ResolveObjectReferenceEquals(compilation);
                StaticObjectEqualsMethod = ResolveStaticObjectEquals(compilation);
                NullableEqualsMethod = ResolveNullableEquals(compilation);
                EqualityComparerEqualsMethod = ResolveEqualityComparerEquals(compilation);
            }

            [CanBeNull]
            private static IPropertySymbol ResolveNullableHasValueProperty([NotNull] Compilation compilation)
            {
                INamedTypeSymbol nullableType = KnownTypes.SystemNullableT(compilation);
                return nullableType?.GetMembers("HasValue").OfType<IPropertySymbol>().FirstOrDefault();
            }

            [CanBeNull]
            private IMethodSymbol ResolveObjectReferenceEquals([NotNull] Compilation compilation)
            {
                INamedTypeSymbol objectType = KnownTypes.SystemObject(compilation);
                return objectType?.GetMembers("ReferenceEquals").OfType<IMethodSymbol>().FirstOrDefault();
            }

            [CanBeNull]
            private IMethodSymbol ResolveStaticObjectEquals([NotNull] Compilation compilation)
            {
                INamedTypeSymbol objectType = KnownTypes.SystemObject(compilation);
                return objectType?.GetMembers("Equals").OfType<IMethodSymbol>().FirstOrDefault(m => m.IsStatic);
            }

            [CanBeNull]
            private IMethodSymbol ResolveNullableEquals([NotNull] Compilation compilation)
            {
                INamedTypeSymbol nullableType = KnownTypes.SystemNullableT(compilation);
                return nullableType?.GetMembers("Equals").OfType<IMethodSymbol>().FirstOrDefault();
            }

            [CanBeNull]
            private IMethodSymbol ResolveEqualityComparerEquals([NotNull] Compilation compilation)
            {
                INamedTypeSymbol equalityComparerType = KnownTypes.SystemCollectionsGenericEqualityComparerT(compilation);
                return equalityComparerType?.GetMembers("Equals").OfType<IMethodSymbol>().FirstOrDefault();
            }
        }
    }
}
