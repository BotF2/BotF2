using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Supremacy.Client.Controls
{
    /// <summary>
    /// Represents a <see cref="Button"/> that can be used in a title bar and supports multiple appearances for various states.
    /// </summary>
    public class InlineWindowTitleBarButton : Button
    {
        #region Dependency Properties
        /// <summary>
        /// Identifies the <see cref="BackgroundActiveDisabled"/> dependency property.  This field is read-only.
        /// </summary>
        /// <value>The identifier for the <see cref="BackgroundActiveDisabled"/> dependency property.</value>
        public static readonly DependencyProperty BackgroundActiveDisabledProperty =
            DependencyProperty.Register(
                "BackgroundActiveDisabled",
                typeof(Brush),
                typeof(InlineWindowTitleBarButton),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Identifies the <see cref="BackgroundActiveHover"/> dependency property.  This field is read-only.
        /// </summary>
        /// <value>The identifier for the <see cref="BackgroundActiveHover"/> dependency property.</value>
        public static readonly DependencyProperty BackgroundActiveHoverProperty =
            DependencyProperty.Register(
                "BackgroundActiveHover",
                typeof(Brush),
                typeof(InlineWindowTitleBarButton),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Identifies the <see cref="BackgroundActiveNormal"/> dependency property.  This field is read-only.
        /// </summary>
        /// <value>The identifier for the <see cref="BackgroundActiveNormal"/> dependency property.</value>
        public static readonly DependencyProperty BackgroundActiveNormalProperty =
            DependencyProperty.Register(
                "BackgroundActiveNormal",
                typeof(Brush),
                typeof(InlineWindowTitleBarButton),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Identifies the <see cref="BackgroundActivePressed"/> dependency property.  This field is read-only.
        /// </summary>
        /// <value>The identifier for the <see cref="BackgroundActivePressed"/> dependency property.</value>
        public static readonly DependencyProperty BackgroundActivePressedProperty =
            DependencyProperty.Register(
                "BackgroundActivePressed",
                typeof(Brush),
                typeof(InlineWindowTitleBarButton),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Identifies the <see cref="BackgroundActivePressedHighlight"/> dependency property.  This field is read-only.
        /// </summary>
        /// <value>The identifier for the <see cref="BackgroundActivePressedHighlight"/> dependency property.</value>
        public static readonly DependencyProperty BackgroundActivePressedHighlightProperty =
            DependencyProperty.Register(
                "BackgroundActivePressedHighlight",
                typeof(Brush),
                typeof(InlineWindowTitleBarButton),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Identifies the <see cref="BackgroundInactiveDisabled"/> dependency property.  This field is read-only.
        /// </summary>
        /// <value>The identifier for the <see cref="BackgroundInactiveDisabled"/> dependency property.</value>
        public static readonly DependencyProperty BackgroundInactiveDisabledProperty =
            DependencyProperty.Register(
                "BackgroundInactiveDisabled",
                typeof(Brush),
                typeof(InlineWindowTitleBarButton),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Identifies the <see cref="BackgroundInactiveHover"/> dependency property.  This field is read-only.
        /// </summary>
        /// <value>The identifier for the <see cref="BackgroundInactiveHover"/> dependency property.</value>
        public static readonly DependencyProperty BackgroundInactiveHoverProperty =
            DependencyProperty.Register(
                "BackgroundInactiveHover",
                typeof(Brush),
                typeof(InlineWindowTitleBarButton),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Identifies the <see cref="BackgroundInactiveHoverHighlight"/> dependency property.  This field is read-only.
        /// </summary>
        /// <value>The identifier for the <see cref="BackgroundInactiveHoverHighlight"/> dependency property.</value>
        public static readonly DependencyProperty BackgroundInactiveHoverHighlightProperty =
            DependencyProperty.Register(
                "BackgroundInactiveHoverHighlight",
                typeof(Brush),
                typeof(InlineWindowTitleBarButton),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Identifies the <see cref="BackgroundInactiveNormal"/> dependency property.  This field is read-only.
        /// </summary>
        /// <value>The identifier for the <see cref="BackgroundInactiveNormal"/> dependency property.</value>
        public static readonly DependencyProperty BackgroundInactiveNormalProperty =
            DependencyProperty.Register(
                "BackgroundInactiveNormal",
                typeof(Brush),
                typeof(InlineWindowTitleBarButton),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Identifies the <see cref="BackgroundActiveHoverHighlight"/> dependency property.  This field is read-only.
        /// </summary>
        /// <value>The identifier for the <see cref="BackgroundActiveHoverHighlight"/> dependency property.</value>
        public static readonly DependencyProperty BackgroundActiveHoverHighlightProperty =
            DependencyProperty.Register(
                "BackgroundActiveHoverHighlight",
                typeof(Brush),
                typeof(InlineWindowTitleBarButton),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Identifies the <see cref="BorderActiveDisabledBrush"/> dependency property.  This field is read-only.
        /// </summary>
        /// <value>The identifier for the <see cref="BorderActiveDisabledBrush"/> dependency property.</value>
        public static readonly DependencyProperty BorderActiveDisabledBrushProperty =
            DependencyProperty.Register(
                "BorderActiveDisabledBrush",
                typeof(Brush),
                typeof(InlineWindowTitleBarButton),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Identifies the <see cref="BorderActiveHoverBrush"/> dependency property.  This field is read-only.
        /// </summary>
        /// <value>The identifier for the <see cref="BorderActiveHoverBrush"/> dependency property.</value>
        public static readonly DependencyProperty BorderActiveHoverBrushProperty =
            DependencyProperty.Register(
                "BorderActiveHoverBrush",
                typeof(Brush),
                typeof(InlineWindowTitleBarButton),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Identifies the <see cref="BorderActiveNormalBrush"/> dependency property.  This field is read-only.
        /// </summary>
        /// <value>The identifier for the <see cref="BorderActiveNormalBrush"/> dependency property.</value>
        public static readonly DependencyProperty BorderActiveNormalBrushProperty =
            DependencyProperty.Register(
                "BorderActiveNormalBrush",
                typeof(Brush),
                typeof(InlineWindowTitleBarButton),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Identifies the <see cref="BorderActivePressedBrush"/> dependency property.  This field is read-only.
        /// </summary>
        /// <value>The identifier for the <see cref="BorderActivePressedBrush"/> dependency property.</value>
        public static readonly DependencyProperty BorderActivePressedBrushProperty =
            DependencyProperty.Register(
                "BorderActivePressedBrush",
                typeof(Brush),
                typeof(InlineWindowTitleBarButton),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Identifies the <see cref="BorderInactiveDisabledBrush"/> dependency property.  This field is read-only.
        /// </summary>
        /// <value>The identifier for the <see cref="BorderInactiveDisabledBrush"/> dependency property.</value>
        public static readonly DependencyProperty BorderInactiveDisabledBrushProperty =
            DependencyProperty.Register(
                "BorderInactiveDisabledBrush",
                typeof(Brush),
                typeof(InlineWindowTitleBarButton),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Identifies the <see cref="BorderInactiveHoverBrush"/> dependency property.  This field is read-only.
        /// </summary>
        /// <value>The identifier for the <see cref="BorderInactiveHoverBrush"/> dependency property.</value>
        public static readonly DependencyProperty BorderInactiveHoverBrushProperty =
            DependencyProperty.Register(
                "BorderInactiveHoverBrush",
                typeof(Brush),
                typeof(InlineWindowTitleBarButton),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Identifies the <see cref="BorderInactiveNormalBrush"/> dependency property.  This field is read-only.
        /// </summary>
        /// <value>The identifier for the <see cref="BorderInactiveNormalBrush"/> dependency property.</value>
        public static readonly DependencyProperty BorderInactiveNormalBrushProperty =
            DependencyProperty.Register(
                "BorderInactiveNormalBrush",
                typeof(Brush),
                typeof(InlineWindowTitleBarButton),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Identifies the <see cref="CornerRadius"/> dependency property.  This field is read-only.
        /// </summary>
        /// <value>The identifier for the <see cref="CornerRadius"/> dependency property.</value>
        public static readonly DependencyProperty CornerRadiusProperty =
            Border.CornerRadiusProperty.AddOwner(typeof(InlineWindowTitleBarButton));

        /// <summary>
        /// Identifies the <see cref="ForegroundActiveDisabled"/> dependency property.  This field is read-only.
        /// </summary>
        /// <value>The identifier for the <see cref="ForegroundActiveDisabled"/> dependency property.</value>
        public static readonly DependencyProperty ForegroundActiveDisabledProperty =
            DependencyProperty.Register(
                "ForegroundActiveDisabled",
                typeof(Brush),
                typeof(InlineWindowTitleBarButton),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Identifies the <see cref="ForegroundActiveHover"/> dependency property.  This field is read-only.
        /// </summary>
        /// <value>The identifier for the <see cref="ForegroundActiveHover"/> dependency property.</value>
        public static readonly DependencyProperty ForegroundActiveHoverProperty =
            DependencyProperty.Register(
                "ForegroundActiveHover",
                typeof(Brush),
                typeof(InlineWindowTitleBarButton),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Identifies the <see cref="ForegroundActiveNormal"/> dependency property.  This field is read-only.
        /// </summary>
        /// <value>The identifier for the <see cref="ForegroundActiveNormal"/> dependency property.</value>
        public static readonly DependencyProperty ForegroundActiveNormalProperty =
            DependencyProperty.Register(
                "ForegroundActiveNormal",
                typeof(Brush),
                typeof(InlineWindowTitleBarButton),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Identifies the <see cref="ForegroundActivePressed"/> dependency property.  This field is read-only.
        /// </summary>
        /// <value>The identifier for the <see cref="ForegroundActivePressed"/> dependency property.</value>
        public static readonly DependencyProperty ForegroundActivePressedProperty =
            DependencyProperty.Register(
                "ForegroundActivePressed",
                typeof(Brush),
                typeof(InlineWindowTitleBarButton),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Identifies the <see cref="ForegroundInactiveDisabled"/> dependency property.  This field is read-only.
        /// </summary>
        /// <value>The identifier for the <see cref="ForegroundInactiveDisabled"/> dependency property.</value>
        public static readonly DependencyProperty ForegroundInactiveDisabledProperty =
            DependencyProperty.Register(
                "ForegroundInactiveDisabled",
                typeof(Brush),
                typeof(InlineWindowTitleBarButton),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Identifies the <see cref="ForegroundInactiveHover"/> dependency property.  This field is read-only.
        /// </summary>
        /// <value>The identifier for the <see cref="ForegroundInactiveHover"/> dependency property.</value>
        public static readonly DependencyProperty ForegroundInactiveHoverProperty =
            DependencyProperty.Register(
                "ForegroundInactiveHover",
                typeof(Brush),
                typeof(InlineWindowTitleBarButton),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Identifies the <see cref="ForegroundInactiveNormal"/> dependency property.  This field is read-only.
        /// </summary>
        /// <value>The identifier for the <see cref="ForegroundInactiveNormal"/> dependency property.</value>
        public static readonly DependencyProperty ForegroundInactiveNormalProperty =
            DependencyProperty.Register(
                "ForegroundInactiveNormal",
                typeof(Brush),
                typeof(InlineWindowTitleBarButton),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Identifies the <see cref="IsActive"/> dependency property.  This field is read-only.
        /// </summary>
        /// <value>The identifier for the <see cref="IsActive"/> dependency property.</value>
        public static readonly DependencyProperty IsActiveProperty = DependencyProperty.RegisterAttached(
            "IsActive",
            typeof(bool),
            typeof(InlineWindowTitleBarButton),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));
        #endregion

        /////////////////////////////////////////////////////////////////////////////////////////////////////
        // OBJECT
        /////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes the <c>InlineWindowTitleBarButton</c> class.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static InlineWindowTitleBarButton()
        {
            // Override default properties
            IsTabStopProperty.OverrideMetadata(
                typeof(InlineWindowTitleBarButton), 
                new FrameworkPropertyMetadata(false));

            FocusableProperty.OverrideMetadata(
                typeof(InlineWindowTitleBarButton),
                new FrameworkPropertyMetadata(false));
            
            //DefaultStyleKeyProperty.OverrideMetadata(
            //    typeof(InlineWindowTitleBarButton), 
            //    new FrameworkPropertyMetadata(typeof(InlineWindowTitleBarButton)));
        }

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> that is used to render the background of the button when active and in a disabled state.
        /// </summary>
        /// <value>The <see cref="Brush"/> that is used to render the background of the button when active and in a disabled state.</value>
        public Brush BackgroundActiveDisabled
        {
            get { return (Brush)GetValue(BackgroundActiveDisabledProperty); }
            set { SetValue(BackgroundActiveDisabledProperty, value); }
        }

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> that is used to render the background of the button when active and in a hover state.
        /// </summary>
        /// <value>The <see cref="Brush"/> that is used to render the background of the button when active and in a hover state.</value>
        public Brush BackgroundActiveHover
        {
            get { return (Brush)GetValue(BackgroundActiveHoverProperty); }
            set { SetValue(BackgroundActiveHoverProperty, value); }
        }

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> that is used to render the background of the button when active and in a hover state.
        /// </summary>
        /// <value>The <see cref="Brush"/> that is used to render the background of the button when active and in a hover state.</value>
        public Brush BackgroundActiveHoverHighlight
        {
            get { return (Brush)GetValue(BackgroundActiveHoverHighlightProperty); }
            set { SetValue(BackgroundActiveHoverHighlightProperty, value); }
        }

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> that is used to render the background of the button when active and in a normal state.
        /// </summary>
        /// <value>The <see cref="Brush"/> that is used to render the background of the button when active and in a normal state.</value>
        public Brush BackgroundActiveNormal
        {
            get { return (Brush)GetValue(BackgroundActiveNormalProperty); }
            set { SetValue(BackgroundActiveNormalProperty, value); }
        }

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> that is used to render the background of the button when active and in a pressed state.
        /// </summary>
        /// <value>The <see cref="Brush"/> that is used to render the background of the button when active and in a pressed state.</value>
        public Brush BackgroundActivePressed
        {
            get { return (Brush)GetValue(BackgroundActivePressedProperty); }
            set { SetValue(BackgroundActivePressedProperty, value); }
        }

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> that is used to render the background of the button when active and in a pressed state.
        /// </summary>
        /// <value>The <see cref="Brush"/> that is used to render the background of the button when active and in a pressed state.</value>
        public Brush BackgroundActivePressedHighlight
        {
            get { return (Brush)GetValue(BackgroundActivePressedHighlightProperty); }
            set { SetValue(BackgroundActivePressedHighlightProperty, value); }
        }

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> that is used to render the background of the button when inactive and in a disabled state.
        /// </summary>
        /// <value>The <see cref="Brush"/> that is used to render the background of the button when inactive and in a disabled state.</value>
        public Brush BackgroundInactiveDisabled
        {
            get { return (Brush)GetValue(BackgroundInactiveDisabledProperty); }
            set { SetValue(BackgroundInactiveDisabledProperty, value); }
        }

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> that is used to render the background of the button when inactive and in a hover state.
        /// </summary>
        /// <value>The <see cref="Brush"/> that is used to render the background of the button when inactive and in a hover state.</value>
        public Brush BackgroundInactiveHover
        {
            get { return (Brush)GetValue(BackgroundInactiveHoverProperty); }
            set { SetValue(BackgroundInactiveHoverProperty, value); }
        }

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> that is used to render the background of the button when inactive and in a hover state.
        /// </summary>
        /// <value>The <see cref="Brush"/> that is used to render the background of the button when inactive and in a hover state.</value>
        public Brush BackgroundInactiveHoverHighlight
        {
            get { return (Brush)GetValue(BackgroundInactiveHoverHighlightProperty); }
            set { SetValue(BackgroundInactiveHoverHighlightProperty, value); }
        }

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> that is used to render the background of the button when inactive and in a normal state.
        /// </summary>
        /// <value>The <see cref="Brush"/> that is used to render the background of the button when inactive and in a normal state.</value>
        public Brush BackgroundInactiveNormal
        {
            get { return (Brush)GetValue(BackgroundInactiveNormalProperty); }
            set { SetValue(BackgroundInactiveNormalProperty, value); }
        }

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> that is used to render the border of the button when active and in a disabled state.
        /// </summary>
        /// <value>The <see cref="Brush"/> that is used to render the border of the button when active and in a disabled state.</value>
        public Brush BorderActiveDisabledBrush
        {
            get { return (Brush)GetValue(BorderActiveDisabledBrushProperty); }
            set { SetValue(BorderActiveDisabledBrushProperty, value); }
        }

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> that is used to render the border of the button when active and in a hover state.
        /// </summary>
        /// <value>The <see cref="Brush"/> that is used to render the border of the button when active and in a hover state.</value>
        public Brush BorderActiveHoverBrush
        {
            get { return (Brush)GetValue(BorderActiveHoverBrushProperty); }
            set { SetValue(BorderActiveHoverBrushProperty, value); }
        }

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> that is used to render the border of the button when active and in a normal state.
        /// </summary>
        /// <value>The <see cref="Brush"/> that is used to render the border of the button when active and in a normal state.</value>
        public Brush BorderActiveNormalBrush
        {
            get { return (Brush)GetValue(BorderActiveNormalBrushProperty); }
            set { SetValue(BorderActiveNormalBrushProperty, value); }
        }

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> that is used to render the border of the button when active and in a pressed state.
        /// </summary>
        /// <value>The <see cref="Brush"/> that is used to render the border of the button when active and in a pressed state.</value>
        public Brush BorderActivePressedBrush
        {
            get { return (Brush)GetValue(BorderActivePressedBrushProperty); }
            set { SetValue(BorderActivePressedBrushProperty, value); }
        }

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> that is used to render the border of the button when inactive and in a disabled state.
        /// </summary>
        /// <value>The <see cref="Brush"/> that is used to render the border of the button when inactive and in a disabled state.</value>
        public Brush BorderInactiveDisabledBrush
        {
            get { return (Brush)GetValue(BorderInactiveDisabledBrushProperty); }
            set { SetValue(BorderInactiveDisabledBrushProperty, value); }
        }

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> that is used to render the border of the button when inactive and in a hover state.
        /// </summary>
        /// <value>The <see cref="Brush"/> that is used to render the border of the button when inactive and in a hover state.</value>
        public Brush BorderInactiveHoverBrush
        {
            get { return (Brush)GetValue(BorderInactiveHoverBrushProperty); }
            set { SetValue(BorderInactiveHoverBrushProperty, value); }
        }

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> that is used to render the border of the button when inactive and in a normal state.
        /// </summary>
        /// <value>The <see cref="Brush"/> that is used to render the border of the button when inactive and in a normal state.</value>
        public Brush BorderInactiveNormalBrush
        {
            get { return (Brush)GetValue(BorderInactiveNormalBrushProperty); }
            set { SetValue(BorderInactiveNormalBrushProperty, value); }
        }

        /// <summary>
        /// Gets or sets the <see cref="System.Windows.CornerRadius"/> of the button's border.
        /// </summary>
        /// <value>The <see cref="System.Windows.CornerRadius"/> of the button's border.</value>
        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> that is used to render the foreground of the button when active and in a pressed state.
        /// </summary>
        /// <value>The <see cref="Brush"/> that is used to render the foreground of the button when active and in a pressed state.</value>
        public Brush ForegroundActivePressed
        {
            get { return (Brush)GetValue(ForegroundActivePressedProperty); }
            set { SetValue(ForegroundActivePressedProperty, value); }
        }

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> that is used to render the foreground of the button when active and in a disabled state.
        /// </summary>
        /// <value>The <see cref="Brush"/> that is used to render the foreground of the button when active and in a disabled state.</value>
        public Brush ForegroundActiveDisabled
        {
            get { return (Brush)GetValue(ForegroundActiveDisabledProperty); }
            set { SetValue(ForegroundActiveDisabledProperty, value); }
        }

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> that is used to render the foreground of the button when active and in a hover state.
        /// </summary>
        /// <value>The <see cref="Brush"/> that is used to render the foreground of the button when active and in a hover state.</value>
        public Brush ForegroundActiveHover
        {
            get { return (Brush)GetValue(ForegroundActiveHoverProperty); }
            set { SetValue(ForegroundActiveHoverProperty, value); }
        }

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> that is used to render the foreground of the button when active and in a normal state.
        /// </summary>
        /// <value>The <see cref="Brush"/> that is used to render the foreground of the button when active and in a normal state.</value>
        public Brush ForegroundActiveNormal
        {
            get { return (Brush)GetValue(ForegroundActiveNormalProperty); }
            set { SetValue(ForegroundActiveNormalProperty, value); }
        }

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> that is used to render the foreground of the button when inactive and in a disabled state.
        /// </summary>
        /// <value>The <see cref="Brush"/> that is used to render the foreground of the button when inactive and in a disabled state.</value>
        public Brush ForegroundInactiveDisabled
        {
            get { return (Brush)GetValue(ForegroundInactiveDisabledProperty); }
            set { SetValue(ForegroundInactiveDisabledProperty, value); }
        }

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> that is used to render the foreground of the button when inactive and in a hover state.
        /// </summary>
        /// <value>The <see cref="Brush"/> that is used to render the foreground of the button when inactive and in a hover state.</value>
        public Brush ForegroundInactiveHover
        {
            get { return (Brush)GetValue(ForegroundInactiveHoverProperty); }
            set { SetValue(ForegroundInactiveHoverProperty, value); }
        }

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> that is used to render the foreground of the button when inactive and in a normal state.
        /// </summary>
        /// <value>The <see cref="Brush"/> that is used to render the foreground of the button when inactive and in a normal state.</value>
        public Brush ForegroundInactiveNormal
        {
            get { return (Brush)GetValue(ForegroundInactiveNormalProperty); }
            set { SetValue(ForegroundInactiveNormalProperty, value); }
        }

        /// <summary>
        /// Gets or sets whether the title bar that contains the button is active.
        /// </summary>
        /// <value>
        /// <c>true</c> if the title bar that contains the button is active; otherwise, <c>false</c>.
        /// </value>
        public bool IsActive
        {
            get { return (bool)GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }

        /// <summary>
        /// Gets the value of the <c>IsActive</c> attached property for the specified object.
        /// </summary>
        /// <param name="obj">The object from which the property value is read.</param>
        /// <returns>The object's value.</returns>
        public static bool GetIsActive(DependencyObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            return (bool)obj.GetValue(IsActiveProperty);
        }

        /// <summary>
        /// Sets the value of the <c>IsActive</c> attached property to the specified object. 
        /// </summary>
        /// <param name="obj">The object to which the attached property is written.</param>
        /// <param name="value">The value to set.</param>
        public static void SetIsActive(DependencyObject obj, bool value)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            obj.SetValue(IsActiveProperty, value);
        }
    }
}