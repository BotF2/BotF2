namespace Supremacy.Client.Controls
{
    public class GamePopupCollection : DeferrableObservableCollection<GamePopup>
    {
        public new GamePopup[] ToArray()
        {
            var result = new GamePopup[Count];
            CopyTo(result, 0);
            return result;
        }
    }
}