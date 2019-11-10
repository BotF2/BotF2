using System;
using System.Collections.Generic;

using Supremacy.Collections;

namespace Supremacy.Effects
{
    public interface IEffectParameterBindingCollection
        : IIndexedKeyedCollection<string, IEffectParameterBinding> { }

    [Serializable]
    internal class EffectParameterBindingCollection
        : KeyedCollectionBase<string, IEffectParameterBinding>,
          IEffectParameterBindingCollection
    {
        public EffectParameterBindingCollection() : base(o => o.Parameter.Name) { }

        public EffectParameterBindingCollection(IEnumerable<IEffectParameterBinding> bindings)
            : this()
        {
            AddRange(bindings);
        }

        public EffectParameterBindingCollection(params IEffectParameterBinding[] bindings)
            : this((IEnumerable<IEffectParameterBinding>)bindings) { }
    }
}