using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace Supremacy.Client.Controls
{
    public class GameButton : GameButtonBase
    {
        #region Fields

        private object _defaultCommandParameter;

        #endregion

        #region Template Keys

        #region NormalTemplateKey Resource Key

        private static ComponentResourceKey _normalTemplateKey;

        public static ResourceKey NormalTemplateKey
        {
            get
            {
                if (_normalTemplateKey == null)
                {
                    _normalTemplateKey = new ComponentResourceKey(
                        typeof(GameButton),
                        "NormalTemplateKey");
                }
                return _normalTemplateKey;
            }
        }

        #endregion

        #region MinimalTemplateKey Resource Key

        private static ComponentResourceKey _minimalTemplateKey;

        public static ResourceKey MinimalTemplateKey
        {
            get
            {
                if (_minimalTemplateKey == null)
                {
                    _minimalTemplateKey = new ComponentResourceKey(
                        typeof(GameButton),
                        "MinimalTemplateKey");
                }
                return _minimalTemplateKey;
            }
        }

        #endregion

        #region TinyTemplateKey Resource Key

        private static ComponentResourceKey _tinyTemplateKey;

        public static ResourceKey TinyTemplateKey
        {
            get
            {
                if (_tinyTemplateKey == null)
                {
                    _tinyTemplateKey = new ComponentResourceKey(
                        typeof(GameButton),
                        "TinyTemplateKey");
                }
                return _tinyTemplateKey;
            }
        }

        #endregion

        #region GroupedVerticallyTemplateKey Resource Key

        private static ComponentResourceKey _groupedVerticallyTemplateKey;

        public static ResourceKey GroupedVerticallyTemplateKey
        {
            get
            {
                if (_groupedVerticallyTemplateKey == null)
                {
                    _groupedVerticallyTemplateKey = new ComponentResourceKey(
                        typeof(GameButton),
                        "GroupedVerticallyTemplateKey");
                }
                return _groupedVerticallyTemplateKey;
            }
        }

        #endregion

        #region GroupedHorizontallyTemplateKey Resource Key

        private static ComponentResourceKey _groupedHorizontallyTemplateKey;

        public static ResourceKey GroupedHorizontallyTemplateKey
        {
            get
            {
                if (_groupedHorizontallyTemplateKey == null)
                {
                    _groupedHorizontallyTemplateKey = new ComponentResourceKey(
                        typeof(GameButton),
                        "GroupedHorizontallyTemplateKey");
                }
                return _groupedHorizontallyTemplateKey;
            }
        }

        #endregion

        #region CheckboxTemplateKey Resource Key

        private static ComponentResourceKey _checkboxTemplateKey;

        public static ResourceKey CheckboxTemplateKey
        {
            get
            {
                if (_checkboxTemplateKey == null)
                {
                    _checkboxTemplateKey = new ComponentResourceKey(
                        typeof(GameButton),
                        "CheckboxTemplateKey");
                }
                return _checkboxTemplateKey;
            }
        }

        #endregion

        #region HyperlinkTemplateKey Resource Key

        private static ComponentResourceKey _hyperlinkTemplateKey;

        public static ResourceKey HyperlinkTemplateKey
        {
            get
            {
                if (_hyperlinkTemplateKey == null)
                {
                    _hyperlinkTemplateKey = new ComponentResourceKey(
                        typeof(GameButton),
                        "HyperlinkTemplateKey");
                }
                return _hyperlinkTemplateKey;
            }
        }

        #endregion

        #endregion

        #region Properties

        #region DisplayMode Property

        public static readonly DependencyProperty DisplayModeProperty = DependencyProperty.Register(
            "DisplayMode",
            typeof(GameButtonDisplayMode),
            typeof(GameButton),
            new FrameworkPropertyMetadata(
                GameButtonDisplayMode.Normal,
                FrameworkPropertyMetadataOptions.AffectsRender,
                (o, args) => ((GameButton)o).UpdateStyleForVisualStudioBug()));

        public GameButtonDisplayMode DisplayMode
        {
            get => (GameButtonDisplayMode)GetValue(DisplayModeProperty);
            set => SetValue(DisplayModeProperty, value);
        }

        #endregion

        #endregion

        #region Constructors and Finalizers

        static GameButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(GameButton),
                new FrameworkPropertyMetadata(typeof(GameButton)));

            ContextProperty.OverrideMetadata(
                typeof(GameButton),
                new FrameworkPropertyMetadata((o, args) => ((GameButton)o).UpdateStyleForVisualStudioBug()));
        }

        public GameButton() { }

        public GameButton(ICommand command)
            : this()
        {
            Command = command;
        }

        #endregion

        #region Method Overrides

        protected override object CoerceCommandParameter(DependencyObject obj, object value)
        {
            if (value != null || Command == null)
            {
                return value;
            }

            if (_defaultCommandParameter == null)
            {
                _defaultCommandParameter = new CheckableCommandParameter();
            }

            return _defaultCommandParameter;
        }

        #endregion

        private void UpdateStyleForVisualStudioBug()
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }

            switch (DisplayMode)
            {
                case GameButtonDisplayMode.Minimal:
                    SetResourceReference(TemplateProperty, MinimalTemplateKey);
                    break;

                case GameButtonDisplayMode.Tiny:
                    SetResourceReference(TemplateProperty, TinyTemplateKey);
                    break;

                case GameButtonDisplayMode.GroupedVertically:
                    SetResourceReference(TemplateProperty, GroupedVerticallyTemplateKey);
                    break;

                case GameButtonDisplayMode.GroupedHorizontally:
                    SetResourceReference(TemplateProperty, GroupedHorizontallyTemplateKey);
                    break;

                case GameButtonDisplayMode.CheckBox:
                    SetResourceReference(TemplateProperty, CheckboxTemplateKey);
                    break;

                case GameButtonDisplayMode.Hyperlink:
                    SetResourceReference(TemplateProperty, HyperlinkTemplateKey);
                    break;

                default:
                    SetResourceReference(TemplateProperty, NormalTemplateKey);
                    break;
            }

            if (!this.HasDefaultValue(ContextProperty))
            {
                switch (Context)
                {
                    case GameControlContext.VerticalGroupItem:
                        SetResourceReference(TemplateProperty, GroupedVerticallyTemplateKey);
                        break;
                    case GameControlContext.HorizontalGroupItem:
                        SetResourceReference(TemplateProperty, GroupedHorizontallyTemplateKey);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        internal static void BindToValue(DependencyObject obj, DependencyProperty property, object value)
        {
            Binding binding = new Binding { BindsDirectlyToSource = true, Source = value };
            _ = BindingOperations.SetBinding(obj, property, binding);
        }
    }
}