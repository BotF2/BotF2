using System;
using System.Dynamic;

namespace Supremacy.Scripting.Runtime.Binders
{
    public class SxeGetMemberBinder :GetMemberBinder
    {
        private readonly BinderState _binder;
        private readonly bool _isNoThrow;

        public SxeGetMemberBinder(BinderState binder, string name, bool isNoThrow)
            : base(name, false)
        {
            if (binder == null)
                throw new ArgumentNullException("binder");

            _binder = binder;
            _isNoThrow = isNoThrow;
        }

        public bool IsNoThrow
        {
            get { return _isNoThrow; }
        }

        public BinderState Binder
        {
            get { return _binder; }
        }

        public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
        {
            throw new NotImplementedException();
        }
    }
}