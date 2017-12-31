using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Supremacy.Scripting.Utility
{
    /// 
    ///<summary>
    ///  The MemberCache is used by dynamic and non-dynamic types to speed up
    ///  member lookups.  It has a member name based hash table; it maps each member
    ///  name to a list of CacheEntry objects.  Each CacheEntry contains a MemberInfo
    ///  and the BindingFlags that were initially used to get it.  The cache contains
    ///  all members of the current class and all inherited members.  If this cache is
    ///  for an interface types, it also contains all inherited members.
    ///
    ///  There are two ways to get a MemberCache:
    ///  * if this is a dynamic type, lookup the corresponding DeclSpace and then
    ///  use the DeclSpace.MemberCache property.
    ///  * if this not a dynamic type, call TypeHandle.GetTypeHandle() to get a
    ///  TypeHandle instance for the type and then use TypeHandle.MemberCache.
    ///</summary>
    public class MemberCache
    {
        public readonly IMemberContainer Container;
        protected Hashtable MemberHash;
        protected Hashtable MethodHash;

        /// <summary>Create a new MemberCache for the given IMemberContainer `container'.</summary>
        public MemberCache(IMemberContainer container)
        {
            Container = container;

            // If we have a base class (we have a base class unless we're
            // TypeManager.object_type), we deep-copy its MemberCache here.
            if (Container.BaseCache != null)
                MemberHash = SetupCache(Container.BaseCache);
            else
                MemberHash = new Hashtable();

            // If this is neither a dynamic type nor an interface, create a special
            // method cache with all declared and inherited methods.
            var type = container.Type;
            if (!(type is TypeBuilder) && !type.IsInterface &&
                // !(type.IsGenericType && (type.GetGenericTypeDefinition () is TypeBuilder)) &&
                !TypeManager.IsGenericType(type) && !TypeManager.IsGenericParameter(type) &&
                (Container.BaseCache == null || Container.BaseCache.MethodHash != null))
            {
                MethodHash = new Hashtable();
                AddMethods(type);
            }

            // Add all members from the current class.
            AddMembers(Container);
        }

        public MemberCache(Type baseType, IMemberContainer container)
        {
            Container = container;
            if (baseType == null)
                MemberHash = new Hashtable();
            else
                MemberHash = SetupCache(TypeHandle.GetMemberCache(baseType));
        }

        public MemberCache(Type[] ifaces)
        {
            //
            // The members of this cache all belong to other caches.  
            // So, 'Container' will not be used.
            //
            Container = null;

            MemberHash = new Hashtable();
            if (ifaces == null)
                return;

            foreach (var itype in ifaces)
                AddCacheContents(TypeHandle.GetMemberCache(itype));
        }

        public MemberCache(IMemberContainer container, Type baseClass, Type[] ifaces)
        {
            Container = container;

            // If we have a base class (we have a base class unless we're
            // TypeManager.object_type), we deep-copy its MemberCache here.
            if (Container.BaseCache != null)
                MemberHash = SetupCache(Container.BaseCache);
            else
                MemberHash = new Hashtable();

            if (baseClass != null)
                AddCacheContents(TypeHandle.GetMemberCache(baseClass));
            if (ifaces != null)
            {
                foreach (var itype in ifaces)
                {
                    var cache = TypeHandle.GetMemberCache(itype);
                    if (cache != null)
                        AddCacheContents(cache);
                }
            }
        }

        /// <summary>Bootstrap this member cache by doing a deep-copy of our base.</summary>
        private static Hashtable SetupCache(MemberCache baseClass)
        {
            if (baseClass == null)
                return new Hashtable();

            var hash = new Hashtable(baseClass.MemberHash.Count);
            var it = baseClass.MemberHash.GetEnumerator();
            while (it.MoveNext())
            {
                hash.Add(it.Key, ((ArrayList)it.Value).Clone());
            }

            return hash;
        }

/*
        //
        // Converts ModFlags to BindingFlags
        //
        static BindingFlags GetBindingFlags(int modifiers)
        {
            BindingFlags bf;

            if ((modifiers & Modifiers.Static) != 0)
                bf = BindingFlags.Static;
            else
                bf = BindingFlags.Instance;

            if ((modifiers & Modifiers.Private) != 0)
                bf |= BindingFlags.NonPublic;
            else
                bf |= BindingFlags.Public;

            return bf;
        }
*/

        /// <summary>Add the contents of `cache' to the member_hash.</summary>
        private void AddCacheContents(MemberCache cache)
        {
            var it = cache.MemberHash.GetEnumerator();
            while (it.MoveNext())
            {
                var list = (ArrayList)MemberHash[it.Key];
                if (list == null)
                    MemberHash[it.Key] = list = new ArrayList();

                var entries = (ArrayList)it.Value;
                for (var i = entries.Count - 1; i >= 0; i--)
                {
                    var entry = (CacheEntry)entries[i];

                    if (entry.Container != cache.Container)
                        break;
                    list.Add(entry);
                }
            }
        }

        /// <summary>Add all members from class `container' to the cache.</summary>
        private void AddMembers(IMemberContainer container)
        {
            // We need to call AddMembers() with a single member type at a time
            // to get the member type part of CacheEntry.EntryType right.
            if (!container.IsInterface)
            {
                AddMembers(MemberTypes.Constructor, container);
                AddMembers(MemberTypes.Field, container);
            }
            AddMembers(MemberTypes.Method, container);
            AddMembers(MemberTypes.Property, container);
            AddMembers(MemberTypes.Event, container);
            // Nested types are returned by both Static and Instance searches.
            AddMembers(
                MemberTypes.NestedType,
                BindingFlags.Static | BindingFlags.Public,
                container);
            AddMembers(
                MemberTypes.NestedType,
                BindingFlags.Static | BindingFlags.NonPublic,
                container);
        }

        private void AddMembers(MemberTypes mt, IMemberContainer container)
        {
            AddMembers(mt, BindingFlags.Static | BindingFlags.Public, container);
            AddMembers(mt, BindingFlags.Static | BindingFlags.NonPublic, container);
            AddMembers(mt, BindingFlags.Instance | BindingFlags.Public, container);
            AddMembers(mt, BindingFlags.Instance | BindingFlags.NonPublic, container);
        }

        public void AddInterface(MemberCache baseCache)
        {
            if (baseCache.MemberHash.Count > 0)
                AddCacheContents(baseCache);
        }

        private void AddMember(
            MemberTypes mt,
            BindingFlags bf,
            IMemberContainer container,
            string name,
            MemberInfo member)
        {
            // We use a name-based hash table of ArrayList's.
            var list = (ArrayList)MemberHash[name];
            if (list == null)
            {
                list = new ArrayList(1);
                MemberHash.Add(name, list);
            }

            // When this method is called for the current class, the list will
            // already contain all inherited members from our base classes.
            // We cannot add new members in front of the list since this'd be an
            // expensive operation, that's why the list is sorted in reverse order
            // (ie. members from the current class are coming last).
            list.Add(new CacheEntry(container, member, mt, bf));
        }

        /// <summary>
        ///   Add all members from class `container' with the requested MemberTypes and
        ///   BindingFlags to the cache.  This method is called multiple times with different
        ///   MemberTypes and BindingFlags.
        /// </summary>
        private void AddMembers(MemberTypes mt, BindingFlags bf, IMemberContainer container)
        {
            var members = container.GetMembers(mt, bf);

            foreach (var member in members)
            {
                var name = member.Name;

                AddMember(mt, bf, container, name, member);

                if (member is MethodInfo)
                {
                    var gname = TypeManager.GetMethodName((MethodInfo)member);
                    if (gname != name)
                        AddMember(mt, bf, container, gname, member);
                }
            }
        }

        /// <summary>Add all declared and inherited methods from class `type' to the method cache.</summary>
        private void AddMethods(Type type)
        {
            AddMethods(
                BindingFlags.Static | BindingFlags.Public |
                BindingFlags.FlattenHierarchy,
                type);
            AddMethods(
                BindingFlags.Static | BindingFlags.NonPublic |
                BindingFlags.FlattenHierarchy,
                type);
            AddMethods(BindingFlags.Instance | BindingFlags.Public, type);
            AddMethods(BindingFlags.Instance | BindingFlags.NonPublic, type);
        }

        private static readonly ArrayList _overrides = new ArrayList();

        private void AddMethods(BindingFlags bf, Type type)
        {
            MethodBase[] members = type.GetMethods(bf);

            Array.Reverse(members);

            foreach (var member in members)
            {
                var name = member.Name;

                // We use a name-based hash table of ArrayList's.
                var list = (ArrayList)MethodHash[name];
                if (list == null)
                {
                    list = new ArrayList(1);
                    MethodHash.Add(name, list);
                }

                var curr = (MethodInfo)member;
                while (curr.IsVirtual && (curr.Attributes & MethodAttributes.NewSlot) == 0)
                {
                    var baseMethod = curr.GetBaseDefinition();

                    if (baseMethod == curr)
                        // Not every virtual function needs to have a NewSlot flag.
                        break;

                    _overrides.Add(curr);
                    list.Add(new CacheEntry(null, baseMethod, MemberTypes.Method, bf));
                    curr = baseMethod;
                }

                if (_overrides.Count > 0)
                {
                    foreach (var t in _overrides)
                        TypeManager.RegisterOverride((MethodBase)t, curr);
                    _overrides.Clear();
                }

                // Unfortunately, the elements returned by Type.GetMethods() aren't
                // sorted so we need to do this check for every member.
                var newBf = bf;
                if (member.DeclaringType == type)
                    newBf |= BindingFlags.DeclaredOnly;

                list.Add(new CacheEntry(Container, member, MemberTypes.Method, newBf));
            }
        }

        /// <summary>
        ///   Compute and return a appropriate `EntryType' magic number for the given
        ///   MemberTypes and BindingFlags.
        /// </summary>
        protected static EntryType GetEntryType(MemberTypes mt, BindingFlags bf)
        {
            var type = EntryType.None;

            if ((mt & MemberTypes.Constructor) != 0)
                type |= EntryType.Constructor;
            if ((mt & MemberTypes.Event) != 0)
                type |= EntryType.Event;
            if ((mt & MemberTypes.Field) != 0)
                type |= EntryType.Field;
            if ((mt & MemberTypes.Method) != 0)
                type |= EntryType.Method;
            if ((mt & MemberTypes.Property) != 0)
                type |= EntryType.Property;
            // Nested types are returned by static and instance searches.
            if ((mt & MemberTypes.NestedType) != 0)
                type |= EntryType.NestedType | EntryType.Static | EntryType.Instance;

            if ((bf & BindingFlags.Instance) != 0)
                type |= EntryType.Instance;
            if ((bf & BindingFlags.Static) != 0)
                type |= EntryType.Static;
            if ((bf & BindingFlags.Public) != 0)
                type |= EntryType.Public;
            if ((bf & BindingFlags.NonPublic) != 0)
                type |= EntryType.NonPublic;
            if ((bf & BindingFlags.DeclaredOnly) != 0)
                type |= EntryType.Declared;

            return type;
        }

        /// <summary>
        ///   The `MemberTypes' enumeration type is a [Flags] type which means that it may
        ///   denote multiple member types.  Returns true if the given flags value denotes a
        ///   single member types.
        /// </summary>
        public static bool IsSingleMemberType(MemberTypes mt)
        {
            switch (mt)
            {
                case MemberTypes.Constructor:
                case MemberTypes.Event:
                case MemberTypes.Field:
                case MemberTypes.Method:
                case MemberTypes.Property:
                case MemberTypes.NestedType:
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        ///   We encode the MemberTypes and BindingFlags of each members in a "magic"
        ///   number to speed up the searching process.
        /// </summary>
        [Flags]
        protected enum EntryType
        {
            None = 0x000,

            Instance = 0x001,
            Static = 0x002,
            MaskStatic = Instance | Static,

            Public = 0x004,
            NonPublic = 0x008,
            MaskProtection = Public | NonPublic,

            Declared = 0x010,

            Constructor = 0x020,
            Event = 0x040,
            Field = 0x080,
            Method = 0x100,
            Property = 0x200,
            NestedType = 0x400,

            NotExtensionMethod = 0x800,

            MaskType = Constructor | Event | Field | Method | Property | NestedType
        }

        protected class CacheEntry
        {
            public readonly IMemberContainer Container;
            public EntryType EntryType;
            public readonly MemberInfo Member;

            public CacheEntry(
                IMemberContainer container,
                MemberInfo member,
                MemberTypes mt,
                BindingFlags bf)
            {
                Container = container;
                Member = member;
                EntryType = GetEntryType(mt, bf);
            }

            public override string ToString()
            {
                return String.Format(
                    "CacheEntry ({0}:{1}:{2})",
                    Container.Name,
                    EntryType,
                    Member);
            }
        }

        /// <summary>
        ///   This is called each time we're walking up one level in the class hierarchy
        ///   and checks whether we can abort the search since we've already found what
        ///   we were looking for.
        /// </summary>
        protected bool DoneSearching(List<MemberInfo> list)
        {
            //
            // We've found exactly one member in the current class and it's not
            // a method or constructor.
            //
            if (list.Count == 1 && !(list[0] is MethodBase))
                return true;

            //
            // Multiple properties: we query those just to find out the indexer
            // name
            //
            if ((list.Count > 0) && (list[0] is PropertyInfo))
                return true;

            return false;
        }

        /// 
        ///<summary>
        ///  Looks up members with name `name'.  If you provide an optional
        ///  filter function, it'll only be called with members matching the
        ///  requested member name.
        ///
        ///  This method will try to use the cache to do the lookup if possible.
        ///
        ///  Unlike other FindMembers implementations, this method will always
        ///  check all inherited members - even when called on an interface type.
        ///
        ///  If you know that you're only looking for methods, you should use
        ///  MemberTypes.Method alone since this speeds up the lookup a bit.
        ///  When doing a method-only search, it'll try to use a special method
        ///  cache (unless it's a dynamic type or an interface) and the returned
        ///  MemberInfo's will have the correct ReflectedType for inherited methods.
        ///  The lookup process will automatically restart itself in method-only
        ///  search mode if it discovers that it's about to return methods.
        ///</summary>
        private readonly List<MemberInfo> _global = new List<MemberInfo>();

        private bool _usingGlobal;
        private static readonly MemberInfo[] _emptyMemberInfo = new MemberInfo[0];

        public MemberInfo[] FindMembers(
            MemberTypes mt,
            BindingFlags bf,
            string name,
            MemberFilter filter,
            object criteria)
        {
            if (_usingGlobal)
                throw new Exception();

            var declaredOnly = (bf & BindingFlags.DeclaredOnly) != 0;
            var methodSearch = mt == MemberTypes.Method;
            // If we have a method cache and we aren't already doing a method-only search,
            // then we restart a method search if the first match is a method.
            var doMethodSearch = !methodSearch && (MethodHash != null);

            ArrayList applicable;

            // If this is a method-only search, we try to use the method cache if
            // possible; a lookup in the method cache will return a MemberInfo with
            // the correct ReflectedType for inherited methods.

            if (methodSearch && (MethodHash != null))
                applicable = (ArrayList)MethodHash[name];
            else
                applicable = (ArrayList)MemberHash[name];

            if (applicable == null)
                return _emptyMemberInfo;

            //
            // 32  slots gives 53 rss/54 size
            // 2/4 slots gives 55 rss
            //
            // Strange: from 25,000 calls, only 1,800
            // are above 2.  Why does this impact it?
            //
            _global.Clear();
            _usingGlobal = true;

            var type = GetEntryType(mt, bf);

            var current = Container;

            var doInterfaceSearch = current.IsInterface;

            // `applicable' is a list of all members with the given member name `name'
            // in the current class and all its base classes.  The list is sorted in
            // reverse order due to the way how the cache is initialy created (to speed
            // things up, we're doing a deep-copy of our base).

            for (var i = applicable.Count - 1; i >= 0; i--)
            {
                var entry = (CacheEntry)applicable[i];

                // This happens each time we're walking one level up in the class
                // hierarchy.  If we're doing a DeclaredOnly search, we must abort
                // the first time this happens (this may already happen in the first
                // iteration of this loop if there are no members with the name we're
                // looking for in the current class).
                if (entry.Container != current)
                {
                    if (declaredOnly)
                        break;

                    if (!doInterfaceSearch && DoneSearching(_global))
                        break;

                    current = entry.Container;
                }

                // Is the member of the correct type ?
                if ((entry.EntryType & type & EntryType.MaskType) == 0)
                    continue;

                // Is the member static/non-static ?
                if ((entry.EntryType & type & EntryType.MaskStatic) == 0)
                    continue;

                // Apply the filter to it.
                if (filter(entry.Member, criteria))
                {
                    if ((entry.EntryType & EntryType.MaskType) != EntryType.Method)
                    {
                        doMethodSearch = false;
                    }

                    // Because interfaces support multiple inheritance we have to be sure that
                    // base member is from same interface, so only top level member will be returned
                    if (doInterfaceSearch && _global.Count > 0)
                    {
                        var memberAlreadyExists = _global
                            .Where(mi => !(mi is MethodBase))
                            .Any(
                            mi =>
                            IsInterfaceBaseInterface(
                                TypeManager.GetInterfaces(mi.DeclaringType), entry.Member.DeclaringType));

                        if (memberAlreadyExists)
                            continue;
                    }

                    _global.Add(entry.Member);
                }
            }

            // If we have a method cache and we aren't already doing a method-only
            // search, we restart in method-only search mode if the first match is
            // a method.  This ensures that we return a MemberInfo with the correct
            // ReflectedType for inherited methods.
            if (doMethodSearch && (_global.Count > 0))
            {
                _usingGlobal = false;
                return FindMembers(MemberTypes.Method, bf, name, filter, criteria);
            }

            _usingGlobal = false;
            return _global.ToArray();
        }

        /// <summary>Returns true if iterface exists in any base interfaces (ifaces)</summary>
        private static bool IsInterfaceBaseInterface(Type[] ifaces, Type ifaceToFind)
        {
            foreach (var iface in ifaces)
            {
                if (iface == ifaceToFind)
                    return true;

                var baseInterfaces = TypeManager.GetInterfaces(iface);
                if (baseInterfaces.Length > 0 && IsInterfaceBaseInterface(baseInterfaces, ifaceToFind))
                    return true;
            }
            return false;
        }

        // find the nested type @name in @this.
        public Type FindNestedType(string name)
        {
            var applicable = (ArrayList)MemberHash[name];
            if (applicable == null)
                return null;

            for (var i = applicable.Count - 1; i >= 0; i--)
            {
                var entry = (CacheEntry)applicable[i];
                if ((entry.EntryType & EntryType.NestedType & EntryType.MaskType) != 0)
                    return (Type)entry.Member;
            }

            return null;
        }

        public MemberInfo FindBaseEvent(Type invocationType, string name)
        {
            var applicable = (ArrayList)MemberHash[name];
            if (applicable == null)
                return null;

            //
            // Walk the chain of events, starting from the top.
            //
            for (var i = applicable.Count - 1; i >= 0; i--)
            {
                var entry = (CacheEntry)applicable[i];
                if ((entry.EntryType & EntryType.Event) == 0)
                    continue;

                var ei = (EventInfo)entry.Member;
                return ei.GetAddMethod(true);
            }

            return null;
        }

        private static readonly ArrayList EmptyArrayList = new ArrayList(0);
        //
        // Looks for extension methods with defined name and extension type
        //
        public ArrayList FindExtensionMethods(Assembly thisAssembly, Type extensionType, string name, bool publicOnly)
        {
            ArrayList entries;
            if (MethodHash != null)
                entries = (ArrayList)MethodHash[name];
            else
                entries = (ArrayList)MemberHash[name];

            if (entries == null)
                return EmptyArrayList;

            const EntryType entryType = EntryType.Static | EntryType.Method | EntryType.NotExtensionMethod;
            const EntryType foundEntryType = entryType & ~EntryType.NotExtensionMethod;

            ArrayList candidates = null;
            foreach (CacheEntry entry in entries)
            {
                if ((entry.EntryType & entryType) == foundEntryType)
                {
                    var mb = (MethodBase)entry.Member;

                    // Simple accessibility check
                    if ((entry.EntryType & EntryType.Public) == 0 && publicOnly)
                    {
                        var ma = mb.Attributes & MethodAttributes.MemberAccessMask;
                        if (ma != MethodAttributes.Assembly && ma != MethodAttributes.FamORAssem)
                            continue;

                        if (!TypeManager.IsThisOrFriendAssembly(thisAssembly, mb.DeclaringType.Assembly))
                            continue;
                    }

                    var pd = TypeManager.GetParameterData(mb);
                    var exType = pd.ExtensionMethodType;

                    if (exType == null)
                    {
                        entry.EntryType |= EntryType.NotExtensionMethod;
                        continue;
                    }

                    //if (implicit conversion between ex_type and extensionType exist) {
                    if (candidates == null)
                        candidates = new ArrayList(2);
                    candidates.Add(mb);
                    //}
                }
            }

            return candidates;
        }

/*
        //
        // This finds the method or property for us to override. invocation_type is the type where
        // the override is going to be declared, name is the name of the method/property, and
        // param_types is the parameters, if any to the method or property
        //
        // Because the MemberCache holds members from this class and all the base classes,
        // we can avoid tons of reflection stuff.
        //
        public MemberInfo FindMemberToOverride(Type invocationType, string name, ParametersCollection parameters, GenericMethod generic_method, bool is_property)
        {
            ArrayList applicable;
            if (method_hash != null && !is_property)
                applicable = (ArrayList)method_hash[name];
            else
                applicable = (ArrayList)member_hash[name];

            if (applicable == null)
                return null;
            //
            // Walk the chain of methods, starting from the top.
            //
            for (int i = applicable.Count - 1; i >= 0; i--)
            {
                CacheEntry entry = (CacheEntry)applicable[i];

                if ((entry.EntryType & (is_property ? (EntryType.Property | EntryType.Field) : EntryType.Method)) == 0)
                    continue;

                PropertyInfo pi = null;
                MethodInfo mi = null;
                FieldInfo fi = null;
                ParametersCollection cmp_attrs;

                if (is_property)
                {
                    if ((entry.EntryType & EntryType.Field) != 0)
                    {
                        fi = (FieldInfo)entry.Member;
                        cmp_attrs = ParametersCompiled.EmptyReadOnlyParameters;
                    }
                    else
                    {
                        pi = (PropertyInfo)entry.Member;
                        cmp_attrs = TypeManager.GetParameterData(pi);
                    }
                }
                else
                {
                    mi = (MethodInfo)entry.Member;
                    cmp_attrs = TypeManager.GetParameterData(mi);
                }

                if (fi != null)
                {
                    // TODO: Almost duplicate !
                    // Check visibility
                    switch (fi.Attributes & FieldAttributes.FieldAccessMask)
                    {
                        case FieldAttributes.PrivateScope:
                            continue;
                        case FieldAttributes.Private:
                            //
                            // A private method is Ok if we are a nested subtype.
                            // The spec actually is not very clear about this, see bug 52458.
                            //
                            if (!invocationType.Equals(entry.Container.Type) &&
                                !TypeManager.IsNestedChildOf(invocationType, entry.Container.Type))
                                continue;
                            break;
                        case FieldAttributes.FamANDAssem:
                        case FieldAttributes.Assembly:
                            //
                            // Check for assembly methods
                            //
                            if (fi.DeclaringType.Assembly != TypeManager.ThisAssembly)
                                continue;
                            break;
                    }
                    return entry.Member;
                }

                //
                // Check the arguments
                //
                if (cmp_attrs.Count != parameters.Count)
                    continue;

                int j;
                for (j = 0; j < cmp_attrs.Count; ++j)
                {
                    //
                    // LAMESPEC: No idea why `params' modifier is ignored
                    //
                    if ((parameters.FixedParameters[j].ModFlags & ~Parameter.Modifier.PARAMS) !=
                        (cmp_attrs.FixedParameters[j].ModFlags & ~Parameter.Modifier.PARAMS))
                        break;

                    if (!TypeManager.IsEqual(parameters.Types[j], cmp_attrs.Types[j]))
                        break;
                }

                if (j < cmp_attrs.Count)
                    continue;

                //
                // check generic arguments for methods
                //
                if (mi != null)
                {
                    Type[] cmpGenArgs = TypeManager.GetGenericArguments(mi);
                    if (generic_method == null && cmpGenArgs != null && cmpGenArgs.Length != 0)
                        continue;
                    if (generic_method != null && cmpGenArgs != null && cmpGenArgs.Length != generic_method.TypeParameters.Length)
                        continue;
                }

                //
                // get one of the methods because this has the visibility info.
                //
                if (is_property)
                {
                    mi = pi.GetGetMethod(true);
                    if (mi == null)
                        mi = pi.GetSetMethod(true);
                }

                //
                // Check visibility
                //
                switch (mi.Attributes & MethodAttributes.MemberAccessMask)
                {
                    case MethodAttributes.PrivateScope:
                        continue;
                    case MethodAttributes.Private:
                        //
                        // A private method is Ok if we are a nested subtype.
                        // The spec actually is not very clear about this, see bug 52458.
                        //
                        if (!invocationType.Equals(entry.Container.Type) &&
                            !TypeManager.IsNestedChildOf(invocationType, entry.Container.Type))
                            continue;
                        break;
                    case MethodAttributes.FamANDAssem:
                    case MethodAttributes.Assembly:
                        //
                        // Check for assembly methods
                        //
                        if (!TypeManager.IsThisOrFriendAssembly(invocationType.Assembly, mi.DeclaringType.Assembly))
                            continue;
                        break;
                }
                return entry.Member;
            }

            return null;
        }
*/

        /// <summary>
        ///   The method is looking for conflict with inherited symbols (errors CS0108, CS0109).
        ///   We handle two cases. The first is for types without parameters (events, field, properties).
        ///   The second are methods, indexers and this is why ignore_complex_types is here.
        ///   The latest param is temporary hack. See DoDefineMembers method for more info.
        /// </summary>
        public MemberInfo FindMemberWithSameName(string name, bool ignoreComplexTypes, MemberInfo ignoreMember)
        {
            ArrayList applicable = null;

            if (MethodHash != null)
                applicable = (ArrayList)MethodHash[name];

            if (applicable != null)
            {
                for (var i = applicable.Count - 1; i >= 0; i--)
                {
                    var entry = (CacheEntry)applicable[i];
                    if ((entry.EntryType & EntryType.Public) != 0)
                        return entry.Member;
                }
            }

            if (MemberHash == null)
                return null;
            applicable = (ArrayList)MemberHash[name];

            if (applicable != null)
            {
                for (var i = applicable.Count - 1; i >= 0; i--)
                {
                    var entry = (CacheEntry)applicable[i];
                    if ((entry.EntryType & EntryType.Public) != 0 & entry.Member != ignoreMember)
                    {
                        if (ignoreComplexTypes)
                        {
                            if ((entry.EntryType & EntryType.Method) != 0)
                                continue;

                            // Does exist easier way how to detect indexer ?
                            if ((entry.EntryType & EntryType.Property) != 0)
                            {
                                var argTypes = TypeManager.GetParameterData((PropertyInfo)entry.Member);
                                if (argTypes.Count > 0)
                                    continue;
                            }
                        }
                        return entry.Member;
                    }
                }
            }
            return null;
        }

        private Hashtable _lowercaseTable;

        /// <summary>Builds low-case table for CLS Compliance test</summary>
        public Hashtable GetPublicMembers()
        {
            if (_lowercaseTable != null)
                return _lowercaseTable;

            _lowercaseTable = new Hashtable();
            foreach (DictionaryEntry entry in MemberHash)
            {
                var members = (ArrayList)entry.Value;
                foreach (var t in members) {
                    var memberEntry = (CacheEntry)t;

                    if ((memberEntry.EntryType & EntryType.Public) == 0)
                        continue;

                    // TODO: Does anyone know easier way how to detect that member is internal ?
                    switch (memberEntry.EntryType & EntryType.MaskType)
                    {
                        case EntryType.Constructor:
                            continue;

                        case EntryType.Field:
                            if ((((FieldInfo)memberEntry.Member).Attributes &
                                 (FieldAttributes.Assembly | FieldAttributes.Public)) == FieldAttributes.Assembly)
                                continue;
                            break;

                        case EntryType.Method:
                            if ((((MethodInfo)memberEntry.Member).Attributes &
                                 (MethodAttributes.Assembly | MethodAttributes.Public)) == MethodAttributes.Assembly)
                                continue;
                            break;

                        case EntryType.Property:
                            var pi = (PropertyInfo)memberEntry.Member;
                            if (pi.GetSetMethod() == null && pi.GetGetMethod() == null)
                                continue;
                            break;

                        case EntryType.Event:
                            var ei = (EventInfo)memberEntry.Member;
                            var mi = ei.GetAddMethod();
                            if ((mi.Attributes & (MethodAttributes.Assembly | MethodAttributes.Public)) ==
                                MethodAttributes.Assembly)
                                continue;
                            break;
                    }
                    var lcase = ((string)entry.Key).ToLower(CultureInfo.InvariantCulture);
                    _lowercaseTable[lcase] = memberEntry.Member;
                    break;
                }
            }
            return _lowercaseTable;
        }

        public Hashtable Members
        {
            get { return MemberHash; }
        }

/*
        /// <summary>
        /// Cls compliance check whether methods or constructors parameters differing only in ref or out, or in array rank
        /// </summary>
        /// 
        // TODO: refactor as method is always 'this'
        public static void VerifyClsParameterConflict(ArrayList al, MethodCore method, MemberInfo this_builder, Report Report)
        {
            EntryType tested_type = (method is Constructor ? EntryType.Constructor : EntryType.Method) | EntryType.Public;

            for (int i = 0; i < al.Count; ++i)
            {
                MemberCache.CacheEntry entry = (MemberCache.CacheEntry)al[i];

                // skip itself
                if (entry.Member == this_builder)
                    continue;

                if ((entry.EntryType & tested_type) != tested_type)
                    continue;

                MethodBase method_to_compare = (MethodBase)entry.Member;
                AttributeTester.Result result = AttributeTester.AreOverloadedMethodParamsClsCompliant(
                    method.Parameters, TypeManager.GetParameterData(method_to_compare));

                if (result == AttributeTester.Result.Ok)
                    continue;

                IMethodData md = TypeManager.GetMethod(method_to_compare);

                // TODO: now we are ignoring CLSCompliance(false) on method from other assembly which is buggy.
                // However it is exactly what csc does.
                if (md != null && !md.IsClsComplianceRequired())
                    continue;

                Report.SymbolRelatedToPreviousError(entry.Member);
                switch (result)
                {
                    case AttributeTester.Result.RefOutArrayError:
                        Report.Warning(3006, 1, method.Location,
                                "Overloaded method `{0}' differing only in ref or out, or in array rank, is not CLS-compliant",
                                method.GetSignatureForError());
                        continue;
                    case AttributeTester.Result.ArrayArrayError:
                        Report.Warning(3007, 1, method.Location,
                                "Overloaded method `{0}' differing only by unnamed array types is not CLS-compliant",
                                method.GetSignatureForError());
                        continue;
                }

                throw new NotImplementedException(result.ToString());
            }
        }
*/

/*
        public bool CheckExistingMembersOverloads(MemberCore member, string name, ParametersCompiled parameters, Report Report)
        {
            ArrayList entries = (ArrayList)member_hash[name];
            if (entries == null)
                return true;

            int method_param_count = parameters.Count;
            for (int i = entries.Count - 1; i >= 0; --i)
            {
                CacheEntry ce = (CacheEntry)entries[i];

                if (ce.Container != member.Parent.PartialContainer)
                    return true;

                Type[] p_types;
                AParametersCollection pd;
                if ((ce.EntryType & EntryType.Property) != 0)
                {
                    pd = TypeManager.GetParameterData((PropertyInfo)ce.Member);
                    p_types = pd.Types;
                }
                else
                {
                    MethodBase mb = (MethodBase)ce.Member;

                    // TODO: This is more like a hack, because we are adding generic methods
                    // twice with and without arity name
                    if (TypeManager.IsGenericMethod(mb) && !member.MemberName.IsGeneric)
                        continue;

                    pd = TypeManager.GetParameterData(mb);
                    p_types = pd.Types;
                }

                if (p_types.Length != method_param_count)
                    continue;

                if (method_param_count > 0)
                {
                    int ii = method_param_count - 1;
                    Type type_a, type_b;
                    do
                    {
                        type_a = parameters.Types[ii];
                        type_b = p_types[ii];

                        if (TypeManager.IsGenericParameter(type_a) && type_a.DeclaringMethod != null)
                            type_a = typeof(TypeParameter);

                        if (TypeManager.IsGenericParameter(type_b) && type_b.DeclaringMethod != null)
                            type_b = typeof(TypeParameter);

                        if ((pd.FixedParameters[ii].ModFlags & Parameter.Modifier.ISBYREF) !=
                            (parameters.FixedParameters[ii].ModFlags & Parameter.Modifier.ISBYREF))
                            type_a = null;

                    } while (type_a == type_b && ii-- != 0);

                    if (ii >= 0)
                        continue;

                    //
                    // Operators can differ in return type only
                    //
                    if (member is Operator)
                    {
                        Operator op = TypeManager.GetMethod((MethodBase)ce.Member) as Operator;
                        if (op != null && op.ReturnType != ((Operator)member).ReturnType)
                            continue;
                    }

                    //
                    // Report difference in parameter modifiers only
                    //
                    if (pd != null && member is MethodCore)
                    {
                        ii = method_param_count;
                        while (ii-- != 0 && parameters.FixedParameters[ii].ModFlags == pd.FixedParameters[ii].ModFlags &&
                            parameters.ExtensionMethodType == pd.ExtensionMethodType) ;

                        if (ii >= 0)
                        {
                            MethodCore mc = TypeManager.GetMethod((MethodBase)ce.Member) as MethodCore;
                            Report.SymbolRelatedToPreviousError(ce.Member);
                            if ((member.ModFlags & Modifiers.Partial) != 0 && (mc.ModFlags & Modifiers.Partial) != 0)
                            {
                                if (parameters.HasParams || pd.HasParams)
                                {
                                    Report.Error(758, member.Location,
                                        "A partial method declaration and partial method implementation cannot differ on use of `params' modifier");
                                }
                                else
                                {
                                    Report.Error(755, member.Location,
                                        "A partial method declaration and partial method implementation must be both an extension method or neither");
                                }
                            }
                            else
                            {
                                if (member is Constructor)
                                {
                                    Report.Error(851, member.Location,
                                        "Overloaded contructor `{0}' cannot differ on use of parameter modifiers only",
                                        member.GetSignatureForError());
                                }
                                else
                                {
                                    Report.Error(663, member.Location,
                                        "Overloaded method `{0}' cannot differ on use of parameter modifiers only",
                                        member.GetSignatureForError());
                                }
                            }
                            return false;
                        }
                    }
                }

                if ((ce.EntryType & EntryType.Method) != 0)
                {
                    Method method_a = member as Method;
                    Method method_b = TypeManager.GetMethod((MethodBase)ce.Member) as Method;
                    if (method_a != null && method_b != null && (method_a.ModFlags & method_b.ModFlags & Modifiers.Partial) != 0)
                    {
                        const int partial_modifiers = Modifiers.Static | Modifiers.Unsafe;
                        if (method_a.IsPartialDefinition == method_b.IsPartialImplementation)
                        {
                            if ((method_a.ModFlags & partial_modifiers) == (method_b.ModFlags & partial_modifiers) ||
                                method_a.Parent.IsUnsafe && method_b.Parent.IsUnsafe)
                            {
                                if (method_a.IsPartialImplementation)
                                {
                                    method_a.SetPartialDefinition(method_b);
                                    entries.RemoveAt(i);
                                }
                                else
                                {
                                    method_b.SetPartialDefinition(method_a);
                                }
                                continue;
                            }

                            if ((method_a.ModFlags & Modifiers.Static) != (method_b.ModFlags & Modifiers.Static))
                            {
                                Report.SymbolRelatedToPreviousError(ce.Member);
                                Report.Error(763, member.Location,
                                    "A partial method declaration and partial method implementation must be both `static' or neither");
                            }

                            Report.SymbolRelatedToPreviousError(ce.Member);
                            Report.Error(764, member.Location,
                                "A partial method declaration and partial method implementation must be both `unsafe' or neither");
                            return false;
                        }

                        Report.SymbolRelatedToPreviousError(ce.Member);
                        if (method_a.IsPartialDefinition)
                        {
                            Report.Error(756, member.Location, "A partial method `{0}' declaration is already defined",
                                member.GetSignatureForError());
                        }

                        Report.Error(757, member.Location, "A partial method `{0}' implementation is already defined",
                            member.GetSignatureForError());
                        return false;
                    }

                    Report.SymbolRelatedToPreviousError(ce.Member);
                    IMethodData duplicate_member = TypeManager.GetMethod((MethodBase)ce.Member);
                    if (member is Operator && duplicate_member is Operator)
                    {
                        Report.Error(557, member.Location, "Duplicate user-defined conversion in type `{0}'",
                            member.Parent.GetSignatureForError());
                        return false;
                    }

                    bool is_reserved_a = member is AbstractPropertyEventMethod || member is Operator;
                    bool is_reserved_b = duplicate_member is AbstractPropertyEventMethod || duplicate_member is Operator;

                    if (is_reserved_a || is_reserved_b)
                    {
                        Report.Error(82, member.Location, "A member `{0}' is already reserved",
                            is_reserved_a ?
                            TypeManager.GetFullNameSignature(ce.Member) :
                            member.GetSignatureForError());
                        return false;
                    }
                }
                else
                {
                    Report.SymbolRelatedToPreviousError(ce.Member);
                }

                Report.Error(111, member.Location,
                    "A member `{0}' is already defined. Rename this member or use different parameter types",
                    member.GetSignatureForError());
                return false;
            }

            return true;
        }
*/
    }
}