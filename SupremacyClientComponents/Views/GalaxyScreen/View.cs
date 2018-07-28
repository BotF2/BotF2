using System;
using System.Windows;

namespace Supremacy.Client.Views
{
    public static class View
    {
        #region InteractionNode Attached Property

        public static readonly DependencyProperty InteractionNodeProperty = DependencyProperty.RegisterAttached(
            "InteractionNode",
            typeof(IInteractionNode),
            typeof(View));

        public static IInteractionNode GetInteractionNode(DependencyObject source)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            return (IInteractionNode)source.GetValue(InteractionNodeProperty);
        }

        public static void SetInteractionNode(DependencyObject source, IInteractionNode value)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            source.SetValue(InteractionNodeProperty, value);
        }

        #endregion
    }
}