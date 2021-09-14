using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Windows.Input;

using Supremacy.Entities;
using Supremacy.Utility;

namespace Supremacy.Client.Views
{
    public class DiplomacyGraphNode : INotifyPropertyChanged
    {
        private readonly ObservableCollection<DiplomacyGraphNode> _children;

        public DiplomacyGraphNode(Civilization civilization, ICommand selectNodeCommand)
        {
            Civilization = civilization ?? throw new ArgumentNullException("civilization");
            SelectNodeCommand = selectNodeCommand;
            _children = new ObservableCollection<DiplomacyGraphNode>();
        }

        public Civilization Civilization { get; }

        public ICommand SelectNodeCommand { get; }

        public ObservableCollection<DiplomacyGraphNode> Children => _children;

        public string ToolTip => Civilization.ShortName;

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
                    {
                        return;
                    }
                }
            }
            remove
            {
                while (true)
                {
                    PropertyChangedEventHandler oldHandler = _propertyChanged;
                    PropertyChangedEventHandler newHandler = (PropertyChangedEventHandler)Delegate.Remove(oldHandler, value);

                    if (Interlocked.CompareExchange(ref _propertyChanged, newHandler, oldHandler) == oldHandler)
                    {
                        return;
                    }
                }
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            _propertyChanged.Raise(this, propertyName);
        }

        #endregion
    }
}