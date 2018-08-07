// BuildProject.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.ComponentModel;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.IO.Serialization;
using Supremacy.Resources;
using Supremacy.Tech;
using Supremacy.Types;
using Supremacy.Universe;
using Supremacy.Utility;
using Supremacy.Buildings;

namespace Supremacy.Economy
{
    [Flags]
    public enum BuildProjectFlags : byte
    {
        None = 0x00,
        OnHold = 0x01,
        Cancelled = 0x02,
        DeuteriumShortage = 0x04,
        DilithiumShortage = 0x08,
        RawMaterialsShortage = 0x10,
        Rushed = 0x20,
    }

    /// <summary>
    /// The base class for all construction projects in the game.
    /// </summary>
    [Serializable]
    public abstract class BuildProject : INotifyPropertyChanged
    {
        public const int MaxPriority = Byte.MaxValue;
        public const int MinPriority = Byte.MinValue;

        private readonly int _productionCenterId;

        private int _buildTypeId;
        private int _industryInvested;
        private MapLocation _location;
        private int _ownerId;
        private byte _priority;
        private BuildProjectFlags _flags;
        private ResourceValueCollection _resourcesInvested;
        public bool _buildTracing = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="BuildProject"/> class.
        /// </summary>
        /// <param name="owner">The civilization initiating the <see cref="BuildProject"/>.</param>
        /// <param name="productionCenter">The construction location.</param>
        /// <param name="buildType">The design of the item being constructed.</param>
        protected BuildProject(
            Civilization owner,
            // ReSharper disable SuggestBaseTypeForParameter
            IProductionCenter productionCenter,
            // ReSharper restore SuggestBaseTypeForParameter
            TechObjectDesign buildType)
        {
            if (owner == null)
                throw new ArgumentNullException("owner");
            if (buildType == null)
                throw new ArgumentNullException("buildType");
            _ownerId = owner.CivID;
            _buildTypeId = buildType.DesignID;
            _location = productionCenter.Location;
            _productionCenterId = productionCenter.ObjectID;
            _resourcesInvested = new ResourceValueCollection();

            if (_buildTracing)
                if (owner.Name == "borg")
                GameLog.Print("available entries: owner = {0}, _location = {2}, productionCenterID = {3}, buildType = {1}", 
                owner.Name, buildType.Name, _location.ToString(), productionCenter.ObjectID);
        }

        /// <summary>
        /// Gets the production center at which construction is taking place.
        /// </summary>
        /// <value>The production center at which construction is taking place.</value>
        public virtual IProductionCenter ProductionCenter
        {
            get { return GameContext.Current.Universe.Objects[_productionCenterId] as IProductionCenter; }
        }

