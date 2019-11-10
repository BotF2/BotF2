// FleetOrder.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Diagnostics;
using Supremacy.Game;
using Supremacy.Resources;
using Supremacy.Types;
using Supremacy.Universe;

namespace Supremacy.Orbitals
{
    /// <summary>
    /// Base class for all default and user-defined fleet orders.
    /// </summary>
    [Serializable]
    [DebuggerDisplay("{DisplayText}")]
    public abstract class FleetOrder : INotifyPropertyChanged
    {
        private int _fleetId;
        private int _turnAssigned;

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="FleetOrder"/> has been
        /// assigned to a <see cref="Fleet"/>.
        /// </summary>
        /// <value>
        /// <c>true</c> if this <see cref="FleetOrder"/> has been assigned; otherwise, <c>false</c>.
        /// </value>
        public bool IsAssigned
        {
            get { return TurnAssigned != 0; }
            private set
            {
                if (value)
                {
                    if (!IsAssigned)
                        TurnAssigned = Math.Max(1, GameContext.Current.TurnNumber);
                }
                else
                {
                    TurnAssigned = 0;
                }
            }
        }

        public int TurnAssigned
        {
            get { return _turnAssigned; }
            private set
            {
                 _turnAssigned = value;
                OnPropertyChanged("TurnAssigned");
                OnPropertyChanged("IsAssigned");
            }
        }

        public int AssignmentDuration
        {
            get
            {
                if (!IsAssigned)
                    return 0;
                return (TurnAssigned - GameContext.Current.TurnNumber);
            }
        }

        /// <summary>
        /// Gets the name of the order.
        /// </summary>
        /// <value>The name of the order.</value>
        public abstract string OrderName { get; }

        /// <summary>
        /// Gets the text displaying the current status of the order.
        /// </summary>
        /// <value>The status text.</value>
        public abstract string Status { get; }

        /// <summary>
        /// Gets the name of the member of the target class that should be displayed in the target list.
        /// </summary>
        /// <value>The target display member.</value>
        /// <remarks>
        /// <c>TargetDisplayMember</c> is typically a property name, and is used primarily for data-binding
        /// in the game client.
        /// </remarks>
        public virtual string TargetDisplayMember
        {
            get { return "."; }
        }

        /// <summary>
        /// Gets the complete status text that should be displayed in the task forces list in the game.
        /// </summary>
        /// <value>The complete status text.</value>
        public virtual string DisplayText
        {
            get
            {
                string displayText;
                Percentage? percentComplete = PercentComplete;
                
                if (Fleet.IsInTow)
                {
                    displayText = String.Format(
                        ResourceManager.GetString("ORDER_IN_TOW"),
                        Status);
                }
                else if (Fleet.IsStranded)
                {
                    displayText = String.Format(
                        ResourceManager.GetString("ORDER_STRANDED"),
                        Status);
                }
                else
                {
                    displayText = Status;
                }

                if (percentComplete.HasValue)
                {
                    displayText = String.Format(displayText + " ({0})", percentComplete.Value);
                }

                if (!Fleet.Route.IsEmpty)
                {
                    int turns = Fleet.Route.Length / Fleet.Speed;
                    string formatString;
                    if ((Fleet.Route.Length % Fleet.Speed) != 0)
                        turns++;
                    if (turns == 1)
                        formatString = ResourceManager.GetString("ORDER_ETA_TURN");
                    else
                        formatString = ResourceManager.GetString("ORDER_ETA_TURNS");
                    displayText = String.Format(formatString, displayText, turns);
                }

                return displayText;
            }
        }

