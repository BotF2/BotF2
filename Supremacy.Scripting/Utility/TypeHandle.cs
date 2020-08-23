using System;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;

namespace Supremacy.Scripting.Utility
{
    /// <summary>There is exactly one instance of this class per type.</summary>
    public sealed class TypeHandle : IMemberContainer
    {
        private static int _nextId;

        private static PtrHashtable _typeHash;

        private static TypeHandle _objectType;
        private static TypeHandle _arrayType;
        private readonly IMemberContainer _baseContainer;
        private readonly int _id = ++_nextId;

        static TypeHandle()
        {
            Reset();
        }

        private TypeHandle(Type type)
        {
            Type = type;
            Name = type.FullName ?? type.Name;
            if (type.BaseType != null)
            {
                BaseCache = GetMemberCache(type.BaseType);
                _baseContainer = BaseCache.Container;
            }
            else if (type.IsInterface)
            {
                BaseCache = TypeManager.LookupBaseInterfacesCache(type);
            }

            IsInterface = type.IsInterface || TypeManager.IsGenericParameter(type);
            MemberCache = new MemberCache(this);
        }

        /// <summary>Returns the TypeHandle for TypeManager.object_type.</summary>
        public static IMemberContainer ObjectType
        {
            get
            {
                if (_objectType != null)
                {
                    return _objectType;
                }

                _objectType = GetTypeHandle(TypeManager.CoreTypes.Object);

                return _objectType;
            }
        }

        /// <summary>Returns the TypeHandle for TypeManager.array_type.</summary>
        public static TypeHandle ArrayType
        {
            get
            {
                if (_arrayType != null)
                {
                    return _arrayType;
                }

                _arrayType = GetTypeHandle(TypeManager.CoreTypes.Array);

                return _arrayType;
            }
        }

        public MemberCache MemberCache { get; }

        // IMemberContainer methods

        #region IMemberContainer Members
        public string Name { get; }

        public Type Type { get; }

        public MemberCache BaseCache { get; }

        public bool IsInterface { get; }

        public MemberInfo[] GetMembers(MemberTypes mt, BindingFlags bf)
        {
            MemberInfo[] members;

            if (Type is GenericTypeParameterBuilder)
                return new MemberInfo[0];

            //_type = TypeManager.DropGenericTypeArguments(_type);

            if (mt == MemberTypes.Event)
                members = Type.GetEvents(bf | BindingFlags.DeclaredOnly);
            else
                members = Type.FindMembers(
                    mt,
                    bf | BindingFlags.DeclaredOnly,
                    null,
                    null);

            if (members.Length == 0)
                return new MemberInfo[0];

            Array.Reverse(members);
            return members;
        }
        #endregion

        /// <summary>
        ///   Lookup a TypeHandle instance for the given type.  If the type doesn't have
        ///   a TypeHandle yet, a new instance of it is created.  This static method
        ///   ensures that we'll only have one TypeHandle instance per type.
        /// </summary>
        private static TypeHandle GetTypeHandle(Type t)
        {
            TypeHandle handle = (TypeHandle)_typeHash[t];
            if (handle != null)
            {
                return handle;
            }

            handle = new TypeHandle(t);
            _typeHash.Add(t, handle);
            return handle;
        }

        public static MemberCache GetMemberCache(Type t)
        {
            return GetTypeHandle(t).MemberCache;
        }

        public static void CleanUp()
        {
            _typeHash = null;
        }

        public static void Reset()
        {
            _typeHash = new PtrHashtable();
        }

        // IMemberFinder methods

        public MemberInfo[] FindMembers(
            MemberTypes mt,
            BindingFlags bf,
            string name,
            MemberFilter filter,
            object criteria)
        {
            return MemberCache.FindMembers(mt, bf, name, filter, criteria);
        }

        public override string ToString()
        {
            if (_baseContainer != null)
                return "TypeHandle (" + _id + "," + Name + " : " + _baseContainer + ")";
            return "TypeHandle (" + _id + "," + Name + ")";
        }
    }

    internal class PtrHashtable : Hashtable
    {
        public PtrHashtable() : base(PtrComparer.Instance) { }

        //
        // Workaround System.InvalidOperationException for enums
        //
        protected override int GetHash(object key)
        {
            TypeBuilder tb = key as TypeBuilder;
            if (tb != null && tb.BaseType == TypeManager.CoreTypes.Enum && tb.BaseType != null)
            {
                key = tb.BaseType;
            }

            return base.GetHash(key);
        }

        #region Nested type: PtrComparer
        private sealed class PtrComparer : IComparer, IEqualityComparer
        {
            public static readonly PtrComparer Instance = new PtrComparer();
            private PtrComparer() { }

            #region IComparer Members
            public int Compare(object x, object y)
            {
                return (x == y) ? 0 : 1;
            }
            #endregion

            #region IEqualityComparer Members
            bool IEqualityComparer.Equals(object x, object y)
            {
                return (x == y);
            }

            int IEqualityComparer.GetHashCode(object obj)
            {
                return obj.GetHashCode();
            }
            #endregion
        }
        #endregion
    }

    /// <summary>
    ///   This interface is used to get all members of a class when creating the
    ///   member cache.  It must be implemented by all DeclSpace derivatives which
    ///   want to support the member cache and by TypeHandle to get caching of
    ///   non-dynamic types.
    /// </summary>
    public interface IMemberContainer
    {
        /// <summary>The name of the IMemberContainer.  This is only used for
        ///   debugging purposes.</summary>
        string Name { get; }

        /// <summary>The type of this IMemberContainer.</summary>
        Type Type { get; }

        /// <summary>
        ///   Returns the IMemberContainer of the base class or null if this
        ///   is an interface or TypeManger.object_type.
        ///   This is used when creating the member cache for a class to get all
        ///   members from the base class.
        /// </summary>
        MemberCache BaseCache { get; }

        /// <summary>Whether this is an interface.</summary>
        bool IsInterface { get; }

        /// <summary>Returns all members of this class with the corresponding MemberTypes
        ///   and BindingFlags.</summary>
        /// <remarks>
        ///   When implementing this method, make sure not to return any inherited
        ///   members and check the MemberTypes and BindingFlags properly.
        ///   Unfortunately, System.Reflection is lame and doesn't provide a way to
        ///   get the BindingFlags (static/non-static,public/non-public) in the
        ///   MemberInfo class, but the cache needs this information.  That's why
        ///   this method is called multiple times with different BindingFlags.
        /// </remarks>
        MemberInfo[] GetMembers(MemberTypes mt, BindingFlags bf);
    }
}