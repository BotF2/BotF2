using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Composite.Regions;
using Microsoft.Practices.Unity;
using Supremacy.Annotations;
using Supremacy.Client.Commands;
using Supremacy.Client.Context;
using Supremacy.Client.Events;
using Supremacy.Resources;
using Supremacy.Utility;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CompositeRegionManager = Microsoft.Practices.Composite.Presentation.Regions.RegionManager;

namespace Supremacy.Client.Views
{
    public abstract class GameScreenPresenterBase<TPresentationModel, TView> : IGameScreenPresenter<TPresentationModel, TView>
        where TView : class, IGameScreenView<TPresentationModel>
    {
        #region Fields
        private readonly IUnityContainer _container;
        private readonly IAppContext _appContext;
        private readonly IEventAggregator _eventAggregator;
        private readonly TPresentationModel _model;
        private readonly IRegionManager _regionManager;
        private readonly IResourceManager _resourceManager;
        private readonly INavigationCommandsProxy _navigationCommands;
        private readonly IPlayerOrderService _playerOrderService;
        private readonly TView _view;
        private readonly EventHandler _commandManagerInvalidateRequeryHandler;
        #endregion

        #region Constructors and Finalizers
        protected GameScreenPresenterBase(
            [NotNull] IUnityContainer container,
            [NotNull] TPresentationModel model,
            [NotNull] TView view)
        {
            if (model == null)
                throw new ArgumentNullException("model");
            if (view is null)
                throw new ArgumentNullException("view");

            _container = container;
            _regionManager = _container.Resolve<IRegionManager>();
            _resourceManager = _container.Resolve<IResourceManager>();
            _eventAggregator = _container.Resolve<IEventAggregator>();
            _appContext = _container.Resolve<IAppContext>();
            _navigationCommands = _container.Resolve<INavigationCommandsProxy>();
            _playerOrderService = _container.Resolve<IPlayerOrderService>();

            _model = model;
            _view = view;

            _commandManagerInvalidateRequeryHandler = OnCommandManagerRequerySuggested;
        }
        #endregion

        #region Properties and Indexers
        protected bool IsRunning { get; private set; }

        [NotNull]
        protected IPlayerOrderService PlayerOrderService => _playerOrderService;

        [NotNull]
        protected INavigationCommandsProxy NavigationCommands => _navigationCommands;

        [NotNull]
        protected IAppContext AppContext => _appContext;

        [NotNull]
        public TPresentationModel Model => _model;

        [NotNull]
        protected IResourceManager ResourceManager => _resourceManager;

        [NotNull]
        protected IRegionManager RegionManager
        {
            get
            {
                if (View is DependencyObject view)
                {
                    IRegionManager regionManager = CompositeRegionManager.GetRegionManager(view);
                    if (regionManager != null)
                        return regionManager;
                }
                return _regionManager;
            }
        }

        [NotNull]
        protected IEventAggregator EventAggregator => _eventAggregator;

        [NotNull]
        protected abstract string ViewName { get; }
        #endregion

        #region Implementation of IGameScreenPresenter
        public void Run()
        {
            View.Model = _model;
            View.AppContext = _appContext;

            SetInteractionNode();

            View.OnCreated();

            IsRunning = true;
            View.IsActiveChanged += OnViewIsActiveChanged;

            RegisterViewWithRegion();

            CommandManager.RequerySuggested += _commandManagerInvalidateRequeryHandler;

            RegisterCommandAndEventHandlers();
            RunOverride();
        }

        protected virtual void RegisterViewWithRegion()
        {
            _regionManager.Regions[ClientRegions.GameScreens].Add(View, ViewName, true);
            string _text = "registering Screen " + ViewName;
            Console.WriteLine(_text);
            GameLog.Client.UI.InfoFormat(_text);
            if (ViewName == "ColonyScreen")
                GameLog.Client.UI.InfoFormat(_text);
        }

        protected virtual void UnregisterViewWithRegion()
        {
            _regionManager.Regions[ClientRegions.GameScreens].Remove(View);
        }

        protected virtual void SetInteractionNode()
        {
            if (View is DependencyObject viewElement)
                Views.View.SetInteractionNode(viewElement, this);
        }

        private void OnCommandManagerRequerySuggested(object sender, EventArgs e)
        {
            InvalidateCommands();
        }

        protected virtual void InvalidateCommands() { }
        protected virtual void RegisterCommandAndEventHandlers() { }
        protected virtual void UnregisterCommandAndEventHandlers() { }

        private void OnViewIsActiveChanged(object sender, EventArgs args)
        {
            if (!View.IsActive)
            {
                OnViewDeactivating();
                return;
            }
            OnViewActivating();
            ClientEvents.ScreenActivated.Publish(new ScreenActivatedEventArgs(ViewName));
        }

        protected virtual void OnViewActivating() { }
        protected virtual void OnViewDeactivating() { }

        public void Terminate()
        {
            CommandManager.RequerySuggested -= _commandManagerInvalidateRequeryHandler;

            View.OnDestroyed();

            ClearInteractionNode();

            RemoveNestedViews();

            View.IsActiveChanged -= OnViewIsActiveChanged;

            UnregisterViewWithRegion();

            UnregisterCommandAndEventHandlers();
            TerminateOverride();

            View.Model = default;
            View.AppContext = null;

            if (View is IAnimationsHost animationsHost)
                animationsHost.StopAnimations();

            IsRunning = false;
        }

        protected virtual void ClearInteractionNode()
        {
            if (View is DependencyObject viewElement)
                viewElement.ClearValue(Views.View.InteractionNodeProperty);
        }

        protected virtual void RemoveNestedViews()
        {
            IRegionManager scopedRegionManager = CompositeRegionManager.GetRegionManager(View as DependencyObject);
            if (scopedRegionManager == null)
                return;

            foreach (IRegion nestedRegion in scopedRegionManager.Regions.ToList())
            {
                foreach (object nestedView in nestedRegion.Views.ToList())
                {
                    if (nestedView is IAnimationsHost animationsHost)
                        animationsHost.StopAnimations();
                    nestedRegion.Remove(nestedView);
                }
                scopedRegionManager.Regions.Remove(nestedRegion.Name);
            }
        }
        #endregion

        #region IGameScreenPresenter<TPresentationModel,TView> Implementation
        [NotNull]
        public TView View => _view;
        #endregion

        #region Public and Protected Methods
        protected virtual void RunOverride() { }
        protected virtual void TerminateOverride() { }

        protected void ShowEndOfTurnSummary()
        {
            if (View.IsActive)
                ClientCommands.ShowEndOfTurnSummary.Execute(null);
        }

        //protected void ShowShipOverview()
        //{
        //    if (View.IsActive)
        //        ClientCommands.ShowShipOverview.Execute(null);
        //}

        protected virtual IInteractionNode FindParentInteractionNode()
        {
            if (!(View is DependencyObject view))
                return null;

            IInteractionNode ancestorNode = null;

            view.FindVisualAncestor(
                o =>
                {
                    ancestorNode = Views.View.GetInteractionNode(o);
                    return (ancestorNode != null);
                });

            if (ancestorNode == null)
            {
                view.FindLogicalAncestor(
                    o =>
                    {
                        ancestorNode = Views.View.GetInteractionNode(o);
                        return (ancestorNode != null);
                    });
            }

            return ancestorNode;
        }
        #endregion

        #region Implementation of IInteractionNode

        object IInteractionNode.UIElement => View;

        IInteractionNode IInteractionNode.FindParent()
        {
            return FindParentInteractionNode();
        }

        #endregion
    }

