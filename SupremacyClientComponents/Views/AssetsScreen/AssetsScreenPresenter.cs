using Microsoft.Practices.Composite.Presentation.Events;
using Microsoft.Practices.Unity;
using Supremacy.Annotations;
using Supremacy.Client.Context;
using Supremacy.Client.Events;
using Supremacy.Intelligence;

namespace Supremacy.Client.Views
{
    public class AssetsScreenPresenter : GameScreenPresenterBase<AssetsScreenPresentationModel, IAssetsScreenView>, IAssetsScreenPresenter
    {
        public AssetsScreenPresenter([NotNull] IUnityContainer container, [NotNull] AssetsScreenPresentationModel model, [NotNull] IAssetsScreenView view)
            : base(container, model, view) { }

        #region Overrides of GameScreenPresenterBase<AssetsScreenPresentationModel,IAssetsScreenView>

        protected override string ViewName => StandardGameScreens.IntelScreen;

        protected override void RegisterCommandAndEventHandlers()
        {
            _ = ClientEvents.TurnStarted.Subscribe(OnTurnStarted, ThreadOption.UIThread);
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
            Model.SpiedZeroColonies = null;
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
            //GameLog.Core.Test.DebugFormat("Update on Turn Started at line 61");
            Model.Colonies = IntelHelper.LocalCivManager.Colonies;
            Model.SpiedZeroColonies = DesignTimeObjects.SpiedCiv_0.Colonies;
            Model.SpiedOneColonies = DesignTimeObjects.SpiedCiv_1.Colonies;
            Model.SpiedTwoColonies = DesignTimeObjects.SpiedCiv_2.Colonies;
            Model.SpiedThreeColonies = DesignTimeObjects.SpiedCiv_3.Colonies;
            Model.SpiedFourColonies = DesignTimeObjects.SpiedCiv_4.Colonies;
            Model.SpiedFiveColonies = DesignTimeObjects.SpiedCiv_5.Colonies;
            Model.SpiedSixColonies = DesignTimeObjects.SpiedCiv_6.Colonies;
        }
        #endregion
    }
}