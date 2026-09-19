using System.Windows.Input;

using Supremacy.Economy;

namespace Supremacy.Client.Views
{
    /// <summary>
    /// Interaction logic for ColonyPlanetaryBuildListView.xaml
    /// </summary>
    public partial class ColonyPlanetaryBuildListView
    {
        public ColonyPlanetaryBuildListView()
        {
            InitializeComponent();
        }

        private void OnBuildListPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount < 2)
            {
                return;
            }

            if (!(BuildList.SelectedItem is BuildProject selectedProject))
            {
                return;
            }

            ColonyScreenPresentationModel presentationModel = PresentationModel;
            if (presentationModel == null)
            {
                return;
            }

            ICommand command = presentationModel.AddToPlanetaryBuildQueueCommand;
            if (command != null && command.CanExecute(selectedProject))
            {
                command.Execute(selectedProject);
            }
        }

        private ColonyScreenPresentationModel PresentationModel => DataContext as ColonyScreenPresentationModel;
    }
}
