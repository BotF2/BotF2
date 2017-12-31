using System.Windows;
using System.Windows.Controls.Primitives;

namespace Supremacy.Client.Controls
{
    internal interface IGamePopupAnchor : IInputElement
    {
        bool IgnoreNextLeftRelease { get; set; }
        bool IgnoreNextRightRelease { get; set; }
        bool IsPopupOpen { get; set; }
        GamePopupCloseReason LastCloseReason { get; set; }
        void OnPopupClosed();
        void OnPopupOpened();
        bool OnPopupOpening();
        GamePopup Popup { get; }
        bool PopupOpenedWithMouse { get; set; }
        Thumb ResizeGrip { get; }
    }
}