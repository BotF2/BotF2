using System.Collections.Generic;

using Supremacy.Annotations;

namespace Supremacy.Collections
{
    public interface IKeyedCollection<TKey, TValue> : IEnumerable<TValue>
    {
        [NotNull]
        TValue this[TKey key] { get; }
        bool Contains(TKey key);
        bool TryGetValue(TKey key, out TValue value);
        TKey GetKeyForItem(TValue item);
        [NotNull]
        IEqualityComparer<TKey> KeyComparer { get; }
    }
}