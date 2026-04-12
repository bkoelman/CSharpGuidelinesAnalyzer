using System.Collections;
using System.Reflection;
using CSharpGuidelinesAnalyzer.Extensions;
using Microsoft.CodeAnalysis;

namespace CSharpGuidelinesAnalyzer;

internal sealed class OperationEqualityComparer
{
    private static readonly Type SymbolInterface = typeof(ISymbol);

    private static readonly Type OperationInterface = typeof(IOperation);

    private static readonly Type EnumerableInterface = typeof(IEnumerable);

    private static readonly IReadOnlyCollection<string> PropertyNamesToSkip = new[]
    {
        nameof(IOperation.Parent),
        nameof(IOperation.Syntax),
        "SemanticModel",
        "Compilation"
    };

    public static readonly OperationEqualityComparer Default = new();

    public bool Equals(IOperation? left, IOperation? right)
    {
        if (ReferenceEquals(left, right))
        {
            return true;
        }

        if (left is null || right is null)
        {
            return false;
        }

        IReadOnlyCollection<Type> leftInterfaces = left.GetType().GetMostSpecificOperationInterfaces();
        IReadOnlyCollection<Type> rightInterfaces = right.GetType().GetMostSpecificOperationInterfaces();

        return leftInterfaces.SequenceEqual(rightInterfaces) && AreOperationPropertiesEqual(leftInterfaces, left, right);
    }

    private bool AreOperationPropertiesEqual(IReadOnlyCollection<Type> interfaces, IOperation left, IOperation right)
    {
        foreach (PropertyInfo property in interfaces.DeepGetOperationProperties())
        {
            if (PropertyNamesToSkip.Contains(property.Name))
            {
                continue;
            }

            if (!ArePropertyValuesEqual(property, left, right))
            {
                return false;
            }
        }

        return true;
    }

    private bool ArePropertyValuesEqual(PropertyInfo property, object left, object right)
    {
        object leftValue = property.GetMethod.Invoke(left, []);
        object rightValue = property.GetMethod.Invoke(right, []);
        Type propertyType = property.PropertyType;

        if (EnumerableInterface.IsAssignableFrom(propertyType))
        {
            Type elementType = property.PropertyType.GetSequenceElementType();

            return AreOptionalSequenceValuesEqual(elementType, (IEnumerable)leftValue, (IEnumerable)rightValue);
        }

        return AreValuesEqual(property.PropertyType, leftValue, rightValue);
    }

    private bool AreOptionalSequenceValuesEqual(Type elementType, IEnumerable? leftSequence, IEnumerable? rightSequence)
    {
        if (ReferenceEquals(leftSequence, rightSequence))
        {
            return true;
        }

        if (leftSequence is null || rightSequence is null)
        {
            return false;
        }

        IEnumerator leftEnumerator = leftSequence.GetEnumerator();
        IEnumerator rightEnumerator = rightSequence.GetEnumerator();

        using var leftDisposable = leftEnumerator as IDisposable;
        using var rightDisposable = rightEnumerator as IDisposable;

        return AreSequenceValuesEqual(elementType, leftEnumerator, rightEnumerator);
    }

    private bool AreSequenceValuesEqual(Type elementType, IEnumerator leftEnumerator, IEnumerator rightEnumerator)
    {
        while (true)
        {
            if (leftEnumerator.MoveNext())
            {
                if (!rightEnumerator.MoveNext())
                {
                    return false;
                }

                if (!AreValuesEqual(elementType, leftEnumerator.Current, rightEnumerator.Current))
                {
                    return false;
                }
            }
            else
            {
                return !rightEnumerator.MoveNext();
            }
        }
    }

    private bool AreValuesEqual(Type type, object? leftValue, object? rightValue)
    {
        if (SymbolInterface.IsAssignableFrom(type))
        {
            var leftSymbol = (ISymbol?)leftValue;
            var rightSymbol = (ISymbol?)rightValue;

            return leftSymbol.IsEqualTo(rightSymbol);
        }

        if (OperationInterface.IsAssignableFrom(type))
        {
            return Equals((IOperation?)leftValue, (IOperation?)rightValue);
        }

        return EqualityComparer<object?>.Default.Equals(leftValue, rightValue);
    }
}
