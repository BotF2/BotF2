using System;
using System.Collections.Generic;

namespace Supremacy.Scripting.Ast
{
    public class CloneContext
    {
        private readonly Dictionary<Scope, Scope> _scopeMap = new Dictionary<Scope, Scope>();
        private Dictionary<Parameter, Parameter> _variableMap;

        public void AddVariableMap(Parameter from, Parameter to)
        {
            if (_variableMap == null)
            {
                _variableMap = new Dictionary<Parameter, Parameter>();
            }


            if (_variableMap.TryGetValue(from, out Parameter existingClone) && existingClone != to)
            {
                throw new ArgumentException("AddVariableMap: tried to map a variable that has already been mapped.", "from");
            }

            _variableMap[from] = to;
        }

        public void AddBlockMap(Scope from, Scope to)
        {

            if (_scopeMap.TryGetValue(from, out Scope existingClone) && existingClone != to)
            {
                throw new ArgumentException("AddBlockMap: tried to map a block that has already been mapped.", "from");
            }

            _scopeMap[from] = to;
        }

        public Scope LookupBlock(Scope from)
        {

            if (_scopeMap.TryGetValue(from, out Scope result))
            {
                return result;
            }

            result = from.Clone<Scope>(this);
            _scopeMap[from] = result;

            return result;
        }

        /// <summary>
        /// Remaps block to cloned copy if one exists.
        /// </summary>
        public Scope RemapBlockCopy(Scope from)
        {
            return _scopeMap.TryGetValue(from, out Scope clone) ? clone : from;
        }

        public Parameter LookupVariable(Parameter from)
        {

            if (!_variableMap.TryGetValue(from, out Parameter result))
            {
                throw new ArgumentException("LookupVariable: looking up a variable that has not been registered yet.");
            }

            return result;
        }
    }
}