using System;

using Supremacy.Collections;

namespace Supremacy.Effects
{
    public interface IEffectGroupBindingCollection : IIndexedCollection<EffectGroupBinding> { }

    [Serializable]
    public sealed class EffectGroupBindingCollection : CollectionBase<EffectGroupBinding>, IEffectGroupBindingCollection {}
}