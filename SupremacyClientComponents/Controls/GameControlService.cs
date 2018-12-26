using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Markup;
using System.Windows.Navigation;

namespace Supremacy.Client.Controls
{

    /// <summary>
    /// Represents a service that provides properties and events for controls that are intended to be hosted in a <see cref="Game"/>.
    /// </summary>
    public static class GameControlService
    {
        private static readonly Dictionary<IGameCommandUIProvider, CommandUIProviderPropertyChangedRouter> _uiProviderPropertyChangedRouter = new Dictionary<IGameCommandUIProvider, CommandUIProviderPropertyChangedRouter>();

        #region Routed Events

        public static readonly RoutedEvent ActiveItemChangedEvent = EventManager.RegisterRoutedEvent(
            "ActiveItemChanged",
            RoutingStrategy.Bubble,
            typeof(EventHandler<ObjectPropertyChangedRoutedEventArgs>),
            typeof(GameControlService));

        public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent(
            "Click",
            RoutingStrategy.Bubble,
            typeof(EventHandler<ExecuteRoutedEventArgs>),
            typeof(GameControlService));

        public static readonly RoutedEvent ItemClickEvent = EventManager.RegisterRoutedEvent(
            "ItemClick",
            RoutingStrategy.Bubble,
            typeof(EventHandler<ObjectItemRoutedEventArgs>),
            typeof(GameControlService));

        public static readonly RoutedEvent PreviewClickEvent = EventManager.RegisterRoutedEvent(
            "PreviewClick",
            RoutingStrategy.Bubble,
            typeof(EventHandler<ExecuteRoutedEventArgs>),
            typeof(GameControlService));

        #endregion

        #region Dependency Properties
        public static readonly DependencyProperty CommandProperty = DependencyProperty.RegisterAttached(
            "Command",
            typeof(ICommand),
            typeof(GameControlService),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
                OnCommandPropertyValueChanged));

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.RegisterAttached(
                "CommandParameter",
                typeof(object),
                typeof(GameControlService),
                new FrameworkPropertyMetadata(
                    null,
                    OnCommandParameterPropertyValueChanged,
                    CoerceCommandParameterPropertyValue));

