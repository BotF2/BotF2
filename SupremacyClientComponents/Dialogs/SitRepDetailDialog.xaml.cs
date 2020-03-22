using Supremacy.Game;
using Supremacy.Resources;
using Supremacy.Utility;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Supremacy.Client.Dialogs
{
    /// <summary>
    /// Interaction logic for SitRepDetailDialog.xaml
    /// </summary>
    public partial class SitRepDetailDialog
    {
        private readonly SitRepEntry _sitRepEntry;
        private readonly MediaPlayer _mediaPlayer;

        private SitRepDetailDialog(SitRepEntry sitRepEntry)
        {
            _sitRepEntry = sitRepEntry;
            if (sitRepEntry == null)
                throw new ArgumentNullException("sitRepEntry");
            GameLog.Core.SitReps.DebugFormat("Sitrep for {0} (sound = {2}): {1} ", sitRepEntry.Owner, sitRepEntry.HeaderText, sitRepEntry.HasSoundEffect);
            
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

        public static void Show(SitRepEntry sitRepEntry)
        {
            if (sitRepEntry == null)
            {
                GameLog.Core.SitReps.DebugFormat("##### problem at Sitrep for {0}: {1}, sound = {2}", sitRepEntry.Owner, sitRepEntry.HeaderText, sitRepEntry.HasSoundEffect);
                throw new ArgumentNullException("sitRepEntry");
            }
            new SitRepDetailDialog(sitRepEntry).ShowDialog();
        }

        private void ExecuteCloseCommand(object sender, ExecutedRoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
