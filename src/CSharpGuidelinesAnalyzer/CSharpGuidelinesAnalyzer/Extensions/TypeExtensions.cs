using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace CSharpGuidelinesAnalyzer.Extensions;

internal static class TypeExtensions
{
    [NotNull]
    [ItemNotNull]
    public static IReadOnlyCollection<Type> GetMostSpecificOperationInterfaces([NotNull] this Type type)
    {
        Type[] operationInterfaces = GetPublicOperationInterfaces(type, true).ToArray();

        var mostSpecificInterfaces = new List<Type>(operationInterfaces);

        foreach (Type parentInterface in operationInterfaces.SelectMany(@interface => GetPublicOperationInterfaces(@interface, false)))
        {
            mostSpecificInterfaces.Remove(parentInterface);
        }

        return mostSpecificInterfaces.OrderBy(@interface => @interface.FullName).ToArray();
    }

    [NotNull]
    [ItemNotNull]
    private static IEnumerable<Type> GetPublicOperationInterfaces([NotNull] Type type, bool includeSelf)
    {
        if (includeSelf && IsPublicOperationInterface(type))
        {
            yield return type;
        }

        foreach (Type @interface in type.GetTypeInfo().ImplementedInterfaces.Where(IsPublicOperationInterface))
        {
            yield return @interface;
        }
    }

    private static bool IsPublicOperationInterface([NotNull] Type type)
    {
        return type.GetTypeInfo().IsInterface && type.GetTypeInfo().IsPublic && type.Name.EndsWith("Operation", StringComparison.Ordinal);
    }

    [NotNull]
    [ItemNotNull]
    public static IReadOnlyCollection<PropertyInfo> DeepGetOperationProperties([NotNull] [ItemNotNull] this IEnumerable<Type> operationInterfaces)
    {
        var properties = new HashSet<PropertyInfo>();

        foreach (PropertyInfo property in operationInterfaces.SelectMany(@interface => GetPublicOperationInterfaces(@interface, true))
            .SelectMany(operationInterface => operationInterface.GetTypeInfo().DeclaredProperties))
        {
            properties.Add(property);
        }

        return properties;
    }

    [NotNull]
    public static Type GetSequenceElementType([NotNull] this Type type)
    {
        if (IsGenericEnumerable(type))
        {
            return type.GenericTypeArguments.Single();
        }

        Type enumerable = type.GetTypeInfo().ImplementedInterfaces.FirstOrDefault(IsGenericEnumerable);
        return enumerable != null ? enumerable.GenericTypeArguments.Single() : typeof(object);
    }

    private static bool IsGenericEnumerable([NotNull] Type type)
    {
        return type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>);
    }
}
