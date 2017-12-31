using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Supremacy.Client.Controls
{
    public class GameCommandUIProvider : IGameCommandUIProvider
    {
        private ImageSource _imageSourceLarge;
        private ImageSource _imageSourceSmall;
        private string _label;

        public event PropertyChangedEventHandler PropertyChanged;

        public GameCommandUIProvider() : this(null, (ImageSource)null, null) { }

        public GameCommandUIProvider(string label) : this(label, (ImageSource)null, null) { }

        public GameCommandUIProvider(string label, string imageSourceLarge, string imageSourceSmall) :
            this(label,
                (!String.IsNullOrEmpty(imageSourceLarge) ? new BitmapImage(new Uri(imageSourceLarge, UriKind.RelativeOrAbsolute)) : null),
                (!String.IsNullOrEmpty(imageSourceSmall) ? new BitmapImage(new Uri(imageSourceSmall, UriKind.RelativeOrAbsolute)) : null)) { }

        public GameCommandUIProvider(string label, ImageSource imageSourceLarge, ImageSource imageSourceSmall)
        {
            Label = label;
            ImageSourceLarge = imageSourceLarge;
            ImageSourceSmall = imageSourceSmall;
        }

        public ImageSource ImageSourceLarge
        {
            get { return _imageSourceLarge; }
            set
            {
                if (_imageSourceLarge == value)
                    return;
                _imageSourceLarge = value;
                NotifyPropertyChanged("ImageSourceLarge");
            }
        }

        public ImageSource ImageSourceSmall
        {
            get { return _imageSourceSmall; }
            set
            {
                if (_imageSourceSmall == value)
                    return;
                _imageSourceSmall = value;
                NotifyPropertyChanged("ImageSourceSmall");
            }
        }

        [Localizability(LocalizationCategory.Label)]
        public string Label
        {
            get { return _label; }
            set
            {
                if (_label == value)
                    return;
                _label = value;
                NotifyPropertyChanged("Label");
            }
        }

        protected void NotifyPropertyChanged(string name)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(name));
        }
    }
}

