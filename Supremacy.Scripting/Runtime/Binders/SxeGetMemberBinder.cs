using System;
using System.Dynamic;

namespace Supremacy.Scripting.Runtime.Binders
{
    public class SxeGetMemberBinder :GetMemberBinder
    {
        public SxeGetMemberBinder(BinderState binder, string name, bool isNoThrow)
            : base(name, false)
        {
            Binder = binder ?? throw new ArgumentNullException("binder");
            IsNoThrow = isNoThrow;
        }

        public bool IsNoThrow { get; }

        public BinderState Binder { get; }

        public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
        {
            throw new NotImplementedException();
        }
    }
}