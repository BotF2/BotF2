using System;

namespace Supremacy.Collections
{
    public class WeakReference<T> : WeakReference where T : class
    {
        public static WeakReference<T> Create(T target)
        {
            if (target == null)
                return WeakNullReference<T>.Singleton;

            return new WeakReference<T>(target);
        }

        protected WeakReference(T target)
            : base(target, false) { }

        public new T Target => (T)base.Target;
    }
}