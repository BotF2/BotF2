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

        private void OnBuildQueueItemClicked(object sender, object clickedItem) 
        {
            var buildQueueItem = clickedItem as BuildQueueItem;
            if (buildQueueItem == null)
                return;

            var presentationModel = PresentationModel;
            if (presentationModel == null)
                return;

            var command = presentationModel.RemoveFromShipyardBuildQueueCommand;
            if ((command != null) || !command.CanExecute(buildQueueItem))
                return;

            command.Execute(buildQueueItem);
        }

        private ColonyScreenPresentationModel PresentationModel
        {
            get { return DataContext as ColonyScreenPresentationModel; }
        }

        bool IsCurrentBuildProjectValid()
        {
            var presentationModel = PresentationModel;
            if (presentationModel == null)
                return false;

            var colony = presentationModel.SelectedColony;
            if (colony == null)
                return false;

            var shipyard = presentationModel.SelectedColony.Shipyard;
            if (shipyard == null)
                return false;

            var project = shipyard.BuildSlots[0].Project; 
            if (project == null)
                return false;

            return true;
        }
        private void OnCurrentBuildProjectClicked(object sender, MouseButtonEventArgs e)
        {
            if (IsCurrentBuildProjectValid())
            {
                var presentationModel = PresentationModel;
                var project = presentationModel.SelectedColony.Shipyard.BuildSlots[0].Project;

                var command = presentationModel.CancelBuildProjectCommand;
                if ((command != null) && command.CanExecute(project))
                    command.Execute(project);
            }
        }
        //void OnClickBuyButton(object sender, RoutedEventArgs e)
        //{
        //    if (IsCurrentBuildProjectValid())
        //    {
        //        var presentationModel = PresentationModel;
        //        var project = presentationModel.SelectedColony.BuildSlots[0].Project;

        //        var command = presentationModel.BuyBuildProjectCommand;
        //        if ((command != null) && command.CanExecute(project))
        //            command.Execute(project);
        //    }
        //}

        //private void OnBuildQueueItemClicked(object sender, object clickedItem)
        //{
        //    if (IsCurrentBuildProjectValid())
        //    {
        //        var buildQueueItem = clickedItem as BuildQueueItem;
        //        if (buildQueueItem == null)
        //            return;

        //        var presentationModel = PresentationModel;
        //        if (presentationModel == null)
        //            return;

        //        var command = presentationModel.RemoveFromShipyardBuildQueueCommand;
        //        if ((command == null) || !command.CanExecute(buildQueueItem))
        //            return;

        //        command.Execute(buildQueueItem);
        //    }
        //}
    }
}
