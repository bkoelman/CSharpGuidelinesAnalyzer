using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;

namespace CSharpGuidelinesAnalyzer
{
    /// <summary />
    internal struct TextCollection
    {
        [NotNull]
        [ItemNotNull]
        private readonly ICollection<string> items;

        [NotNull]
        [ItemNotNull]
        private readonly Lazy<ICollection<string>> lowerCaseItemsLazy;

        public TextCollection([NotNull] [ItemNotNull] ICollection<string> items)
        {
            Guard.NotNull(items, nameof(items));

            this.items = items;
            lowerCaseItemsLazy = new Lazy<ICollection<string>>(() => ToLowerCase(items), LazyThreadSafetyMode.None);
        }

        [NotNull]
        [ItemNotNull]
        private static ICollection<string> ToLowerCase([NotNull] [ItemNotNull] ICollection<string> items)
        {
            return items.Select(w => w.ToLowerInvariant()).ToArray();
        }

        public bool Contains([NotNull] string textToFind, TextMatchMode matchMode)
        {
            Guard.NotNull(textToFind, nameof(textToFind));

            if (items.Contains(textToFind))
            {
                return true;
            }

            if (matchMode == TextMatchMode.AllowLowerCaseMatch)
            {
                return lowerCaseItemsLazy.Value.Contains(textToFind.ToLowerInvariant());
            }

            return false;
        }
    }
}