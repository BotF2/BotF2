using System.Windows.Automation.Peers;

namespace Supremacy.Client.Views
{
    public partial class SpiedZeroColonyList
    {
        public SpiedZeroColonyList()
        {
            InitializeComponent();
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return null;
        }
    }
}