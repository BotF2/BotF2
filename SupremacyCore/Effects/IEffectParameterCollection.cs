using System;

using Supremacy.Collections;

namespace Supremacy.Effects
{
    public interface IEffectParameterCollection : IIndexedKeyedCollection<string, IEffectParameter> {}

    [Serializable]
    public sealed class EffectParameterCollection
        : KeyedCollectionBase<string, IEffectParameter>, IEffectParameterCollection
    {
        public EffectParameterCollection()
            : base(parameter => parameter.Name) {}
    }
}