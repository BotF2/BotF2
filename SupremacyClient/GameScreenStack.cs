// GameScreenStack.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Linq;

using Microsoft.Practices.Composite;
using Microsoft.Practices.Composite.Presentation.Regions;
using Microsoft.Practices.Composite.Presentation.Regions.Behaviors;
using Microsoft.Practices.Composite.Regions;

using Supremacy.Annotations;
using Supremacy.Client.Events;

using TransitionEffects;

namespace Supremacy.Client
{
    [UsedImplicitly]
    internal sealed class GameScreenStackRegionAdapter :RegionAdapterBase<GameScreenStack>
    {
        public GameScreenStackRegionAdapter(IRegionBehaviorFactory defaultBehaviors) : base(defaultBehaviors) {}

        #region Overrides of RegionAdapterBase<GameScreenStack>
        protected override void Adapt(IRegion region, GameScreenStack regionTarget) {}

        protected override void AttachBehaviors(IRegion region, GameScreenStack regionTarget)
        {
            region.Behaviors.Add(
                GameScreenStackSelectionSyncBehavior.BehaviorKey,
                new GameScreenStackSelectionSyncBehavior { HostControl = regionTarget });
            base.AttachBehaviors(region, regionTarget);
        }

        protected override IRegion CreateRegion()
        {
            return new SingleActiveRegion();
        }
        #endregion
    }

    public class GameScreenStackSelectionSyncBehavior : RegionBehavior, IHostAwareRegionBehavior
    {
        private bool _updating;

        public const string BehaviorKey = "SelectorItemsSourceSyncBehavior";

        private GameScreenStack _hostControl;

        public DependencyObject HostControl
        {
            get { return _hostControl; }
            set { _hostControl = value as GameScreenStack; }
        }

        public void Detach()
        {
            if (_hostControl != null)
            {
                _hostControl.CurrentScreenChanged -= HostControlSelectionChanged;
            }

            if (Region != null)
            {
                Region.ActiveViews.CollectionChanged -= OnActiveViewsCollectionChanged;
            }
        }

        protected override void OnAttach()
        {
            AddItemsToRegionViews();

            _hostControl.CurrentScreenChanged += HostControlSelectionChanged;
            ((INotifyCollectionChanged)_hostControl.Screens).CollectionChanged += OnHostControlCollectionChanged;
            Region.Views.CollectionChanged += OnViewsCollectionChanged;
            Region.ActiveViews.CollectionChanged += OnActiveViewsCollectionChanged;
        }

