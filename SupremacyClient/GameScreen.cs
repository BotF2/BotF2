// GameScreen.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Composite.Presentation.Events;
using Microsoft.Practices.Composite.Regions;
using Microsoft.Practices.Unity;

using Supremacy.Annotations;
using Supremacy.Client.Events;
using Supremacy.Client.Views;
using Supremacy.Game;
using Supremacy.Client.Context;
using Supremacy.Utility;

namespace Supremacy.Client
{
    public class GameScreenBase : Control
    {
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly IUnityContainer _container;
        private readonly IRegionManager _regionManager;
        private readonly IPlayerOrderService _playerOrderService;

        #region Static Members
        public static readonly DependencyProperty AppContextProperty;
        public static readonly RoutedEvent ChatMessageReceivedEvent;

        static GameScreenBase()
        {
            AppContextProperty = GameScreenViewBase.AppContextProperty.AddOwner(typeof(GameScreenBase));

            SnapsToDevicePixelsProperty.OverrideMetadata(
                typeof(GameScreenBase),
                new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.Inherits));

            IsTabStopProperty.OverrideMetadata(
                typeof(GameScreenBase),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));

            ChatMessageReceivedEvent = EventManager.RegisterRoutedEvent(
                "ChatMessageReceived",
                RoutingStrategy.Direct,
                typeof(RoutedEventHandler),
                typeof(GameScreenBase));
        }

        #region Public and Protected Methods
        public static IAppContext GetAppContext(DependencyObject element)
        {
            return element?.GetValue(AppContextProperty) as IAppContext;
        }

        public static void SetAppContext(DependencyObject element, IAppContext value)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            element.SetValue(AppContextProperty, value);
        }
        #endregion

        protected GameScreenBase([NotNull] IUnityContainer container)
        {
            if (container == null)
                throw new ArgumentNullException("container");

            _container = container;
            _regionManager = _container.Resolve<IRegionManager>();
            _playerOrderService = _container.Resolve<IPlayerOrderService>();

            AppContext = _container.Resolve<IAppContext>();
        }
        #endregion

        public IAppContext AppContext
        {
            get { return GetValue(AppContextProperty) as IAppContext; }
            set { SetValue(AppContextProperty, value); }
        }

        protected IPlayerOrderService PlayerOrderService
        {
            get { return _playerOrderService; }
        }

        protected IRegionManager RegionManager
        {
            get { return _regionManager; }
        }

        protected void PauseAnimations()
        {
            foreach (var animationsHost in this.FindVisualDescendantsByType<DependencyObject>().OfType<IAnimationsHost>())
            {
                try
                {
                    animationsHost.PauseAnimations();
                }
                catch (Exception e) //ToDo: Just log or additional handling necessary?
                {
                    GameLog.LogException(e);
                }
            }
        }

        protected void ResumeAnimations()
        {
            foreach (var animationsHost in this.FindVisualDescendantsByType<DependencyObject>().OfType<IAnimationsHost>())
            {
                try
                {
                    animationsHost.ResumeAnimations();
                }
                catch (Exception e) //ToDo: Just log or additional handling necessary?
                {
                    GameLog.LogException(e);
                }
            }
        }
    }

    public abstract class GameScreen<TPresentationModel> : GameScreenBase, IGameScreenView<TPresentationModel>
    {
        private bool _isActive;

        protected GameScreen([NotNull] IUnityContainer container) : base(container)
        {
            Loaded += GameScreenLoaded;
            Unloaded += GameScreenUnloaded;
            IsVisibleChanged += GameScreenIsVisibleChanged;
            IsActiveChanged += HandleIsActiveChanged;

            ClientEvents.ScreenRefreshRequired.Subscribe(e => RefreshScreen());
        }

        private void HandleIsActiveChanged(object sender, EventArgs args)
        {
            var region = RegionManager.Regions[ClientRegions.GameScreens];
            if (!region.Views.Contains(this))
                return;

            if (_isActive)
                region.Activate(this);
            else
                region.Deactivate(this);
        }

        public void BringDescendantIntoView(DependencyObject descendant)
        {
            var target = descendant;

            while (target != null && target != this)
            {
                if (target.ReadLocalValue(Selector.IsSelectedProperty) != DependencyProperty.UnsetValue)
                    Selector.SetIsSelected(target, true);

                target = WpfHelper.GetParent(target as FrameworkElement);
            }
            
            (descendant as FrameworkElement)?.BringIntoView();
        }

        private void GameScreenIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible)
                OnShowCore();
            else
                OnHideCore();
        }

        private void OnHideCore()
        {
            PauseAnimations();
            OnHide();
        }

        private void OnShowCore()
        {
            ResumeAnimations();
            OnShow();
        }

        protected virtual void OnHide() {}

        protected virtual void OnShow() {}

        private void StopDescendantAnimations()
        {
            foreach (var animationsHost in this.FindVisualDescendantsByType<DependencyObject>().OfType<IAnimationsHost>())
            {
                try
                {
                    animationsHost.StopAnimations();
                }
                catch (Exception e) //ToDo: Just log or additional handling necessary?
                {
                    GameLog.LogException(e);
                }
            }
        }

        private void GameScreenUnloaded(object sender, RoutedEventArgs e)
        {
            StopDescendantAnimations();
            CommandBindings.Clear();
            InputBindings.Clear();
            ClientEvents.ChatMessageReceived.Unsubscribe(OnChatMessageReceived);
            ClientEvents.AllTurnEnded.Unsubscribe(OnAllTurnEnded);
            ClientEvents.TurnStarted.Unsubscribe(OnTurnStarted);
        }

        private void OnChatMessageReceived(DataEventArgs<ChatMessage> args)
        {
            var chatMessage = args.Value;
            if (chatMessage != null)
                ProcessChatMessage(chatMessage);
        }

        private void GameScreenLoaded(object sender, RoutedEventArgs e)
        {
            ClientEvents.ChatMessageReceived.Subscribe(OnChatMessageReceived);
            ClientEvents.AllTurnEnded.Subscribe(OnAllTurnEnded, ThreadOption.UIThread);
            ClientEvents.TurnStarted.Subscribe(OnTurnStarted, ThreadOption.UIThread);
            RefreshScreen();
        }

        private void OnTurnStarted(ClientEventArgs obj)
        {
            ResumeAnimations();
        }

        private void OnAllTurnEnded(ClientEventArgs obj)
        {
            PauseAnimations();
        }

        private void ProcessChatMessage(ChatMessage message)
        {
            if (!IsActive || !IsVisible || !AppContext.IsGameInPlay)
                return;
            if (ReferenceEquals(message.Sender, AppContext.LocalPlayer))
                return;
            RaiseEvent(new RoutedEventArgs(ChatMessageReceivedEvent, this));
        }

        protected internal virtual void OnClosing(object sender, CancelEventArgs e) {}

        public virtual void RefreshScreen() {}

        #region Implementation of IActiveAware
        public event EventHandler IsActiveChanged;

        private void OnIsActiveChanged()
        {
            IsActiveChanged?.Invoke(this, EventArgs.Empty);
        }

        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                if (Equals(_isActive, value))
                    return;
                _isActive = value;
                OnIsActiveChanged();
            }
        }
        #endregion

        #region Implementation of IGameScreenView
        public TPresentationModel Model { get; set; }

        public virtual void OnCreated() {}
        public virtual void OnDestroyed() {}
        #endregion
    }

    public class PriorityGameScreen<TPresentationModel> : GameScreen<TPresentationModel>
    {
        public PriorityGameScreen([NotNull] IUnityContainer container) : base(container) {}
    }
}
