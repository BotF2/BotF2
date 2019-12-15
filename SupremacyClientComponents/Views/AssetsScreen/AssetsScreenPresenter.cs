using Microsoft.Practices.Composite.Presentation.Events;
using Microsoft.Practices.Unity;
using Supremacy.Annotations;
using Supremacy.Client.Context;
using Supremacy.Client.Events;
using Supremacy.Utility;

namespace Supremacy.Client.Views
{
    public class AssetsScreenPresenter : GameScreenPresenterBase<AssetsScreenPresentationModel, IAssetsScreenView>, IAssetsScreenPresenter
    {
        public AssetsScreenPresenter([NotNull] IUnityContainer container, [NotNull] AssetsScreenPresentationModel model, [NotNull] IAssetsScreenView view)
            : base(container, model, view) { }

        #region Overrides of GameScreenPresenterBase<AssetsScreenPresentationModel,IAssetsScreenView>

        protected override string ViewName
        {
            get { return StandardGameScreens.IntelScreen; }
        }

        protected override void RegisterCommandAndEventHandlers()
        {
            ClientEvents.TurnStarted.Subscribe(OnTurnStarted, ThreadOption.UIThread);
        }

        protected override void UnregisterCommandAndEventHandlers()
        {
            ClientEvents.TurnStarted.Unsubscribe(OnTurnStarted);
        }

        protected override void RunOverride()
        {
            base.RunOverride();
            Update();
        }

        protected override void TerminateOverride()
        {
            base.TerminateOverride();
            Model.Colonies = null;
            Model.SpiedOneColonies = null;
            Model.SpiedTwoColonies = null;
            Model.SpiedThreeColonies = null;
            Model.SpiedFourColonies = null;
            Model.SpiedFiveColonies = null;
            Model.SpiedSixColonies = null;
        }

        #endregion

        #region Event Handlers

        private void OnTurnStarted(GameContextEventArgs obj)
        {
            Update();
        }

        private void Update()
        {
            GameLog.Core.UI.DebugFormat("AssetsScreenPresenter Update at line 61");
            Model.Colonies = AppContext.LocalPlayerEmpire.Colonies;
            //Model.SpiedOneColonies = DesignTimeAppContext.Instance.SpiedOneEmpire.Colonies;
            Model.SpiedOneColonies = DesignTimeObjects.SpiedCivOne.Colonies;
            Model.SpiedTwoColonies = DesignTimeObjects.SpiedCivTwo.Colonies;
            Model.SpiedThreeColonies = DesignTimeObjects.SpiedCivThree.Colonies;
            Model.SpiedFourColonies = DesignTimeObjects.SpiedCivFour.Colonies;
            Model.SpiedFiveColonies = DesignTimeObjects.SpiedCivFive.Colonies;
            Model.SpiedSixColonies = DesignTimeObjects.SpiedCivSix.Colonies;
        }
        #endregion
    }
}