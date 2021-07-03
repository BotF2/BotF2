using System;
using System.Collections.Generic;

using Supremacy.Annotations;

using System.Linq;

using Supremacy.Collections;

namespace Supremacy.Types
{
    /// <summary>
    /// The base class from which all cloneable game objects should derive.
    /// </summary>
    [Serializable]
    public abstract class Cloneable : ICloneable
    {
        /// <summary>
        /// Creates a new instance for cloning purposes.
        /// </summary>
        /// <param name="context">The cloning context.</param>
        /// <returns>A new instance.</returns>
        /// <remarks>
        /// Since the object returned by this method will be cloned from this
        /// instance, this method should invoke the appropriate constructor
        /// so as to ensure that any <c>readonly</c> members are properly
        /// cloned (if necessary) and initialized.
        /// </remarks>
        protected abstract Cloneable CreateInstance(ICloneContext context);

        /// <summary>
        /// Clones this instance from <paramref name="source"/>.
        /// </summary>
        /// <param name="source">The object to be cloned.</param>
        /// <param name="context">The cloning context.</param>
        public virtual void CloneFrom(Cloneable source, ICloneContext context) { }

        /// <summary>
        /// Creates a clone of <paramref name="source"/>.  This is equivalent to
        /// calling <c>source.CreateInstance().CloneFrom(source, new CloneContext())</c>.
        /// </summary>
        /// <typeparam name="T">The type of <paramref name="source"/>.</typeparam>
        /// <param name="source">The object to be cloned.</param>
        /// <returns></returns>
        public static T Clone<T>(T source) where T : Cloneable
        {
            return Clone(source, new CloneContext());
        }

        /// <summary>
        /// Creates a clone of <paramref name="source"/>.  This is equivalent to
        /// calling <c>source.CreateInstance().CloneFrom(source, context)</c>.
        /// </summary>
        /// <typeparam name="T">The type of <paramref name="source"/>.</typeparam>
        /// <param name="source">The object to be cloned.</param>
        /// <param name="context">The cloning context.</param>
        /// <returns></returns>
        public static T Clone<T>(T source, ICloneContext context) where T : Cloneable
        {
            Cloneable clone = source.CreateInstance(context);
            clone.CloneFrom(source, context);
            return (T)clone;
        }

        public static IEnumerable<T> CloneAll<T>(IEnumerable<T> sourceObjects, ICloneContext context) where T : Cloneable
        {
            if (sourceObjects is IIndexedEnumerable<T> indexedEnumerable)
            {
                ArrayWrapper<T> resultArray = new ArrayWrapper<T>(indexedEnumerable.Count);
                sourceObjects.SelectInto(o => Clone(o, context), resultArray);
                return resultArray;
            }

            return sourceObjects.Select(o => Clone(o, context));
        }

        #region Implementation of ICloneable

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        object ICloneable.Clone()
        {
            return Clone(this);
        }

        #endregion
    }

    public interface ICloneContext
    {
        /// <summary>
        /// Gets or sets an optional user context.
        /// </summary>
        object UserContext { get; set; }

        /// <summary>
        /// Adds a mapping from <paramref name="source"/> to <paramref name="target"/>.
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <param name="target">The target object.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="source"/> has already been mapped.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> or <paramref name="target"/> is null.</exception>
        void AddMapping<T>([NotNull] T source, [NotNull] T target) where T : class;

        /// <summary>
        /// Retrieves the mapping target for <paramref name="source"/> if one exists and returns
        /// a <see cref="bool"/> indicating whether a mapping was found.
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <param name="target">The target object if a mapping exists; otherwise, <c>null</c>.</param>
        /// <returns><c>true</c> if a mapping exists; otherwise <c>false</c>.</returns>
        bool TryRemap<T>(T source, out T target) where T : class;

        /// <summary>
        /// If a mapping exists for <paramref name="source"/>, then the target is retuned;
        /// otherwise, <paramref name="source"/> is returned.
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <returns>The mapped object if one exists; otherwise, the source object.</returns>
        T Remap<T>(T source) where T : class;

        /// <summary>
        /// Retrieves the object mapped from <paramref name="source"/> if one exists.  If no
        /// mapping exists, the source is cloned, a mapping is created, and the clone is
        /// returned.
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <returns>The previously mapped clone, or a newly created and mapped clone.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> is null.</exception>
        /// <remarks>
        /// For the best results, <paramref name="source"/> should derive from <see cref="Cloneable"/>
        /// so that the clone may be created using this <see cref="CloneContext"/>.
        /// </remarks>
        T RemapOrClone<T>([NotNull] T source) where T : class, ICloneable;

        /// <summary>
        /// Clones <paramref name="source"/> and creates a mapping to the clone.
        /// </summary>
        /// <typeparam name="T">The type of <paramref name="source"/>.</typeparam>
        /// <param name="source">The object to be cloned.</param>
        /// <returns>The clone created from <paramref name="source"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if a mapping already exists for <paramref name="source"/>.</exception>
        T CloneAnMap<T>([NotNull] T source) where T : class, ICloneable;
    }

    /// <summary>
    /// Provides useful services for deep cloning operations, such as mappings from
    /// original objects to their clones.
    /// </summary>
    public class CloneContext : ICloneContext
    {
        public static readonly ICloneContext Null = new NullCloneContext();

        private Dictionary<object, object> _mappings;

