// IInfoCardSubject.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Windows;

using Supremacy.Annotations;
using Supremacy.Game;
using Supremacy.Universe;

namespace Supremacy.Client.Controls
{
    public interface IInfoCardSubject
    {
        object Data { get; }
        event EventHandler DataChanged;

        bool Matches(InfoCard infoCard);
        void RefreshData();
        InfoCard CreateInfoCard();
    }

    public abstract class InfoCardSubject : Freezable, IInfoCardSubject
    {
        #region Fields
        private Func<object> _dataResolver;
        #endregion

        #region Constructors and Finalizers
        protected InfoCardSubject([NotNull] Func<object> dataResolver)
        {
            DataResolver = dataResolver ?? throw new ArgumentNullException("dataResolver");
        }

        protected InfoCardSubject() { }
        #endregion

        #region Properties

        #region DataResolver Property
        protected Func<object> DataResolver
        {
            get => _dataResolver;
            set
            {
                _dataResolver = value;
                RefreshData();
            }
        }
        #endregion

        #endregion

        #region Implementation of Freezable
        protected override Freezable CreateInstanceCore()
        {
            return (Freezable)Activator.CreateInstance(GetType());
        }
        #endregion

        #region Implementation of IInfoCardSubject

        #region Data Property
        protected static readonly DependencyPropertyKey DataPropertyKey = DependencyProperty.RegisterReadOnly(
            "Data",
            typeof(object),
            typeof(InfoCardSubject),
            new FrameworkPropertyMetadata(
                (d, e) => ((InfoCardSubject)d).OnDataChanged(),
                (d, v) => ((InfoCardSubject)d).CoerceData()));

        private object CoerceData()
        {
            Func<object> dataResolver = DataResolver;
            if (dataResolver == null)
            {
                return null;
            }

            return dataResolver();
        }

        public static readonly DependencyProperty DataProperty = DataPropertyKey.DependencyProperty;

        public object Data => GetValue(DataProperty);
        #endregion

        public event EventHandler DataChanged;

        private void OnDataChanged()
        {
            DataChanged?.Invoke(this, EventArgs.Empty);
        }

        public bool Matches(InfoCard infoCard)
        {
            if (infoCard == null)
            {
                return false;
            }

            return Equals(infoCard.DataContext, Data);
        }

        public void RefreshData()
        {
            CoerceValue(DataProperty);
        }

        public InfoCard CreateInfoCard()
        {
            return new InfoCard(this);
        }
        #endregion
    }

    public class UniverseObjectInfoCardSubject : InfoCardSubject
    {
        #region UniverseObject Property
        public static readonly DependencyProperty TargetProperty = DependencyProperty.Register(
            "Target",
            typeof(UniverseObject),
            typeof(UniverseObjectInfoCardSubject),
            new FrameworkPropertyMetadata((d, e) => ((UniverseObjectInfoCardSubject)d).OnTargetChanged(e)));

        private void OnTargetChanged(DependencyPropertyChangedEventArgs e)
        {
            DataResolver = CreateDataResolver(e.NewValue as UniverseObject);
        }

        public UniverseObject Target
        {
            set => SetValue(TargetProperty, value);
        }
        #endregion

        public UniverseObjectInfoCardSubject([NotNull] UniverseObject target)
            : base(CreateDataResolver(target))
        {
            Target = target;
        }

        public UniverseObjectInfoCardSubject() { }

        protected static Func<object> CreateDataResolver(UniverseObject target)
        {
            if (target == null)
            {
                return () => null;
            }

            return () => GameContext.Current.Universe.Objects[target.ObjectID];
        }
    }
}