        public static readonly DependencyProperty CommandTargetProperty =
            DependencyProperty.RegisterAttached(
                "CommandTarget",
                typeof(IInputElement),
                typeof(GameControlService),
                new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty ForegroundDisabledProperty =
            DependencyProperty.RegisterAttached(
                "ForegroundDisabled",
                typeof(Brush),
                typeof(GameControlService),
                new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty HasImageProperty = DependencyProperty.RegisterAttached(
            "HasImage",
            typeof(bool),
            typeof(GameControlService),
            new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty HasLabelProperty = DependencyProperty.RegisterAttached(
            "HasLabel",
            typeof(bool),
            typeof(GameControlService),
            new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty IdProperty = DependencyProperty.RegisterAttached(
            "Id",
            typeof(string),
            typeof(GameControlService),
            new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty ImageSourceLargeProperty =
            DependencyProperty.RegisterAttached(
                "ImageSourceLarge",
                typeof(ImageSource),
                typeof(GameControlService),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.AffectsMeasure,
                    OnImageSourceLargePropertyValueChanged,
                    CoerceImageSourceLargeProperty));

        public static readonly DependencyProperty ImageSourceSmallProperty =
            DependencyProperty.RegisterAttached(
                "ImageSourceSmall",
                typeof(ImageSource),
                typeof(GameControlService),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.AffectsMeasure,
                    OnImageSourceSmallPropertyValueChanged,
                    CoerceImageSourceSmallProperty));

        public static readonly DependencyProperty IsExternalContentSupportedProperty =
            DependencyProperty.RegisterAttached(
                "IsExternalContentSupported",
                typeof(bool),
                typeof(GameControlService),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public static readonly DependencyProperty IsHighlightedProperty =
            DependencyProperty.RegisterAttached(
                "IsHighlighted",
                typeof(bool),
                typeof(GameControlService),
                new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty LabelProperty = DependencyProperty.RegisterAttached(
            "Label",
            typeof(string),
            typeof(GameControlService),
            new FrameworkPropertyMetadata(
                null, FrameworkPropertyMetadataOptions.AffectsMeasure, OnLabelPropertyValueChanged, CoerceLabelProperty));

        public static readonly DependencyProperty MenuItemDescriptionProperty =
            DependencyProperty.RegisterAttached(
                "MenuItemDescription", typeof(string), typeof(GameControlService), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty VariantSizeProperty =
            DependencyProperty.RegisterAttached(
                "VariantSize",
                typeof(VariantSize),
                typeof(GameControlService),
                new FrameworkPropertyMetadata(VariantSize.Medium));

        public static readonly DependencyProperty ContextProperty =
            DependencyProperty.RegisterAttached(
                "Context",
                typeof(GameControlContext),
                typeof(GameControlService),
                new FrameworkPropertyMetadata(GameControlContext.None, FrameworkPropertyMetadataOptions.Inherits));
        #endregion

        #region Flags Management
        [Flags]
        internal enum GameControlFlags
        {
            IsAttachedToCommandUIProvider = 0x100,
            IsAttachedToCommandCanExecuteChanged = 0x200,
        }

        internal class GameControlFlagManager
        {
            private GameControlFlags _flags;

            internal bool GetFlag(GameControlFlags flag)
            {
                return ((_flags & flag) == flag);
            }

            internal void SetFlag(GameControlFlags flag, bool set)
            {
                if (set)
                    _flags |= flag;
                else
                    _flags &= (~flag);
            }
        }

        #endregion

        #region CommandUIProviderPropertyChangedRouter

        private class CommandUIProviderPropertyChangedRouter
        {
            private readonly List<WeakReference> _controls = new List<WeakReference>();

            public void Add(IGameControl control)
            {
                var wasFound = false;
                for (var index = _controls.Count - 1; index >= 0; index--)
                {
                    var controlRef = _controls[index];
                    if (controlRef.IsAlive)
                    {
                        if (controlRef.Target == control)
                            wasFound = true;
                    }
                    else
                    {
                        _controls.RemoveAt(index);
                    }
                }

                if (!wasFound)
                    _controls.Add(new WeakReference(control));
            }

            public bool HasLiveReferences
            {
                get { return (_controls.Count > 0); }
            }

            public void OnCommandUIProviderPropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                for (int index = _controls.Count - 1; index >= 0; index--)
                {
                    var controlReference = _controls[index];
                    if (controlReference.IsAlive)
                    {
                        var control = controlReference.Target as IGameControl;
                        if (control != null)
                            control.OnCommandUIProviderPropertyChanged(sender, e);
                    }
                    else
                    {
                        _controls.RemoveAt(index);
                    }
                }
            }

            public void Remove(IGameControl control)
            {
                for (var index = _controls.Count - 1; index >= 0; index--)
                {
                    var controlReference = _controls[index];
                    if (controlReference.IsAlive)
                    {
                        if (controlReference.Target == control)
                            _controls.RemoveAt(index);
                    }
                    else
                    {
                        _controls.RemoveAt(index);
                    }
                }
            }
        }

        #endregion

        private static object CoerceCommandParameterPropertyValue(DependencyObject obj, object value)
        {
            var control = obj as IGameControl;
            if (control != null)
                return control.CoerceCommandParameter(obj, value);
            return value;
        }

        private static object CoerceImageSourceLargeProperty(DependencyObject obj, object value)
        {
            if (value == null)
            {
                var control = obj as IGameControl;
                if ((control != null) && (control.Command != null))
                {
                    var commandUIProvider = GameCommandUIManager.GetUIProviderResolved(control.Command);
                    if (commandUIProvider != null)
                        return commandUIProvider.ImageSourceLarge;
                }
            }
            return value;
        }

        private static object CoerceImageSourceSmallProperty(DependencyObject obj, object value)
        {
            if (value == null)
            {
                var control = obj as IGameControl;
                if ((control != null) && (control.Command != null))
                {
                    var commandUIProvider = GameCommandUIManager.GetUIProviderResolved(control.Command);
                    if (commandUIProvider != null)
                        return commandUIProvider.ImageSourceSmall;
                }
            }
            return value;
        }

        private static object CoerceLabelProperty(DependencyObject obj, object value)
        {
            if (value == null)
            {
                var control = obj as IGameControl;
                if ((control != null) && (control.Command != null))
                {
                    var commandUIProvider = GameCommandUIManager.GetUIProviderResolved(control.Command);
                    if (commandUIProvider != null)
                        return commandUIProvider.Label;
                }
            }
            return value;
        }

        internal static void OnCommandParameterPropertyValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var control = obj as IGameControl;
            var oldCommand = e.OldValue;
            var newCommand = e.NewValue;

            if (control == null)
                return;

            control.OnCommandParameterChanged(oldCommand, newCommand);
        }

        internal static void OnCommandPropertyValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var control = obj as IGameControl;
            var oldCommand = (ICommand)e.OldValue;
            var newCommand = (ICommand)e.NewValue;

            if (control == null)
                return;

            control.OnCommandChanged(oldCommand, newCommand);
            HookCommands(control, oldCommand, newCommand);
            obj.CoerceValue(CommandParameterProperty);
        }

        internal static void OnImageSourceLargePropertyValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue == e.NewValue)
                return;

            var control = obj as IGameControl;
            if (control != null)
                control.HasImage = (e.NewValue != null) || (control.ImageSourceSmall != null);
        }

        internal static void OnImageSourceSmallPropertyValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue == e.NewValue)
                return;

