using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

using Supremacy.Client.Themes;

namespace Supremacy.Client.Controls
{
    public abstract class GameControlBase : Control, IGameControl, ILogicalParent
    {
        #region Fields
        private readonly GameControlService.GameControlFlagManager _flags;
        #endregion

        #region Routed Events
        public static readonly RoutedEvent ClickEvent;
        public static readonly RoutedEvent PreviewClickEvent;
        #endregion

        #region Dependency Properties
        public static readonly DependencyProperty CommandProperty;
        public static readonly DependencyProperty CommandParameterProperty;
        public static readonly DependencyProperty CommandTargetProperty;
        public static readonly DependencyProperty HasImageProperty;
        public static readonly DependencyProperty HasLabelProperty;
        public static readonly DependencyProperty IdProperty;
        public static readonly DependencyProperty ImageSourceLargeProperty;
        public static readonly DependencyProperty ImageSourceSmallProperty;
        public static readonly DependencyProperty IsHighlightedProperty;
        public static readonly DependencyProperty LabelProperty;
        public static readonly DependencyProperty LabelTextTrimmingProperty;
        public static readonly DependencyProperty LabelTextWrappingProperty;
        public static readonly DependencyProperty VariantSizeProperty;
        public static readonly DependencyProperty ContextProperty;
        #endregion

        #region Events
        public event EventHandler<ExecuteRoutedEventArgs> Click
        {
            add { AddHandler(ClickEvent, value); }
            remove { RemoveHandler(ClickEvent, value); }
        }

        public event EventHandler<ExecuteRoutedEventArgs> PreviewClick
        {
            add { AddHandler(PreviewClickEvent, value); }
            remove { RemoveHandler(PreviewClickEvent, value); }
        }
        #endregion

        #region Constructors and Finalizers
        static GameControlBase()
        {
            ClickEvent = GameControlService.ClickEvent.AddOwner(typeof(GameControlBase));
            PreviewClickEvent = GameControlService.PreviewClickEvent.AddOwner(typeof(GameControlBase));

            CommandProperty = GameControlService.CommandProperty.AddOwner(typeof(GameControlBase));
            CommandParameterProperty = GameControlService.CommandParameterProperty.AddOwner(typeof(GameControlBase));
            CommandTargetProperty = GameControlService.CommandTargetProperty.AddOwner(typeof(GameControlBase));
            HasImageProperty = GameControlService.HasImageProperty.AddOwner(typeof(GameControlBase));
            HasLabelProperty = GameControlService.HasLabelProperty.AddOwner(typeof(GameControlBase));
            IdProperty = GameControlService.IdProperty.AddOwner(typeof(GameControlBase));
            ImageSourceLargeProperty = GameControlService.ImageSourceLargeProperty.AddOwner(typeof(GameControlBase));
            ImageSourceSmallProperty = GameControlService.ImageSourceSmallProperty.AddOwner(typeof(GameControlBase));
            IsHighlightedProperty = GameControlService.IsHighlightedProperty.AddOwner(typeof(GameControlBase));
            LabelProperty = GameControlService.LabelProperty.AddOwner(typeof(GameControlBase));
            LabelTextTrimmingProperty = ThemeProperties.TextTrimmingProperty.AddOwner(typeof(GameControlBase));
            LabelTextWrappingProperty = ThemeProperties.TextWrappingProperty.AddOwner(typeof(GameControlBase));

            VariantSizeProperty = GameControlService.VariantSizeProperty.AddOwner(
                typeof(GameControlBase),
                new FrameworkPropertyMetadata(
                    VariantSize.Medium,
                    OnVariantSizePropertyValueChanged));

            ContextProperty = GameControlService.ContextProperty.AddOwner(
                typeof(GameControlBase),
                new FrameworkPropertyMetadata(OnContextPropertyValueChanged));

            VisibilityProperty.OverrideMetadata(
                typeof(GameControlBase),
                new FrameworkPropertyMetadata(VisibilityProperty.DefaultMetadata.DefaultValue,
                    null,
                    CoerceVisibility));
        }

        private static object CoerceVisibility(DependencyObject d, object baseValue)
        {
            if (d is GameControlBase gameControl &&
                gameControl.VariantSize == VariantSize.Collapsed)
            {
                return Visibility.Collapsed;
            }
            return baseValue;
        }

        protected GameControlBase()
        {
            _flags = new GameControlService.GameControlFlagManager();
        }

        #endregion

        #region ILogicalParent Implementation
        void ILogicalParent.AddLogicalChild(object child)
        {
            AddLogicalChild(child);
        }

        void ILogicalParent.RemoveLogicalChild(object child)
        {
            RemoveLogicalChild(child);
        }
        #endregion

        #region IGameControl Implementation
        bool IGameControl.CanUpdateCanExecuteWhenHidden => CanUpdateCanExecuteWhenHidden;

        object IGameControl.CoerceCommandParameter(DependencyObject obj, object value)
        {
            return CoerceCommandParameter(obj, value);
        }

        EventHandler IGameControl.CommandCanExecuteHandler { get; set; }

        GameControlService.GameControlFlagManager IGameControl.Flags => _flags;

        bool IGameControl.HasImage
        {
            get => HasImage;
            set => HasImage = value;
        }

        bool IGameControl.HasLabel
        {
            get => HasLabel;
            set => HasLabel = value;
        }

        void IGameControl.OnCanExecuteChanged(object sender, EventArgs e)
        {
            UpdateCanExecute();
        }

        void IGameControl.OnCommandChanged(ICommand oldCommand, ICommand newCommand)
        {
            OnCommandChanged(oldCommand, newCommand);
        }

        void IGameControl.OnCommandUIProviderPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            GameControlService.UpdateUIFromCommand(this);
        }

