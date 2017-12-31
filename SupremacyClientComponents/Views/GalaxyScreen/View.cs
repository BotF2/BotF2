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

/*
        #region InteractionNode Attached Property

        public static readonly AttachableMemberIdentifier InteractionNodeProperty = new AttachableMemberIdentifier(
            typeof(View),
            "InteractionNode");

        public static IInteractionNode GetInteractionNode(object instance)
        {
            IInteractionNode value;

            if (AttachablePropertyServices.TryGetProperty(instance, InteractionNodeProperty, out value))
                return value;

            return default(IInteractionNode);
        }

        public static void SetInteractionNode(object instance, IInteractionNode value)
        {
            if (value == default(IInteractionNode))
                AttachablePropertyServices.RemoveProperty(instance, InteractionNodeProperty);
            else
                AttachablePropertyServices.SetProperty(instance, InteractionNodeProperty, value);
        }

        #endregion
*/
    }
}