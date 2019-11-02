using System.Windows.Automation.Peers;

namespace Supremacy.Client.Views
{
    public partial class SpiedFiveColonyList
    {
        public SpiedFiveColonyList()
        {
            InitializeComponent();
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return null;
        }
    }
}