            var control = obj as IGameControl;
            if (control != null)
                control.HasImage = (e.NewValue != null) || (control.ImageSourceLarge != null);
        }

        internal static void OnLabelPropertyValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue == e.NewValue)
                return;

            var control = obj as IGameControl;
            if (control != null)
                control.HasLabel = !string.IsNullOrEmpty((string)e.NewValue);
        }

        internal static void HookCommands(IGameControl control, ICommand oldCommand, ICommand newCommand)
        {
            var wasAttached = control.Flags.GetFlag(GameControlFlags.IsAttachedToCommandCanExecuteChanged);
            if ((wasAttached) && (oldCommand != null))
            {
                control.Flags.SetFlag(GameControlFlags.IsAttachedToCommandCanExecuteChanged, false);
                if (control.CommandCanExecuteHandler != null)
                {
                    oldCommand.CanExecuteChanged -= control.CommandCanExecuteHandler;
                    control.CommandCanExecuteHandler = null;
                }
            }

            if (!control.Flags.GetFlag(GameControlFlags.IsAttachedToCommandCanExecuteChanged) && 
                (newCommand != null) &&
                (control.CanUpdateCanExecuteWhenHidden || control.IsVisible))
            {
                control.Flags.SetFlag(GameControlFlags.IsAttachedToCommandCanExecuteChanged, true);
                control.CommandCanExecuteHandler = new EventHandler(control.OnCanExecuteChanged);
                newCommand.CanExecuteChanged += control.CommandCanExecuteHandler;
            }

            // If no change was made, don't bother updating the CanExecute since this was called by an IsVisible change
            if (((oldCommand != newCommand) ||
                !wasAttached ||
                !control.Flags.GetFlag(GameControlFlags.IsAttachedToCommandCanExecuteChanged)))
            {
                control.UpdateCanExecute();
            }

            wasAttached = control.Flags.GetFlag(GameControlFlags.IsAttachedToCommandUIProvider);
            if ((wasAttached) && (oldCommand != null))
            {
                var commandUIProvider = GameCommandUIManager.GetUIProviderResolved(oldCommand);
                if (commandUIProvider != null)
                {
                    control.Flags.SetFlag(GameControlFlags.IsAttachedToCommandUIProvider, false);

                    CommandUIProviderPropertyChangedRouter router;
                    if (_uiProviderPropertyChangedRouter.TryGetValue(commandUIProvider, out router))
                    {
                        router.Remove(control);
                        if (!router.HasLiveReferences)
                        {
                            commandUIProvider.PropertyChanged -= router.OnCommandUIProviderPropertyChanged;
                            _uiProviderPropertyChangedRouter.Remove(commandUIProvider);
                        }
                    }
                }
            }

            if ((!control.Flags.GetFlag(GameControlFlags.IsAttachedToCommandUIProvider)) &&
                (newCommand != null) &&
                ((control.CanUpdateCanExecuteWhenHidden) || (control.IsVisible)))
            {

                var commandUIProvider = GameCommandUIManager.GetUIProviderResolved(newCommand);
                if (commandUIProvider != null)
                {
                    control.Flags.SetFlag(GameControlFlags.IsAttachedToCommandUIProvider, true);

                    CommandUIProviderPropertyChangedRouter router;
                    if (!_uiProviderPropertyChangedRouter.TryGetValue(commandUIProvider, out router))
                    {
                        router = new CommandUIProviderPropertyChangedRouter();
                        commandUIProvider.PropertyChanged += router.OnCommandUIProviderPropertyChanged;
                        _uiProviderPropertyChangedRouter.Add(commandUIProvider, router);
                    }

                    router.Add(control);
                }
            }

            // Update the UI based on the command
            UpdateUIFromCommand(control);
        }

        internal static void UpdateUIFromCommand(IGameControl control)
        {
            control.CoerceValue(ImageSourceLargeProperty);
            control.CoerceValue(ImageSourceSmallProperty);
            control.CoerceValue(LabelProperty);
        }

        private static FormattedText CreateFormattedTextForLabel(UIElement element)
        {
            return new FormattedText(GetLabel(element), CultureInfo.CurrentUICulture, FrameworkElement.GetFlowDirection(element),
                new Typeface(
                    TextElement.GetFontFamily(element),
                    TextElement.GetFontStyle(element),
                    TextElement.GetFontWeight(element),
                    TextElement.GetFontStretch(element)
                    ),
                TextElement.GetFontSize(element), TextElement.GetForeground(element),
                VisualTreeHelper.GetDpi(element).PixelsPerDip);

        }

        internal static void DrawExternalLabelImage(DrawingContext drawingContext, UIElement element, Rect bounds)
        {
            // Quit if this is not a control that has an external label or image
            if ((!GetIsExternalContentSupported(element)) || (!HasLabelOrImage(element)))
                return;

            // Get the variant size
            var variantSize = VariantSize.Medium;

            var gameControl = element as IGameControl;
            if (gameControl != null)
                variantSize = gameControl.VariantSize;

            var x = bounds.Left;

            switch (variantSize)
            {
                case VariantSize.Large:
                case VariantSize.Medium:
                {
                    var imageSource = GetImageSourceSmall(element) ?? GetImageSourceLarge(element);
                    if (imageSource != null)
                    {
                        UpdateBaseUri(element, imageSource);

                        if (!element.IsEnabled)
                            drawingContext.PushOpacity(0.4);

                        try
                        {
                            DrawImage(
                                drawingContext,
                                FrameworkElement.GetFlowDirection(element),
                                imageSource,
                                new Rect(x + 3, bounds.Top + ((bounds.Height - 16) / 2), 16, 16));
                        }
                        finally
                        {
                            if (!element.IsEnabled)
                                drawingContext.Pop();
                        }

                        x += 22; // 3 pixels padding on each side of image
                    }

                    // Draw the label
                    var label = GetLabel(element);
                    if (!string.IsNullOrEmpty(label))
                    {
                        var oldForeground = (object)TextElement.GetForeground(element);
                        var frameworkElement = element as FrameworkElement;
                        if ((!element.IsEnabled) && (frameworkElement != null))
                        {
                            if (element.GetBaseValueSource(TextElement.ForegroundProperty) != BaseValueSource.Local)
                                oldForeground = DependencyProperty.UnsetValue;

                            // Set the disabled brush
                            var foreground = (Brush)frameworkElement.GetValue(ForegroundDisabledProperty);
                            if (foreground != null)
                                TextElement.SetForeground(element, foreground);
                        }

                        try
                        {
                            var text = CreateFormattedTextForLabel(element);
                            DrawText(
                                drawingContext,
                                FrameworkElement.GetFlowDirection(element),
                                text,
                                new Point(x, bounds.Top + ((bounds.Height - text.Height) / 2)));
                        }
                        finally
                        {
                            // If disabled, restore the old foreground
                            if ((!element.IsEnabled) && (frameworkElement != null))
                            {
                                if (oldForeground == DependencyProperty.UnsetValue)
                                    element.ClearValue(TextElement.ForegroundProperty);
                                else
                                    TextElement.SetForeground(element, (Brush)oldForeground);
                            }

                        }
                    }
                    break;
                }
            }
        }

        internal static void DrawImage(DrawingContext drawingContext, FlowDirection flowDirection, ImageSource imageSource, Rect bounds)
        {
            // If in RTL, un-mirror the glyph shapes (since the drawing canvas is mirrored)
            var antiMirror = (flowDirection == FlowDirection.RightToLeft ? new ScaleTransform(-1, 1) : null);
            
            if (flowDirection == FlowDirection.RightToLeft)
                bounds.X = -bounds.X - bounds.Width;

            // Unmirror glyph shapes
            if (antiMirror != null)
                drawingContext.PushTransform(antiMirror);

            // Draw the text
            drawingContext.DrawImage(imageSource, bounds);

            // Pop the unmirroring transform
            if (antiMirror != null)
                drawingContext.Pop();
        }

        internal static void DrawText(DrawingContext drawingContext, FlowDirection flowDirection, FormattedText text, Point point)
        {
            // If in RTL, un-mirror the glyph shapes (since the drawing canvas is mirrored)
            var antiMirror = (flowDirection == FlowDirection.RightToLeft ? new ScaleTransform(-1, 1) : null);
            
            if (text.FlowDirection == FlowDirection.RightToLeft)
                point.X = -point.X;

            // Unmirror glyph shapes
            if (antiMirror != null)
                drawingContext.PushTransform(antiMirror);

            // Draw the text
            drawingContext.DrawText(text, point);

            // Pop the unmirroring transform
            if (antiMirror != null)
                drawingContext.Pop();
        }

        internal static Size GetExternalLabelImageDesiredSize(UIElement element)
        {
            // Quit if this is not a control that has an external label or image
            if (!GetIsExternalContentSupported(element) || !HasLabelOrImage(element))
                return new Size(0, 0);

            // Get the variant size
            var variantSize = VariantSize.Medium;
            
            var gameControl = element as IGameControl;
            if (gameControl != null)
                variantSize = gameControl.VariantSize;

            switch (variantSize)
            {
                case VariantSize.Large:
                case VariantSize.Medium:
                {
                    var size = new Size();

                    // Add space for the 16z16 image
                    var imageSource = GetImageSourceSmall(element) ?? GetImageSourceLarge(element);
                    if (imageSource != null)
                    {
                        UpdateBaseUri(element, imageSource);
                        size.Width += 22; // 3 pixels padding on each side of image
                        size.Height = Math.Max(size.Height, 16);
                    }

                    // Add space for the label
                    var label = GetLabel(element);
                    if (!string.IsNullOrEmpty(label))
                    {
                        var text = CreateFormattedTextForLabel(element);
                        size.Width += text.Width + 5; // 5 pixels padding on right side of label
                        size.Height = Math.Max(size.Height, text.Height);
                    }

                    return size;
                }

                default:
                {
                    return new Size(0, 0);
                }
            }
        }

        internal static bool HasLabelOrImage(DependencyObject obj)
        {
            return (GetLabel(obj) != null) ||
                   (GetImageSourceLarge(obj) != null) ||
                   (GetImageSourceSmall(obj) != null);
        }

        private static void UpdateBaseUri(DependencyObject obj, ImageSource source)
        {
            var uriContext = source as IUriContext;
            if ((uriContext != null) && 
                !source.IsFrozen && 
                (uriContext.BaseUri == null) && 
                (BaseUriHelper.GetBaseUri(obj) != null))
            {
                uriContext.BaseUri = BaseUriHelper.GetBaseUri(obj);
            }
        }

        internal static void OnAccessKeyPressed(object sender, AccessKeyPressedEventArgs e)
        {
            if (!e.Handled && (e.Scope == null) && (e.Target == null))
                e.Target = (UIElement)sender;
        }

        public static ICommand GetCommand(DependencyObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            return (ICommand)obj.GetValue(CommandProperty);
        }

        public static void SetCommand(DependencyObject obj, ICommand value)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            obj.SetValue(CommandProperty, value);
        }

        public static object GetCommandParameter(DependencyObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            return obj.GetValue(CommandParameterProperty);
        }
        
        public static void SetCommandParameter(DependencyObject obj, object value)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            obj.SetValue(CommandParameterProperty, value);
        }

        public static IInputElement GetCommandTarget(DependencyObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            return (IInputElement)obj.GetValue(CommandTargetProperty);
        }
        
        public static void SetCommandTarget(DependencyObject obj, IInputElement value)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            obj.SetValue(CommandTargetProperty, value);
        }

        public static bool GetHasImage(DependencyObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            return (bool)obj.GetValue(HasImageProperty);
        }

        public static void SetHasImage(DependencyObject obj, bool value)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            obj.SetValue(HasImageProperty, value);
        }

        public static bool GetHasLabel(DependencyObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            return (bool)obj.GetValue(HasLabelProperty);
        }
        
        public static void SetHasLabel(DependencyObject obj, bool value)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            obj.SetValue(HasLabelProperty, value);
        }

        public static string GetId(DependencyObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            return (string)obj.GetValue(IdProperty);
        }

        public static void SetId(DependencyObject obj, string value)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            obj.SetValue(IdProperty, value);
        }

        public static ImageSource GetImageSourceLarge(DependencyObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            return (ImageSource)obj.GetValue(ImageSourceLargeProperty);
        }
        
        public static void SetImageSourceLarge(DependencyObject obj, ImageSource value)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            obj.SetValue(ImageSourceLargeProperty, value);
        }

        public static ImageSource GetImageSourceSmall(DependencyObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            return (ImageSource)obj.GetValue(ImageSourceSmallProperty);
        }
        
        public static void SetImageSourceSmall(DependencyObject obj, ImageSource value)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            obj.SetValue(ImageSourceSmallProperty, value);
        }

        public static bool GetIsExternalContentSupported(DependencyObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            return (bool)obj.GetValue(IsExternalContentSupportedProperty);
        }
        
        public static void SetIsExternalContentSupported(DependencyObject obj, bool value)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            obj.SetValue(IsExternalContentSupportedProperty, value);
        }

        public static bool GetIsHighlighted(DependencyObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            return (bool)obj.GetValue(IsHighlightedProperty);
        }
        
        public static void SetIsHighlighted(DependencyObject obj, bool value)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            obj.SetValue(IsHighlightedProperty, value);
        }

        public static string GetLabel(DependencyObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            return (string)obj.GetValue(LabelProperty);
        }
        
        public static void SetLabel(DependencyObject obj, string value)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            obj.SetValue(LabelProperty, value);
        }

        public static string GetMenuItemDescription(DependencyObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            return (string)obj.GetValue(MenuItemDescriptionProperty);
        }
        
        public static void SetMenuItemDescription(DependencyObject obj, string value)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            obj.SetValue(MenuItemDescriptionProperty, value);
        }

        public static VariantSize GetVariantSize(DependencyObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            return (VariantSize)obj.GetValue(VariantSizeProperty);
        }
        
        public static void SetVariantSize(DependencyObject obj, VariantSize value)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            obj.SetValue(VariantSizeProperty, value);
        }

        public static GameControlContext GetContext(DependencyObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            return (GameControlContext)obj.GetValue(ContextProperty);
        }

        public static void SetContext(DependencyObject obj, GameControlContext value)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            obj.SetValue(ContextProperty, value);
        }
    }
}

