// File:GalaxyScreenCommands.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Microsoft.Practices.Composite.Presentation.Commands;
using Supremacy.Annotations;
using Supremacy.Collections;
using Supremacy.Orbitals;
using Supremacy.Tech;
using Supremacy.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Supremacy.Client.Commands
{
    public class TargetSelectionArgs
    {
        public string Prompt { get; private set; }
        public string TargetDisplayMember { get; private set; }
        public IEnumerable TargetList { get; private set; }
        public object Result { get; set; }

        public TargetSelectionArgs([CanBeNull] string title, [CanBeNull] string displayMember, [NotNull] IEnumerable targetList)
        {
            Prompt = title;
            TargetDisplayMember = displayMember;
            TargetList = targetList ?? throw new ArgumentNullException("targetList");
        }
    }

    public static class GalaxyScreenCommands
    {
        public static readonly CompositeCommand SetInputMode = new CompositeCommand();
        public static readonly CompositeCommand SetOverviewMode = new CompositeCommand();
        public static readonly CompositeCommand MapZoomIn = new CompositeCommand();
        public static readonly CompositeCommand MapZoomOut = new CompositeCommand();
        public static readonly CompositeCommand SelectTaskForce = new CompositeCommand();
        public static readonly CompositeCommand AddShipToTaskForce = new CompositeCommand();
        public static readonly CompositeCommand RemoveShipFromTaskForce = new CompositeCommand();
        public static readonly CompositeCommand SelectSector = new CompositeCommand();
        public static readonly CompositeCommand CenterOnSector = new CompositeCommand();
        public static readonly CompositeCommand CenterOnHomeSector = new CompositeCommand();
        public static readonly CompositeCommand CenterOn1 = new CompositeCommand();
        public static readonly CompositeCommand CenterOn2 = new CompositeCommand();
        public static readonly CompositeCommand CenterOn3 = new CompositeCommand();
        public static readonly CompositeCommand CenterOn4 = new CompositeCommand();
        public static readonly CompositeCommand SummaryOnOff = new CompositeCommand();
        public static readonly CompositeCommand ToggleTaskForceCloak = new CompositeCommand();
        public static readonly CompositeCommand ToggleTaskForceCamouflage = new CompositeCommand();
        public static readonly CompositeCommand IssueTaskForceOrder = new CompositeCommand();
        public static readonly CompositeCommand ShowTargetSelectionDialog = new CompositeCommand();
        public static readonly CompositeCommand CancelTradeRoute = new CompositeCommand();
        public static readonly CompositeCommand Scrap = new CompositeCommand();
    }

    public class RedeployShipCommandArgs
    {
        private readonly Fleet _targetFleet;

        public Ship Ship { get; }

        public Fleet TargetFleet => _targetFleet;

        public bool HasTargetFleet => _targetFleet != null;

        public RedeployShipCommandArgs([NotNull] Ship ship)
            : this(ship, null) { }

        public RedeployShipCommandArgs([NotNull] Ship ship, [CanBeNull] Fleet targetFleet)
        {
            Ship = ship ?? throw new ArgumentNullException("ship");
            _targetFleet = targetFleet;
        }
    }

    public class ScrapCommandArgs : ICheckableCommandParameter
    {
        private readonly ArrayWrapper<TechObject> _objects;
        private bool? _scrap;

        public ScrapCommandArgs([NotNull] TechObject @object)
        {
            if (@object == null)
            {
                throw new ArgumentNullException("object");
            }

            _objects = new ArrayWrapper<TechObject>(new[] { @object });

            SetInitialScrapValue();
        }

        public ScrapCommandArgs([NotNull] IEnumerable<TechObject> objects)
        {
            if (objects == null)
            {
                throw new ArgumentNullException("objects");
            }

            _objects = new ArrayWrapper<TechObject>(objects.ToArray());

            SetInitialScrapValue();
        }

        private void SetInitialScrapValue()
        {
            _scrap = _objects.All(o => o.Scrap) ? true : _objects.Any(o => o.Scrap) ? null : (bool?)false;
        }

        public IIndexedCollection<TechObject> Objects => _objects;

        #region Implementation of ICheckableCommandParameter
        public event EventHandler InnerParameterChanged;
        public event EventHandler IsCheckedChanged;

        public bool Handled { get; set; }

        public bool? IsChecked
        {
            get => _scrap;
            set
            {
                _scrap = value ?? false;
                IsCheckedChanged.Raise(this);
            }
        }

        public object InnerParameter { get; set; }
        #endregion
    }
}