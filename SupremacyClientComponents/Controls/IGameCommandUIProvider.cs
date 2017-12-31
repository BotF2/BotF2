using System.ComponentModel;
using System.Windows.Media;

namespace Supremacy.Client.Controls
{
    public interface IGameCommandUIProvider : INotifyPropertyChanged
    {
        ImageSource ImageSourceLarge { get; }
        ImageSource ImageSourceSmall { get; }
        string Label { get; }
    }
}