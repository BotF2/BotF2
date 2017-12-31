using System.Collections.Generic;

namespace Supremacy.Collections
{
    public interface IIndexedKeyedCollection<TKey, TValue> : IKeyedCollection<TKey, TValue>
    {
        int Count { get; }
        IEnumerable<TKey> Keys { get; }
        TValue this[int index] { get; }
        int IndexOf(TValue value);
    }
}