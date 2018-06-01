#if DEBUG
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace CSharpGuidelinesAnalyzer
{
    // The (unused) implementations below and the visitor at the end help to detect IOperation interface changes on package upgrade.
    internal sealed partial class OperationEqualityComparer
    {
        // ReSharper disable UnusedMember.Local
#pragma warning disable RS1009 // Only internal implementations of this interface are allowed.

        private sealed class WrappingAddressOfOperation : WrappingOperation, IAddressOfOperation
        {
            [CanBeNull]
            public IOperation Reference => throw new NotSupportedException();
        }

        private sealed class WrappingAnonymousFunctionOperation : WrappingOperation, IAnonymousFunctionOperation
        {
            [CanBeNull]
            public IMethodSymbol Symbol => throw new NotSupportedException();

            [CanBeNull]
            public IBlockOperation Body => throw new NotSupportedException();
        }

        private sealed class WrappingAnonymousObjectCreationOperation : WrappingOperation, IAnonymousObjectCreationOperation
        {
            [ItemCanBeNull]
            public ImmutableArray<IOperation> Initializers => throw new NotSupportedException();
        }

        private sealed class WrappingArgumentOperation : WrappingOperation, IArgumentOperation
        {
            public ArgumentKind ArgumentKind => throw new NotSupportedException();

            [CanBeNull]
            public IParameterSymbol Parameter => throw new NotSupportedException();

            [CanBeNull]
            public IOperation Value => throw new NotSupportedException();

            public CommonConversion InConversion => throw new NotSupportedException();
            public CommonConversion OutConversion => throw new NotSupportedException();
        }

        private sealed class WrappingArrayCreationOperation : WrappingOperation, IArrayCreationOperation
        {
            [ItemCanBeNull]
            public ImmutableArray<IOperation> DimensionSizes => throw new NotSupportedException();

            [CanBeNull]
            public IArrayInitializerOperation Initializer => throw new NotSupportedException();
        }

        private sealed class WrappingArrayElementReferenceOperation : WrappingOperation, IArrayElementReferenceOperation
        {
            [CanBeNull]
            public IOperation ArrayReference => throw new NotSupportedException();

            [ItemCanBeNull]
            public ImmutableArray<IOperation> Indices => throw new NotSupportedException();
        }

        private sealed class WrappingArrayInitializerOperation : WrappingOperation, IArrayInitializerOperation
        {
            [ItemCanBeNull]
            public ImmutableArray<IOperation> ElementValues => throw new NotSupportedException();
        }

        private sealed class WrappingAwaitOperation : WrappingOperation, IAwaitOperation
        {
            [CanBeNull]
            public IOperation Operation => throw new NotSupportedException();
        }

        private sealed class WrappingBinaryOperation : WrappingOperation, IBinaryOperation
        {
            public BinaryOperatorKind OperatorKind => throw new NotSupportedException();

            [CanBeNull]
            public IOperation LeftOperand => throw new NotSupportedException();

            [CanBeNull]
            public IOperation RightOperand => throw new NotSupportedException();

            [CanBeNull]
            public IMethodSymbol OperatorMethod => throw new NotSupportedException();

            public bool IsLifted => throw new NotSupportedException();
            public bool IsChecked => throw new NotSupportedException();
            public bool IsCompareText => throw new NotSupportedException();
        }

        private sealed class WrappingBlockOperation : WrappingOperation, IBlockOperation
        {
            [ItemCanBeNull]
            public ImmutableArray<IOperation> Operations => throw new NotSupportedException();

            [ItemCanBeNull]
            public ImmutableArray<ILocalSymbol> Locals => throw new NotSupportedException();
        }

        private sealed class WrappingBranchOperation : WrappingOperation, IBranchOperation
        {
            [CanBeNull]
            public ILabelSymbol Target => throw new NotSupportedException();

            public BranchKind BranchKind => throw new NotSupportedException();
        }

        private sealed class WrappingCatchClauseOperation : WrappingOperation, ICatchClauseOperation
        {
            [CanBeNull]
            public IBlockOperation Handler => throw new NotSupportedException();

            [ItemCanBeNull]
            public ImmutableArray<ILocalSymbol> Locals => throw new NotSupportedException();

            [CanBeNull]
            public ITypeSymbol ExceptionType => throw new NotSupportedException();

            [CanBeNull]
            public IOperation ExceptionDeclarationOrExpression => throw new NotSupportedException();

            [CanBeNull]
            public IOperation Filter => throw new NotSupportedException();
        }

        private sealed class WrappingCoalesceOperation : WrappingOperation, ICoalesceOperation
        {
            [CanBeNull]
            public IOperation Value => throw new NotSupportedException();

            [CanBeNull]
            public IOperation WhenNull => throw new NotSupportedException();
        }

        private sealed class WrappingCollectionElementInitializerOperation
            : WrappingOperation, ICollectionElementInitializerOperation
        {
            [CanBeNull]
            public IMethodSymbol AddMethod => throw new NotSupportedException();

            [ItemCanBeNull]
            public ImmutableArray<IOperation> Arguments => throw new NotSupportedException();

            public bool IsDynamic => throw new NotSupportedException();
        }

        private sealed class WrappingCompoundAssignmentOperation : WrappingAssignmentOperation, ICompoundAssignmentOperation
        {
            public BinaryOperatorKind OperatorKind => throw new NotSupportedException();

            [CanBeNull]
            public IMethodSymbol OperatorMethod => throw new NotSupportedException();

            public bool IsLifted => throw new NotSupportedException();
            public bool IsChecked => throw new NotSupportedException();
            public CommonConversion InConversion => throw new NotSupportedException();
            public CommonConversion OutConversion => throw new NotSupportedException();
        }

        private sealed class WrappingConditionalOperation : WrappingOperation, IConditionalOperation
        {
            [CanBeNull]
            public IOperation Condition => throw new NotSupportedException();

            [CanBeNull]
            public IOperation WhenTrue => throw new NotSupportedException();

            [CanBeNull]
            public IOperation WhenFalse => throw new NotSupportedException();

            public bool IsRef => throw new NotSupportedException();
        }

        private sealed class WrappingConditionalAccessOperation : WrappingOperation, IConditionalAccessOperation
        {
            [CanBeNull]
            public IOperation Operation => throw new NotSupportedException();

            [CanBeNull]
            public IOperation WhenNotNull => throw new NotSupportedException();
        }

        private sealed class WrappingConditionalAccessInstanceOperation : WrappingOperation, IConditionalAccessInstanceOperation
        {
        }

        private sealed class WrappingConstantPatternOperation : WrappingPatternOperation, IConstantPatternOperation
        {
            [CanBeNull]
            public IOperation Value => throw new NotSupportedException();
        }

        private sealed class WrappingConversionOperation : WrappingOperation, IConversionOperation
        {
            [CanBeNull]
            public IOperation Operand => throw new NotSupportedException();

            [CanBeNull]
            public IMethodSymbol OperatorMethod => throw new NotSupportedException();

            public CommonConversion Conversion => throw new NotSupportedException();
            public bool IsTryCast => throw new NotSupportedException();
            public bool IsChecked => throw new NotSupportedException();
        }

        private sealed class WrappingDeclarationExpressionOperation : WrappingOperation, IDeclarationExpressionOperation
        {
            [CanBeNull]
            public IOperation Expression => throw new NotSupportedException();
        }

        private sealed class WrappingDeclarationPatternOperation : WrappingPatternOperation, IDeclarationPatternOperation
        {
            [CanBeNull]
            public ISymbol DeclaredSymbol => throw new NotSupportedException();
        }

        private sealed class WrappingDeconstructionAssignmentOperation
            : WrappingAssignmentOperation, IDeconstructionAssignmentOperation
        {
        }

        private sealed class WrappingDefaultCaseClauseOperation : WrappingCaseClauseOperation, IDefaultCaseClauseOperation
        {
        }

        private sealed class WrappingDefaultValueOperation : WrappingOperation, IDefaultValueOperation
        {
        }

        private sealed class WrappingDelegateCreationOperation : WrappingOperation, IDelegateCreationOperation
        {
            [CanBeNull]
            public IOperation Target => throw new NotSupportedException();
        }

        private sealed class WrappingDynamicIndexerAccessOperation : WrappingOperation, IDynamicIndexerAccessOperation
        {
            [CanBeNull]
            public IOperation Operation => throw new NotSupportedException();

            [ItemCanBeNull]
            public ImmutableArray<IOperation> Arguments => throw new NotSupportedException();
        }

        private sealed class WrappingDynamicInvocationOperation : WrappingOperation, IDynamicInvocationOperation
        {
            [CanBeNull]
            public IOperation Operation => throw new NotSupportedException();

            [ItemCanBeNull]
            public ImmutableArray<IOperation> Arguments => throw new NotSupportedException();
        }

        private sealed class WrappingDynamicMemberReferenceOperation : WrappingOperation, IDynamicMemberReferenceOperation
        {
            [CanBeNull]
            public IOperation Instance => throw new NotSupportedException();

            [CanBeNull]
            public string MemberName => throw new NotSupportedException();

            [ItemCanBeNull]
            public ImmutableArray<ITypeSymbol> TypeArguments => throw new NotSupportedException();

            [CanBeNull]
            public ITypeSymbol ContainingType => throw new NotSupportedException();
        }

        private sealed class WrappingDynamicObjectCreationOperation : WrappingOperation, IDynamicObjectCreationOperation
        {
            [CanBeNull]
            public IObjectOrCollectionInitializerOperation Initializer => throw new NotSupportedException();

            [ItemCanBeNull]
            public ImmutableArray<IOperation> Arguments => throw new NotSupportedException();
        }

        private sealed class WrappingEmptyOperation : WrappingOperation, IEmptyOperation
        {
        }

        private sealed class WrappingEndOperation : WrappingOperation, IEndOperation
        {
        }

        private sealed class WrappingEventAssignmentOperation : WrappingOperation, IEventAssignmentOperation
        {
            [CanBeNull]
            public IEventReferenceOperation EventReference => throw new NotSupportedException();

            [CanBeNull]
            public IOperation HandlerValue => throw new NotSupportedException();

            public bool Adds => throw new NotSupportedException();
        }

        private sealed class WrappingEventReferenceOperation : WrappingMemberReferenceOperation, IEventReferenceOperation
        {
            [CanBeNull]
            public IEventSymbol Event => throw new NotSupportedException();
        }

        private sealed class WrappingExpressionStatementOperation : WrappingOperation, IExpressionStatementOperation
        {
            [CanBeNull]
            public IOperation Operation => throw new NotSupportedException();
        }

        private sealed class WrappingFieldInitializerOperation : WrappingSymbolInitializerOperation, IFieldInitializerOperation
        {
            [ItemCanBeNull]
            public ImmutableArray<IFieldSymbol> InitializedFields => throw new NotSupportedException();
        }

        private sealed class WrappingFieldReferenceOperation : WrappingMemberReferenceOperation, IFieldReferenceOperation
        {
            [CanBeNull]
            public IFieldSymbol Field => throw new NotSupportedException();

            public bool IsDeclaration => throw new NotSupportedException();
        }

        private sealed class WrappingForEachLoopOperation : WrappingLoopOperation, IForEachLoopOperation
        {
            [CanBeNull]
            public IOperation LoopControlVariable => throw new NotSupportedException();

            [CanBeNull]
            public IOperation Collection => throw new NotSupportedException();

            [ItemCanBeNull]
            public ImmutableArray<IOperation> NextVariables => throw new NotSupportedException();
        }

        private sealed class WrappingForLoopOperation : WrappingLoopOperation, IForLoopOperation
        {
            [ItemCanBeNull]
            public ImmutableArray<IOperation> Before => throw new NotSupportedException();

            [CanBeNull]
            public IOperation Condition => throw new NotSupportedException();

            [ItemCanBeNull]
            public ImmutableArray<IOperation> AtLoopBottom => throw new NotSupportedException();
        }

        private sealed class WrappingForToLoopOperation : WrappingLoopOperation, IForToLoopOperation
        {
            [CanBeNull]
            public IOperation LoopControlVariable => throw new NotSupportedException();

            [CanBeNull]
            public IOperation InitialValue => throw new NotSupportedException();

            [CanBeNull]
            public IOperation LimitValue => throw new NotSupportedException();

            [CanBeNull]
            public IOperation StepValue => throw new NotSupportedException();

            [ItemCanBeNull]
            public ImmutableArray<IOperation> NextVariables => throw new NotSupportedException();
        }

        private sealed class WrappingIncrementOrDecrementOperation : WrappingOperation, IIncrementOrDecrementOperation
        {
            [CanBeNull]
            public IOperation Target => throw new NotSupportedException();

            [CanBeNull]
            public IMethodSymbol OperatorMethod => throw new NotSupportedException();

            public bool IsPostfix => throw new NotSupportedException();

            public bool IsLifted => throw new NotSupportedException();

            public bool IsChecked => throw new NotSupportedException();
        }

        private sealed class WrappingInstanceReferenceOperation : WrappingOperation, IInstanceReferenceOperation
        {
        }

        private sealed class WrappingInterpolatedStringOperation : WrappingOperation, IInterpolatedStringOperation
        {
            [ItemCanBeNull]
            public ImmutableArray<IInterpolatedStringContentOperation> Parts => throw new NotSupportedException();
        }

        private sealed class WrappingInterpolatedStringTextOperation
            : WrappingInterpolatedStringContentOperation, IInterpolatedStringTextOperation
        {
            [CanBeNull]
            public IOperation Text => throw new NotSupportedException();
        }

        private sealed class WrappingInterpolationOperation : WrappingInterpolatedStringContentOperation, IInterpolationOperation
        {
            [CanBeNull]
            public IOperation Expression => throw new NotSupportedException();

            [CanBeNull]
            public IOperation Alignment => throw new NotSupportedException();

            [CanBeNull]
            public IOperation FormatString => throw new NotSupportedException();
        }

        private sealed class WrappingInvalidOperation : WrappingOperation, IInvalidOperation
        {
        }

        private sealed class WrappingInvocationOperation : WrappingOperation, IInvocationOperation
        {
            [CanBeNull]
            public IMethodSymbol TargetMethod => throw new NotSupportedException();

            [CanBeNull]
            public IOperation Instance => throw new NotSupportedException();

            public bool IsVirtual => throw new NotSupportedException();

            [ItemCanBeNull]
            public ImmutableArray<IArgumentOperation> Arguments => throw new NotSupportedException();
        }

        private sealed class WrappingIsPatternOperation : WrappingOperation, IIsPatternOperation
        {
            [CanBeNull]
            public IOperation Value => throw new NotSupportedException();

            [CanBeNull]
            public IPatternOperation Pattern => throw new NotSupportedException();
        }

        private sealed class WrappingIsTypeOperation : WrappingOperation, IIsTypeOperation
        {
            [CanBeNull]
            public IOperation ValueOperand => throw new NotSupportedException();

            [CanBeNull]
            public ITypeSymbol TypeOperand => throw new NotSupportedException();

            public bool IsNegated => throw new NotSupportedException();
        }

        private sealed class WrappingLabeledOperation : WrappingOperation, ILabeledOperation
        {
            [CanBeNull]
            public ILabelSymbol Label => throw new NotSupportedException();

            [CanBeNull]
            public IOperation Operation => throw new NotSupportedException();
        }

        private sealed class WrappingLiteralOperation : WrappingOperation, ILiteralOperation
        {
        }

        private sealed class WrappingLocalFunctionOperation : WrappingOperation, ILocalFunctionOperation
        {
            [CanBeNull]
            public IMethodSymbol Symbol => throw new NotSupportedException();

            [CanBeNull]
            public IBlockOperation Body => throw new NotSupportedException();
        }

        private sealed class WrappingLocalReferenceOperation : WrappingOperation, ILocalReferenceOperation
        {
            [CanBeNull]
            public ILocalSymbol Local => throw new NotSupportedException();

            public bool IsDeclaration => throw new NotSupportedException();
        }

        private sealed class WrappingLockOperation : WrappingOperation, ILockOperation
        {
            [CanBeNull]
            public IOperation LockedValue => throw new NotSupportedException();

            [CanBeNull]
            public IOperation Body => throw new NotSupportedException();
        }

        private sealed class WrappingMemberInitializerOperation : WrappingOperation, IMemberInitializerOperation
        {
            [CanBeNull]
            public IOperation InitializedMember => throw new NotSupportedException();

            [CanBeNull]
            public IObjectOrCollectionInitializerOperation Initializer => throw new NotSupportedException();
        }

        private sealed class WrappingMethodReferenceOperation : WrappingMemberReferenceOperation, IMethodReferenceOperation
        {
            [CanBeNull]
            public IMethodSymbol Method => throw new NotSupportedException();

            public bool IsVirtual => throw new NotSupportedException();
        }

        private sealed class WrappingNameOfOperation : WrappingOperation, INameOfOperation
        {
            [CanBeNull]
            public IOperation Argument => throw new NotSupportedException();
        }

        private sealed class WrappingObjectCreationOperation : WrappingOperation, IObjectCreationOperation
        {
            [CanBeNull]
            public IMethodSymbol Constructor => throw new NotSupportedException();

            [ItemCanBeNull]
            public ImmutableArray<IArgumentOperation> Arguments => throw new NotSupportedException();

            [CanBeNull]
            public IObjectOrCollectionInitializerOperation Initializer => throw new NotSupportedException();
        }

        private sealed class WrappingObjectOrCollectionInitializerOperation
            : WrappingOperation, IObjectOrCollectionInitializerOperation
        {
            [ItemCanBeNull]
            public ImmutableArray<IOperation> Initializers => throw new NotSupportedException();
        }

        private sealed class WrappingOmittedArgumentOperation : WrappingOperation, IOmittedArgumentOperation
        {
        }

        private sealed class WrappingParameterInitializerOperation
            : WrappingSymbolInitializerOperation, IParameterInitializerOperation
        {
            [CanBeNull]
            public IParameterSymbol Parameter => throw new NotSupportedException();
        }

        private sealed class WrappingParameterReferenceOperation : WrappingOperation, IParameterReferenceOperation
        {
            [CanBeNull]
            public IParameterSymbol Parameter => throw new NotSupportedException();
        }

        private sealed class WrappingParenthesizedOperation : WrappingOperation, IParenthesizedOperation
        {
            [CanBeNull]
            public IOperation Operand => throw new NotSupportedException();
        }

        private sealed class WrappingPatternCaseClauseOperation : WrappingCaseClauseOperation, IPatternCaseClauseOperation
        {
            [CanBeNull]
            public ILabelSymbol Label => throw new NotSupportedException();

            [CanBeNull]
            public IPatternOperation Pattern => throw new NotSupportedException();

            [CanBeNull]
            public IOperation Guard => throw new NotSupportedException();
        }

        private sealed class WrappingPropertyInitializerOperation
            : WrappingSymbolInitializerOperation, IPropertyInitializerOperation
        {
            [ItemCanBeNull]
            public ImmutableArray<IPropertySymbol> InitializedProperties => throw new NotSupportedException();
        }

        private sealed class WrappingPropertyReferenceOperation : WrappingMemberReferenceOperation, IPropertyReferenceOperation
        {
            [CanBeNull]
            public IPropertySymbol Property => throw new NotSupportedException();

            [ItemCanBeNull]
            public ImmutableArray<IArgumentOperation> Arguments => throw new NotSupportedException();
        }

        private sealed class WrappingRaiseEventOperation : WrappingOperation, IRaiseEventOperation
        {
            [CanBeNull]
            public IEventReferenceOperation EventReference => throw new NotSupportedException();

            [ItemCanBeNull]
            public ImmutableArray<IArgumentOperation> Arguments => throw new NotSupportedException();
        }

        private sealed class WrappingRangeCaseClauseOperation : WrappingCaseClauseOperation, IRangeCaseClauseOperation
        {
            [CanBeNull]
            public IOperation MinimumValue => throw new NotSupportedException();

            [CanBeNull]
            public IOperation MaximumValue => throw new NotSupportedException();
        }

        private sealed class WrappingRelationalCaseClauseOperation : WrappingCaseClauseOperation, IRelationalCaseClauseOperation
        {
            [CanBeNull]
            public IOperation Value => throw new NotSupportedException();

            public BinaryOperatorKind Relation => throw new NotSupportedException();
        }

        private sealed class WrappingReturnOperation : WrappingOperation, IReturnOperation
        {
            [CanBeNull]
            public IOperation ReturnedValue => throw new NotSupportedException();
        }

        private sealed class WrappingSimpleAssignmentOperation : WrappingAssignmentOperation, ISimpleAssignmentOperation
        {
            public bool IsRef => throw new NotSupportedException();
        }

        private sealed class WrappingSingleValueCaseClauseOperation : WrappingCaseClauseOperation, ISingleValueCaseClauseOperation
        {
            [CanBeNull]
            public IOperation Value => throw new NotSupportedException();
        }

        private sealed class WrappingSizeOfOperation : WrappingOperation, ISizeOfOperation
        {
            [CanBeNull]
            public ITypeSymbol TypeOperand => throw new NotSupportedException();
        }

        private sealed class WrappingStopOperation : WrappingOperation, IStopOperation
        {
        }

        private sealed class WrappingSwitchOperation : WrappingOperation, ISwitchOperation
        {
            [CanBeNull]
            public IOperation Value => throw new NotSupportedException();

            [ItemCanBeNull]
            public ImmutableArray<ISwitchCaseOperation> Cases => throw new NotSupportedException();
        }

        private sealed class WrappingSwitchCaseOperation : WrappingOperation, ISwitchCaseOperation
        {
            [ItemCanBeNull]
            public ImmutableArray<ICaseClauseOperation> Clauses => throw new NotSupportedException();

            [ItemCanBeNull]
            public ImmutableArray<IOperation> Body => throw new NotSupportedException();
        }

        private sealed class WrappingThrowOperation : WrappingOperation, IThrowOperation
        {
            [CanBeNull]
            public IOperation Exception => throw new NotSupportedException();
        }

        private sealed class WrappingTranslatedQueryOperation : WrappingOperation, ITranslatedQueryOperation
        {
            [CanBeNull]
            public IOperation Operation => throw new NotSupportedException();
        }

        private sealed class WrappingTryOperation : WrappingOperation, ITryOperation
        {
            [CanBeNull]
            public IBlockOperation Body => throw new NotSupportedException();

            [ItemCanBeNull]
            public ImmutableArray<ICatchClauseOperation> Catches => throw new NotSupportedException();

            [CanBeNull]
            public IBlockOperation Finally => throw new NotSupportedException();
        }

        private sealed class WrappingTupleOperation : WrappingOperation, ITupleOperation
        {
            [ItemCanBeNull]
            public ImmutableArray<IOperation> Elements => throw new NotSupportedException();
        }

        private sealed class WrappingTypeOfOperation : WrappingOperation, ITypeOfOperation
        {
            [CanBeNull]
            public ITypeSymbol TypeOperand => throw new NotSupportedException();
        }

        private sealed class WrappingTypeParameterObjectCreationOperation
            : WrappingOperation, ITypeParameterObjectCreationOperation
        {
            [CanBeNull]
            public IObjectOrCollectionInitializerOperation Initializer => throw new NotSupportedException();
        }

        private sealed class WrappingUnaryOperation : WrappingOperation, IUnaryOperation
        {
            public UnaryOperatorKind OperatorKind => throw new NotSupportedException();

            [CanBeNull]
            public IOperation Operand => throw new NotSupportedException();

            [CanBeNull]
            public IMethodSymbol OperatorMethod => throw new NotSupportedException();

            public bool IsLifted => throw new NotSupportedException();

            public bool IsChecked => throw new NotSupportedException();
        }

        private sealed class WrappingUsingOperation : WrappingOperation, IUsingOperation
        {
            [CanBeNull]
            public IOperation Body => throw new NotSupportedException();

            [CanBeNull]
            public IOperation Resources => throw new NotSupportedException();
        }

        private sealed class WrappingVariableDeclarationOperation : WrappingOperation, IVariableDeclarationOperation
        {
            [ItemCanBeNull]
            public ImmutableArray<IVariableDeclaratorOperation> Declarators => throw new NotSupportedException();

            [CanBeNull]
            public IVariableInitializerOperation Initializer => throw new NotSupportedException();
        }

        private sealed class WrappingVariableDeclarationGroupOperation : WrappingOperation, IVariableDeclarationGroupOperation
        {
            [ItemCanBeNull]
            public ImmutableArray<IVariableDeclarationOperation> Declarations => throw new NotSupportedException();
        }

        private sealed class WrappingVariableDeclaratorOperation : WrappingOperation, IVariableDeclaratorOperation
        {
            [CanBeNull]
            public ILocalSymbol Symbol => throw new NotSupportedException();

            [CanBeNull]
            public IVariableInitializerOperation Initializer => throw new NotSupportedException();

            [ItemCanBeNull]
            public ImmutableArray<IOperation> IgnoredArguments => throw new NotSupportedException();
        }

        private sealed class WrappingVariableInitializerOperation
            : WrappingSymbolInitializerOperation, IVariableInitializerOperation
        {
        }

        private sealed class WrappingWhileLoopOperation : WrappingLoopOperation, IWhileLoopOperation
        {
            [CanBeNull]
            public IOperation Condition => throw new NotSupportedException();

            public bool ConditionIsTop => throw new NotSupportedException();

            public bool ConditionIsUntil => throw new NotSupportedException();

            [CanBeNull]
            public IOperation IgnoredCondition => throw new NotSupportedException();
        }
        // ReSharper restore UnusedMember.Local

        private abstract class WrappingOperation : IOperation
        {
            public void Accept([CanBeNull] OperationVisitor visitor) => throw new NotSupportedException();

            [CanBeNull]
            public TResult Accept<TArgument, TResult>([CanBeNull] OperationVisitor<TArgument, TResult> visitor,
                [CanBeNull] TArgument argument) =>
                throw new NotSupportedException();

            [CanBeNull]
            public IOperation Parent => throw new NotSupportedException();

            public OperationKind Kind => throw new NotSupportedException();

            [CanBeNull]
            public SyntaxNode Syntax => throw new NotSupportedException();

            [CanBeNull]
            public ITypeSymbol Type => throw new NotSupportedException();

            public Optional<object> ConstantValue => throw new NotSupportedException();

            [CanBeNull]
            [ItemCanBeNull]
            public IEnumerable<IOperation> Children => throw new NotSupportedException();

            [CanBeNull]
            public string Language => throw new NotSupportedException();

            public bool IsImplicit => throw new NotSupportedException();
        }

        private abstract class WrappingAssignmentOperation : WrappingOperation, IAssignmentOperation
        {
            [CanBeNull]
            public IOperation Target => throw new NotSupportedException();

            [CanBeNull]
            public IOperation Value => throw new NotSupportedException();
        }

        private abstract class WrappingPatternOperation : WrappingOperation, IPatternOperation
        {
        }

        private abstract class WrappingCaseClauseOperation : WrappingOperation, ICaseClauseOperation
        {
            public CaseKind CaseKind => throw new NotSupportedException();
        }

        private abstract class WrappingMemberReferenceOperation : WrappingOperation, IMemberReferenceOperation
        {
            [CanBeNull]
            public IOperation Instance => throw new NotSupportedException();

            [CanBeNull]
            public ISymbol Member => throw new NotSupportedException();
        }

        private abstract class WrappingSymbolInitializerOperation : WrappingOperation, ISymbolInitializerOperation
        {
            [CanBeNull]
            public IOperation Value => throw new NotSupportedException();
        }

        private abstract class WrappingLoopOperation : WrappingOperation, ILoopOperation
        {
            public LoopKind LoopKind => throw new NotSupportedException();

            [CanBeNull]
            public IOperation Body => throw new NotSupportedException();

            [ItemCanBeNull]
            public ImmutableArray<ILocalSymbol> Locals => throw new NotSupportedException();
        }

        private abstract class WrappingInterpolatedStringContentOperation : WrappingOperation, IInterpolatedStringContentOperation
        {
        }
#pragma warning restore RS1009 // Only internal implementations of this interface are allowed.

        // ReSharper disable once UnusedMember.Local
        private sealed class OperationOverrideVisitor : OperationVisitor
        {
            public override void DefaultVisit([NotNull] IOperation operation)
            {
                base.DefaultVisit(operation);
            }

            public override void Visit([NotNull] IOperation operation)
            {
                base.Visit(operation);
            }

            public override void VisitAddressOf([NotNull] IAddressOfOperation operation)
            {
                base.VisitAddressOf(operation);
            }

            public override void VisitAnonymousFunction([NotNull] IAnonymousFunctionOperation operation)
            {
                base.VisitAnonymousFunction(operation);
            }

            public override void VisitAnonymousObjectCreation([NotNull] IAnonymousObjectCreationOperation operation)
            {
                base.VisitAnonymousObjectCreation(operation);
            }

            public override void VisitArgument([NotNull] IArgumentOperation operation)
            {
                base.VisitArgument(operation);
            }

            public override void VisitArrayCreation([NotNull] IArrayCreationOperation operation)
            {
                base.VisitArrayCreation(operation);
            }

            public override void VisitArrayElementReference([NotNull] IArrayElementReferenceOperation operation)
            {
                base.VisitArrayElementReference(operation);
            }

            public override void VisitArrayInitializer([NotNull] IArrayInitializerOperation operation)
            {
                base.VisitArrayInitializer(operation);
            }

            public override void VisitAwait([NotNull] IAwaitOperation operation)
            {
                base.VisitAwait(operation);
            }

            public override void VisitBinaryOperator([NotNull] IBinaryOperation operation)
            {
                base.VisitBinaryOperator(operation);
            }

            public override void VisitBlock([NotNull] IBlockOperation operation)
            {
                base.VisitBlock(operation);
            }

            public override void VisitBranch([NotNull] IBranchOperation operation)
            {
                base.VisitBranch(operation);
            }

            public override void VisitCatchClause([NotNull] ICatchClauseOperation operation)
            {
                base.VisitCatchClause(operation);
            }

            public override void VisitCoalesce([NotNull] ICoalesceOperation operation)
            {
                base.VisitCoalesce(operation);
            }

            public override void VisitCollectionElementInitializer([NotNull] ICollectionElementInitializerOperation operation)
            {
                base.VisitCollectionElementInitializer(operation);
            }

            public override void VisitCompoundAssignment([NotNull] ICompoundAssignmentOperation operation)
            {
                base.VisitCompoundAssignment(operation);
            }

            public override void VisitConditional([NotNull] IConditionalOperation operation)
            {
                base.VisitConditional(operation);
            }

            public override void VisitConditionalAccess([NotNull] IConditionalAccessOperation operation)
            {
                base.VisitConditionalAccess(operation);
            }

            public override void VisitConditionalAccessInstance([NotNull] IConditionalAccessInstanceOperation operation)
            {
                base.VisitConditionalAccessInstance(operation);
            }

            public override void VisitConstantPattern([NotNull] IConstantPatternOperation operation)
            {
                base.VisitConstantPattern(operation);
            }

            public override void VisitConversion([NotNull] IConversionOperation operation)
            {
                base.VisitConversion(operation);
            }

            public override void VisitDeclarationExpression([NotNull] IDeclarationExpressionOperation operation)
            {
                base.VisitDeclarationExpression(operation);
            }

            public override void VisitDeclarationPattern([NotNull] IDeclarationPatternOperation operation)
            {
                base.VisitDeclarationPattern(operation);
            }

            public override void VisitDeconstructionAssignment([NotNull] IDeconstructionAssignmentOperation operation)
            {
                base.VisitDeconstructionAssignment(operation);
            }

            public override void VisitDefaultCaseClause([NotNull] IDefaultCaseClauseOperation operation)
            {
                base.VisitDefaultCaseClause(operation);
            }

            public override void VisitDefaultValue([NotNull] IDefaultValueOperation operation)
            {
                base.VisitDefaultValue(operation);
            }

            public override void VisitDelegateCreation([NotNull] IDelegateCreationOperation operation)
            {
                base.VisitDelegateCreation(operation);
            }

            public override void VisitDynamicIndexerAccess([NotNull] IDynamicIndexerAccessOperation operation)
            {
                base.VisitDynamicIndexerAccess(operation);
            }

            public override void VisitDynamicInvocation([NotNull] IDynamicInvocationOperation operation)
            {
                base.VisitDynamicInvocation(operation);
            }

            public override void VisitDynamicMemberReference([NotNull] IDynamicMemberReferenceOperation operation)
            {
                base.VisitDynamicMemberReference(operation);
            }

            public override void VisitDynamicObjectCreation([NotNull] IDynamicObjectCreationOperation operation)
            {
                base.VisitDynamicObjectCreation(operation);
            }

            public override void VisitEmpty([NotNull] IEmptyOperation operation)
            {
                base.VisitEmpty(operation);
            }

            public override void VisitEnd([NotNull] IEndOperation operation)
            {
                base.VisitEnd(operation);
            }

            public override void VisitEventAssignment([NotNull] IEventAssignmentOperation operation)
            {
                base.VisitEventAssignment(operation);
            }

            public override void VisitEventReference([NotNull] IEventReferenceOperation operation)
            {
                base.VisitEventReference(operation);
            }

            public override void VisitExpressionStatement([NotNull] IExpressionStatementOperation operation)
            {
                base.VisitExpressionStatement(operation);
            }

            public override void VisitFieldInitializer([NotNull] IFieldInitializerOperation operation)
            {
                base.VisitFieldInitializer(operation);
            }

            public override void VisitFieldReference([NotNull] IFieldReferenceOperation operation)
            {
                base.VisitFieldReference(operation);
            }

            public override void VisitForEachLoop([NotNull] IForEachLoopOperation operation)
            {
                base.VisitForEachLoop(operation);
            }

            public override void VisitForLoop([NotNull] IForLoopOperation operation)
            {
                base.VisitForLoop(operation);
            }

            public override void VisitForToLoop([NotNull] IForToLoopOperation operation)
            {
                base.VisitForToLoop(operation);
            }

            public override void VisitIncrementOrDecrement([NotNull] IIncrementOrDecrementOperation operation)
            {
                base.VisitIncrementOrDecrement(operation);
            }

            public override void VisitInstanceReference([NotNull] IInstanceReferenceOperation operation)
            {
                base.VisitInstanceReference(operation);
            }

            public override void VisitInterpolatedString([NotNull] IInterpolatedStringOperation operation)
            {
                base.VisitInterpolatedString(operation);
            }

            public override void VisitInterpolatedStringText([NotNull] IInterpolatedStringTextOperation operation)
            {
                base.VisitInterpolatedStringText(operation);
            }

            public override void VisitInterpolation([NotNull] IInterpolationOperation operation)
            {
                base.VisitInterpolation(operation);
            }

            public override void VisitInvalid([NotNull] IInvalidOperation operation)
            {
                base.VisitInvalid(operation);
            }

            public override void VisitInvocation([NotNull] IInvocationOperation operation)
            {
                base.VisitInvocation(operation);
            }

            public override void VisitIsPattern([NotNull] IIsPatternOperation operation)
            {
                base.VisitIsPattern(operation);
            }

            public override void VisitIsType([NotNull] IIsTypeOperation operation)
            {
                base.VisitIsType(operation);
            }

            public override void VisitLabeled([NotNull] ILabeledOperation operation)
            {
                base.VisitLabeled(operation);
            }

            public override void VisitLiteral([NotNull] ILiteralOperation operation)
            {
                base.VisitLiteral(operation);
            }

            public override void VisitLocalFunction([NotNull] ILocalFunctionOperation operation)
            {
                base.VisitLocalFunction(operation);
            }

            public override void VisitLocalReference([NotNull] ILocalReferenceOperation operation)
            {
                base.VisitLocalReference(operation);
            }

            public override void VisitLock([NotNull] ILockOperation operation)
            {
                base.VisitLock(operation);
            }

            public override void VisitMemberInitializer([NotNull] IMemberInitializerOperation operation)
            {
                base.VisitMemberInitializer(operation);
            }

            public override void VisitMethodReference([NotNull] IMethodReferenceOperation operation)
            {
                base.VisitMethodReference(operation);
            }

            public override void VisitNameOf([NotNull] INameOfOperation operation)
            {
                base.VisitNameOf(operation);
            }

            public override void VisitObjectCreation([NotNull] IObjectCreationOperation operation)
            {
                base.VisitObjectCreation(operation);
            }

            public override void VisitObjectOrCollectionInitializer([NotNull] IObjectOrCollectionInitializerOperation operation)
            {
                base.VisitObjectOrCollectionInitializer(operation);
            }

            public override void VisitOmittedArgument([NotNull] IOmittedArgumentOperation operation)
            {
                base.VisitOmittedArgument(operation);
            }

            public override void VisitParameterInitializer([NotNull] IParameterInitializerOperation operation)
            {
                base.VisitParameterInitializer(operation);
            }

            public override void VisitParameterReference([NotNull] IParameterReferenceOperation operation)
            {
                base.VisitParameterReference(operation);
            }

            public override void VisitParenthesized([NotNull] IParenthesizedOperation operation)
            {
                base.VisitParenthesized(operation);
            }

            public override void VisitPatternCaseClause([NotNull] IPatternCaseClauseOperation operation)
            {
                base.VisitPatternCaseClause(operation);
            }

            public override void VisitPropertyInitializer([NotNull] IPropertyInitializerOperation operation)
            {
                base.VisitPropertyInitializer(operation);
            }

            public override void VisitPropertyReference([NotNull] IPropertyReferenceOperation operation)
            {
                base.VisitPropertyReference(operation);
            }

            public override void VisitRaiseEvent([NotNull] IRaiseEventOperation operation)
            {
                base.VisitRaiseEvent(operation);
            }

            public override void VisitRangeCaseClause([NotNull] IRangeCaseClauseOperation operation)
            {
                base.VisitRangeCaseClause(operation);
            }

            public override void VisitRelationalCaseClause([NotNull] IRelationalCaseClauseOperation operation)
            {
                base.VisitRelationalCaseClause(operation);
            }

            public override void VisitReturn([NotNull] IReturnOperation operation)
            {
                base.VisitReturn(operation);
            }

            public override void VisitSimpleAssignment([NotNull] ISimpleAssignmentOperation operation)
            {
                base.VisitSimpleAssignment(operation);
            }

            public override void VisitSingleValueCaseClause([NotNull] ISingleValueCaseClauseOperation operation)
            {
                base.VisitSingleValueCaseClause(operation);
            }

            public override void VisitSizeOf([NotNull] ISizeOfOperation operation)
            {
                base.VisitSizeOf(operation);
            }

            public override void VisitStop([NotNull] IStopOperation operation)
            {
                base.VisitStop(operation);
            }

            public override void VisitSwitch([NotNull] ISwitchOperation operation)
            {
                base.VisitSwitch(operation);
            }

            public override void VisitSwitchCase([NotNull] ISwitchCaseOperation operation)
            {
                base.VisitSwitchCase(operation);
            }

            public override void VisitThrow([NotNull] IThrowOperation operation)
            {
                base.VisitThrow(operation);
            }

            public override void VisitTranslatedQuery([NotNull] ITranslatedQueryOperation operation)
            {
                base.VisitTranslatedQuery(operation);
            }

            public override void VisitTry([NotNull] ITryOperation operation)
            {
                base.VisitTry(operation);
            }

            public override void VisitTuple([NotNull] ITupleOperation operation)
            {
                base.VisitTuple(operation);
            }

            public override void VisitTypeOf([NotNull] ITypeOfOperation operation)
            {
                base.VisitTypeOf(operation);
            }

            public override void VisitTypeParameterObjectCreation([NotNull] ITypeParameterObjectCreationOperation operation)
            {
                base.VisitTypeParameterObjectCreation(operation);
            }

            public override void VisitUnaryOperator([NotNull] IUnaryOperation operation)
            {
                base.VisitUnaryOperator(operation);
            }

            public override void VisitUsing([NotNull] IUsingOperation operation)
            {
                base.VisitUsing(operation);
            }

            public override void VisitVariableDeclaration([NotNull] IVariableDeclarationOperation operation)
            {
                base.VisitVariableDeclaration(operation);
            }

            public override void VisitVariableDeclarationGroup([NotNull] IVariableDeclarationGroupOperation operation)
            {
                base.VisitVariableDeclarationGroup(operation);
            }

            public override void VisitVariableDeclarator([NotNull] IVariableDeclaratorOperation operation)
            {
                base.VisitVariableDeclarator(operation);
            }

            public override void VisitVariableInitializer([NotNull] IVariableInitializerOperation operation)
            {
                base.VisitVariableInitializer(operation);
            }

            public override void VisitWhileLoop([NotNull] IWhileLoopOperation operation)
            {
                base.VisitWhileLoop(operation);
            }
        }
    }
}
#endif
