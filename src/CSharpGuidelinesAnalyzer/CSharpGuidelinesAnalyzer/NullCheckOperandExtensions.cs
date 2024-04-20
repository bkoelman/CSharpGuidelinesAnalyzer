namespace CSharpGuidelinesAnalyzer;

internal static class NullCheckOperandExtensions
{
    public static NullCheckOperand Toggle(this NullCheckOperand operand)
    {
        return operand == NullCheckOperand.IsNull ? NullCheckOperand.IsNotNull : NullCheckOperand.IsNull;
    }

    public static NullCheckOperand CombineWith(this NullCheckOperand first, NullCheckOperand second)
    {
        if (first == NullCheckOperand.IsNull)
        {
            return second == NullCheckOperand.IsNull ? NullCheckOperand.IsNull : NullCheckOperand.IsNotNull;
        }

        return second == NullCheckOperand.IsNull ? NullCheckOperand.IsNotNull : NullCheckOperand.IsNull;
    }
}
