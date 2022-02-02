using JetBrains.Annotations;
using Microsoft.CodeAnalysis;

namespace CSharpGuidelinesAnalyzer
{
    internal static class KnownTypes
    {
        [CanBeNull]
        public static INamedTypeSymbol SystemObject([NotNull] Compilation compilation)
        {
            Guard.NotNull(compilation, nameof(compilation));

            return compilation.GetSpecialType(SpecialType.System_Object);
        }

        [CanBeNull]
        public static INamedTypeSymbol SystemBoolean([NotNull] Compilation compilation)
        {
            Guard.NotNull(compilation, nameof(compilation));

            return compilation.GetSpecialType(SpecialType.System_Boolean);
        }

        [CanBeNull]
        public static INamedTypeSymbol SystemNullableT([NotNull] Compilation compilation)
        {
            Guard.NotNull(compilation, nameof(compilation));

            return compilation.GetSpecialType(SpecialType.System_Nullable_T);
        }

        [CanBeNull]
        public static INamedTypeSymbol SystemEventArgs([NotNull] Compilation compilation)
        {
            Guard.NotNull(compilation, nameof(compilation));

            return compilation.GetTypeByMetadataName("System.EventArgs");
        }

        [CanBeNull]
        public static INamedTypeSymbol SystemException([NotNull] Compilation compilation)
        {
            Guard.NotNull(compilation, nameof(compilation));

            return compilation.GetTypeByMetadataName("System.Exception");
        }

        [CanBeNull]
        public static INamedTypeSymbol SystemSystemException([NotNull] Compilation compilation)
        {
            Guard.NotNull(compilation, nameof(compilation));

            return compilation.GetTypeByMetadataName("System.SystemException");
        }

        [CanBeNull]
        public static INamedTypeSymbol SystemApplicationException([NotNull] Compilation compilation)
        {
            Guard.NotNull(compilation, nameof(compilation));

            return compilation.GetTypeByMetadataName("System.ApplicationException");
        }

        [CanBeNull]
        public static INamedTypeSymbol SystemThreadingTasksTask([NotNull] Compilation compilation)
        {
            Guard.NotNull(compilation, nameof(compilation));

            return compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
        }

        [CanBeNull]
        public static INamedTypeSymbol SystemThreadingTasksTaskT([NotNull] Compilation compilation)
        {
            Guard.NotNull(compilation, nameof(compilation));

            return compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");
        }

        [CanBeNull]
        public static INamedTypeSymbol SystemThreadingTasksValueTask([NotNull] Compilation compilation)
        {
            Guard.NotNull(compilation, nameof(compilation));

            return compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask");
        }

        [CanBeNull]
        public static INamedTypeSymbol SystemThreadingTasksValueTaskT([NotNull] Compilation compilation)
        {
            Guard.NotNull(compilation, nameof(compilation));

            return compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask`1");
        }

        [CanBeNull]
        public static INamedTypeSymbol SystemCollectionsGenericEqualityComparerT([NotNull] Compilation compilation)
        {
            Guard.NotNull(compilation, nameof(compilation));

            return compilation.GetTypeByMetadataName("System.Collections.Generic.EqualityComparer`1");
        }

        [CanBeNull]
        public static INamedTypeSymbol SystemCollectionsGenericIEnumerableT([NotNull] Compilation compilation)
        {
            Guard.NotNull(compilation, nameof(compilation));

            return compilation.GetTypeByMetadataName("System.Collections.Generic.IEnumerable`1");
        }

        [CanBeNull]
        public static INamedTypeSymbol SystemCollectionsGenericIAsyncEnumerableT([NotNull] Compilation compilation)
        {
            Guard.NotNull(compilation, nameof(compilation));

            return compilation.GetTypeByMetadataName("System.Collections.Generic.IAsyncEnumerable`1");
        }

        [CanBeNull]
        public static INamedTypeSymbol SystemCollectionsGenericIReadOnlyCollectionT([NotNull] Compilation compilation)
        {
            Guard.NotNull(compilation, nameof(compilation));

            return compilation.GetTypeByMetadataName("System.Collections.Generic.IReadOnlyCollection`1");
        }

        [CanBeNull]
        public static INamedTypeSymbol SystemCollectionsGenericIReadOnlyListT([NotNull] Compilation compilation)
        {
            Guard.NotNull(compilation, nameof(compilation));

            return compilation.GetTypeByMetadataName("System.Collections.Generic.IReadOnlyList`1");
        }

        [CanBeNull]
        public static INamedTypeSymbol SystemCollectionsGenericIReadOnlySetT([NotNull] Compilation compilation)
        {
            Guard.NotNull(compilation, nameof(compilation));

            return compilation.GetTypeByMetadataName("System.Collections.Generic.IReadOnlySet`1");
        }

        [CanBeNull]
        public static INamedTypeSymbol SystemCollectionsGenericIReadOnlyDictionaryTKeyTValue([NotNull] Compilation compilation)
        {
            Guard.NotNull(compilation, nameof(compilation));

            return compilation.GetTypeByMetadataName("System.Collections.Generic.IReadOnlyDictionary`2");
        }

        [CanBeNull]
        public static INamedTypeSymbol SystemRuntimeRemotingObjectHandle([NotNull] Compilation compilation)
        {
            Guard.NotNull(compilation, nameof(compilation));

            return compilation.GetTypeByMetadataName("System.Runtime.Remoting.ObjectHandle");
        }

        [CanBeNull]
        public static INamedTypeSymbol SystemLinqIOrderedEnumerableT([NotNull] Compilation compilation)
        {
            Guard.NotNull(compilation, nameof(compilation));

            return compilation.GetTypeByMetadataName("System.Linq.IOrderedEnumerable`1");
        }

        [CanBeNull]
        public static INamedTypeSymbol SystemLinqIGroupingTKeyTElement([NotNull] Compilation compilation)
        {
            Guard.NotNull(compilation, nameof(compilation));

            return compilation.GetTypeByMetadataName("System.Linq.IGrouping`2");
        }

        [CanBeNull]
        public static INamedTypeSymbol SystemLinqILookupTKeyTElement([NotNull] Compilation compilation)
        {
            Guard.NotNull(compilation, nameof(compilation));

            return compilation.GetTypeByMetadataName("System.Linq.ILookup`2");
        }

        [CanBeNull]
        public static INamedTypeSymbol SystemLinqIQueryable([NotNull] Compilation compilation)
        {
            Guard.NotNull(compilation, nameof(compilation));

            return compilation.GetTypeByMetadataName("System.Linq.IQueryable");
        }

        [CanBeNull]
        public static INamedTypeSymbol SystemLinqIQueryableT([NotNull] Compilation compilation)
        {
            Guard.NotNull(compilation, nameof(compilation));

            return compilation.GetTypeByMetadataName("System.Linq.IQueryable`1");
        }

        [CanBeNull]
        public static INamedTypeSymbol SystemLinqIOrderedQueryable([NotNull] Compilation compilation)
        {
            Guard.NotNull(compilation, nameof(compilation));

            return compilation.GetTypeByMetadataName("System.Linq.IOrderedQueryable");
        }

        [CanBeNull]
        public static INamedTypeSymbol SystemLinqIOrderedQueryableT([NotNull] Compilation compilation)
        {
            Guard.NotNull(compilation, nameof(compilation));

            return compilation.GetTypeByMetadataName("System.Linq.IOrderedQueryable`1");
        }
    }
}
