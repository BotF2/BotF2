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
        private readonly string _name;
        private readonly CallInfo _info;

        public InvokeMemberBinderKey(string name, CallInfo info)
        {
            _name = name;
            _info = info;
        }

        public string Name
        {
            get { return _name; }
        }

        public CallInfo Info
        {
            get { return _info; }
        }

        public override bool Equals(object obj)
        {
            var key = obj as InvokeMemberBinderKey;
            return (key != null) && (key._name == _name) && Equals(key._info, _info);
        }

        public override int GetHashCode()
        {
            return 0x28000000 ^ _name.GetHashCode() ^ _info.GetHashCode();
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
            var restrictions = target.Restrictions.Merge(
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

            foreach (var o in args)
                restrictions = restrictions.Merge(BindingRestrictionsHelpers.GetRuntimeTypeRestriction(o));

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
            if (binder == null)
                throw new ArgumentNullException("binder");
            _binder = binder;
        }

        public override DynamicMetaObject FallbackInvokeMember(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion)
        {
            var typeArguments = args[0].Value as Type[];
            args = ArrayUtils.RemoveFirst(args);

            throw new NotImplementedException();
        }

        public override DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion)
        {
            return _binder.Invoke(args.Length).Bind(target, args);
        }
    }
}