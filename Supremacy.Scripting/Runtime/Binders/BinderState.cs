using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading;

using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Actions.Calls;
using System.Linq.Expressions;

namespace Supremacy.Scripting.Runtime.Binders
{
    public class BinderState
    {
        private readonly ScriptBinder _binder;

        private Dictionary<Type, SxeConvertBinder>[] _conversionBinders;
        private Dictionary<Type, DynamicMetaObjectBinder>[] _convertRetObjectBinders;
        private Dictionary<ExpressionType, SxeBinaryOperationBinder> _binaryBinders;
        private Dictionary<ExpressionType, SxeUnaryOperationBinder> _unaryBinders;
        private SxeGetIndexBinder[] _getIndexBinders;
        private SxeInvokeBinder[] _invokeBinders;
        private Dictionary<string, SxeGetMemberBinder> _tryGetMemberBinders;
        private Dictionary<string, SxeGetMemberBinder> _getMemberBinders;
        private Dictionary<InvokeMemberBinderKey, SxeInvokeMemberBinder> _invokeMemberBinders;

        internal BinderState(ScriptBinder binder)
        {
            if (binder == null)
                throw new ArgumentNullException("binder");
            _binder = binder;
        }

        public ScriptBinder Binder
        {
            get { return _binder; }
        }

        internal SxeConvertBinder Convert(Type type)
        {
            return Convert(type, ConversionResultKind.ImplicitTry, _binder.Context.OverloadResolver);
        }

        internal SxeConvertBinder Convert(Type type, ConversionResultKind resultKind)
        {
            return Convert(type, resultKind, _binder.Context.OverloadResolver);
        }

        internal SxeConvertBinder Convert(Type type, ConversionResultKind resultKind, OverloadResolverFactory resolverFactory)
        {
            if (_conversionBinders == null)
            {
                Interlocked.CompareExchange(
                    ref _conversionBinders,
                    new Dictionary<Type, SxeConvertBinder>[(int)ConversionResultKind.ExplicitTry + 1], // max conversion result kind
                    null
                );
            }

            if (_conversionBinders[(int)resultKind] == null)
            {
                Interlocked.CompareExchange(
                    ref _conversionBinders[(int)resultKind],
                    new Dictionary<Type, SxeConvertBinder>(),
                    null
                );
            }

            var binders = _conversionBinders[(int)resultKind];
            lock (binders)
            {
                SxeConvertBinder result;
                if (!binders.TryGetValue(type, out result))
                    binders[type] = result = new SxeConvertBinder(this, type, resultKind, resolverFactory);
                return result;
            }
        }

        internal DynamicMetaObjectBinder ConvertAndReturnObject(Type type, ConversionResultKind resultKind)
        {
            if (_convertRetObjectBinders == null)
            {
                Interlocked.CompareExchange(
                    ref _convertRetObjectBinders,
                    new Dictionary<Type, DynamicMetaObjectBinder>[(int)ConversionResultKind.ExplicitTry + 1], // max conversion result kind
                    null);
            }

            if (_convertRetObjectBinders[(int)resultKind] == null)
            {
                Interlocked.CompareExchange(
                    ref _convertRetObjectBinders[(int)resultKind],
                    new Dictionary<Type, DynamicMetaObjectBinder>(),
                    null);
            }

            var binder = _convertRetObjectBinders[(int)resultKind];
            lock (binder)
            {
                DynamicMetaObjectBinder result;
                if (!binder.TryGetValue(type, out result))
                    binder[type] = result = new SxeConvertBinder(this, type, resultKind, _binder.Context.OverloadResolver);

                return result;
            }
        }

        internal SxeBinaryOperationBinder BinaryOperation(ExpressionType operation)
        {
            if (_binaryBinders == null)
            {
                Interlocked.CompareExchange(
                    ref _binaryBinders,
                    new Dictionary<ExpressionType, SxeBinaryOperationBinder>(),
                    null);
            }

            lock (_binaryBinders)
            {
                SxeBinaryOperationBinder binder;
                if (!_binaryBinders.TryGetValue(operation, out binder))
                {
                    binder = new SxeBinaryOperationBinder(this, operation);
                    _binaryBinders[operation] = binder;
                }
                return binder;
            }
        }

