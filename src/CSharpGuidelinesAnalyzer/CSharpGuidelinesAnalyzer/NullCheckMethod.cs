namespace CSharpGuidelinesAnalyzer;

internal enum NullCheckMethod
{
    EqualityOperator,
    IsPattern,
    NullableHasValueMethod,
    NullableEqualsMethod,
    StaticObjectEqualsMethod,
    StaticObjectReferenceEqualsMethod,
    EqualityComparerEqualsMethod
}