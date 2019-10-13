using Microsoft.Practices.Composite.Presentation.Events;
using Microsoft.Practices.Unity;
using Supremacy.Annotations;
using Supremacy.Client.Context;
using Supremacy.Client.Events;

namespace Supremacy.Client.Views
{
    public class AssetsScreenPresenter : GameScreenPresenterBase<AssetsScreenPresentationModel, IAssetsScreenView>, IAssetsScreenPresenter
    {
        public AssetsScreenPresenter([NotNull] IUnityContainer container, [NotNull] AssetsScreenPresentationModel model, [NotNull] IAssetsScreenView view)
            : base(container, model, view) {}

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
            Model.SpyColonies = null;
        }

        #endregion

        #region Event Handlers

        private void OnTurnStarted(GameContextEventArgs obj)
        {
            Update();
        }

        private void Update()
        {
            Model.Colonies = AppContext.LocalPlayerEmpire.Colonies;
            Model.SpyColonies = DesignTimeAppContext.Instance.SpyEmpire.Colonies;
        }



        #endregion
    }
}