    public class DiplomacyScreenPresenter
        : GameScreenPresenterBase<DiplomacyScreenPresentationModel, IDiplomacyScreenView>, IDiplomacyScreenPresenter
    {
        #region Constructors and Finalizers
        public DiplomacyScreenPresenter(
            [NotNull] IUnityContainer container,
            [NotNull] DiplomacyScreenPresentationModel model,
            [NotNull] IDiplomacyScreenView view)
            : base(container, model, view) { }
        #endregion

        protected override string ViewName => StandardGameScreens.DiplomacyScreen;
    }

    public class ScienceScreenPresenter
        : GameScreenPresenterBase<ScienceScreenPresentationModel, IScienceScreenView>, IScienceScreenPresenter
    {
        #region Constructors and Finalizers
        public ScienceScreenPresenter(
            [NotNull] IUnityContainer container,
            [NotNull] ScienceScreenPresentationModel model,
            [NotNull] IScienceScreenView view)
            : base(container, model, view) { }
        #endregion

        protected override string ViewName => StandardGameScreens.ScienceScreen;
    }

    public class EncyclopediaScreenPresenter
    : GameScreenPresenterBase<EncyclopediaScreenPresentationModel, IEncyclopediaScreenView>, IEncyclopediaScreenPresenter
    {
        #region Constructors and Finalizers
        public EncyclopediaScreenPresenter(
            [NotNull] IUnityContainer container,
            [NotNull] EncyclopediaScreenPresentationModel model,
            [NotNull] IEncyclopediaScreenView view)
            : base(container, model, view) { }
        #endregion

        protected override string ViewName => StandardGameScreens.EncyclopediaScreen;
    }

    //public class IntelScreenPresenter
    //    : GameScreenPresenterBase<IntelScreenPresentationModel, IIntelScreenView>, IIntelScreenPresenter
    //{
    //    #region Constructors and Finalizers
    //    public IntelScreenPresenter(
    //        [NotNull] IUnityContainer container,
    //        [NotNull] IntelScreenPresentationModel model,
    //        [NotNull] IIntelScreenView view)
    //        : base(container, model, view) { }
    //    #endregion

    //    protected override string ViewName
    //    {
    //        get { return StandardGameScreens.IntelScreen; }
    //    }
    //}
}