        internal SxeUnaryOperationBinder UnaryOperation(ExpressionType operation)
        {
            if (_unaryBinders == null)
            {
                Interlocked.CompareExchange(
                    ref _unaryBinders,
                    new Dictionary<ExpressionType, SxeUnaryOperationBinder>(),
                    null);
            }

            lock (_unaryBinders)
            {
                SxeUnaryOperationBinder binder;
                if (!_unaryBinders.TryGetValue(operation, out binder))
                {
                    binder = new SxeUnaryOperationBinder(this, operation);
                    _unaryBinders[operation] = binder;
                }
                return binder;
            }
        }

        internal SxeInvokeBinder Invoke(int argCount)
        {
            if (_invokeBinders == null)
            {
                Interlocked.CompareExchange(
                    ref _invokeBinders,
                    new SxeInvokeBinder[argCount + 1],
                    null);
            }

            lock (this)
            {
                if (_invokeBinders.Length <= argCount)
                    Array.Resize(ref _invokeBinders, argCount + 1);

                if (_invokeBinders[argCount] == null)
                    _invokeBinders[argCount] = new SxeInvokeBinder(this, new CallInfo(argCount));

                return _invokeBinders[argCount];
            }
        }

        public SxeInvokeMemberBinder InvokeMember(string memberName, int argCount)
        {
            if (_invokeMemberBinders == null)
            {
                Interlocked.CompareExchange(
                    ref _invokeMemberBinders,
                    new Dictionary<InvokeMemberBinderKey, SxeInvokeMemberBinder>(),
                    null);
            }

            var key = new InvokeMemberBinderKey(memberName, new CallInfo(argCount));

            lock (_invokeMemberBinders)
            {
                SxeInvokeMemberBinder binder;
                if (_invokeMemberBinders.TryGetValue(key, out binder))
                    return binder;
                binder = new SxeInvokeMemberBinder(this, key.Name, key.Info);
                _invokeMemberBinders[key] = binder;
                return binder;
            }
        }

        internal SxeGetMemberBinder GetMember(string memberName)
        {
            return GetMember(memberName, false);
        }

        internal SxeGetMemberBinder GetMember(string memberName, bool isNoThrow)
        {
            Dictionary<string, SxeGetMemberBinder> dict;
            if (isNoThrow) {
                if (_tryGetMemberBinders == null) {
                    Interlocked.CompareExchange(
                        ref _tryGetMemberBinders,
                        new Dictionary<string, SxeGetMemberBinder>(),
                        null
                    );
                }

                dict = _tryGetMemberBinders;
            } else {
                if (_getMemberBinders == null) {
                    Interlocked.CompareExchange(
                        ref _getMemberBinders,
                        new Dictionary<string, SxeGetMemberBinder>(),
                        null
                    );
                }

                dict = _getMemberBinders;
            }

            lock (dict) {
                SxeGetMemberBinder res;
                if (!dict.TryGetValue(memberName, out res)) {
                    dict[memberName] = res = new SxeGetMemberBinder(this, memberName, isNoThrow);
                }

                return res;
            }
        }

        internal SxeGetIndexBinder GetIndex(int argCount)
        {
            if (_getIndexBinders == null)
            {
                Interlocked.CompareExchange(
                    ref _getIndexBinders, 
                    new SxeGetIndexBinder[argCount + 1],
                    null);
            }

            lock (this)
            {
                if (_getIndexBinders.Length <= argCount)
                    Array.Resize(ref _getIndexBinders, argCount + 1);

                if (_getIndexBinders[argCount] == null)
                    _getIndexBinders[argCount] = new SxeGetIndexBinder(this, new CallInfo(argCount));

                return _getIndexBinders[argCount];
            }
        }
    }
}