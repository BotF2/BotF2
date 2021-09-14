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
            {
                return;
            }

            GameLog.Client.ShipProductionDetails.DebugFormat("BuildList doubleclicked", e.ClickCount);
            if (!(BuildList.SelectedItem is BuildProject selectedProject))
            {
                return;
            }


            ColonyScreenPresentationModel presentationModel = PresentationModel;
            if (presentationModel == null)
            {
                return;
            }

            //if (this)

            int _howMany = 5;

            ICommand command = presentationModel.AddToShipyardBuildQueueCommand;

            if ((command != null) && command.CanExecute(selectedProject))
            {
                for (int i = 0; i < _howMany; i++)
                {
                    command.Execute(selectedProject);

                }
            }
        }

        private ColonyScreenPresentationModel PresentationModel => DataContext as ColonyScreenPresentationModel;
    }
}
