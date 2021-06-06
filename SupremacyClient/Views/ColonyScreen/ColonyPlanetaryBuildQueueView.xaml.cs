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
            BuildQueueItem buildQueueItem = clickedItem as BuildQueueItem;
            if (buildQueueItem == null)
                return;

            ColonyScreenPresentationModel presentationModel = PresentationModel;
            if (presentationModel == null)
                return;

            ICommand command = presentationModel.RemoveFromPlanetaryBuildQueueCommand;
            if ((command != null) && command.CanExecute(buildQueueItem))
                command.Execute(buildQueueItem);
        }

        private ColonyScreenPresentationModel PresentationModel => DataContext as ColonyScreenPresentationModel;

        bool IsCurrentBuildProjectValid()
        {
            ColonyScreenPresentationModel presentationModel = PresentationModel;
            if (presentationModel == null)
                return false;

            Universe.Colony colony = presentationModel.SelectedColony;
            if (colony == null)
                return false;

            BuildProject project = colony.BuildSlots[0].Project;
            if (project == null)
                return false;

            return true;
        }

        private void OnCurrentBuildProjectClicked(object sender, MouseButtonEventArgs e)
        {
            if (IsCurrentBuildProjectValid())
            {
                ColonyScreenPresentationModel presentationModel = PresentationModel;
                BuildProject project = presentationModel.SelectedColony.BuildSlots[0].Project;

                ICommand command = presentationModel.CancelBuildProjectCommand;
                if ((command != null) && command.CanExecute(project))
                    command.Execute(project);
            }
        }

        void OnClickBuyButton(object sender, RoutedEventArgs e)
        {
            if (IsCurrentBuildProjectValid())
            {
                ColonyScreenPresentationModel presentationModel = PresentationModel;
                BuildProject project = presentationModel.SelectedColony.BuildSlots[0].Project;

                ICommand command = presentationModel.BuyBuildProjectCommand;
                if ((command != null) && command.CanExecute(project))
                    command.Execute(project);
            }
        }
    }
}
