using System.Windows.Automation.Peers;

namespace Supremacy.Client.Views
{
    public partial class SpiedSixColonyList
    {
        public SpiedSixColonyList()
        {
            InitializeComponent();
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return null;
        }
    }
}