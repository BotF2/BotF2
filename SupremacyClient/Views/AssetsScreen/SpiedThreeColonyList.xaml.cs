using System.Windows.Automation.Peers;

namespace Supremacy.Client.Views
{
    public partial class SpiedThreeColonyList
    {
        public SpiedThreeColonyList()
        {
            InitializeComponent();
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return null;
        }
    }
}