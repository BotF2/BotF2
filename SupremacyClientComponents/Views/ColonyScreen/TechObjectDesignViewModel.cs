// TechObjectDesignViewModel.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

using Microsoft.Practices.ServiceLocation;

using Supremacy.Buildings;
using Supremacy.Entities;
using Supremacy.Tech;
using Supremacy.Universe;
using Supremacy.Client.Context;

namespace Supremacy.Client.Views
{
    public class TechObjectDesignViewModel : Freezable
    {
        #region Design Property
        public static readonly DependencyProperty DesignProperty = DependencyProperty.Register(
            "Design",
            typeof(TechObjectDesign),
            typeof(TechObjectDesignViewModel),
            new FrameworkPropertyMetadata(OnViewPropertyChanged));

        public TechObjectDesign Design
        {
            get { return (TechObjectDesign)GetValue(DesignProperty); }
            set { SetValue(DesignProperty, value); }
        }
        #endregion

        #region Civilization Property
        public static readonly DependencyProperty CivilizationProperty = DependencyProperty.Register(
            "Civilization",
            typeof(Civilization),
            typeof(TechObjectDesignViewModel),
            new FrameworkPropertyMetadata(OnViewPropertyChanged));

        public Civilization Civilization
        {
            get { return (Civilization)GetValue(CivilizationProperty); }
            set { SetValue(CivilizationProperty, value); }
        }
        #endregion

        #region Location Property
        public static readonly DependencyProperty LocationProperty = DependencyProperty.Register(
            "Location",
            typeof(MapLocation?),
            typeof(TechObjectDesignViewModel),
            new FrameworkPropertyMetadata(OnViewPropertyChanged));

        private static void OnViewPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.InvalidateProperty(UpgradeableDesignsResolvedProperty);
            d.InvalidateProperty(ObsoletedDesignsResolvedProperty);
        }

        public MapLocation? Location
        {
            get { return (MapLocation?)GetValue(LocationProperty); }
            set { SetValue(LocationProperty, value); }
        }
        #endregion

        #region UpgradeableDesignsResolved Property
        private static readonly DependencyPropertyKey UpgradeableDesignsResolvedPropertyKey = DependencyProperty.RegisterReadOnly(
            "UpgradeableDesignsResolved",
            typeof(ReadOnlyCollection<TechObjectDesign>),
            typeof(TechObjectDesignViewModel),
            new FrameworkPropertyMetadata(
                new ReadOnlyCollection<TechObjectDesign>(new List<TechObjectDesign>(0)),
                FrameworkPropertyMetadataOptions.None,
                null,
                CoerceUpgradeableDesignsResolved));

        public static readonly DependencyProperty UpgradeableDesignsResolvedProperty = UpgradeableDesignsResolvedPropertyKey.DependencyProperty;

        public ReadOnlyCollection<TechObjectDesign> UpgradeableDesignsResolved => (ReadOnlyCollection<TechObjectDesign>)GetValue(UpgradeableDesignsResolvedProperty);

        private static object CoerceUpgradeableDesignsResolved(DependencyObject d, object baseValue)
        {
            TechObjectDesignViewModel viewModel = (TechObjectDesignViewModel)d;

            TechObjectDesign design = viewModel.Design;
            if (design == null)
                return baseValue;

            TechTree primaryTechTree;
            TechTree secondaryTechTree;

            viewModel.GetTechTrees(out primaryTechTree, out secondaryTechTree);

            return design.UpgradableDesigns
                .Where(
                o => primaryTechTree.Contains(o) ||
                     ((secondaryTechTree != null) && secondaryTechTree.Contains(o)))
                .ToList()
                .AsReadOnly();
        }
        #endregion

        #region ObsoletedDesignsResolved Property
        private static readonly DependencyPropertyKey ObsoletedDesignsResolvedPropertyKey = DependencyProperty.RegisterReadOnly(
            "ObsoletedDesignsResolved",
            typeof(ReadOnlyCollection<TechObjectDesign>),
            typeof(TechObjectDesignViewModel),
            new FrameworkPropertyMetadata(
                new ReadOnlyCollection<TechObjectDesign>(new List<TechObjectDesign>(0)),
                FrameworkPropertyMetadataOptions.None,
                null,
                CoerceObsoletedDesignsResolved));

        public static readonly DependencyProperty ObsoletedDesignsResolvedProperty = ObsoletedDesignsResolvedPropertyKey.DependencyProperty;

        public ReadOnlyCollection<TechObjectDesign> ObsoletedDesignsResolved => (ReadOnlyCollection<TechObjectDesign>)GetValue(ObsoletedDesignsResolvedProperty);

        private static object CoerceObsoletedDesignsResolved(DependencyObject d, object baseValue)
        {
            TechObjectDesignViewModel viewModel = (TechObjectDesignViewModel)d;

            TechObjectDesign design = viewModel.Design;
            if (design == null)
                return baseValue;

            TechTree primaryTechTree;
            TechTree secondaryTechTree;

            viewModel.GetTechTrees(out primaryTechTree, out secondaryTechTree);

            return design.ObsoletedDesigns
                .Where(
                o => primaryTechTree.Contains(o) ||
                     ((secondaryTechTree != null) && secondaryTechTree.Contains(o)))
                .ToList()
                .AsReadOnly();
        }
        #endregion

        #region Helper Methods
        private void GetTechTrees(out TechTree primaryTechTree, out TechTree secondaryTechTree)
        {
            primaryTechTree = null;
            secondaryTechTree = null;

            IAppContext appContext = ServiceLocator.Current.GetInstance<IAppContext>();
            if (appContext == null)
                return;

            Game.IGameContext gameContext = appContext.CurrentGame;
            if (gameContext == null)
                return;

            TechObjectDesign design = Design;
            if (design == null)
                return;

            Civilization civilization = Civilization;
            if (civilization == null)
            {
                Game.CivilizationManager localPlayerEmpire = appContext.LocalPlayerEmpire;
                if (localPlayerEmpire == null)
                    return;
                civilization = localPlayerEmpire.Civilization;
            }

            primaryTechTree = gameContext.TechTrees[civilization];

            MapLocation? location = Location;
            if ((!(design is BuildingDesign)) || !location.HasValue)
                return;

            StarSystem system = gameContext.Universe.Map[location.Value].System;
            if (system == null)
                return;

            Colony colony = system.Colony;
            if (colony == null)
                return;

            Civilization originalOwner = colony.OriginalOwner;
            if (originalOwner != civilization)
                secondaryTechTree = gameContext.TechTrees[civilization];
        }
        #endregion

        #region Overrides of Freezable
        protected override Freezable CreateInstanceCore()
        {
            return new TechObjectDesignViewModel();
        }
        #endregion
    }
}