        private void OnViewsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_updating)
                return;
            try
            {
                _updating = true;
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    foreach (var childItem in e.NewItems.OfType<Control>())
                    {
                        if (_hostControl.Screens.Contains(childItem))
                            continue;
                        _hostControl.AddScreen(childItem);
                        var activeAware = childItem as IActiveAware;
                        if ((_hostControl.CurrentScreen == null) && (activeAware != null) && activeAware.IsActive)
                        {
                            Region.Activate(childItem);
                        }
                    }
                }
                else if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    foreach (var childItem in e.OldItems.OfType<Control>())
                    {
                        if (_hostControl.Screens.Contains(childItem))
                        {
                            _hostControl.RemoveScreen(childItem);
                        }
                    }
                }
            }
            finally
            {
                _updating = false;
            }
        }

        private void OnHostControlCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_updating)
                return;

            try
            {
                _updating = true;
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    foreach (var childItem in e.NewItems)
                    {
                        if (Region.Views.Contains(childItem))
                            continue;
                        Region.Add(childItem);
                    }
                }
                else if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    foreach (var childItem in e.OldItems)
                    {
                        if (Region.Views.Contains(childItem))
                            Region.Remove(childItem);
                    }
                }
            }
            finally
            {
                _updating = false;
            }
        }

        private void AddItemsToRegionViews()
        {
            foreach (var childItem in _hostControl.Screens)
            {
                Region.Add(childItem);
            }
        }

        private void OnActiveViewsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_updating) 
                return;

            try
            {
                _updating = true;
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    _hostControl.CurrentScreen = (Control)e.NewItems[0];
                }
                else if (e.Action == NotifyCollectionChangedAction.Remove &&
                         e.OldItems.Contains(_hostControl.CurrentScreen))
                {
                    _hostControl.CurrentScreen = null;
                    //_hostControl.CurrentScreen = (_hostControl.CurrentScreen == _hostControl.FallbackScreen)
                    //                                 ? null
                    //                                 : _hostControl.FallbackScreen;
                }
            }
            finally
            {
                _updating = false;
            }
        }

        private void HostControlSelectionChanged(object sender, EventArgs e)
        {
            if (_updating)
                return;

            try
            {
                _updating = true;
                var currentScreen = _hostControl.CurrentScreen;
                if ((currentScreen != null) && !Region.ActiveViews.Contains(currentScreen))
                {
                    Region.Activate(currentScreen);
                }
            }
            finally
            {
                _updating = false;
            }
        }
    }

    public sealed class GameScreenStack : Control
    {
        private readonly ObservableCollection<Control> _screens;
        private readonly Grid _itemsContainer;
        private readonly ReadOnlyObservableCollection<Control> _screensView;
        private Control _currentScreen;
        private Control _fallbackScreen;
        //private RenderTargetBitmap _lastScreenBitmap;

        public event EventHandler CurrentScreenChanged;

        public GameScreenStack()
        {
            _itemsContainer = new Grid();
            _screens = new ObservableCollection<Control>();
            _screensView = new ReadOnlyObservableCollection<Control>(_screens);
            _currentScreen = null;

            AddVisualChild(_itemsContainer);
            AddLogicalChild(_itemsContainer);

            Loaded += OnLoaded;
        }

        public ReadOnlyObservableCollection<Control> Screens
        {
            get { return _screensView; }
        }

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            lock (_screens)
            {
                foreach (var screen in _screens)
                {
                    screen.Measure(RenderSize);
                    screen.Arrange(new Rect(RenderSize));
                }
            }
            ClientEvents.ScreenRefreshRequired.Publish(ClientEventArgs.Default);
        }

        public void AddScreen(Control screen)
        {
            // works
            //GameLog.Client.GameData.DebugFormat("GameScreenStack.cs: screen={0}", screen);
            if (screen == null)
                throw new ArgumentNullException("screen");
            lock (_screens)
            {
                if (_screens.Contains(screen))
                    return;
                var wasEmpty = (_screens.Count == 0);
                _screens.Add(screen);
                _itemsContainer.Children.Add(screen);
                if (IsVisible)
                {
                    screen.Measure(RenderSize);
                    screen.Arrange(new Rect(RenderSize));
                }
                screen.Visibility = Visibility.Hidden;
                if (wasEmpty)
                    FallbackScreen = screen;
            }
        }

        public void RemoveScreen(Control screen)
        {
            lock (_screens)
            {
                if ((screen == null) || !_screens.Contains(screen))
                    return;
                if (screen == CurrentScreen)
                {
                    if (FallbackScreen != null)
                    {
                        if (FallbackScreen != screen)
                            CurrentScreen = FallbackScreen;
                        else
                            FallbackScreen = null;
                    }
                    else
                        CurrentScreen = null;
                }
                _itemsContainer.Children.Remove(screen);
                _screens.Remove(screen);
                
                if(ClientApp.Current.IsShuttingDown)
                    return;

                screen.Style = null;
                screen.Template = null;
                screen.InvalidateMeasure();
            }
        }

        public Control CurrentScreen
        {
            get { return _currentScreen; }
            set
            {
                if (value == _currentScreen)
                    return;
                lock (_screens)
                {
                    var lastScreen = _currentScreen;
                    if (value != null && !_screens.Contains(value))
                        AddScreen(value);
                    if (lastScreen != null)
                    {
                        if (lastScreen.Effect is TransitionEffect)
                            lastScreen.ClearValue(EffectProperty);
                        lastScreen.Visibility = Visibility.Hidden;
                        var activeAwareScreen = lastScreen as IActiveAware;
                        if (activeAwareScreen != null)
                            activeAwareScreen.IsActive = false;
                    }
                    _currentScreen = value;
                    if (_currentScreen != null)
                    {
                        _currentScreen.Visibility = Visibility.Visible;
                        _currentScreen.Focus();

                        var activeAwareScreen = _currentScreen as IActiveAware;
                        if (activeAwareScreen != null)
                            activeAwareScreen.IsActive = true;
                    }
                    if (CurrentScreenChanged != null)
                        CurrentScreenChanged(this, new EventArgs());
                }
            }
        }


        public Control FallbackScreen
        {
            get { return _fallbackScreen; }
            set
            {
                if ((value != null) && !_screens.Contains(value))
                {
                    AddScreen(value);
                }
                _fallbackScreen = value;
            }
        }

        protected override int VisualChildrenCount
        {
            get { return 1; }
        }

        protected override Visual GetVisualChild(int index)
        {
            return _itemsContainer;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            _itemsContainer.Measure(constraint);
            return constraint;
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            
            _itemsContainer.Arrange(new Rect(arrangeBounds));
            return arrangeBounds;
        }
    }
}
