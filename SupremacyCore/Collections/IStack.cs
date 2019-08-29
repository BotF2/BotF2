using System.Collections.Generic;

namespace Supremacy.Collections
{
    public interface IStack<T> : IEnumerable<T>
    {
        IStack<T> Pop();
        T Peek();
        bool IsEmpty { get; }
    }
}
