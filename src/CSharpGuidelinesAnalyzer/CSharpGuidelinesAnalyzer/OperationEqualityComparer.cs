using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using ReflectionTypeInfo = System.Reflection.TypeInfo;

namespace CSharpGuidelinesAnalyzer;

internal sealed class OperationEqualityComparer
{
    private static readonly ReflectionTypeInfo SymbolInterface = typeof(ISymbol).GetTypeInfo();

    private static readonly ReflectionTypeInfo OperationInterface = typeof(IOperation).GetTypeInfo();

    private static readonly ReflectionTypeInfo EnumerableInterface = typeof(IEnumerable).GetTypeInfo();

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

    private bool AreOperationPropertiesEqual(IReadOnlyCollection<Type> interfaces, IOperation left,
        IOperation right)
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
        ReflectionTypeInfo propertyType = property.PropertyType.GetTypeInfo();

        if (EnumerableInterface.IsAssignableFrom(propertyType))
        {
            Type elementType = property.PropertyType.GetSequenceElementType();

            return AreOptionalSequenceValuesEqual(elementType, (IEnumerable)leftValue, (IEnumerable)rightValue);
        }

        return AreValuesEqual(property.PropertyType, leftValue, rightValue);
    }

    private bool AreOptionalSequenceValuesEqual(Type elementType, [ItemNotNull] IEnumerable? leftSequence,
        [ItemNotNull] IEnumerable? rightSequence)
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
        ReflectionTypeInfo typeInfo = type.GetTypeInfo();

        if (SymbolInterface.IsAssignableFrom(typeInfo))
        {
            var leftSymbol = (ISymbol?)leftValue;
            var rightSymbol = (ISymbol?)rightValue;

            return leftSymbol.IsEqualTo(rightSymbol);
        }

        if (OperationInterface.IsAssignableFrom(typeInfo))
        {
            return Equals((IOperation?)leftValue, (IOperation?)rightValue);
        }

        return EqualityComparer<object?>.Default.Equals(leftValue, rightValue);
    }
}
