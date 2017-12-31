using System.Windows;

using Microsoft.Practices.Unity;

using Supremacy.Annotations;

namespace Supremacy.Client.Views
{
    public class NewDiplomacyScreenView : GameScreenView<ViewModelBase<INewDiplomacyScreenView, DiplomacyScreenViewModel>>
    {

        static NewDiplomacyScreenView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(NewDiplomacyScreenView),
                new FrameworkPropertyMetadata(typeof(NewDiplomacyScreenView)));
        }

        protected NewDiplomacyScreenView([NotNull] IUnityContainer container)
            : base(container) {}
    }
}