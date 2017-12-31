using Supremacy.Economy;

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

        private void OnBuildSlotListItemClicked(object sender, object clickedItem)
        {
            var buildSlot = clickedItem as BuildSlot;
            if (buildSlot == null)
                return;

            var project = buildSlot.Project;
            if (project == null)
                return;

            var presentationModel = PresentationModel;
            if (presentationModel == null)
                return;

            var command = presentationModel.CancelBuildProjectCommand;
            if ((command == null) || !command.CanExecute(project))
                return;

            command.Execute(project);
        }

        private void OnBuildQueueItemClicked(object sender, object clickedItem)
        {
            var buildQueueItem = clickedItem as BuildQueueItem;
            if (buildQueueItem == null)
                return;

            var presentationModel = PresentationModel;
            if (presentationModel == null)
                return;

            var command = presentationModel.RemoveFromShipyardBuildQueueCommand;
            if ((command == null) || !command.CanExecute(buildQueueItem))
                return;

            command.Execute(buildQueueItem);
        }

        private ColonyScreenPresentationModel PresentationModel
        {
            get { return DataContext as ColonyScreenPresentationModel; }
        }
    }
}
