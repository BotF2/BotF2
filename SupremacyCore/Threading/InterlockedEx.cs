using System.Threading;

namespace Supremacy.Threading
{
    public static class SyncMethods
    {
        public static bool CompareAndSwap<T>(ref T location, T comparand, T newValue) where T : class
        {
            return (comparand == Interlocked.CompareExchange(ref location, newValue, comparand));
        }
    }
}