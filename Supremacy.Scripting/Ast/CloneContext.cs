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
                _variableMap = new Dictionary<Parameter, Parameter>();

            Parameter existingClone;

            if (_variableMap.TryGetValue(from, out existingClone) && existingClone != to)
                throw new ArgumentException("AddVariableMap: tried to map a variable that has already been mapped.", "from");

            _variableMap[from] = to;
        }

        public void AddBlockMap(Scope from, Scope to)
        {
            Scope existingClone;

            if (_scopeMap.TryGetValue(from, out existingClone) && existingClone != to)
                throw new ArgumentException("AddBlockMap: tried to map a block that has already been mapped.", "from");

            _scopeMap[from] = to;
        }

        public Scope LookupBlock(Scope from)
        {
            Scope result;

            if (_scopeMap.TryGetValue(from, out result))
                return result;
            
            result = from.Clone<Scope>(this);
            _scopeMap[from] = result;
            
            return result;
        }

        /// <summary>
        /// Remaps block to cloned copy if one exists.
        /// </summary>
        public Scope RemapBlockCopy(Scope from)
        {
            Scope clone;
            return _scopeMap.TryGetValue(from, out clone) ? clone : from;
        }

        public Parameter LookupVariable(Parameter from)
        {
            Parameter result;
            
            if (!_variableMap.TryGetValue(from, out result))
                throw new ArgumentException("LookupVariable: looking up a variable that has not been registered yet.");

            return result;
        }
    }
}