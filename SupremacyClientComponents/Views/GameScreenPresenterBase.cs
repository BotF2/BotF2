using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;

using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Composite.Regions;
using Microsoft.Practices.Unity;

using Supremacy.Annotations;
using Supremacy.Client.Commands;
using Supremacy.Client.Events;
using Supremacy.Resources;

using CompositeRegionManager = Microsoft.Practices.Composite.Presentation.Regions.RegionManager;
using Supremacy.Client.Context;

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
        //private readonly IPlayerTarget1Service _playerTarget1Service;
        //private readonly IPlayerTarget2Service _playerTarget2Service;
        private readonly TView _view;
        private readonly EventHandler _commandManagerInvalidateRequeryHandler;
        #endregion

        #region Constructors and Finalizers
        protected GameScreenPresenterBase(
            [NotNull] IUnityContainer container,
            [NotNull] TPresentationModel model,
            [NotNull] TView view)
        {
            if (ReferenceEquals(model, null))
                throw new ArgumentNullException("model");
            if (ReferenceEquals(view, null))
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
        protected IPlayerOrderService PlayerOrderService
        {
            get { return _playerOrderService; }
        }

        //protected IPlayerTarget1Service PlayerTarget1Service
        //{
        //    get { return _playerTarget1Service; }
        //}

        //protected IPlayerTarget2Service PlayerTarget2Service
        //{
        //    get { return _playerTarget2Service; }
        //}

        [NotNull]
        protected INavigationCommandsProxy NavigationCommands
        {
            get { return _navigationCommands; }
        }

        [NotNull]
        protected IAppContext AppContext
        {
            get { return _appContext; }
        }

        [NotNull]
        public TPresentationModel Model
        {
            get { return _model; }
        }

        [NotNull]
        protected IResourceManager ResourceManager
        {
            get { return _resourceManager; }
        }

        [NotNull]
        protected IRegionManager RegionManager
        {
            get
            {
                var view = View as DependencyObject;
                if (view != null)
                {
                    var regionManager = CompositeRegionManager.GetRegionManager(view);
                    if (regionManager != null)
                        return regionManager;
                }
                return _regionManager;
            }
        }

        [NotNull]
        protected IEventAggregator EventAggregator
        {
            get { return _eventAggregator; }
        }

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
        }

        protected virtual void UnregisterViewWithRegion()
        {
            _regionManager.Regions[ClientRegions.GameScreens].Remove(View);
        }

        protected virtual void SetInteractionNode()
        {
            var viewElement = View as DependencyObject;
            if (viewElement != null)
                Views.View.SetInteractionNode(viewElement, this);
        }

        private void OnCommandManagerRequerySuggested(object sender, EventArgs e)
        {
            InvalidateCommands();
        }

        protected virtual void InvalidateCommands() {}
        protected virtual void RegisterCommandAndEventHandlers() {}
        protected virtual void UnregisterCommandAndEventHandlers() {}

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

        protected virtual void OnViewActivating() {}
        protected virtual void OnViewDeactivating() {}

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

            View.Model = default(TPresentationModel);
            View.AppContext = null;

            var animationsHost = View as IAnimationsHost;
            if (animationsHost != null)
                animationsHost.StopAnimations();

            IsRunning = false;
        }

        protected virtual void ClearInteractionNode()
        {
            var viewElement = View as DependencyObject;
            if (viewElement != null)
                viewElement.ClearValue(Views.View.InteractionNodeProperty);
        }

        protected virtual void RemoveNestedViews()
        {
            var scopedRegionManager = CompositeRegionManager.GetRegionManager(View as DependencyObject);
            if (scopedRegionManager == null)
                return;

            foreach (var nestedRegion in scopedRegionManager.Regions.ToList())
            {
                foreach (var nestedView in nestedRegion.Views.ToList())
                {
                    var animationsHost = nestedView as IAnimationsHost;
                    if (animationsHost != null)
                        animationsHost.StopAnimations();
                    nestedRegion.Remove(nestedView);
                }
                scopedRegionManager.Regions.Remove(nestedRegion.Name);
            }
        }
        #endregion

        #region IGameScreenPresenter<TPresentationModel,TView> Implementation
        [NotNull]
        public TView View
        {
            get { return _view; }
        }
        #endregion

        #region Public and Protected Methods
        protected virtual void RunOverride() {}
        protected virtual void TerminateOverride() {}

        protected void ShowEndOfTurnSummary()
        {
            if (View.IsActive)
                ClientCommands.ShowEndOfTurnSummary.Execute(null);
        }

        protected virtual IInteractionNode FindParentInteractionNode()
        {
            var view = View as DependencyObject;
            if (view == null)
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

        object IInteractionNode.UIElement
        {
            get { return View; }
        }

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
            : base(container, model, view) {}
        #endregion

        protected override string ViewName
        {
            get { return StandardGameScreens.DiplomacyScreen; }
        }
    }

    public class ScienceScreenPresenter
        : GameScreenPresenterBase<ScienceScreenPresentationModel, IScienceScreenView>, IScienceScreenPresenter
    {
        #region Constructors and Finalizers
        public ScienceScreenPresenter(
            [NotNull] IUnityContainer container,
            [NotNull] ScienceScreenPresentationModel model,
            [NotNull] IScienceScreenView view)
            : base(container, model, view) {}
        #endregion

        protected override string ViewName
        {
            get { return StandardGameScreens.ScienceScreen; }
        }
    }

    public class PersonnelScreenPresenter
        : GameScreenPresenterBase<PersonnelScreenPresentationModel, IPersonnelScreenView>, IPersonnelScreenPresenter
    {
        #region Constructors and Finalizers
        public PersonnelScreenPresenter(
            [NotNull] IUnityContainer container,
            [NotNull] PersonnelScreenPresentationModel model,
            [NotNull] IPersonnelScreenView view)
            : base(container, model, view) {}
        #endregion

        protected override string ViewName
        {
            get { return StandardGameScreens.PersonnelScreen; }
        }
    }
}