using System;
using System.Linq;

using Supremacy.Annotations;
using Supremacy.Scripting;

namespace Supremacy.Effects
{
    public static class EffectParameterExtensions
    {
        public static ScriptParameter ToScriptParameter([NotNull] this IEffectParameter effectParameter)
        {
            if (effectParameter == null)
                throw new ArgumentNullException("effectParameter");

            return new ScriptParameter(
                effectParameter.Name,
                effectParameter.ParameterType,
                effectParameter.IsRequired,
                effectParameter.DefaultValue);
        }

        public static ScriptParameters ToScriptParameters([NotNull] this IEffectParameterCollection effectParameters)
        {
            if (effectParameters == null)
                throw new ArgumentNullException("effectParameters");

            return new ScriptParameters(effectParameters.Select(ToScriptParameter));
        }
    }
}