        /// <summary>
        /// Gets the fleet to which this <see cref="FleetOrder"/> has been assigned.
        /// </summary>
        /// <value>The fleet.</value>
        public Fleet Fleet
        {
            get { return GameContext.Current.Universe.Objects[_fleetId] as Fleet; }
            internal set
            {
                _fleetId = (value == null) ? -1 : value.ObjectID;
                OnPropertyChanged("Fleet");
            }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="FleetOrder"/> has been completed.
        /// </summary>
        /// <value>
        /// <c>true</c> if this <see cref="FleetOrder"/> has been completed; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsComplete
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the percentage of completion of this <see cref="FleetOrder"/> (if applicable).
        /// </summary>
        /// <value>The percentage of completion, or <c>null</c> if not applicable.</value>
        public virtual Percentage? PercentComplete
        {
            get { return null; }
        }

        /// <summary>
        /// Gets the number of turns remaining until this <see cref="FleetOrder"/> has been completed.
        /// </summary>
        /// <value>The number of turns remaining.</value>
        public virtual int? TurnsRemaining
        {
            get { return null; }
        }

        /// <summary>
        /// Gets or sets the object that is the target of this <see cref="FleetOrder"/> (if applicable).
        /// </summary>
        /// <value>The target, or <c>null</c> if not applicable.</value>
        public virtual Object Target
        {
            get { return null; }
            // ReSharper disable ValueParameterNotUsed
            set { }
            // ReSharper restore ValueParameterNotUsed
        }

        /// <summary>
        /// Determines whether a target is required in order to assign this <see cref="FleetOrder"/>.
        /// </summary>
        /// <param name="fleet">The fleet to which this <see cref="FleetOrder"/> might be assigned.</param>
        /// <returns>
        /// <c>true</c> if a target is required; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool IsTargetRequired(Fleet fleet)
        {
            return false;
        }

        /// <summary>
        /// Gets a value indicating whether a fleet cancels its current travel route when this
        /// <see cref="FleetOrder"/> is assigned.
        /// </summary>
        /// <value>
        /// <c>true</c> if the route is cancelled; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsRouteCancelledOnAssign
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="FleetOrder"/> is cancelled when the
        /// travel route of the fleet is manually changed.
        /// </summary>
        /// <value>
        /// <c>true</c> if this <see cref="FleetOrder"/> is cancelled; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsCancelledOnRouteChange
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="FleetOrder"/> is cancelled when the
        /// location of this fleet is changed
        /// </summary>
        /// <value>
        /// <c>true</c> if this <see cref="FleetOrder"/> is cancelled; otherwise, <c>false</c></value>.
        public virtual bool IsCancelledOnMove
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether a fleet will engage hostiles when assigned this <see cref="FleetOrder"/>.
        /// </summary>
        /// <value><c>true</c> if fleet will engage hostiles; otherwise, <c>false</c>.</value>
        public virtual bool WillEngageHostiles
        {
            get
            {
                //GameLog.Print("Fleet.Owner={0}, Fleet.Name={1})", Fleet.Owner, Fleet.Name);
                if (Fleet == null)
                    return false;
                if (Fleet.IsInTow)
                    return false;
                //if (this.Fleet.IsCamouflaged)
                //    return false;
                return true;
            }
        }

        protected internal FleetOrder() { }

        /// <summary>
        /// Generates a list of possible targets for this <see cref="FleetOrder"/> (if applicable).
        /// </summary>
        /// <param name="source">The fleet to which this <see cref="FleetOrder"/> might be assigned.</param>
        /// <returns>The list of targets.</returns>
        /// <remarks>
        /// An empty list will be returned if this <see cref="FleetOrder"/> does not require a target.
        /// </remarks>
        public virtual IEnumerable<object> FindTargets(Fleet source)
        {
            return new object[0];
        }

        /// <summary>
        /// Determines whether this <see cref="FleetOrder"/> can be carried out by the specified fleet.
        /// </summary>
        /// <param name="fleet">The fleet.</param>
        /// <returns>
        /// <c>true</c> if this <see cref="FleetOrder"/> can be carried out; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool IsValidOrder(Fleet fleet)
        {
            if (fleet != null)
            {   
                if (IsTargetRequired(fleet))
                {
                    if (Target != null)
                        return true;
                    return FindTargets(fleet).Any();
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Determines whether this <see cref="FleetOrder"/> can be assigned to the specified fleet.
        /// </summary>
        /// <param name="fleet">The fleet.</param>
        /// <returns>
        /// <c>true</c> if this <see cref="FleetOrder"/> can be assigned; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool CanAssignOrder(Fleet fleet)
        {
            return IsValidOrder(fleet);
        }

        /// <summary>
        /// Called when this <see cref="FleetOrder"/> is assigned to a fleet.
        /// </summary>
        protected internal virtual void OnOrderAssigned()
        {
            IsAssigned = true;
            if ((Fleet != null) && IsRouteCancelledOnAssign)
                Fleet.SetRouteInternal(TravelRoute.Empty);
        }

        /// <summary>
        /// Called when a fleet's route is changed while this <see cref="FleetOrder"/> is assigned.
        /// </summary>
        protected internal virtual void OnFleetRouteChanged()
        {
            Fleet fleet = Fleet;
            if ((fleet != null) && IsCancelledOnRouteChange)
            {
                fleet.SetOrder(fleet.GetDefaultOrder());
            }
            OnPropertyChanged("DisplayText");
        }

        /// <summary>
        /// Called when a fleet moves while this <see cref="FleetOrder"/> is assigned.
        /// </summary>
        public virtual void OnFleetMoved() {
            Fleet fleet = Fleet;
            if ((fleet != null) && IsCancelledOnMove)
            {
                fleet.SetOrder(fleet.GetDefaultOrder());
            }
        }

        /// <summary>
        /// Called when ships are added or removed from a fleet when this <see cref="FleetOrder"/> is assigned.
        /// </summary>
        protected internal virtual void OnShipRedeployed()
        {
            Fleet fleet = Fleet;
            if ((fleet != null) && !IsValidOrder(fleet))
            {
                fleet.SetOrder(fleet.GetDefaultOrder());
            }
        }

        /// <summary>
        /// Called when a ship is destroyed in a fleet when this <see cref="FleetOrder"/> is assigned.
        /// </summary>
        protected internal virtual void OnShipDestroyed() { }

        /// <summary>
        /// Called when this <see cref="FleetOrder"/> is cancelled.
        /// </summary>
        protected internal virtual void OnOrderCancelled()
        {
            IsAssigned = false;
        }

        /// <summary>
        /// Called when this <see cref="FleetOrder"/> has completed.
        /// </summary>
        protected internal virtual void OnOrderCompleted()
        {
            IsAssigned = false;
        }

        /// <summary>
        /// Called at the beginning of each turn.
        /// </summary>
        protected internal virtual void OnTurnBeginning()
        {
            if (IsAssigned && IsTargetRequired(Fleet) && (Target == null))
            {
                if (Fleet != null)
                {
                    Fleet.SetOrder(Fleet.GetDefaultOrder());
                    if (Fleet.Order != null)
                        Fleet.Order.OnTurnBeginning();
                }
            }
        }

        protected internal virtual void OnTurnEnding() { }

        /// <summary>
        /// Creates an instance of this <see cref="FleetOrder"/>.
        /// </summary>
        /// <returns></returns>
        public abstract FleetOrder Create();

        /// <summary>
        /// Updates the references lost during reserialization.
        /// </summary>
        protected internal virtual void UpdateReferences() { }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="FleetOrder"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="FleetOrder"/>.
        /// </returns>
        /// <filterPriority>2</filterPriority>
        public override string ToString()
        {
            return OrderName;
        }

        #region INotifyPropertyChanged Members
        /// <summary>
        /// Occurs when a property value is changed.
        /// </summary>
        [field:NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">Name of the property that changed.</param>
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
