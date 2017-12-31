using System;

using Supremacy.Collections;

namespace Supremacy.Effects
{
    public interface IEffectBindingCollection : IIndexedCollection<EffectBinding> { }

    [Serializable]
    public sealed class EffectBindingCollection : CollectionBase<EffectBinding>, IEffectBindingCollection {}
}