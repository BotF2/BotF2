namespace Supremacy.Effects
{
    public interface IEffectTarget
    {
        IEffectBindingCollection EffectBindings { get; }
    }

    internal interface IEffectTargetInternal
    {
        EffectBindingCollection EffectBindingsInternal { get; }
    }
}