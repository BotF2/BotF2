using System;
using System.Linq;

using Supremacy.Annotations;
using Supremacy.Scripting;

namespace Supremacy.Effects
{
    public static class EffectParameterBindingExtensions
    {
        public static RuntimeScriptParameter ToRuntimeScriptParameter(
            [NotNull] this IEffectParameterBinding effectParameter,
            [NotNull] ScriptParameters scriptParameters)
        {
            if (effectParameter == null)
                throw new ArgumentNullException("effectParameter");
            if (scriptParameters == null)
                throw new ArgumentNullException("scriptParameters");

            string parameterName = effectParameter.Parameter.Name;

            ScriptParameter scriptParameter = scriptParameters.FirstOrDefault(o => o.Name == parameterName);
            if (scriptParameter == null)
            {
                throw new ArgumentException(
                    string.Format(
                        "Could not bind effect parameter '{0}'.",
                        parameterName));
            }

            return new RuntimeScriptParameter(scriptParameter, effectParameter.Value);
        }

        public static RuntimeScriptParameters ToRuntimeScriptParameters(
            [NotNull] this IEffectParameterBindingCollection effectParameters,
            [NotNull] ScriptParameters scriptParameters)
        {
            if (effectParameters == null)
                throw new ArgumentNullException("effectParameters");
            if (scriptParameters == null)
                throw new ArgumentNullException("scriptParameters");

            RuntimeScriptParameters runtimeParameters = new RuntimeScriptParameters();
            
            runtimeParameters.AddRange(effectParameters.Select(o => o.ToRuntimeScriptParameter(scriptParameters)));

            return runtimeParameters;
        }
    }
}