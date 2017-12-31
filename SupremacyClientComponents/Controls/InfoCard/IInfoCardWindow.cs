using System;
using System.Windows;

namespace Supremacy.Client.Controls
{
    /// <summary>
    /// Provides the requirements for a class that implements an info popup.
    /// </summary>
    public interface IInfoCardWindow
    {
        void DragMove();

        /// <summary>
        /// Attempts to bring the window to the foreground and activates it.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the window was successfully activated; otherwise, <c>false</c>.
        /// </returns>
        bool Activate();

        /// <summary>
        /// Occurs when the window is activated.
        /// </summary>
        event EventHandler Activated;

        /// <summary>
        /// Closes the window.
        /// </summary>
        /// <param name="closeReason">A <see cref="InfoCardCloseReason"/> indicating the close reason.</param>
        void Close(InfoCardCloseReason closeReason);

        /// <summary>
        /// Occurs when the window is closed.
        /// </summary>
        event EventHandler Closed;

        /// <summary>
        /// Gets the desired size of the window.
        /// </summary>
        /// <value>A <see cref="Size"/> indicating the desired size of the window.</value>
        Size DesiredSize { get; }

        /// <summary>
        /// Gets the <see cref="InfoCardSite"/> that is managing the rafting window.
        /// </summary>
        /// <value>The <see cref="InfoCardSite"/> that is managing the rafting window.</value>
        InfoCardSite InfoCardSite { get; }

        /// <summary>
        /// Gets whether the window is currently closing.
        /// </summary>
        /// <value>
        /// <c>true</c> if the window is closing; otherwise, <c>false</c>.
        /// </value>
        bool IsClosing { get; }

        /// <summary>
        /// Gets whether the window is currently visible.
        /// </summary>
        /// <value>
        /// <c>true</c> if the window is visible; otherwise, <c>false</c>.
        /// </value>
        bool IsVisible { get; }

        /// <summary>
        /// Gets the location of the window.
        /// </summary>
        /// <value>A <see cref="Point"/> indicating the location of the window.</value>
        Point Location { get; }

        /// <summary>
        /// Occurs when the window is moved.
        /// </summary>
        event EventHandler LocationChanged;

        /// <summary>
        /// Gets the <see cref="InfoCardHost"/> that is hosted within this window.
        /// </summary>
        /// <value>The <see cref="InfoCardHost"/> that is hosted within this window.</value>
        InfoCardHost InfoCardHost { get; }

        /// <summary>
        /// Initializes the position and size of the window.
        /// </summary>
        /// <param name="position">The desired location.</param>
        void Setup(Point? position);

        /// <summary>
        /// Shows the window.
        /// </summary>
        void Show();

        /// <summary>
        /// Ensures the rafting window appears on-screen.
        /// </summary>
        void SnapToScreen();

        /// <summary>
        /// Gets or sets the <see cref="System.Windows.Visibility"/> of the window.
        /// </summary>
        /// <value>The <see cref="System.Windows.Visibility"/> of the window.</value>
        Visibility Visibility { get; set; }
    }
}