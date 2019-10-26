using System.Windows.Automation.Peers;

namespace Supremacy.Client.Views
{
    public partial class SpiedTwoColonyList
    {
        public SpiedTwoColonyList()
        {
            InitializeComponent();
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return null;
        }
    }
}