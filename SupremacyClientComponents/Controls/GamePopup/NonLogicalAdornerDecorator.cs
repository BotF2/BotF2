using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Supremacy.Client.Controls
{
    internal class NonLogicalAdornerDecorator : AdornerDecorator
    {
        private UIElement _child;

        public override UIElement Child
        {
            get => _child;
            set
            {
                if (_child == value)
                {
                    return;
                }

                RemoveVisualChild(_child);
                RemoveVisualChild(AdornerLayer);

                _child = value;

                if (value != null)
                {
                    AddVisualChild(value);
                    AddVisualChild(AdornerLayer);
                }

                InvalidateMeasure();
            }
        }

        protected override Visual GetVisualChild(int index)
        {
            return _child;
        }

        protected override int VisualChildrenCount => _child == null ? 0 : 1;
    }
}