// UniqueFlagsConverter.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections;
using System.Globalization;
using System.Windows.Data;
using System.Linq;

using Expression = System.Linq.Expressions.Expression;

namespace Supremacy.Client
{
    [ValueConversion(typeof(IEnumerable), typeof(IEnumerable))]
    public sealed class UniqueFlagsConverter : IValueConverter
    {
        private Func<object, object, bool> _andTest;

        public Type FlagsType { get; set; }

        private Func<object, object, bool> AndTest
        {
            get
            {
                if (_andTest == null)
                {
                    var p1 = Expression.Parameter(FlagsType, "p1");
                    var p2 = Expression.Parameter(FlagsType, "p2");
                    var test = Expression.Lambda(
                        Expression.Equal(
                            Expression.And(
                                Expression.Convert(p1, typeof(int)),
                                Expression.Convert(p2, typeof(int))),
                            Expression.Convert(p2, typeof(int))),
                        p1,
                        p2).Compile();
                    _andTest = (a, b) => (bool)test.DynamicInvoke(a, b);
                }
                return _andTest;
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            if (!FlagsType.IsAssignableFrom(value.GetType()))
                return value;

            var flagValues = Enum.GetValues(FlagsType).Cast<object>().OrderBy(o => o).ToList();
            var innerFlagValues = flagValues;
            flagValues = flagValues.Where(a => AndTest(value, a) && innerFlagValues.Where(b => !Equals(a, b) && AndTest(a, b)).Count() == 0).ToList();
            return flagValues;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        //private sealed class BindableEnumerable : IEnumerable<TFlag>, INotifyCollectionChanged, IWeakEventListener
        //{
        //    private readonly IEnumerable _baseCollection;

        //    public BindableEnumerable([NotNull] IEnumerable baseCollection)
        //    {
        //        if (baseCollection == null)
        //            throw new ArgumentNullException("baseCollection");
                
        //        _baseCollection = baseCollection;

        //        var observableCollection = _baseCollection as INotifyCollectionChanged;
        //        if (observableCollection != null)
        //            CollectionChangedEventManager.AddListener(observableCollection, this);
        //    }

        //    public IEnumerator<TFlag> GetEnumerator()
        //    {
        //        return _baseCollection.Cast<TFlag>().GetEnumerator();
        //    }

        //    IEnumerator IEnumerable.GetEnumerator()
        //    {
        //        return GetEnumerator();
        //    }

        //    public event NotifyCollectionChangedEventHandler CollectionChanged;

        //    private void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
        //    {
        //        var handler = this.CollectionChanged;
        //        if (handler != null)
        //            handler(this, args);
        //    }

        //    public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        //    {
        //        var args = e as NotifyCollectionChangedEventArgs;
        //        if (args != null)
        //            OnCollectionChanged(args);
        //        return true;
        //    }
        //}
    }
}