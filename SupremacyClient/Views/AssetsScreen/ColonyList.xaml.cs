using System.Windows.Automation.Peers;

namespace Supremacy.Client.Views
{
    public partial class ColonyList
    {
        public ColonyList()
        {
            InitializeComponent();
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return null;
        }
    }
}