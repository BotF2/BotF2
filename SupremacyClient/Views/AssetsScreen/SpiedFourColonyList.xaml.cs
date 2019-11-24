using System.Windows.Automation.Peers;

namespace Supremacy.Client.Views
{
    public partial class SpiedFourColonyList
    {
        public SpiedFourColonyList()
        {
            InitializeComponent();
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return null;
        }
    }
}