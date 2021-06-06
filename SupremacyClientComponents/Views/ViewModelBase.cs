using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Input;

using Microsoft.Practices.Composite.Regions;

using Supremacy.Annotations;
using Supremacy.Utility;
using Supremacy.Client.Context;

namespace Supremacy.Client.Views
{
    public abstract class ViewModelBase<TView, TViewModel> : PresentationModelBase,
        IInteractionNode, 
        INotifyPropertyChanged
        where TView : class, IGameScreenView<TViewModel>
        where TViewModel : ViewModelBase<TView, TViewModel>
    {
        private readonly IRegionManager _regionManager;
        private readonly EventHandler _commandManagerInvalidateRequeryHandler;

        protected ViewModelBase([NotNull] IAppContext appContext, [NotNull] IRegionManager regionManager)
            : base(appContext)
        {
            _regionManager = regionManager;
            _commandManagerInvalidateRequeryHandler = OnCommandManagerRequerySuggested;
        }

        public abstract string ViewName { get; }

        protected IRegionManager RegionManager => _regionManager;

        internal GameScreenPresenterBase<TViewModel, TView> Presenter { get; set; }

        public TView View { get; set; }

        public bool IsRunning { get; private set; }

        protected internal virtual void RegisterViewWithRegion() { }
        protected internal virtual void UnregisterViewWithRegion() { }

        protected virtual void InvalidateCommands() { }
        protected virtual void RegisterCommandAndEventHandlers() { }
        protected virtual void UnregisterCommandAndEventHandlers() { }

        public void Run()
        {
            Presenter.Run();
        }

        public void Terminate()
        {
            Presenter.Terminate();
        }

        internal void RunCore()
        {
            IsRunning = true;

            CommandManager.RequerySuggested += _commandManagerInvalidateRequeryHandler;

            RegisterCommandAndEventHandlers();
            RunOverride();
        }

        internal void TerminateCore()
        {
            CommandManager.RequerySuggested -= _commandManagerInvalidateRequeryHandler;

            TerminateOverride();
            UnregisterCommandAndEventHandlers();

            IsRunning = false;
        }

        protected virtual void RunOverride() { }
        protected virtual void TerminateOverride() { }

        private void OnCommandManagerRequerySuggested(object sender, EventArgs e)
        {
            InvalidateCommands();
        }

        //internal abstract void RaisePropertyChangedEvent(string v);

        #region Implementation of INotifyPropertyChanged

        [NonSerialized] private PropertyChangedEventHandler _propertyChanged;

        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add
            {
                while (true)
                {
                    PropertyChangedEventHandler oldHandler = _propertyChanged;
                    PropertyChangedEventHandler newHandler = (PropertyChangedEventHandler)Delegate.Combine(oldHandler, value);

                    if (Interlocked.CompareExchange(ref _propertyChanged, newHandler, oldHandler) == oldHandler)
                        return;
                }
            }
            remove
            {
                while (true)
                {
                    PropertyChangedEventHandler oldHandler = _propertyChanged;
                    PropertyChangedEventHandler newHandler = (PropertyChangedEventHandler)Delegate.Remove(oldHandler, value);

                    if (Interlocked.CompareExchange(ref _propertyChanged, newHandler, oldHandler) == oldHandler)
                        return;
                }
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            _propertyChanged.Raise(this, propertyName);
        }

        #endregion

        object IInteractionNode.UIElement => View;

        IInteractionNode IInteractionNode.FindParent()
        {
            return FindParentInteractionNode();
        }

        protected virtual IInteractionNode FindParentInteractionNode()
        {
            DependencyObject view = View as DependencyObject;
            if (view == null)
                return null;

            IInteractionNode ancestorNode = null;

            view.FindVisualAncestor(
                o =>
                {
                    ancestorNode = Views.View.GetInteractionNode(o);

                    return ancestorNode != null &&
                           ancestorNode != this;
                });

            if (ancestorNode == null)
            {
                view.FindLogicalAncestor(
                    o =>
                    {
                        ancestorNode = Views.View.GetInteractionNode(o);

                        return ancestorNode != null &&
                               ancestorNode != this;
                    });
            }

            return ancestorNode;

        }
    }
}