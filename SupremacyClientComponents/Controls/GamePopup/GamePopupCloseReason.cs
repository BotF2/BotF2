namespace Supremacy.Client.Controls
{
    public enum GamePopupCloseReason
    {
        Unknown,
        ControlClick,
        ClickThrough,
        EscapeKeyPressed,
        IsPopupOpenChanged,
        LostKeyboardFocusWithin,
        LostMouseCapture,
        OtherPopupOpened,
        SystemMenuKeyPressed
    }
}