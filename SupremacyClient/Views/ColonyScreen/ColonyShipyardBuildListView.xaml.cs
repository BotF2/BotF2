using System.Windows.Input;

using Supremacy.Economy;
using Supremacy.Utility;

namespace Supremacy.Client.Views
{
    /// <summary>
    /// Interaction logic for ColonyShipyardBuildListView.xaml
    /// </summary>
    public partial class ColonyShipyardBuildListView
    {
        public ColonyShipyardBuildListView()
        {
            InitializeComponent();
        }

        private void OnBuildListPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount < 2)
                return;
            GameLog.Client.ShipProduction.DebugFormat("this is a gamelog ={0}", e.ClickCount);
            BuildProject selectedProject = BuildList.SelectedItem as BuildProject;
            if (selectedProject == null)
                return;

            ColonyScreenPresentationModel presentationModel = PresentationModel;
            if (presentationModel == null)
                return;

            ICommand command = presentationModel.AddToShipyardBuildQueueCommand;
            if ((command != null) && command.CanExecute(selectedProject))
                command.Execute(selectedProject);
        }

        private ColonyScreenPresentationModel PresentationModel => DataContext as ColonyScreenPresentationModel;
    }
}
