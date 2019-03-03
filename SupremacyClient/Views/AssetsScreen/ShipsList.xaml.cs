using System.Windows.Automation.Peers;

namespace Supremacy.Client.Views
{
    public partial class ShipsList
    {
        public ShipsList()
        {
            InitializeComponent();
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return null;
        }
    }
}
