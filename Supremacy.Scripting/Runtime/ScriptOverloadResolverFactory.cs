using System;
using System.Collections.Generic;
using System.Dynamic;

using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Runtime;

namespace Supremacy.Scripting.Runtime
{
    public class ScriptOverloadResolverFactory : OverloadResolverFactory
    {
        private readonly ScriptLanguageContext _context;

        public ScriptOverloadResolverFactory(ScriptLanguageContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            _context = context;
        }

        public override DefaultOverloadResolver CreateOverloadResolver(IList<DynamicMetaObject> args, CallSignature signature, CallTypes callType)
        {
            return new DefaultOverloadResolver(
                _context.DefaultBinderState.Binder,
                args,
                signature,
                callType);
        }
    }
}