        /// <summary>
        /// Creates a new <see cref="CloneContext"/> instance.
        /// </summary>
        /// <param name="userContext">An optional user context.</param>
        public CloneContext(object userContext = null)
        {
            UserContext = userContext;
        }

        /// <summary>
        /// Gets or sets an optional user context.
        /// </summary>
        public object UserContext { get; set; }

        /// <summary>
        /// Adds a mapping from <paramref name="source"/> to <paramref name="target"/>.
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <param name="target">The target object.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="source"/> has already been mapped.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> or <paramref name="target"/> is null.</exception>
        public void AddMapping<T>([NotNull] T source, [NotNull] T target) where T : class
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            if (_mappings == null)
            {
                _mappings = new Dictionary<object, object>();
            }


            if (_mappings.TryGetValue(source, out object existingMapping) && !ReferenceEquals(existingMapping, target))
            {
                throw new ArgumentException("A mapping has already been created for the specified object.", "source");
            }

            _mappings[source] = target;
        }

        /// <summary>
        /// Retrieves the mapping target for <paramref name="source"/> if one exists and returns
        /// a <see cref="bool"/> indicating whether a mapping was found.
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <param name="target">The target object if a mapping exists; otherwise, <c>null</c>.</param>
        /// <returns><c>true</c> if a mapping exists; otherwise <c>false</c>.</returns>
        public bool TryRemap<T>(T source, out T target) where T : class
        {

            if (_mappings == null)
            {
                target = null;
                return false;
            }

            if (_mappings.TryGetValue(source, out object untypedTarget))
            {
                return (target = untypedTarget as T) != null;
            }

            target = null;
            return false;
        }

        /// <summary>
        /// If a mapping exists for <paramref name="source"/>, then the target is retuned;
        /// otherwise, <paramref name="source"/> is returned.
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <returns>The mapped object if one exists; otherwise, the source object.</returns>
        public T Remap<T>(T source) where T : class
        {
            return TryRemap(source, out T target) ? target : source;
        }

        /// <summary>
        /// Retrieves the object mapped from <paramref name="source"/> if one exists.  If no
        /// mapping exists, the source is cloned, a mapping is created, and the clone is
        /// returned.
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <returns>The previously mapped clone, or a newly created and mapped clone.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> is null.</exception>
        /// <remarks>
        /// For the best results, <paramref name="source"/> should derive from <see cref="Cloneable"/>
        /// so that the clone may be created using this <see cref="CloneContext"/>.
        /// </remarks>
        public T RemapOrClone<T>([NotNull] T source) where T : class, ICloneable
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }


            if (_mappings == null)
            {
                _mappings = new Dictionary<object, object>();
            }
            else if (TryRemap(source, out T clone))
            {
                return clone;
            }

            return CloneAndMapCore(source);
        }

        /// <summary>
        /// Clones <paramref name="source"/> and creates a mapping to the clone.
        /// </summary>
        /// <typeparam name="T">The type of <paramref name="source"/>.</typeparam>
        /// <param name="source">The object to be cloned.</param>
        /// <returns>The clone created from <paramref name="source"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if a mapping already exists for <paramref name="source"/>.</exception>
        public T CloneAnMap<T>([NotNull] T source) where T : class, ICloneable
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (_mappings != null && _mappings.TryGetValue(source, out object existingMapping))
            {
                throw new ArgumentException("A mapping already exists for the specified object.", "source");
            }

            return CloneAndMapCore(source);
        }

        /// <summary>
        /// Performs the actual cloning and mapping for the <see cref="CloneAnMap{T}"/> and
        /// <see cref="RemapOrClone{T}"/> methods.
        /// </summary>
        /// <typeparam name="T">The type of <paramref name="source"/>.</typeparam>
        /// <param name="source">The object to be cloned.</param>
        /// <returns>The clone created from <paramref name="source"/>.</returns>
        /// <remarks>
        /// This method does not check whether a mapping already exists for <paramref name="source"/>.
        /// <see cref="CloneAnMap{T}"/> and <see cref="RemapOrClone{T}"/> already perform this check,
        /// and it is the responsibility of any other caller to perform this check if necessary.
        /// </remarks>
        protected T CloneAndMapCore<T>(T source) where T : class, ICloneable
        {
            T clone;

            if (source is Cloneable cloneable)
            {
                clone = Cloneable.Clone(cloneable, this) as T;
            }
            else
            {
                clone = (T)source.Clone();
            }

            _mappings[source] = clone;

            return clone;
        }

        private sealed class NullCloneContext : ICloneContext
        {
            #region Implementation of ICloneContext

            public object UserContext
            {
                get => null;
                set { }
            }

            public void AddMapping<T>(T source, T target) where T : class { }

            public bool TryRemap<T>(T source, out T target) where T : class
            {
                target = null;
                return false;
            }

            public T Remap<T>(T source) where T : class
            {
                return source;
            }

            // ReSharper disable ConditionIsAlwaysTrueOrFalse
            // ReSharper disable HeuristicUnreachableCode
            public T RemapOrClone<T>(T source) where T : class, ICloneable
            {
                if (source == null)
                {
                    return null;
                }

                return source.Clone() as T;
            }

            public T CloneAnMap<T>(T source) where T : class, ICloneable
            {
                if (source == null)
                {
                    return null;
                }

                return source.Clone() as T;
            }
            // ReSharper restore HeuristicUnreachableCode
            // ReSharper restore ConditionIsAlwaysTrueOrFalse

            #endregion
        }
    }
}