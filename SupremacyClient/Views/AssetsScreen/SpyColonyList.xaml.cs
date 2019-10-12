using System.Windows.Automation.Peers;

namespace Supremacy.Client.Views
{
    public partial class SpyColonyList
    {
        public SpyColonyList()
        {
            InitializeComponent();
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return null;
        }
    }
}