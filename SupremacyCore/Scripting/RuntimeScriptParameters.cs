using System;

using Supremacy.Collections;

namespace Supremacy.Scripting
{
    [Serializable]
    public sealed class RuntimeScriptParameters : KeyedCollectionBase<ScriptParameter, RuntimeScriptParameter>
    {
        public RuntimeScriptParameters() : base(o => o.Parameter) { }
    }
}