        public override string ToString()
        {
            return BuildDesign.LocalizedName;
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="BuildProject"/> is an upgrade project.
        /// </summary>
        /// <value>
        /// <c>true</c> if this <see cref="BuildProject"/> is an upgrade project; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsUpgrade
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the description of the item under construction.
        /// </summary>
        /// <value>The description.</value>
        public virtual string Description
        {
            get { return ResourceManager.GetString(BuildDesign.Name); }
        }

        /// <summary>
        /// Gets or sets the location where construction is taking place.
        /// </summary>
        /// <value>The location.</value>
        public MapLocation Location
        {
            get { return _location; }
            set { _location = value; }
        }

        /// <summary>
        /// Gets the civilization that initiated this <see cref="BuildProject"/>.
        /// </summary>
        /// <value>The builder.</value>
        public Civilization Builder
        {
            get { return GameContext.Current.Civilizations[_ownerId]; }
        }

        /// <summary>
        /// Gets the design of the item being constructed.
        /// </summary>
        /// <value>The construction design.</value>
        public TechObjectDesign BuildDesign
        {
            get { return GameContext.Current.TechDatabase[_buildTypeId]; }
        }

        /// <summary>
        /// Gets the player-assigned priority of this <see cref="BuildProject"/>.
        /// </summary>
        /// <value>The player-assigned priority.</value>
        public int Priority
        {
            get { return _priority; }
            set { _priority = (byte)Math.Max(MinPriority, Math.Min(MaxPriority, value)); }
        }

        /// <summary>
        /// Gets or sets the amount of industry that has been invested thus far.
        /// </summary>
        /// <value>The industry invested.</value>
        public virtual int IndustryInvested
        {
            get { return _industryInvested; }
            protected set { _industryInvested = value; }
        }

        /// <summary>
        /// Gets the amount of resources that have been invested thus far.
        /// </summary>
        /// <value>The resources invested.</value>
        public ResourceValueCollection ResourcesInvested
        {
            get { return _resourcesInvested; }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="BuildProject"/> is completed.
        /// </summary>
        /// <value>
        /// <c>true</c> if this <see cref="BuildProject"/> is completed; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsCompleted
        {
            get
            {
                if (_buildTracing)
                    GameLog.Print("checking whether BuildProject IsCompleted...");
                if (_industryInvested < IndustryRequired)
                {
                    if (_buildTracing)
                        GameLog.Print("BuildProject: _industryInvested = {0}  < IndustryRequired = {1}", _industryInvested, IndustryRequired);
                    return false;
                }

                // if it is a shipyard with an uncomplete ship building -> IsCompleted = false
                //if ()
                    foreach (ResourceType resource in EnumUtilities.GetValues<ResourceType>())
                    {
                        if (_resourcesInvested[resource] < ResourcesRequired[resource])
                        {
                        if (_buildTracing)
                            GameLog.Print("BuildProject: Resource: {0}: _resourcesInvested[resource] = {1}  < ResourcesRequired[resource] = {2}", resource, _resourcesInvested, ResourcesRequired[resource]);
                        return false;
                        }
                    }

                    if (_buildTracing)
                        GameLog.Print("BuildProject IsCompleted..should be true, Building should be done ");

                return true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this project is cancelled.
        /// </summary>
        /// <value>
        /// <c>true</c> if this project is cancelled; otherwise, <c>false</c>.
        /// </value>
        public bool IsCancelled
        {
            get { return GetFlag(BuildProjectFlags.Cancelled); }
            protected set { SetFlag(BuildProjectFlags.Cancelled, value); }
        }

        /// <summary>
        /// Gets a value indicating whether this project is rushed.
        /// </summary>
        /// <value>
        /// <c>true</c> if this project is rushed; otherwise, <c>false</c>.
        /// </value>
        public bool IsRushed
        {
            get { return GetFlag(BuildProjectFlags.Rushed); }
            set
            {
                SetFlag(BuildProjectFlags.Rushed, value); OnPropertyChanged("IsRushed");
            }
        }

        /// <summary>
        /// Gets a value indicating whether work on this project has been paused.
        /// </summary>
        /// <value>
        /// <c>true</c> if work on this project is paused; otherwise, <c>false</c>.
        /// </value>
        public bool IsPaused
        {
            get { return GetFlag(BuildProjectFlags.OnHold); }
            set { SetFlag(BuildProjectFlags.OnHold, value); }
        }

        /// <summary>
        /// Gets a value indicating whether the project is partially complete.
        /// </summary>
        /// <value>
        /// <c>true</c> if this project is partially complete; otherwise, <c>false</c>.
        /// </value>
        public bool IsPartiallyComplete
        {
            get
            {
                // works but shows every shipyard of each race      GameLog.Print("IsPartiallyComplete = {0}percent, {1}", this.PercentComplete, this.PercentComplete > 0.0f);
                return (PercentComplete > 0.0f);
            }
        }

        /// <summary>
        /// Gets the percent completion of this <see cref="BuildProject"/>.
        /// </summary>
        /// <value>The percent completion.</value>
        public virtual Percentage PercentComplete
        {
            get { return (Percentage)((double)_industryInvested / IndustryRequired); }
        }

        /// <summary>
        /// Gets the total industry required to complete this <see cref="BuildProject"/>.
        /// </summary>
        /// <value>The industry required.</value>
        protected virtual int IndustryRequired
        {
            get { return BuildDesign.BuildCost; }
        }

        /// <summary>
        /// Gets the total resources required to complete this <see cref="BuildProject"/>.
        /// </summary>
        /// <value>The resources required.</value>
        protected virtual ResourceValueCollection ResourcesRequired
        {
            get { return BuildDesign.BuildResourceCosts; }
        }

        /// <summary>
        /// Gets the number of turns remaining until this <see cref="BuildProject"/> is completed.
        /// </summary>
        /// <value>The turns remaining.</value>
        public virtual int TurnsRemaining
        {
            get { return GetTimeEstimate(); }
        }

        protected bool GetFlag(BuildProjectFlags flag)
        {
            return (_flags & flag) == flag;
        }

        protected void ClearFlag(BuildProjectFlags flag)
        {
            _flags &= ~flag;
        }

        protected void SetFlag(BuildProjectFlags flag, bool value = true)
        {
            if (value)
                _flags |= flag;
            else
                ClearFlag(flag);
        }

        public void InvalidateTurnsRemaining()
        {
            OnPropertyChanged("TurnsRemaining");
        }

        #region INotifyPropertyChanged Members
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region IOwnedDataSerializableAndRecreatable Members
        public virtual void SerializeOwnedData(SerializationWriter writer, object context)
        {
            writer.WriteOptimized(_buildTypeId);
            writer.WriteOptimized(_industryInvested);
            writer.Write((byte)_location.X);
            writer.Write((byte)_location.Y);
            writer.WriteOptimized(_ownerId);
            writer.Write(_priority);
            writer.WriteObject(_resourcesInvested);
            writer.Write((byte)_flags);
        }

        public virtual void DeserializeOwnedData(SerializationReader reader, object context)
        {
            _buildTypeId = reader.ReadOptimizedInt32();
            _industryInvested = reader.ReadOptimizedInt32();
            _location = new MapLocation(reader.ReadByte(), reader.ReadByte());
            _ownerId = reader.ReadOptimizedInt32();
            _priority = reader.ReadByte();
            _resourcesInvested = (ResourceValueCollection)reader.ReadObject();
            _flags = (BuildProjectFlags)reader.ReadByte();
        }
        #endregion

        /// <summary>
        /// Gets the amount of industry available for investment during the current turn.
        /// </summary>
        /// <returns>The industry available.</returns>
        protected abstract int GetIndustryAvailable();

        /// <summary>
        /// Cancels this <see cref="BuildProject"/>.
        /// </summary>
        public virtual void Cancel()
        {
            IsCancelled = true;
        }

        /// <summary>
        /// Finishes this <see cref="BuildProject"/> and creates the newly constructed item.
        /// </summary>
        public virtual void Finish()
        {

            bool _tracingBuildProject = false;    // turn true if you need

            if (_buildTracing)
                _tracingBuildProject = true;

                if (_tracingBuildProject)
                GameLog.Print("trying to Finish a BuildProject...");

            var civManager = GameContext.Current.CivilizationManagers[Builder];

            TechObject spawnedInstance;

            if (civManager == null || !BuildDesign.TrySpawn(Location, Builder, out spawnedInstance))
                return;

            ItemBuiltSitRepEntry newEntry = null;
            if (spawnedInstance != null)
            {
                if (spawnedInstance.ObjectType == UniverseObjectType.Building)
                {
                    newEntry = new BuildingBuiltSitRepEntry(Builder, BuildDesign, Location, (spawnedInstance as Building).IsActive);
                    if (_tracingBuildProject)
                        GameLog.Print("newEntry = {0}", newEntry);
                }
            }

            if (newEntry == null)
            {
                if (_tracingBuildProject)
                    GameLog.Print("{1} built by {0} at {2}", Builder, BuildDesign, Location);
                newEntry = new ItemBuiltSitRepEntry(Builder, BuildDesign, Location);
            }

            if (_tracingBuildProject)
                GameLog.Print("creating SitRepEntry...");

            civManager.SitRepEntries.Add(newEntry);
        }

        /// <summary>
        /// Gets estimated number of turns until this <see cref="BuildProject"/> is completed.
        /// </summary>
        /// <returns>The time estimate.</returns>
        public virtual int GetTimeEstimate()
        {
            var industryAvailable = GetIndustryAvailable();
            var industryRemaining = IndustryRequired - IndustryInvested;

            var turns = industryRemaining / industryAvailable;
            
            if (industryRemaining % industryAvailable > 0)
                ++turns;

            if (turns == 0 && !IsCompleted)
                turns = 1;

            return turns;
        }

        /// <summary>
        /// Gets the current industry costs left to this <see cref="BuildProject"/>
        /// </summary>
        /// <returns>The time estimate.</returns>
        public virtual int GetCurrentIndustryCost()
        {
            return IndustryRequired - IndustryInvested;
        }

        /// <summary>
        /// Gets the current industry costs left to this <see cref="BuildProject"/>
        /// </summary>
        /// <returns>The time estimate.</returns>
        public virtual int GetCurrentResourceCost(ResourceType resource)
        {
            return ResourcesRequired[resource] - ResourcesInvested[resource];
        }

        /// <summary>
        /// Advances this <see cref="BuildProject"/> by one turn.
        /// </summary>
        /// <param name="industry">The industry available for investment.</param>
        /// <param name="resources">The resources available for investment.</param>
        /// <remarks>
        /// Prior to returning, this function updates the <paramref name="industry"/>
        /// and <paramref name="resources"/> parameters to reflect the values
        /// remaining (the surplus that was not invested).
        /// </remarks>
        public void Advance(ref int industry, ResourceValueCollection resources)
        {
            if (IsPaused || IsCancelled)
                return;

            AdvanceOverride(ref industry, resources);
        }

        protected virtual void AdvanceOverride(ref int industry, ResourceValueCollection resources)
        {
            var timeEstimate = GetTimeEstimate();
            if (timeEstimate <= 0)
                return;

            if (_buildTracing)
                GameLog.Print("------------------------------------");

            var delta = Math.Min(
                industry,
                Math.Max(0, IndustryRequired - _industryInvested));

            if (_buildTracing)
                GameLog.Print("delta = {0}", delta);


            industry -= delta;

            IndustryInvested += delta;
            if (_buildTracing)
                GameLog.Print("IndustryInvested = {0}", IndustryInvested);
            ApplyIndustry(delta);
            OnPropertyChanged("IndustryInvested");

            ClearFlag(
                BuildProjectFlags.DeuteriumShortage |
                BuildProjectFlags.DilithiumShortage |
                BuildProjectFlags.RawMaterialsShortage);
            
            var resourceTypes = EnumHelper.GetValues<ResourceType>();

            for (var i = 0; i < resourceTypes.Length; i++)
            {
                var resource = resourceTypes[i];

                delta = ResourcesRequired[resource] - _resourcesInvested[resource];
                if (_buildTracing)
                    GameLog.Print("delta = {0} for  {1}", delta, resource);

                if (delta <= 0)
                    continue;

                if (timeEstimate == 1 &&
                    delta > resources[resource])
                {
                    SetFlag((BuildProjectFlags)((int)BuildProjectFlags.DeuteriumShortage << i));
                    if (_buildTracing)
                        GameLog.Print("delta = {0} for {1}", delta, resource);
                }

                if (resources[resource] <= 0)
                    continue;

                delta /= timeEstimate;
                delta += ((ResourcesRequired[resource] - _resourcesInvested[resource]) % timeEstimate);
                
                delta = Math.Min(delta, resources[resource]);

                resources[resource] -= delta;
                _resourcesInvested[resource] += delta;

                ApplyResource(resource, delta);
            }

            OnPropertyChanged("TurnsRemaining");
            OnPropertyChanged("PercentComplete");
            OnPropertyChanged("IsPartiallyComplete");

            if (IsCompleted)
                OnPropertyChanged("IsCompleted");
        }

        /// <summary>
        /// Determines whether a given project is equivalent to this <see cref="BuildProject"/>
        /// (the designs of the items being constructed are the same).
        /// </summary>
        /// <param name="project">The project.</param>
        /// <returns>
        /// <c>true</c> if the project is equivalent; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool IsEquivalent(BuildProject project)
        {
            if (project == null)
                return false;
            if (project.GetType() != GetType())
                return false;
            if (project._buildTypeId != _buildTypeId)
                return false;
            return true;
        }

        /// <summary>
        /// Creates an equivalent clone of this <see cref="BuildProject"/>.
        /// </summary>
        /// <returns>The clone.</returns>
        public virtual BuildProject CloneEquivalent()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Applies the specified amount of industry to this <see cref="BuildProject"/>.
        /// </summary>
        /// <param name="industry">The industry to apply.</param>
        protected virtual void ApplyIndustry(int industry) { }

        /// <summary>
        /// Applies the specified amount of a given resource to this <see cref="BuildProject"/>.
        /// </summary>
        /// <param name="resource">The type of resource.</param>
        /// <param name="amount">The amount to apply.</param>
        protected virtual void ApplyResource(ResourceType resource, int amount) { }

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">Name of the property that changed.</param>
        protected void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}