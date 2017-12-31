using System.Windows;

using Microsoft.Practices.Unity;

using Supremacy.Annotations;

namespace Supremacy.Client.Views
{
    public sealed class ViewModelPresenter<TViewModel, TView> : GameScreenPresenterBase<TViewModel, TView>
        where TViewModel : ViewModelBase<TView, TViewModel>
        where TView : class, IGameScreenView<TViewModel>
    {
        public ViewModelPresenter([NotNull] IUnityContainer container, [NotNull] TViewModel model, [NotNull] TView view)
            : base(container, model, view)
        {
            model.Presenter = this;
            model.View = view;
        }

        protected override string ViewName
        {
            get { return Model.ViewName; }
        }

        protected override void RegisterViewWithRegion()
        {
            Model.RegisterViewWithRegion();
        }

        protected override void UnregisterViewWithRegion()
        {
            Model.UnregisterViewWithRegion();
        }

        protected override void SetInteractionNode()
        {
            var viewElement = View as DependencyObject;
            if (viewElement != null)
                Views.View.SetInteractionNode(viewElement, Model);
        }

        protected override void RunOverride()
        {
            Model.RunCore();
        }

        protected override void TerminateOverride()
        {
            Model.TerminateCore();
        }
    }
}