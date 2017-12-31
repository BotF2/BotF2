using System.Collections.ObjectModel;

namespace Supremacy.Client.Controls
{
    public class ReadOnlyGamePopupCollection : ReadOnlyObservableCollection<GamePopup>
    {
        public ReadOnlyGamePopupCollection(ObservableCollection<GamePopup> list) 
            : base(list) {}
    }
}