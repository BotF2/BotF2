namespace Supremacy.Client.Controls
{
    public class GamePopupCollection : DeferrableObservableCollection<GamePopup>
    {
        public new GamePopup[] ToArray()
        {
            GamePopup[] result = new GamePopup[Count];
            CopyTo(result, 0);
            return result;
        }
    }
}