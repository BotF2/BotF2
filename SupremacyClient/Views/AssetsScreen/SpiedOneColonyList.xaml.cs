using System.Windows.Automation.Peers;

namespace Supremacy.Client.Views
{
    public partial class SpiedOneColonyList
    {
        public SpiedOneColonyList()
        {
            InitializeComponent();
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return null;
        }
    }
}