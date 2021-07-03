using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;

namespace Supremacy.Client.Controls
{
    /// <summary>
    /// Provides numerous helper methods for docking controls.
    /// </summary>
    public class InfoCardHelper
    {
        /// <summary>
        /// Creates a <see cref="Binding"/> for given source object and dependency property.
        /// The binding is in default mode.
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <param name="dp">The dependency property.</param>
        /// <returns>The <see cref="Binding"/> that was created.</returns>
        internal static Binding CreateBinding(object source, DependencyProperty dp)
        {
            return CreateBinding(source, dp, BindingMode.Default);
        }

        /// <summary>
        /// Creates a <see cref="Binding"/> for given source object and dependency property.
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <param name="dp">The dependency property.</param>
        /// <param name="mode">The binding mode.</param>
        /// <returns>The <see cref="Binding"/> that was created.</returns>
        internal static Binding CreateBinding(object source, DependencyProperty dp, BindingMode mode)
        {
            Binding binding = new Binding { Source = source };
            if (dp != null)
            {
                binding.Path = new PropertyPath(dp);
            }

            binding.Mode = mode;
            return binding;
        }

        /// <summary>
        /// Transforms device units to logical units.
        /// </summary>
        /// <param name="source"><c>HwndSource</c> to get transformation from.</param>
        /// <param name="deviceUnits"><c>Point</c> in device units.</param>
        /// <returns>Returns <c>Point</c> in logical units.</returns>
        internal static Point DeviceToLogicalUnits(HwndSource source, Point deviceUnits)
        {
            return source.CompositionTarget.TransformFromDevice.Transform(deviceUnits);
        }

        /// <summary>
        /// Transforms device units to logical units.
        /// </summary>
        /// <param name="source"><c>HwndSource</c> to get transformation from.</param>
        /// <param name="deviceUnits"><c>Size</c> in device units.</param>
        /// <returns>Returns <c>Size</c> in logical units.</returns>
        internal static Size DeviceToLogicalUnits(HwndSource source, Size deviceUnits)
        {
            return (Size)DeviceToLogicalUnits(source, (Point)deviceUnits);
        }

        /// <summary>
        /// Converts a double value to an integer by rounding it first.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted value.</returns>
        internal static int DoubleToInt(double value)
        {
            return Convert.ToInt32(Math.Round(value));
        }

        /// <summary>
        /// Returns whether a Control key is pressed.
        /// </summary>
        /// <returns>
        /// <c>true</c> if a Control key is pressed; otherwise, <c>false</c>.
        /// </returns>
        internal static bool IsCtrlPressed()
        {
            return Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
        }

        /// <summary>
        /// Determines whether the specified <see cref="Size"/> is empty or zeroed-out.
        /// </summary>
        /// <param name="size">The <see cref="Size"/> to examine.</param>
        /// <returns>
        /// <c>true</c> if the specified size is empty or zeroed-out; otherwise, <c>false</c>.
        /// </returns>
        internal static bool IsEmpty(Size size)
        {
            return size.IsEmpty || ((size.Width == 0) && (size.Height == 0));
        }

        /// <summary>
        /// Returns whether the specified numeric value is a valid non-infinite number.
        /// </summary>
        /// <param name="value">The numeric value to examine.</param>
        /// <returns>
        /// <c>true</c> if specified numeric value is a valid non-infinite number; otherwise, <c>false</c>.
        /// </returns>
        internal static bool IsNumber(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value);
        }

        /// <summary>
        /// Returns whether the <see cref="Window"/> that contains the specified <see cref="DependencyObject"/> is active.
        /// </summary>
        /// <param name="obj">The <see cref="DependencyObject"/> to examine.</param>
        /// <returns>
        /// <c>true</c> if the <see cref="Window"/> that contains the specified <see cref="DependencyObject"/> is active; otherwise, <c>false</c>.
        /// </returns>
        internal static bool IsWindowActive(DependencyObject obj)
        {
            if (BrowserInteropHelper.IsBrowserHosted)
            {
                return true;
            }

            Window window = Window.GetWindow(obj);
            return (window == null) || window.IsActive;
        }

        /// <summary>
        /// Returns whether the <see cref="Window"/> that contains the specified <see cref="DependencyObject"/> is visible.
        /// </summary>
        /// <param name="obj">The <see cref="DependencyObject"/> to examine.</param>
        /// <returns>
        /// <c>true</c> if the <see cref="Window"/> that contains the specified <see cref="DependencyObject"/> is visible; otherwise, <c>false</c>.
        /// </returns>
        internal static bool IsWindowVisible(DependencyObject obj)
        {
            if (BrowserInteropHelper.IsBrowserHosted)
            {
                return true;
            }

            Window window = Window.GetWindow(obj);
            return (window == null) || window.IsVisible;
        }

        /// <summary>
        /// Transforms logical units to device units.
        /// </summary>
        /// <param name="source"><c>HwndSource</c> to get transformation from.</param>
        /// <param name="logicalUnits"><c>Point</c> in logical units.</param>
        /// <returns>Returns <c>Point</c> in device units.</returns>
        internal static Point LogicalToDeviceUnits(HwndSource source, Point logicalUnits)
        {
            return source.CompositionTarget.TransformToDevice.Transform(logicalUnits);
        }

        /// <summary>
        /// Transforms logical units to device units.
        /// </summary>
        /// <param name="source"><c>HwndSource</c> to get transformation from.</param>
        /// <param name="logicalUnits"><c>Size</c> in logical units.</param>
        /// <returns>Returns <c>Size</c> in device units.</returns>
        internal static Size LogicalToDeviceUnits(HwndSource source, Size logicalUnits)
        {
            return (Size)LogicalToDeviceUnits(source, (Point)logicalUnits);
        }

        /// <summary>
        /// Updates keyboad focus to work around a WPF bug.
        /// </summary>
        /// <param name="control">The control to examine.</param>
        /// <remarks>
        /// The WPF bug is described here:
        /// https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=289939&amp;wa=wsignin1.0
        /// </remarks>
        internal static void ResetFocus(DependencyObject control)
        {
            if ((control == null) || BrowserInteropHelper.IsBrowserHosted)
            {
                return;
            }

            if (!(bool)control.GetValue(UIElement.IsKeyboardFocusWithinProperty))
            {
                return;
            }

            Window window = Window.GetWindow(control);
            if (window != null)
            {
                FocusManager.SetFocusedElement(window, null);
            }

            _ = Keyboard.Focus(null);
        }

        /// <summary>
        /// Returns the rounded integer value of the double if it is a number; otherwise, <c>int.MaxValue</c>.
        /// </summary>
        /// <param name="value">The numeric value to examine.</param>
        /// <returns>The result.</returns>
        internal static int SafeDoubleToInt(double value)
        {
            return IsNumber(value) ? DoubleToInt(value) : int.MaxValue;
        }

    }
}