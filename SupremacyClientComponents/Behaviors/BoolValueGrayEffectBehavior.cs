using System.Windows;
using System.Windows.Interactivity;
using System.Windows.Media;

using ShaderEffectLibrary;

namespace Supremacy.Client.Behaviors
{
    public class BoolValueGrayEffectBehavior : Behavior<UIElement>
    {
        private MonochromeEffect _monochromeEffect;

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value",
            typeof(bool),
            typeof(BoolValueGrayEffectBehavior),
            new PropertyMetadata((d, e) => ((BoolValueGrayEffectBehavior)d).UpdateEffect()));

        public bool Value
        {
            get { return (bool)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        private void UpdateEffect()
        {
            if (Value)
            {
                AssociatedObject.ClearValue(UIElement.EffectProperty);
                return;
            }

            if (_monochromeEffect == null)
                _monochromeEffect = new MonochromeEffect { FilterColor = Colors.White };

            AssociatedObject.Effect = _monochromeEffect;
        }

        protected override void OnAttached()
        {
            UpdateEffect();
        }

        protected override void OnDetaching()
        {
            if (AssociatedObject.Effect == _monochromeEffect)
                AssociatedObject.ClearValue(UIElement.EffectProperty);
        }

        protected override Freezable CreateInstanceCore()
        {
            return new BoolValueGrayEffectBehavior();
        }
    }
}