using FMOD;
using Supremacy.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interactivity;

namespace Supremacy.Client.Behaviors
{
    public sealed class VisibilityGroupScopeBehavior : Behavior<FrameworkElement>
    {
        private readonly List<UIElement> _scopedElements = new List<UIElement>();

        #region VisibilityGroup Attached Property

        public static readonly DependencyProperty VisibilityGroupProperty =
            DependencyProperty.Register(
                "VisibilityGroup",
                typeof(string),
                typeof(VisibilityGroupScopeBehavior),
                new PropertyMetadata(
                    null,
                    OnVisibilityGroupChanged));

        public static void SetVisibilityGroup(UIElement target, string value)
        {
            target.SetValue(VisibilityGroupProperty, value);
        }

        public static string GetVisibilityGroup(UIElement target)
        {
            return (string)target.GetValue(VisibilityGroupProperty);
        }

        private static void OnVisibilityGroupChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            _text = "Step_0701: OnVisibilityGroupChanged-Arg e = " + e.NewValue.ToString();
            Console.WriteLine(_text);
            GameLog.Core.UIDetails.DebugFormat(_text);

            if (d is VisibilityGroupScopeBehavior scopeBehavior)
            {
                scopeBehavior.ClearVisibilityBindings();
                scopeBehavior.AddVisibilityBindings();
                return;
            }

            if (!(d is UIElement element))
            {
                return;
            }

            VisibilityGroupScopeBehavior oldScope = GetVisibilityGroupScope(element);
            string newGroup = e.NewValue as string;

            if (oldScope != null && oldScope.VisibilityGroup != newGroup)
            {
                oldScope.ClearVisibilityBindings(element);
            }

            if (newGroup == null)
            {
                return;
            }

            FrameworkElement newScopeElement = element.FindVisualAncestorsByType<FrameworkElement>(
                includeStartElement: true,
                predicate: o => GetHasVisibilityGroupScope(o) &&
                                Interaction.GetBehaviors(o).OfType<VisibilityGroupScopeBehavior>().Any(b => b.VisibilityGroup == newGroup))
                .FirstOrDefault();

            if (newScopeElement == null || !newScopeElement.IsLoaded)
            {
                return;
            }

            VisibilityGroupScopeBehavior newScope = Interaction
                .GetBehaviors(newScopeElement)
                .OfType<VisibilityGroupScopeBehavior>()
                .First(b => b.VisibilityGroup == newGroup);

            newScope.AddVisibilityBindings(element);
        }

        #endregion

        #region HasVisibilityGroupScope Attached Property

        private static readonly DependencyPropertyKey HasVisibilityGroupScopePropertyKey = DependencyProperty.RegisterReadOnly(
            "HasVisibilityGroupScope",
            typeof(bool),
            typeof(VisibilityGroupScopeBehavior),
            new PropertyMetadata(false));

        private static void SetHasVisibilityGroupScope(UIElement target, bool value)
        {
            target.SetValue(HasVisibilityGroupScopePropertyKey, value);
        }

        private static bool GetHasVisibilityGroupScope(UIElement target)
        {
            return (bool)target.GetValue(HasVisibilityGroupScopePropertyKey.DependencyProperty);
        }

        private static void ClearHasVisibilityGroupScope(UIElement target)
        {
            target.ClearValue(HasVisibilityGroupScopePropertyKey.DependencyProperty);
        }

        #endregion

        #region VisibilityGroupScope Attached Property

        private static readonly DependencyPropertyKey VisibilityGroupScopePropertyKey = DependencyProperty.RegisterReadOnly(
            "VisibilityGroupScope",
            typeof(VisibilityGroupScopeBehavior),
            typeof(VisibilityGroupScopeBehavior),
            new PropertyMetadata());

        private static void SetVisibilityGroupScope(UIElement target, VisibilityGroupScopeBehavior value)
        {
            target.SetValue(VisibilityGroupScopePropertyKey, value);
        }

        private static VisibilityGroupScopeBehavior GetVisibilityGroupScope(UIElement target)
        {

            return (VisibilityGroupScopeBehavior)target.GetValue(VisibilityGroupScopePropertyKey.DependencyProperty);
        }

        private static void ClearVisibilityGroupScope(UIElement target)
        {
            target.ClearValue(VisibilityGroupScopePropertyKey);
        }

        #endregion

        #region Visibility Property

        public static readonly DependencyProperty VisibilityProperty = UIElement.VisibilityProperty.AddOwner(typeof(VisibilityGroupScopeBehavior));
        private static string _text;

        public Visibility Visibility
        {
            get => (Visibility)GetValue(VisibilityProperty);
            set => SetValue(VisibilityProperty, value);
        }

        #endregion

        public string VisibilityGroup
        {
            get => (string)GetValue(VisibilityGroupProperty);
            set => SetValue(VisibilityGroupProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            SetHasVisibilityGroupScope(AssociatedObject, true);

            AssociatedObject.Loaded += OnAssociatedObjectLoaded;
            AssociatedObject.Unloaded += OnAssociatedObjectUnloaded;

            if (AssociatedObject.IsLoaded)
            {
                AddVisibilityBindings();
            }
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Loaded -= OnAssociatedObjectLoaded;
            AssociatedObject.Unloaded -= OnAssociatedObjectUnloaded;

            ClearVisibilityBindings();
            ClearHasVisibilityGroupScope(AssociatedObject);

            base.OnDetaching();
        }

        private void OnAssociatedObjectLoaded(object sender, RoutedEventArgs e)
        {
            AddVisibilityBindings();
        }

        private void OnAssociatedObjectUnloaded(object sender, RoutedEventArgs e)
        {
            ClearVisibilityBindings();
        }

        private void ClearVisibilityBindings()
        {
            UIElement[] scopedElements = _scopedElements.ToArray();

            for (int i = scopedElements.Length - 1; i >= 0; i--)
            {
                UIElement scopedElement = scopedElements[i];
                ClearVisibilityBindings(scopedElement);
            }
        }

        private void ClearVisibilityBindings(UIElement target)
        {
            if (GetVisibilityGroupScope(target) != this)
            {
                return;
            }

            Binding binding = BindingOperations.GetBinding(target, UIElement.VisibilityProperty);
            if (binding != null && binding.Source == this)
            {
                BindingOperations.ClearBinding(target, UIElement.VisibilityProperty);
            }

            ClearVisibilityGroupScope(target);

            _ = _scopedElements.Remove(target);
        }

        private void AddVisibilityBindings()
        {
            if (AssociatedObject == null || VisibilityGroup == null)
            {
                return;
            }

            Debug.Assert(_scopedElements.Count == 0, "Illegal state for VisibilityGroupScope: _scopedElements.Count != 0");

            IEnumerable<UIElement> descendants = AssociatedObject.FindVisualDescendantsByType<UIElement>(
                includeStartElement: false,
                predicate: o => GetVisibilityGroup(o) == VisibilityGroup);

            foreach (UIElement descendant in descendants)
            {
                AddVisibilityBindings(descendant);
            }
        }

        private void AddVisibilityBindings(UIElement target)
        {
            _ = BindingOperations.SetBinding(
                target,
                UIElement.VisibilityProperty,
                new Binding
                {
                    Source = this,
                    Path = new PropertyPath(VisibilityProperty),
                    Mode = BindingMode.OneWay
                });

            SetVisibilityGroupScope(target, this);

            _scopedElements.Add(target);
        }
    }
}