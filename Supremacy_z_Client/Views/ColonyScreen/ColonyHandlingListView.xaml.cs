using System;
using System.ComponentModel;
using System.Windows;

using Microsoft.Practices.Unity;

using Supremacy.Annotations;
using Supremacy.Client.Commands;
using Supremacy.Client.Input;
using Supremacy.Diplomacy;
using Supremacy.Game;
using Supremacy.Universe;
using Supremacy.Client.Views.GalaxyScreen;
using Supremacy.Client.Context;
using Supremacy.Client.Dialogs;
using System.IO;
using Supremacy.Resources;

namespace Supremacy.Client.Views
{
    /// <summary>
    /// Interaction logic for ColonyHandlingListView.xaml
    /// </summary>
    public partial class ColonyHandlingListView : IWeakEventListener
    {
        private readonly IUnityContainer _container;
        private readonly IAppContext _appContext;
        private readonly INavigationCommandsProxy _navigationCommands;
        private readonly DelegateCommand<object> _showBuildingsCommand;
        private readonly DelegateCommand<object> _showBuildListCommand;
        private readonly DelegateCommand<object> _showShipyardCommand;
        public ColonyHandlingListView([NotNull] IUnityContainer container)
        {
            _container = container ?? throw new ArgumentNullException("container");
            _appContext = _container.Resolve<IAppContext>();
            _navigationCommands = _container.Resolve<INavigationCommandsProxy>();

            InitializeComponent();

            _showBuildingsCommand = new DelegateCommand<object>(ExecuteShowBuildingsCommand);
            _showBuildListCommand = new DelegateCommand<object>(ExecuteShowBuildListCommand);
            _showShipyardCommand = new DelegateCommand<object>(ExecuteShowShipyardCommand);

            //DebugCommands.ShowBuildings.RegisterCommand(_showBuildingsCommand);
            //DebugCommands.ShowBuildList.RegisterCommand(_showBuildListCommand);
            //DebugCommands.ShowShipyard.RegisterCommand(_showShipyardCommand);
        }

        private void OnUnloaded(object sender, RoutedEventArgs args)
        {
            DebugCommands.ShowBuildings.UnregisterCommand(_showBuildingsCommand);
            DebugCommands.ShowBuildList.UnregisterCommand(_showBuildListCommand);
            DebugCommands.ShowShipyard.UnregisterCommand(_showShipyardCommand);
        }

        private void ExecuteShowBuildingsCommand(object t)
        {
            this.TabIndex = 0;
        }

        private void ExecuteShowBuildListCommand(object t)
        {
            TabIndex = 1;
        }

        private void ExecuteShowShipyardCommand(object t)
        {
            TabIndex = 2;
        }

        public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (!(sender is IAppContext appContext))
            {
                return false;
            }

            if (!(e is PropertyChangedEventArgs propertyChangedEventArgs))
            {
                return false;
            }

            return true;
        }
    }
}
