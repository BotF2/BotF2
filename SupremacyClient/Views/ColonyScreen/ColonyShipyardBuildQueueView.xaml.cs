using Supremacy.Economy;
using System.Windows;
using System.Windows.Input;

namespace Supremacy.Client.Views
{
    /// <summary>
    /// Interaction logic for ColonyShipyardBuildQueueView.xaml
    /// </summary>
    public partial class ColonyShipyardBuildQueueView
    {
        public ColonyShipyardBuildQueueView()
        {
            InitializeComponent();
        }

        private void OnShipyardBuildQueueItemClicked(object sender, object clickedItem) 
        {
            BuildQueueItem buildQueueItem = clickedItem as BuildQueueItem;
            if (buildQueueItem == null)
                return;

            ColonyScreenPresentationModel presentationModel = PresentationModel;
            if (presentationModel == null)
                return;

            ICommand command = presentationModel.RemoveFromShipyardBuildQueueCommand;
            if ((command != null) && command.CanExecute(buildQueueItem))
                command.Execute(buildQueueItem);
        }

        private ColonyScreenPresentationModel PresentationModel => DataContext as ColonyScreenPresentationModel;
    }
}