        void IGameControl.UpdateCanExecute()
        {
            UpdateCanExecute();
        }

        void IGameControl.OnCommandParameterChanged(object oldValue, object newValue)
        {
            OnCommandParameterChanged(oldValue, newValue);
        }
        #endregion

        #region Properties
        protected virtual bool CanUpdateCanExecuteWhenHidden => false;

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public object CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        public IInputElement CommandTarget
        {
            get => (IInputElement)GetValue(CommandTargetProperty);
            set => SetValue(CommandTargetProperty, value);
        }

        public bool HasImage
        {
            get => (bool)GetValue(HasImageProperty);
            private set => SetValue(HasImageProperty, value);
        }

        public bool HasLabel
        {
            get => (bool)GetValue(HasLabelProperty);
            private set => SetValue(HasLabelProperty, value);
        }

        [Localizability(LocalizationCategory.NeverLocalize)]
        public string Id
        {
            get => (string)GetValue(IdProperty);
            set => SetValue(IdProperty, value);
        }

        public ImageSource ImageSourceLarge
        {
            get => (ImageSource)GetValue(ImageSourceLargeProperty);
            set => SetValue(ImageSourceLargeProperty, value);
        }

        public ImageSource ImageSourceSmall
        {
            get => (ImageSource)GetValue(ImageSourceSmallProperty);
            set => SetValue(ImageSourceSmallProperty, value);
        }

        public bool IsHighlighted
        {
            get => (bool)GetValue(IsHighlightedProperty);
            set => SetValue(IsHighlightedProperty, value);
        }

        [Localizability(LocalizationCategory.Label)]
        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public TextTrimming LabelTextTrimming
        {
            get => (TextTrimming)GetValue(LabelTextTrimmingProperty);
            set => SetValue(LabelTextTrimmingProperty, value);
        }

        public TextWrapping LabelTextWrapping
        {
            get => (TextWrapping)GetValue(LabelTextWrappingProperty);
            set => SetValue(LabelTextWrappingProperty, value);
        }

        public VariantSize VariantSize
        {
            get => (VariantSize)GetValue(VariantSizeProperty);
            set => SetValue(VariantSizeProperty, value);
        }

        public GameControlContext Context
        {
            get => (GameControlContext)GetValue(ContextProperty);
            set => SetValue(ContextProperty, value);
        }
        #endregion

        #region Methods
        private static void OnVariantSizePropertyValueChanged(
            DependencyObject obj,
            DependencyPropertyChangedEventArgs e)
        {
            GameControlBase control = (GameControlBase)obj;
            VariantSize oldVariantSize = (VariantSize)e.OldValue;
            VariantSize newVariantSize = (VariantSize)e.NewValue;

            control.OnVariantSizeChanged(oldVariantSize, newVariantSize);
            control.CoerceValue(VisibilityProperty);
        }

        private static void OnContextPropertyValueChanged(
            DependencyObject obj,
            DependencyPropertyChangedEventArgs e)
        {
            GameControlBase control = (GameControlBase)obj;
            GameControlContext oldContext = (GameControlContext)e.OldValue;
            GameControlContext newContext = (GameControlContext)e.NewValue;

            control.OnContextChanged(oldContext, newContext);
        }

        protected virtual object CoerceCommandParameter(DependencyObject obj, object value)
        {
            return value;
        }

        protected override void OnAccessKey(AccessKeyEventArgs e)
        {
            if (e.IsMultiple)
            {
                base.OnAccessKey(e);
            }
            else
            {
                RaiseClickEvent(new ExecuteRoutedEventArgs(ExecuteReason.Keyboard));
            }
        }

        protected virtual void OnClick(ExecuteRoutedEventArgs e)
        {
            e.RoutedEvent = ClickEvent;
            e.Source = this;

            RaiseEvent(e);

            GameCommand.ExecuteCommandSource(this);
        }

        protected virtual void OnCommandChanged(ICommand oldCommand, ICommand newCommand) { }

        protected virtual void OnCommandParameterChanged(object oldValue, object newValue)
        {
            if (ReferenceEquals(oldValue, newValue))
            {
                return;
            }

            if (oldValue is ICheckableCommandParameter oldCheckableCommandParameter)
            {
                oldCheckableCommandParameter.InnerParameterChanged -= OnInnerCommandParameterChanged;
            }

            if (newValue is ICheckableCommandParameter newCheckableCommandParameter)
            {
                newCheckableCommandParameter.InnerParameterChanged += OnInnerCommandParameterChanged;
            }

            UpdateCanExecute();
        }

        private void OnInnerCommandParameterChanged(object sender, EventArgs e)
        {
            UpdateCanExecute();
        }

        protected virtual void OnPreviewClick(ExecuteRoutedEventArgs e)
        {
            e.RoutedEvent = PreviewClickEvent;
            e.Source = this;
            RaiseEvent(e);
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == IsVisibleProperty)
            {
                GameControlService.HookCommands(this, Command, Command);
            }

            base.OnPropertyChanged(e);
        }

        protected virtual void OnVariantSizeChanged(VariantSize oldVariantSize, VariantSize newVariantSize) { }
        protected virtual void OnContextChanged(GameControlContext oldContext, GameControlContext newContext) { }

        public void RaiseClickEvent(ExecuteRoutedEventArgs e)
        {
            OnPreviewClick(e);

            _ = Dispatcher.BeginInvoke(
                DispatcherPriority.Render,
                (Action<ExecuteRoutedEventArgs>)OnClick,
                e);
        }

        public override string ToString()
        {
            return GetType().Name + "[Label=" + Label + "]";
        }

        protected virtual void UpdateCanExecute() { }
        #endregion
    }
}