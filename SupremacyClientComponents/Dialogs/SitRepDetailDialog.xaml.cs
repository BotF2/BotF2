using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

using Supremacy.Annotations;
using Supremacy.Game;
using Supremacy.Resources;

namespace Supremacy.Client.Dialogs
{
    /// <summary>
    /// Interaction logic for SitRepDetailDialog.xaml
    /// </summary>
    public partial class SitRepDetailDialog
    {
        private readonly SitRepEntry _sitRepEntry;
        private readonly MediaPlayer _mediaPlayer;

        private SitRepDetailDialog([NotNull] SitRepEntry sitRepEntry)
        {
            _sitRepEntry = sitRepEntry;
            if (sitRepEntry == null)
                throw new ArgumentNullException("sitRepEntry");
            
            DataContext = sitRepEntry;
            InitializeComponent();

            if (_sitRepEntry.DetailImage == null)
                DetailImage.Visibility = Visibility.Collapsed;

            if (!_sitRepEntry.HasSoundEffect)
                return;

            _mediaPlayer = new MediaPlayer();
            _mediaPlayer.Open(new Uri(ResourceManager.GetResourcePath(_sitRepEntry.SoundEffect), UriKind.Relative));

            Loaded += (s, e) => _mediaPlayer.Play();
            Unloaded += (s, e) => _mediaPlayer.Stop();
        }

        public static void Show([NotNull] SitRepEntry sitRepEntry)
        {
            if (sitRepEntry == null)
                throw new ArgumentNullException("sitRepEntry");
            //works, but only for DetailDialogs     GameLog.Client.GameData.DebugFormat("SitRepDetailDialog.xaml.cs: {0}", sitRepEntry.SummaryText);
            new SitRepDetailDialog(sitRepEntry).ShowDialog();
        }

        private void ExecuteCloseCommand(object sender, ExecutedRoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
