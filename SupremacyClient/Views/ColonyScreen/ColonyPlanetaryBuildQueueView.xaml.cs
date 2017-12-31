using System.Windows.Input;

using Supremacy.Economy;
using System.Windows;

namespace Supremacy.Client.Views
{
    /// <summary>
    /// Interaction logic for ColonyPlanetaryBuildQueueView.xaml
    /// </summary>
    public partial class ColonyPlanetaryBuildQueueView
    {
        public ColonyPlanetaryBuildQueueView()
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

            var command = presentationModel.RemoveFromPlanetaryBuildQueueCommand;
            if ((command != null) && command.CanExecute(buildQueueItem))
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

            var project = colony.BuildSlots[0].Project;
            if (project == null)
                return false;

            return true;
        }

        private void OnCurrentBuildProjectClicked(object sender, MouseButtonEventArgs e)
        {
            if (IsCurrentBuildProjectValid())
            {
                var presentationModel = PresentationModel;
                var project = presentationModel.SelectedColony.BuildSlots[0].Project;

                var command = presentationModel.CancelBuildProjectCommand;
                if ((command != null) && command.CanExecute(project))
                    command.Execute(project);
            }
        }

        void OnClickBuyButton(object sender, RoutedEventArgs e)
        {
            if (IsCurrentBuildProjectValid())
            {
                var presentationModel = PresentationModel;
                var project = presentationModel.SelectedColony.BuildSlots[0].Project;

                var command = presentationModel.BuyBuildProjectCommand;
                if ((command != null) && command.CanExecute(project))
                    command.Execute(project);
            }
        }
    }
}
