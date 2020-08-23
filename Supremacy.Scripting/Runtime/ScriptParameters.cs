using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Supremacy.Annotations;

namespace Supremacy.Scripting
{
    [Serializable]
    public sealed class ScriptParameters : ReadOnlyCollection<ScriptParameter>
    {
        public ScriptParameters([NotNull] IEnumerable<ScriptParameter> parameters)
            : base(parameters.ToList()) { }

        public ScriptParameters([NotNull] params ScriptParameter[] parameters)
            : base(parameters) { }

        public ScriptParameters Merge([NotNull] IEnumerable<ScriptParameter> otherParameters)
        {
            if (otherParameters == null)
            {
                throw new ArgumentNullException("otherParameters");
            }

            return new ScriptParameters(this.Concat(otherParameters));
        }
        
        public ScriptParameters Merge([NotNull] params ScriptParameter[] otherParameters)
        {
            if (otherParameters == null)
            {
                throw new ArgumentNullException("otherParameters");
            }

            return new ScriptParameters(this.Concat(otherParameters));
        }
    }
}