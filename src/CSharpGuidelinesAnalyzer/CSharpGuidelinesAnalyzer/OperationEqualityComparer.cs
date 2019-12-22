using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using ReflectionTypeInfo = System.Reflection.TypeInfo;

namespace CSharpGuidelinesAnalyzer
{
    internal sealed class OperationEqualityComparer
    {
        [NotNull]
        private static readonly ReflectionTypeInfo SymbolInterface = typeof(ISymbol).GetTypeInfo();

        [NotNull]
        private static readonly ReflectionTypeInfo OperationInterface = typeof(IOperation).GetTypeInfo();

        [NotNull]
        private static readonly ReflectionTypeInfo EnumerableInterface = typeof(IEnumerable).GetTypeInfo();

        [NotNull]
        [ItemNotNull]
        private static readonly IReadOnlyCollection<string> PropertyNamesToSkip = new[]
        {
            nameof(IOperation.Parent),
            nameof(IOperation.Syntax),
            "SemanticModel",
            "Compilation"
        };

        [NotNull]
        public static readonly OperationEqualityComparer Default = new OperationEqualityComparer();

        public bool Equals([CanBeNull] IOperation left, [CanBeNull] IOperation right)
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

        private bool AreOperationPropertiesEqual([NotNull] [ItemNotNull] IReadOnlyCollection<Type> interfaces,
            [NotNull] IOperation left, [NotNull] IOperation right)
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

        private bool ArePropertyValuesEqual([NotNull] PropertyInfo property, [NotNull] object left, [NotNull] object right)
        {
            object leftValue = property.GetMethod.Invoke(left, Array.Empty<object>());
            object rightValue = property.GetMethod.Invoke(right, Array.Empty<object>());

            if (EnumerableInterface.IsAssignableFrom(property.PropertyType.GetTypeInfo()))
            {
                Type elementType = property.PropertyType.GetSequenceElementType();

                return AreOptionalSequenceValuesEqual(elementType, (IEnumerable)leftValue, (IEnumerable)rightValue);
            }

            return AreValuesEqual(property.PropertyType, leftValue, rightValue);
        }

        private bool AreOptionalSequenceValuesEqual([NotNull] Type elementType,
            [CanBeNull] [ItemNotNull] IEnumerable leftSequence, [CanBeNull] [ItemNotNull] IEnumerable rightSequence)
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

            return AreSequenceValuesEqual(elementType, leftEnumerator, rightEnumerator);
        }

        private bool AreSequenceValuesEqual([NotNull] Type elementType, [NotNull] IEnumerator leftEnumerator,
            [NotNull] IEnumerator rightEnumerator)
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

        private bool AreValuesEqual([NotNull] Type type, [CanBeNull] object leftValue, [CanBeNull] object rightValue)
        {
            if (SymbolInterface.IsAssignableFrom(type.GetTypeInfo()))
            {
                var leftSymbol = (ISymbol)leftValue;
                var rightSymbol = (ISymbol)rightValue;

                return Equals(leftSymbol, rightSymbol);
            }

            if (OperationInterface.IsAssignableFrom(type.GetTypeInfo()))
            {
                return Equals((IOperation)leftValue, (IOperation)rightValue);
            }

            return EqualityComparer<object>.Default.Equals(leftValue, rightValue);
        }
    }
}
