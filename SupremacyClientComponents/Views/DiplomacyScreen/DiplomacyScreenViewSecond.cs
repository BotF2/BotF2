using System.Windows;

using Microsoft.Practices.Unity;

using Supremacy.Annotations;

namespace Supremacy.Client.Views
{
    public class DiplomacyScreenViewSecond : GameScreenView<ViewModelBase<IDiplomacyScreenViewSecond, DiplomacyScreenViewModel>>
    {

        static DiplomacyScreenViewSecond()
        {

            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(DiplomacyScreenViewSecond),
                new FrameworkPropertyMetadata(typeof(DiplomacyScreenViewSecond)));
        }

        protected DiplomacyScreenViewSecond([NotNull] IUnityContainer container)
            : base(container) { }
    }
}