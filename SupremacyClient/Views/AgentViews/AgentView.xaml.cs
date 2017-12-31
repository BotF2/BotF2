using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Supremacy.Personnel;
using Supremacy.Game;

namespace Supremacy.Client.Views
{
    public partial class AgentView
    {
        public static readonly DependencyProperty CornerRadiusProperty;
        public static readonly DependencyProperty RecallAgentCommandProperty;
        public static readonly DependencyProperty CancelAgentRecallCommandProperty;

        public static readonly RoutedCommand RecallCommand;

        static AgentView()
        {
            CornerRadiusProperty = Border.CornerRadiusProperty.AddOwner(
                typeof(AgentView),
                new FrameworkPropertyMetadata(new CornerRadius(8d)));

            RecallAgentCommandProperty = DependencyProperty.Register(
                "RecallAgentCommand",
                typeof(ICommand),
                typeof(AgentView));

            CancelAgentRecallCommandProperty = DependencyProperty.Register(
                "CancelAgentRecallCommand",
                typeof(ICommand),
                typeof(AgentView));

            RecallCommand = new RoutedCommand("Recall", typeof(AgentView));
        }

        public AgentView()
        {
            InitializeComponent();
            CommandBindings.Add(new CommandBinding(RecallCommand, ExecuteRecallEnvoyCommand, CanExecuteRecallEnvoyCommand));
        }

        private void ExecuteRecallEnvoyCommand(object sender, ExecutedRoutedEventArgs e)
        {
            Agent curAgent = e.Parameter as Agent;
            if (curAgent == null)
                return;

            PlayerOperations.CancelAgentMission(curAgent);
        }

        private void CanExecuteRecallEnvoyCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            Agent curAgent = e.Parameter as Agent;
            if (curAgent == null)
            {
                e.CanExecute = false;
                return;
            }

            e.CanExecute = curAgent.HasMission && curAgent.Mission.CanCancel;
        }

        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        public ICommand CancelAgentRecallCommand
        {
            get { return (ICommand)GetValue(CancelAgentRecallCommandProperty); }
            set { SetValue(CancelAgentRecallCommandProperty, value); }
        }
    }
}
