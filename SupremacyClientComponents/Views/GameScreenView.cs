using System;
using System.Windows;
using System.Windows.Controls;

using Microsoft.Practices.Composite.Presentation.Events;
using Microsoft.Practices.Unity;

using Supremacy.Annotations;
using Supremacy.Client.Commands;
using Supremacy.Client.Events;
using Supremacy.Resources;

using System.Linq;

using Supremacy.Client.Context;
using Supremacy.Client.Themes;
using Supremacy.Utility;

namespace Supremacy.Client.Views
{
    public abstract class GameScreenViewBase : Control, IAnimationsHost
    {
        #region Dependency Properties
        public static readonly DependencyProperty AppContextProperty;
        #endregion

        #region Constructors and Finalizers
        static GameScreenViewBase()
        {
            AppContextProperty = DependencyProperty.Register(
                "AppContext",
                typeof(IAppContext),
                typeof(UIElement),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.Inherits));
        }

        protected GameScreenViewBase()
        {
            InjectThemeResources();
        }

        protected void InjectThemeResources()
        {
            ResourceDictionary themeResources;

            if (ThemeHelper.TryLoadThemeResources(out themeResources))
                Resources.MergedDictionaries.Add(themeResources);
        }

        #endregion

        #region Properties and Indexers
        public IAppContext AppContext
        {
            get { return GetValue(AppContextProperty) as IAppContext; }
            set { SetValue(AppContextProperty, value); }
        }
        #endregion

        #region Public and Protected Methods
        public static IAppContext GetAppContext(DependencyObject element)
        {
            if (element == null)
                return null;
            return element.GetValue(AppContextProperty) as IAppContext;
        }

        public static void SetAppContext(DependencyObject element, IAppContext value)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            element.SetValue(AppContextProperty, value);
        }
        #endregion

        protected void PauseAnimations()
        {
            foreach (IAnimationsHost animationsHost in this.FindVisualDescendantsByType<DependencyObject>().OfType<IAnimationsHost>())
            {
                try
                {
                    animationsHost.PauseAnimations();
                }
                catch (Exception e)
                {
                    GameLog.Client.General.Error(e);
                }
            }
        }

        #region Implementation of IAnimationsHost

        void IAnimationsHost.PauseAnimations()
        {
            PauseAnimations();
        }

        void IAnimationsHost.ResumeAnimations()
        {
            ResumeAnimations();
        }

        void IAnimationsHost.StopAnimations()
        {
            StopAnimations();
        }

        #endregion

        protected void ResumeAnimations()
        {
            foreach (IAnimationsHost animationsHost in this.FindVisualDescendantsByType<DependencyObject>().OfType<IAnimationsHost>())
            {
                try
                {
                    animationsHost.ResumeAnimations();
                }
                catch (Exception e)
                {
                    GameLog.Client.General.Error(e);
                }
            }
        }

        protected void StopAnimations()
        {
            foreach (IAnimationsHost animationsHost in this.FindVisualDescendantsByType<DependencyObject>().OfType<IAnimationsHost>())
            {
                try
                {
                    animationsHost.ResumeAnimations();
                }
                catch (Exception e)
                {
                    GameLog.Client.General.Error(e);
                }
            }
        }
    }

    public class GameScreenView<TPresentationModel> : GameScreenViewBase, IGameScreenView<TPresentationModel>
        where TPresentationModel : class
    {
        #region Fields
        private readonly IUnityContainer _container;
        private readonly IPlayerOrderService _playerOrderService;
        private readonly INavigationCommandsProxy _navigationCommands;
        private readonly IResourceManager _resourceManager;

        private bool _isActive;
        #endregion

        #region Constructors and Finalizers
        protected GameScreenView([NotNull] IUnityContainer container)
        {
            if (container == null)
                throw new ArgumentNullException("container");

            _container = container;
            _playerOrderService = _container.Resolve<IPlayerOrderService>();
            _navigationCommands = _container.Resolve<INavigationCommandsProxy>();
            _resourceManager = _container.Resolve<IResourceManager>();

            IsVisibleChanged += OnIsVisibleChanged;
        }
        #endregion

        #region Properties and Indexers
        protected IUnityContainer Container => _container;

        protected INavigationCommandsProxy NavigationCommands => _navigationCommands;

        protected IResourceManager ResourceManager => _resourceManager;
        #endregion

        #region Public and Protected Methods
        protected virtual void OnHide() { }

        protected virtual void OnIsActiveChanged()
        {
            EventHandler handler = IsActiveChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        protected virtual void OnShow() { }
        #endregion

        #region Implementation of IActiveAware
        public event EventHandler IsActiveChanged;

        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                if (Equals(_isActive, value))
                    return;
                _isActive = value;
                UpdateCommands();
                OnIsActiveChanged();
            }
        }
        #endregion

        #region Implementation of IGameScreenView
        public TPresentationModel Model
        {
            get { return DataContext as TPresentationModel; }
            set { DataContext = value; }
        }

        protected IPlayerOrderService PlayerOrderService => _playerOrderService;

        public virtual void OnCreated()
        {
            OnCreatedOverride();
            RegisterCommandAndEventHandlers();
        }

        public virtual void OnDestroyed()
        {
            UnregisterCommandAndEventHandlers();
            OnDestroyedOverride();
        }
        #endregion

        #region Protected Methods
        protected virtual void RegisterCommandAndEventHandlers() { }
        protected virtual void UnregisterCommandAndEventHandlers() { }

        protected virtual void OnCreatedOverride() { }
        protected virtual void OnDestroyedOverride() { }

        protected virtual void UpdateCommands() { }
        #endregion

        #region Private Methods
        private void OnHideCore()
        {
            ClientEvents.AllTurnEnded.Unsubscribe(OnAllTurnEnded);
            ClientEvents.TurnStarted.Unsubscribe(OnTurnStarted);
            PauseAnimations();
            OnHide();
        }

        private void OnTurnStarted(ClientEventArgs obj)
        {
            OnTurnStarted();
            ResumeAnimations();
        }

        protected virtual void OnTurnStarted() { }

        private void OnAllTurnEnded(ClientEventArgs obj)
        {
            PauseAnimations();
        }

        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible)
                OnShowCore();
            else
                OnHideCore();
        }

        private void OnShowCore()
        {
            ClientEvents.AllTurnEnded.Subscribe(OnAllTurnEnded, ThreadOption.UIThread);
            ClientEvents.TurnStarted.Subscribe(OnTurnStarted, ThreadOption.UIThread);
            ResumeAnimations();
            OnShow();
        }
        #endregion
    }
}