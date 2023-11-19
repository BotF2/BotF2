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
            Model.Spied_0_Colonies = null;
            Model.Spied_1_Colonies = null;
            Model.Spied_2_Colonies = null;
            Model.Spied_3_Colonies = null;
            Model.Spied_4_Colonies = null;
            Model.Spied_5_Colonies = null;
            Model.Spied_6_Colonies = null;
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
            Model.Spied_0_Colonies = DesignTimeObjects.SpiedCiv_0.Colonies;
            Model.Spied_1_Colonies = DesignTimeObjects.SpiedCiv_1.Colonies;
            Model.Spied_2_Colonies = DesignTimeObjects.SpiedCiv_2.Colonies;
            Model.Spied_3_Colonies = DesignTimeObjects.SpiedCiv_3.Colonies;
            Model.Spied_4_Colonies = DesignTimeObjects.SpiedCiv_4.Colonies;
            Model.Spied_5_Colonies = DesignTimeObjects.SpiedCiv_5.Colonies;
            Model.Spied_6_Colonies = DesignTimeObjects.SpiedCiv_6.Colonies;
        }
        #endregion
    }
}