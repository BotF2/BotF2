//File:GameScreenPresenterBase.cs
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Composite.Regions;
using Microsoft.Practices.Unity;
using Supremacy.Annotations;
using Supremacy.Client.Audio;
using Supremacy.Client.Commands;
using Supremacy.Client.Context;
using Supremacy.Client.Events;
using Supremacy.Resources;
using Supremacy.Utility;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;

//using System.Media;
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
        private readonly IRegionManager _regionManager;
        private readonly EventHandler _commandManagerInvalidateRequeryHandler;
        //private readonly ISoundPlayer _soundPlayer = null; // new Supremacy.Client.Audio.ISoundPlayer();

        //private ISoundPlayer _soundPlayer = null;
        #endregion

        #region Constructors and Finalizers
        protected GameScreenPresenterBase(
            [NotNull] IUnityContainer container,
            [NotNull] TPresentationModel model,
            [NotNull] TView view)
        {
            if (model == null)
            {
                throw new ArgumentNullException("model");
            }

            if (view is null)
            {
                throw new ArgumentNullException("view");
            }

            _container = container;
            _regionManager = _container.Resolve<IRegionManager>();
            ResourceManager = _container.Resolve<IResourceManager>();
            EventAggregator = _container.Resolve<IEventAggregator>();
            AppContext = _container.Resolve<IAppContext>();
            NavigationCommands = _container.Resolve<INavigationCommandsProxy>();
            PlayerOrderService = _container.Resolve<IPlayerOrderService>();

            Model = model;
            View = view;

            _commandManagerInvalidateRequeryHandler = OnCommandManagerRequerySuggested;
        }
        #endregion

        #region Properties and Indexers
        protected bool IsRunning { get; private set; }

        [NotNull]
        protected IPlayerOrderService PlayerOrderService { get; }

        [NotNull]
        protected INavigationCommandsProxy NavigationCommands { get; }

        [NotNull]
        protected IAppContext AppContext { get; }

        [NotNull]
        public TPresentationModel Model { get; }

        [NotNull]
        protected IResourceManager ResourceManager { get; }

        [NotNull]
        protected IRegionManager RegionManager
        {
            get
            {
                if (View is DependencyObject view)
                {
                    IRegionManager regionManager = CompositeRegionManager.GetRegionManager(view);
                    if (regionManager != null)
                    {
                        return regionManager;
                    }
                }
                return _regionManager;
            }
        }

        [NotNull]
        protected IEventAggregator EventAggregator { get; }

        [NotNull]
        protected abstract string ViewName { get; }
        #endregion

        #region Implementation of IGameScreenPresenter
        public void Run()
        {
            View.Model = Model;
            View.AppContext = AppContext;

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
            _ = _regionManager.Regions[ClientRegions.GameScreens].Add(View, ViewName, true);
            string _text = "registering Screen " + ViewName;
            //Console.WriteLine(_text);
            GameLog.Client.UIDetails.DebugFormat(_text);
            //if (ViewName == "ColonyScreen")
            //{
            //    GameLog.Client.UI.InfoFormat(_text);
            //}
        }

        protected virtual void UnregisterViewWithRegion()
        {
            _regionManager.Regions[ClientRegions.GameScreens].Remove(View);
        }

        protected virtual void SetInteractionNode()
        {
            if (View is DependencyObject viewElement)
            {
                Views.View.SetInteractionNode(viewElement, this);
            }
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
            //ISoundPlayer _soundPlayer = new Supremacy.Client.Audio.SoundPlayer(FMODAudioEngine.Instance, AppContext);
            IMusicPlayer _musicPlayer = new Supremacy.Client.Audio.MusicPlayer(FMODAudioEngine.Instance, AppContext);

            switch (ViewName)
            {
                case "GalaxyScreen":
                    PlayOGG("F1_ScreenMusic");
                    //PlayOGG("Resources\\SoundFX\\ScreenMusic\\F1_ScreenMusic.wav");
                    //_soundPlayer.PlayFile("Resources/SoundFX/ScreenMusic/F1_ScreenMusic.wav");
                    break;
                case "ColonyScreen":
                    PlayOGG("F2_ScreenMusic");
                    //_soundPlayer.PlayFile("Resources/SoundFX/ScreenMusic/F2_ScreenMusic.ogg");
                    break;
                case "ScienceScreen":
                    PlayOGG("F3_ScreenMusic");
                    //_soundPlayer.PlayFile("Resources/SoundFX/ScreenMusic/F3_ScreenMusic.ogg");
                    break;
                case "DiplomacyScreen":
                    PlayOGG("F4_ScreenMusic");
                    //_soundPlayer.PlayFile("Resources/SoundFX/ScreenMusic/F4_ScreenMusic.ogg");
                    break;
                case "IntelScreen":
                    PlayOGG("F5_ScreenMusic");
                    //_soundPlayer.PlayFile("Resources/SoundFX/ScreenMusic/F5_ScreenMusic.ogg");
                    break;
                case "F8-Screen":
                    PlayOGG("F8_ScreenMusic");
                    //_soundPlayer.PlayFile("Resources/SoundFX/ScreenMusic/F8_ScreenMusic.ogg");
                    break;
                default:
                    Debugger.Break();
                    break;
            }
   
        }

        private void PlayOGG(string _play_file)
        {
            //IMusicPlayer _musicPlayer = null;
            //ISoundPlayer _soundPlayer = new Supremacy.Client.Audio.SoundPlayer();
            //ISoundPlayer _soundPlayer = new Supremacy.Client.Audio.SoundPlayer(_play_file);
            //ISoundPlayer _soundPlayer = null;
            //IMusicPlayer _musicPlayer = new Supremacy.Client.Audio.MusicPlayer(FMODAudioEngine.Instance, AppContext);
            ISoundPlayer _soundPlayer = new Supremacy.Client.Audio.SoundPlayer(FMODAudioEngine.Instance, AppContext);



            //IMusicPlayer _musicPlayer = new Supremacy.Client.Audio.MusicPlayer(FMODAudioEngine.Instance, AppContext);
            //string _file = Path.Combine(Environment.CurrentDirectory, /*"@" + */_play_file);
            //if (File.Exists(_file))
            //{
            //_soundPlayer.PlayFile(_file);
            try
            {
                //_musicPlayer
                _soundPlayer.PlayAny(_play_file);

                //_musicPlayer.PlayMode = PlaybackMode.None | PlaybackMode.Fade;
                //_musicPlayer.SwitchMusic(_play_file/*+".ogg"*/);
                //_musicPlayer.Play();
                Console.WriteLine("Step_0611:; SwitchMusic to _play_file= > " + _play_file);

                //_musicPlayer.SwitchMusic("DefaultMusic");
                //_musicPlayer.Play();
                //Console.WriteLine("Step_0611:; SwitchMusic to _play_file= > " + _play_file);
            }
            catch
            {
                Debugger.Break();
            }

            //}
            //else
            //{
            //    Console.WriteLine("Step_0156:; non-existing file= " + _file);
            //    Debugger.Break();
            //}

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
            {
                animationsHost.StopAnimations();
            }

            IsRunning = false;
        }

        protected virtual void ClearInteractionNode()
        {
            if (View is DependencyObject viewElement)
            {
                viewElement.ClearValue(Views.View.InteractionNodeProperty);
            }
        }

        protected virtual void RemoveNestedViews()
        {
            IRegionManager scopedRegionManager = CompositeRegionManager.GetRegionManager(View as DependencyObject);
            if (scopedRegionManager == null)
            {
                return;
            }

            foreach (IRegion nestedRegion in scopedRegionManager.Regions.ToList())
            {
                foreach (object nestedView in nestedRegion.Views.ToList())
                {
                    if (nestedView is IAnimationsHost animationsHost)
                    {
                        animationsHost.StopAnimations();
                    }

                    nestedRegion.Remove(nestedView);
                }
                _ = scopedRegionManager.Regions.Remove(nestedRegion.Name);
            }
        }
        #endregion

        #region IGameScreenPresenter<TPresentationModel,TView> Implementation
        [NotNull]
        public TView View { get; }
        #endregion

        #region Public and Protected Methods
        protected virtual void RunOverride() { }
        protected virtual void TerminateOverride() { }

        protected void ShowEndOfTurnSummary()
        {
            if (View.IsActive)
            {
                ClientCommands.ShowEndOfTurnSummary.Execute(null);
            }
        }

        //protected void ShowShipOverview()
        //{
        //    if (View.IsActive)
        //        ClientCommands.ShowShipOverview.Execute(null);
        //}

        protected virtual IInteractionNode FindParentInteractionNode()
        {
            if (!(View is DependencyObject view))
            {
                return null;
            }

            IInteractionNode ancestorNode = null;

            _ = view.FindVisualAncestor(
                o =>
                {
                    ancestorNode = Views.View.GetInteractionNode(o);
                    return ancestorNode != null;
                });

            if (ancestorNode == null)
            {
                _ = view.FindLogicalAncestor(
                    o =>
                    {
                        ancestorNode = Views.View.GetInteractionNode(o);
                        return ancestorNode != null;
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