using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using JetBrains.Annotations;

namespace CSharpGuidelinesAnalyzer
{
    internal sealed class InsertionOrderPreservingDictionary<TKey, TItem> : KeyedCollection<TKey, (TKey Key, TItem Item)>
    {
        [NotNull]
        protected override TKey GetKeyForItem((TKey, TItem) item)
        {
            return item.Item1;
        }
    }

#pragma warning disable AV1130 // Return type in method signature should be a collection interface instead of a concrete type

    internal static class InsertionOrderPreservingDictionaryExtensions
    {
        [NotNull]
        public static InsertionOrderPreservingDictionary<TKey, TElement> ToInsertionOrderPreservingDictionary<TSource, TKey, TElement>(
            [NotNull] [ItemNotNull] this IEnumerable<TSource> source, [NotNull] Func<TSource, TKey> keySelector,
            [NotNull] Func<TSource, TElement> elementSelector)
        {
            var dictionary = new InsertionOrderPreservingDictionary<TKey, TElement>();

            foreach (TSource item in source)
            {
                TKey key = keySelector(item);
                TElement element = elementSelector(item);

                dictionary.Add((key, element));
            }

            return dictionary;
        }

        [NotNull]
        public static InsertionOrderPreservingDictionary<TKey, TSource> ToInsertionOrderPreservingDictionary<TSource, TKey>(
            [NotNull] [ItemNotNull] this IEnumerable<TSource> source, [NotNull] Func<TSource, TKey> keySelector)
        {
            return ToInsertionOrderPreservingDictionary(source, keySelector, element => element);
        }
    }
}
