using Microsoft.CodeAnalysis;

namespace CSharpGuidelinesAnalyzer;

internal static class KnownTypes
{
    public static INamedTypeSymbol? SystemObject(Compilation compilation)
    {
        ArgumentNullException.ThrowIfNull(compilation);

        return compilation.GetSpecialType(SpecialType.System_Object);
    }

    public static INamedTypeSymbol? SystemBoolean(Compilation compilation)
    {
        ArgumentNullException.ThrowIfNull(compilation);

        return compilation.GetSpecialType(SpecialType.System_Boolean);
    }

    public static INamedTypeSymbol? SystemNullableT(Compilation compilation)
    {
        ArgumentNullException.ThrowIfNull(compilation);

        return compilation.GetSpecialType(SpecialType.System_Nullable_T);
    }

    public static INamedTypeSymbol? SystemEventArgs(Compilation compilation)
    {
        ArgumentNullException.ThrowIfNull(compilation);

        return compilation.GetTypeByMetadataName("System.EventArgs");
    }

    public static INamedTypeSymbol? SystemException(Compilation compilation)
    {
        ArgumentNullException.ThrowIfNull(compilation);

        return compilation.GetTypeByMetadataName("System.Exception");
    }

    public static INamedTypeSymbol? SystemSystemException(Compilation compilation)
    {
        ArgumentNullException.ThrowIfNull(compilation);

        return compilation.GetTypeByMetadataName("System.SystemException");
    }

    public static INamedTypeSymbol? SystemApplicationException(Compilation compilation)
    {
        ArgumentNullException.ThrowIfNull(compilation);

        return compilation.GetTypeByMetadataName("System.ApplicationException");
    }

    public static INamedTypeSymbol? SystemThreadingTasksTask(Compilation compilation)
    {
        ArgumentNullException.ThrowIfNull(compilation);

        return compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
    }

    public static INamedTypeSymbol? SystemThreadingTasksTaskT(Compilation compilation)
    {
        ArgumentNullException.ThrowIfNull(compilation);

        return compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");
    }

    public static INamedTypeSymbol? SystemThreadingTasksValueTask(Compilation compilation)
    {
        ArgumentNullException.ThrowIfNull(compilation);

        return compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask");
    }

    public static INamedTypeSymbol? SystemThreadingTasksValueTaskT(Compilation compilation)
    {
        ArgumentNullException.ThrowIfNull(compilation);

        return compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask`1");
    }

    public static INamedTypeSymbol? SystemRuntimeCompilerServicesConfiguredValueTaskAwaitable(Compilation compilation)
    {
        ArgumentNullException.ThrowIfNull(compilation);

        return compilation.GetTypeByMetadataName("System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable");
    }

    public static INamedTypeSymbol? SystemRuntimeCompilerServicesCallerArgumentExpressionAttribute(Compilation compilation)
    {
        ArgumentNullException.ThrowIfNull(compilation);

        return compilation.GetTypeByMetadataName("System.Runtime.CompilerServices.CallerArgumentExpressionAttribute");
    }

    public static INamedTypeSymbol? SystemRuntimeCompilerServicesConfiguredValueTaskAwaitableT(Compilation compilation)
    {
        ArgumentNullException.ThrowIfNull(compilation);

        return compilation.GetTypeByMetadataName("System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable`1");
    }

    public static INamedTypeSymbol? SystemCollectionsGenericEqualityComparerT(Compilation compilation)
    {
        ArgumentNullException.ThrowIfNull(compilation);

        return compilation.GetTypeByMetadataName("System.Collections.Generic.EqualityComparer`1");
    }

    public static INamedTypeSymbol? SystemCollectionsGenericIEnumerableT(Compilation compilation)
    {
        ArgumentNullException.ThrowIfNull(compilation);

        return compilation.GetTypeByMetadataName("System.Collections.Generic.IEnumerable`1");
    }

    public static INamedTypeSymbol? SystemCollectionsGenericIAsyncEnumerableT(Compilation compilation)
    {
        ArgumentNullException.ThrowIfNull(compilation);

        return compilation.GetTypeByMetadataName("System.Collections.Generic.IAsyncEnumerable`1");
    }

    public static INamedTypeSymbol? SystemCollectionsGenericIReadOnlyCollectionT(Compilation compilation)
    {
        ArgumentNullException.ThrowIfNull(compilation);

        return compilation.GetTypeByMetadataName("System.Collections.Generic.IReadOnlyCollection`1");
    }

    public static INamedTypeSymbol? SystemCollectionsGenericIReadOnlyListT(Compilation compilation)
    {
        ArgumentNullException.ThrowIfNull(compilation);

        return compilation.GetTypeByMetadataName("System.Collections.Generic.IReadOnlyList`1");
    }

    public static INamedTypeSymbol? SystemCollectionsGenericIReadOnlySetT(Compilation compilation)
    {
        ArgumentNullException.ThrowIfNull(compilation);

        return compilation.GetTypeByMetadataName("System.Collections.Generic.IReadOnlySet`1");
    }

    public static INamedTypeSymbol? SystemCollectionsGenericIReadOnlyDictionaryTKeyTValue(Compilation compilation)
    {
        ArgumentNullException.ThrowIfNull(compilation);

        return compilation.GetTypeByMetadataName("System.Collections.Generic.IReadOnlyDictionary`2");
    }

    public static INamedTypeSymbol? SystemRuntimeRemotingObjectHandle(Compilation compilation)
    {
        ArgumentNullException.ThrowIfNull(compilation);

        return compilation.GetTypeByMetadataName("System.Runtime.Remoting.ObjectHandle");
    }

    public static INamedTypeSymbol? SystemLinqIOrderedEnumerableT(Compilation compilation)
    {
        ArgumentNullException.ThrowIfNull(compilation);

        return compilation.GetTypeByMetadataName("System.Linq.IOrderedEnumerable`1");
    }

    public static INamedTypeSymbol? SystemLinqIGroupingTKeyTElement(Compilation compilation)
    {
        ArgumentNullException.ThrowIfNull(compilation);

        return compilation.GetTypeByMetadataName("System.Linq.IGrouping`2");
    }

    public static INamedTypeSymbol? SystemLinqILookupTKeyTElement(Compilation compilation)
    {
        ArgumentNullException.ThrowIfNull(compilation);

        return compilation.GetTypeByMetadataName("System.Linq.ILookup`2");
    }

    public static INamedTypeSymbol? SystemLinqIQueryable(Compilation compilation)
    {
        ArgumentNullException.ThrowIfNull(compilation);

        return compilation.GetTypeByMetadataName("System.Linq.IQueryable");
    }

    public static INamedTypeSymbol? SystemLinqIQueryableT(Compilation compilation)
    {
        ArgumentNullException.ThrowIfNull(compilation);

        return compilation.GetTypeByMetadataName("System.Linq.IQueryable`1");
    }

    public static INamedTypeSymbol? SystemLinqIOrderedQueryable(Compilation compilation)
    {
        ArgumentNullException.ThrowIfNull(compilation);

        return compilation.GetTypeByMetadataName("System.Linq.IOrderedQueryable");
    }

    public static INamedTypeSymbol? SystemLinqIOrderedQueryableT(Compilation compilation)
    {
        ArgumentNullException.ThrowIfNull(compilation);

        return compilation.GetTypeByMetadataName("System.Linq.IOrderedQueryable`1");
    }
}
