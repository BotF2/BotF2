using System;
using System.Dynamic;
using System.Reflection;

using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using System.Linq;

namespace Supremacy.Scripting.Runtime.Binders
{
    public class InvokeMemberBinderKey
    {
        public InvokeMemberBinderKey(string name, CallInfo info)
        {
            Name = name;
            Info = info;
        }

        public string Name { get; }

        public CallInfo Info { get; }

        public override bool Equals(object obj)
        {
            return (obj is InvokeMemberBinderKey key) && (key.Name == Name) && Equals(key.Info, Info);
        }

        public override int GetHashCode()
        {
            return 0x28000000 ^ Name.GetHashCode() ^ Info.GetHashCode();
        }
    }

    public class SxeQueryMethodBinder : InvokeMemberBinder
    {
        private readonly ScriptBinder _binder;

        public SxeQueryMethodBinder(ScriptBinder binder, string name, bool ignoreCase, CallInfo callInfo) : base(name, ignoreCase, callInfo)
        {
            _binder = binder;
        }

        public override DynamicMetaObject FallbackInvokeMember(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion)
        {
            BindingRestrictions restrictions = target.Restrictions.Merge(
                BindingRestrictionsHelpers.GetRuntimeTypeRestriction(target));

            //target = target.Restrict(target.RuntimeType);
            //args = args.Select(o => o.Restrict(o.RuntimeType)).ToArray();

            MethodBase targetMethod;

            switch (Name)
            {
                case "SelectMany":
                    targetMethod = typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public).Where(
                            o => o.Name == Name).Skip(3).First().MakeGenericMethod(
                            typeof(object), typeof(object), typeof(object));
                    break;
                default:
                    targetMethod = typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public).Where(
                            o => o.Name == Name).First().MakeGenericMethod(
                            typeof(object), typeof(object));
                    break;
            }

            foreach (DynamicMetaObject o in args)
            {
                restrictions = restrictions.Merge(BindingRestrictionsHelpers.GetRuntimeTypeRestriction(o));
            }

            return _binder.CallMethod(
                new DefaultOverloadResolver(_binder, target, args.ToList(), new CallSignature(args.Length)),
                new[] { targetMethod },
                restrictions,
                Name);
        }

        public override DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion)
        {
            throw new NotImplementedException();
        }
    }

    public class SxeInvokeMemberBinder : InvokeMemberBinder
    {
        private readonly BinderState _binder;

        public SxeInvokeMemberBinder(BinderState binder, string name, CallInfo callInfo)
            : base(name, false, callInfo)
        {
            _binder = binder ?? throw new ArgumentNullException("binder");
        }

        public override DynamicMetaObject FallbackInvokeMember(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion)
        {
            _ = args[0].Value as Type[];
            _ = ArrayUtils.RemoveFirst(args);

            throw new NotImplementedException();
        }

        public override DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion)
        {
            return _binder.Invoke(args.Length).Bind(target, args);
        }
    }
}