using System.Collections.Generic;

using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

namespace Supremacy.Scripting.Ast
{
    public class ExplicitScope : Scope
    {
        private Dictionary<string, IKnownVariable> _knownVariables;

        public ExplicitScope(CompilerContext compilerContext, Scope parent, SourceLocation start, SourceLocation end)
            : base(compilerContext, parent, start, end)
		{
			Explicit = this;
		}

        internal void AddKnownVariable(string name, IKnownVariable variable)
        {
            if (_knownVariables == null)
            {
                _knownVariables = new Dictionary<string, IKnownVariable>();
            }

            if (!_knownVariables.ContainsKey(name))
            {
                _knownVariables[name] = variable;
            }

            if (Parent != null)
            {
                Parent.Explicit.AddKnownVariable(name, variable);
            }
        }

        internal IKnownVariable GetKnownVariable(string name)
        {
            if (_knownVariables == null)
            {
                return null;
            }

            return _knownVariables.TryGetValue(name, out IKnownVariable variable) ? variable : null;
        }
    }
}