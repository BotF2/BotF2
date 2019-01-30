  using Supremacy.Annotations;
using Supremacy.Buildings;
using Supremacy.Collections;
using Supremacy.Economy;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Orbitals;
using Supremacy.Tech;
using Supremacy.Types;
using Supremacy.Universe;
using Supremacy.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

namespace Supremacy.Combat
{
    public enum InvasionAction
    {
        AttackOrbitalDefenses,
        BombardPlanet,
        UnloadAllOrdinance,
        LandTroops,
        StandDown
    }

    public enum InvasionStatus
    {
        InProgress,
        Victory,
        Defeat,
        Stalemate
    }

    public enum InvasionTargetingStrategy
    {
        MaximumPrecision,
        Balanced,
        MaximumDamage
    }

    [Serializable]
    public class InvasionOrders
    {
        private readonly int _invasionId;
        private readonly InvasionAction _action;
        private readonly InvasionTargetingStrategy _targetingStrategy;
        private readonly int[] _selectedTransports;

        public InvasionOrders(int invasionId, InvasionAction action, InvasionTargetingStrategy targetingStrategy, params InvasionUnit[] selectedTransports)
        : this(invasionId, action, targetingStrategy, (IEnumerable<InvasionUnit>)selectedTransports) { }

        public InvasionOrders(int invasionId, InvasionAction action, InvasionTargetingStrategy targetingStrategy, IEnumerable<InvasionUnit> selectedTransports)
        {
            _invasionId = invasionId;
            _action = action;
            _targetingStrategy = targetingStrategy;
            _selectedTransports = selectedTransports.Select(o => o.ObjectID).ToArray();
        }

        public int InvasionID
        {
            get { return _invasionId; }
        }

        public InvasionAction Action
        {
            get { return _action; }
        }

        public InvasionTargetingStrategy TargetingStrategy
        {
            get { return _targetingStrategy; }
        }

        public IEnumerable<int> SelectedTransports
        {
            get { return _selectedTransports; }
        }
    }

    [Serializable]
    [DebuggerDisplay("{Name} ({Health.PercentFilled})")]
    public class InvasionUnit : IEquatable<InvasionUnit>
    {
        private readonly int _objectId;
        private readonly int _ownerId;
        private readonly int _designid = TechObjectDesign.InvalidDesignID;
        private readonly Meter _health;

        public InvasionUnit([NotNull] UniverseObject source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            _objectId = source.ObjectID;
            _ownerId = source.OwnerID;
            _health = new Meter(0, 0, 0);
        }

        protected InvasionUnit([NotNull] UniverseObject source, TechObjectDesign design)
            : this(source)
        {
            if (design != null)
                _designid = design.DesignID;
        }

        public int ObjectID
        {
            get { return _objectId; }
        }

        public int OwnerID
        {
            get { return _ownerId; }
        }

        public TechObjectDesign Design
        {
            get
            {
                if (_designid == TechObjectDesign.InvalidDesignID)
                    return null;
                return GameContext.Current.TechDatabase[_designid];
            }
        }

        public virtual string Name
        {
            get { return Source.Name; }
        }

        public UniverseObject Source
        {
            get { return GameContext.Current.Universe.Objects[_objectId]; }
        }

        public Meter Health
        {
            get { return _health; }
        }

        public bool IsDestroyed
        {
            get { return _health.IsMinimized; }
        }

        public virtual void Destroy()
        {
            Health.CurrentValue = Health.Minimum;
        }

        public virtual int TakeDamage(int damage)
        {
            damage = Math.Abs(damage);
            return Health.AdjustCurrent(-damage);
        }

        public virtual void CommitSourceChanges() { }

        public virtual bool Equals(InvasionUnit other)
        {
            if (ReferenceEquals(this, other))
                return true;
            if (ReferenceEquals(null, other))
                return false;
            return other._objectId.Equals(_objectId);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as InvasionUnit);
        }

        public override int GetHashCode()
        {
            return _objectId.GetHashCode();
        }
    }

    [Serializable]
    public class InvasionOrbital : InvasionUnit
    {
        private readonly CombatWeapon[] _weapons;
        [NonSerialized] private ArrayWrapper<CombatWeapon> _weaponsWrapper;

        public InvasionOrbital([NotNull] Orbital source)
            : base(source, source.Design)
        {
            _weapons = CombatWeapon.CreateWeapons(source);
            _weaponsWrapper = new ArrayWrapper<CombatWeapon>(_weapons);

            HookMeterChangeListeners();

            UpdateHealthMeter();
        }

        public Meter ShieldStrength
        {
            get { return Source.ShieldStrength; }
        }

        public Meter HullStrength
        {
            get { return Source.HullStrength; }
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            _weaponsWrapper = new ArrayWrapper<CombatWeapon>(_weapons);
        }

        private void HookMeterChangeListeners()
        {
            var orbital = Source;

            orbital.HullStrength.CurrentValueChanged += OnHealthMeterChanged;
            orbital.ShieldStrength.CurrentValueChanged += OnHealthMeterChanged;
        }

        private void OnHealthMeterChanged(object sender, MeterChangedEventArgs e)
        {
            UpdateHealthMeter();
        }

        private void UpdateHealthMeter()
        {
            var orbital = Source;

            Health.Maximum = orbital.HullStrength.Maximum + orbital.ShieldStrength.Maximum;
            Health.Reset(orbital.HullStrength.CurrentValue + orbital.ShieldStrength.CurrentValue);
        }

        public IIndexedCollection<CombatWeapon> Weapons
        {
            get { return _weaponsWrapper; }
        }

        public new Orbital Source
        {
            get { return (Orbital)base.Source; }
        }

        public override void Destroy()
        {
            ShieldStrength.CurrentValue = ShieldStrength.Minimum;
            HullStrength.CurrentValue = HullStrength.Minimum;
        }

        public override int TakeDamage(int damage)
        {
            damage = Math.Abs(damage);

            var orbital = Source;
            var shieldDamage = orbital.ShieldStrength.AdjustCurrent(-damage);

            return shieldDamage + orbital.HullStrength.AdjustCurrent(-(damage - shieldDamage));
        }

        public override void CommitSourceChanges()
        {
            var orbital = Source;

            if (IsDestroyed)
            {
                orbital.Destroy();
                return;
            }

            orbital.ShieldStrength.UpdateAndReset();
            orbital.HullStrength.UpdateAndReset();
        }

        public void Recharge()
        {
            if (!IsDestroyed)
            {
                _weapons.ForEach(o => o.Recharge());
                Source.RegenerateShields();
            }
        }
    }

    [Serializable]
    [DataContract]
    public class InvasionUnitCollection : KeyedCollectionBase<int, InvasionUnit>
    {
        public InvasionUnitCollection()
            : base(o => o.ObjectID) { }
    }

    [Serializable]
    [KnownType(typeof(InvasionUnit))]
    [KnownType(typeof(InvasionOrbital))]
    [KnownType(typeof(InvasionStructure))]
    [KnownType(typeof(InvasionFacility))]
    [KnownType(typeof(CombatWeapon))]
    [KnownType(typeof(ArrayWrapper<CombatWeapon>))]
    public class InvasionArena : IEquatable<InvasionArena>
    {
        public const int MaxRounds = 5;

        private readonly int _invasionId;
        private readonly int _colonyId;
        private readonly int _invaderId;
        private readonly int _defenderId;
        private readonly Meter _population;
        private readonly Meter _colonyShieldStrength;
        private readonly List<InvasionUnit> _invadingUnits;
        private readonly List<InvasionUnit> _defendingUnits;

        [NonSerialized]
        private Lazy<Colony> _colony;

        private InvasionStatus _status;
        private int _defenderCombatStrength;
        private int _invaderCombatStrength;
        private bool _hasOrbitalDefenses;
        private bool _hasAttackingUnits;
        private bool _canLandTroops;
        public bool IsMultiplayerGame;

        public InvasionArena([NotNull] Colony colony, [NotNull] Civilization invader)
        {
            if (colony == null)
                throw new ArgumentNullException("colony");
            if (invader == null)
                throw new ArgumentNullException("invader");

            _invasionId = GameContext.Current.GenerateID();

            _colonyId = colony.ObjectID;
            _invaderId = invader.CivID;
            _defenderId = colony.OwnerID;
            _colony = new Lazy<Colony>(() => GameContext.Current.Universe.Get<Colony>(_colonyId), false);
            _population = colony.Population;
            _colonyShieldStrength = colony.ShieldStrength;
            _defendingUnits = new List<InvasionUnit>();
            _invadingUnits = new List<InvasionUnit>();

            foreach (OrbitalBattery OB in colony.OrbitalBatteries)
                if (OB.IsActive)
                    _defendingUnits.Add(new InvasionOrbital(OB));
            _defendingUnits.AddRange(colony.Buildings.Select(b => new InvasionStructure(b)));

            // ReSharper disable AccessToModifiedClosure
            foreach (var productionCategory in EnumHelper.GetValues<ProductionCategory>())
            {
                var facilityDesign = colony.GetFacilityType(productionCategory);
                if (facilityDesign == null)
                    continue;

                _defendingUnits.AddRange(
                    Enumerable.Range(0, colony.GetTotalFacilities(productionCategory))
                              .Select(i => new InvasionFacility(colony, productionCategory, i)));
            }
            // ReSharper restore AccessToModifiedClosure


            var invadingUnits =
                (
                    from f in GameContext.Current.Universe.FindAt<Fleet>(colony.Location)
                    where f.OwnerID == _invaderId && f.Order is AssaultSystemOrder
                    from ship in f.Ships
                    select ship
                    //where ship.IsCombatant || ship.ShipType == ShipType.Transport
                );

            _invadingUnits.AddRange(invadingUnits.Select(o => new InvasionOrbital(o)));

            RoundNumber = 1;
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            _colony = new Lazy<Colony>(() => GameContext.Current.Universe.Get<Colony>(_colonyId), false);
        }

        public int InvasionID
        {
            get { return _invasionId; }
        }

        public int InvaderID
        {
            get { return _invaderId; }
        }

        public int DefenderID
        {
            get { return _defenderId; }
        }

        public Civilization Invader
        {
            get { return GameContext.Current.Civilizations[_invaderId]; }
        }

        public Civilization Defender
        {
            get { return GameContext.Current.Civilizations[_defenderId]; }
        }

        public int RoundNumber { get; set; }

        internal bool AttackOccurred { get; set; }
        internal bool InvasionOccurred { get; set; }
        internal bool BombardmentOccurred { get; set; }
        internal bool UnloadAllOrdinanceOccurred { get; set; }
        internal InvasionTargetingStrategy WorstTargetingStrategyUsed { get; set; }

        public string Name
        {
            get { return Colony.Name; }
        }

        public Meter Population
        {
            get { return _population; }
        }

        public Meter ColonyShieldStrength
        {
            get { return _colonyShieldStrength; }
        }

        public bool HasOrbitalDefenses
        {
            get { return _hasOrbitalDefenses; }
        }

        public bool HasAttackingUnits
        {
            get { return _hasAttackingUnits; }
        }

        public bool CanLandTroops
        {
            get { return _canLandTroops; }
        }

        public Colony Colony
        {
            get { return _colony.Value; }
        }

        public int InvaderCombatStrength
        {
            get { return _invaderCombatStrength; }
        }

        public int DefenderCombatStrength
        {
            get { return _defenderCombatStrength; }
        }

        public IEnumerable<InvasionUnit> InvadingUnits
        {
            get { return _invadingUnits; }
        }

        public IEnumerable<InvasionUnit> DefendingUnits
        {
            get { return _defendingUnits; }
        }

        public InvasionStatus Status
        {
            get { return _status; }
        }

        public bool IsFinished
        {
            get { return _status != InvasionStatus.InProgress; }
        }

        public void Retreat()
        {
            if (_status != InvasionStatus.InProgress)
                return;

            // If any orbitals are left when retreating, then the invasion is a defeat, else it's a standoff, the invader being merciful
            if (!_defendingUnits.OfType<InvasionOrbital>().All(o => o.IsDestroyed))
                _status = InvasionStatus.Defeat;
            else
                _status = InvasionStatus.Stalemate;
        }

        public void Stalemate()
        {
            if (_status == InvasionStatus.InProgress)
                _status = InvasionStatus.Stalemate;
        }

        public void Update()
        {
            _defenderCombatStrength = ComputeDefenderCombatStrength();
            _invaderCombatStrength = ComputeInvaderCombatStrength();
            _hasOrbitalDefenses = _defendingUnits.OfType<InvasionOrbital>().Any(o => !o.IsDestroyed);
            _hasAttackingUnits = _invadingUnits.OfType<InvasionOrbital>().Any(o => !o.IsDestroyed && o.Source.IsCombatant);
            _canLandTroops = _invadingUnits.Where(o => !o.IsDestroyed).Select(o => o.Source).OfType<Ship>().Any(o => o.ShipType == ShipType.Transport);

            GameLog.Core.Combat.DebugFormat("_canLandTroops(Transport Ships) = {0}, and/but ColonyShieldStrength = {1}, Last Value = {2}",
                _canLandTroops, ColonyShieldStrength.CurrentValue, ColonyShieldStrength.LastValue);
            if (ColonyShieldStrength.CurrentValue > 0)
                _canLandTroops = false;


            if (_status != InvasionStatus.InProgress)
                return;

            if (_population.CurrentValue == 0 || Colony.OwnerID == _invaderId)
                _status = InvasionStatus.Victory;
            else if (_invadingUnits.All(o => o.IsDestroyed))
                _status = InvasionStatus.Defeat;
            else if (IsMultiplayerGame && RoundNumber == 5) // Change roundnumber in MP to 5 (was 3)
                _status = InvasionStatus.Stalemate;
            else if (RoundNumber > MaxRounds)
                _status = InvasionStatus.Stalemate;
            else
                _status = InvasionStatus.InProgress;
        }

        private int ComputeDefenderCombatStrength()
        {
            return CombatHelper.ComputeGroundCombatStrength(Colony.Owner, Colony.Location, _population.CurrentValue);
        }

        private int ComputeInvaderCombatStrength()
        {
            var location = Colony.Location;
            var civilization = Colony.Owner;

            return _invadingUnits
                .OfType<InvasionOrbital>()
                .Where(o => !o.IsDestroyed)
                .Select(o => o.Source)
                .OfType<Ship>()
                .Where(o => o.ShipType == ShipType.Transport)
                .Select(o => o.ShipDesign.WorkCapacity)
                .Sum(pop => CombatHelper.ComputeGroundCombatStrength(civilization, location, pop));
        }

        public bool Equals(InvasionArena other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return other._invasionId == _invasionId;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as InvasionArena);
        }

        public override int GetHashCode()
        {
            return _invasionId;
        }
    }

    [Serializable]
    public class InvasionStructure : InvasionUnit
    {
        public InvasionStructure([NotNull] Building building)
            : base(building, building.Design)
        {
            if (building == null)
                throw new ArgumentNullException("building");

            var buildingDesign = building.BuildingDesign;
            var techLevel = buildingDesign.TechRequirements.Max(o => o.Value);
            var baseHealth = buildingDesign.BuildCost;

            Health.Maximum = baseHealth;
            Health.ReplenishAndReset();
        }

        public Building Building
        {
            get { return (Building)Source; }
        }

        public override void CommitSourceChanges()
        {
            if (IsDestroyed)
                Building.Destroy();
        }
    }

    [Serializable]
    public class InvasionFacility : InvasionUnit
    {
        private readonly ProductionCategory _category;
        private readonly int _index;

        public InvasionFacility([NotNull] Colony colony, ProductionCategory productionCategory, int index)
            : base(colony, colony.GetFacilityType(productionCategory))
        {
            if (colony == null)
                throw new ArgumentNullException("colony");

            _category = productionCategory;
            _index = index;

            var facilityDesign = colony.GetFacilityType(productionCategory);
            var techLevel = facilityDesign.TechRequirements.Max(o => o.Value);
            var baseHealth = facilityDesign.BuildCost;

            Health.Maximum = baseHealth;
            Health.ReplenishAndReset();
        }

        public ProductionCategory Category
        {
            get { return _category; }
        }

        public override string Name
        {
            get { return ((Colony)Source).GetFacilityType(_category).LocalizedName; }
        }

        public override void CommitSourceChanges()
        {
            if (IsDestroyed)
                ((Colony)Source).RemoveFacility(_category);
        }

        public override bool Equals(InvasionUnit other)
        {
            return base.Equals(other) &&
                   ((InvasionFacility)other)._category == _category &&
                   ((InvasionFacility)other)._index == _index;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ _index;
            }
        }
    }

    public delegate void SendInvasionUpdateCallback(InvasionEngine engine, InvasionArena update);
    public delegate void NotifyInvasionEndedCallback(InvasionEngine engine);

    [Serializable]
    public class InvasionEngine
    {
        private const double PopulationDamageMultiplier = 0.1;

        private readonly SendInvasionUpdateCallback _sendUpdateCallback;
        private readonly NotifyInvasionEndedCallback _invasionEndedCallback;

        private InvasionArena _invasionArena;
        private InvasionOrders _orders;
        private Dictionary<ExperienceRank, double> _experienceAccuracy;

        public InvasionEngine([NotNull] SendInvasionUpdateCallback sendUpdateCallback, [NotNull] NotifyInvasionEndedCallback invasionEndedCallback)
        {
            if (sendUpdateCallback == null)
                throw new ArgumentNullException("sendUpdateCallback");
            if (invasionEndedCallback == null)
                throw new ArgumentNullException("invasionEndedCallback");

            _sendUpdateCallback = sendUpdateCallback;
            _invasionEndedCallback = invasionEndedCallback;
        }

        public InvasionArena InvasionArena
        {
            get { return _invasionArena; }
        }

        public void BeginInvasion([NotNull] InvasionArena invasionArena)
        {
            if (invasionArena == null)
                throw new ArgumentNullException("invasionArena");

            VerifyNoInvasionInProgress();

            _invasionArena = invasionArena;
            _invasionArena.Update();    // make sure all stats are up-to-date

            //TODO: Didn't this get moved out of CombatEngine?
            var accuracyTable = GameContext.Current.Tables.ShipTables["AccuracyModifiers"];
            _experienceAccuracy = new Dictionary<ExperienceRank, double>();
            foreach (var rank in EnumHelper.GetValues<ExperienceRank>())
            { 
                // _experienceAccuracy[rank] = Number.ParseDouble(accuracyTable[rank.ToString()][0]);
                double modifier;
                if (Double.TryParse(accuracyTable[rank.ToString()][0], out modifier))
                {
                    _experienceAccuracy[rank] = modifier;
                }
                else
                    _experienceAccuracy[rank] = 0.75;
            }

            SendUpdate();
        }

        private void SendUpdate()
        {
            _sendUpdateCallback(this, _invasionArena);
        }

        public void SubmitOrders([NotNull] InvasionOrders orders)
        {
            if (orders == null)
                throw new ArgumentNullException("orders");

            VerifyInvasionInProgress();

            if (orders.InvasionID != _invasionArena.InvasionID)
                throw new ArgumentException("Orders submitted for a different invasion.", "orders");

            if (_invasionArena.Status != InvasionStatus.InProgress)
                throw new InvalidOperationException("Orders submitted for an invasion which is no longer in progress.");

            if (orders.Action == InvasionAction.AttackOrbitalDefenses)
            {
                if (!_invasionArena.HasOrbitalDefenses)
                    throw new InvalidOperationException("Cannot give order to attack orbital defenses because no defenses remain.");
                if (!_invasionArena.HasAttackingUnits)
                    throw new InvalidOperationException("Cannot give order to attack orbital defenses because no combat-capable attacking units remain.");
            }

            _orders = orders;

            ProcessRound();

            if (orders.Action == InvasionAction.UnloadAllOrdinance)
            {
                while (!_invasionArena.IsFinished)
                    ProcessRound();
            }

            SendUpdate();

            if (_invasionArena.Status != InvasionStatus.InProgress)
                FinishInvasion();
        }

        private void ProcessRound()
        {
            //RechargeUnits(); // commented, so that Recharge only after invasion is over
            try
            {
                if (_orders.Action == InvasionAction.StandDown)
                {
                    if (_invasionArena.RoundNumber > 1)
                    {
                        var defendingUnits = _invasionArena.DefendingUnits.Where(o => !o.IsDestroyed).OfType<InvasionOrbital>().ToList();
                        var invadingUnits = _invasionArena.InvadingUnits.Where(o => !o.IsDestroyed).OfType<InvasionOrbital>().ToList();
                        // Current X
                        //  ProcessSpaceCombat(invadingUnits, defendingUnits); // out-commented to enable retreat.
                    }
                    _invasionArena.Update();
                    _invasionArena.Retreat();
                    _invasionArena.Update();
                    return;
                }
            }
            catch (Exception e)
            {
                GameLog.Core.Combat.DebugFormat("##### Problem at Invasion-StandDown {0}", e);
            }

            if (_invasionArena.HasOrbitalDefenses)
            {
                var defendingUnits = _invasionArena.DefendingUnits.Where(o => !o.IsDestroyed).OfType<InvasionOrbital>().ToList();
                var invadingUnits = _invasionArena.InvadingUnits.Where(o => !o.IsDestroyed).OfType<InvasionOrbital>().ToList();
                ProcessSpaceCombat(invadingUnits, defendingUnits);
            }

            if (_orders.Action == InvasionAction.BombardPlanet || _orders.Action == InvasionAction.UnloadAllOrdinance)
            {
                if (_orders.Action == InvasionAction.BombardPlanet)
                    GameLog.Core.Combat.DebugFormat("Order is Bombardment");

                if (_orders.Action == InvasionAction.UnloadAllOrdinance)
                    GameLog.Core.Combat.DebugFormat("Order is UnloadAllOrdinance");

                ProcessBombardment();
            }

            if (_orders.Action == InvasionAction.LandTroops)
            {
                GameLog.Core.Combat.DebugFormat("Order is LandTroops");
                ProcessGroundCombat();
            }

            ++_invasionArena.RoundNumber;

            _invasionArena.Update();
        }

        private void ProcessGroundCombat()
        {
            _invasionArena.Update();

            var transports = new List<InvasionOrbital>();

            foreach (var transportId in _orders.SelectedTransports)
            {
                var transport = _invasionArena.InvadingUnits.FirstOrDefault(o => o.ObjectID == transportId);
                if (transport != null && !transport.IsDestroyed)
                    transports.Add((InvasionOrbital)transport);
            }

            if (transports.Count == 0)
                return;

            var defendingUnits = _invasionArena.DefendingUnits.Where(o => !o.IsDestroyed).OfType<InvasionOrbital>().ToList();

            /*
             * Any remaining orbital weapons platforms which have charged weapons will have a chance to shoot down
             * the invader's troop transports as they try to land.
             */

            ProcessSpaceCombat(transports, defendingUnits);

            transports.RemoveWhere(o => o.IsDestroyed);

            if (transports.Count == 0)
                return;

            _invasionArena.Update();

            var defenderCombatStrength = _invasionArena.DefenderCombatStrength;
            //GameLog.Core.Combat.DebugFormat("GroundCombat - LandingTroops? : defenderCombatStrength = {0}, attacking Transports = {1}",
            //    defenderCombatStrength, transports.Count);

            var colony = _invasionArena.Colony;
            var invader = GameContext.Current.Civilizations[_invasionArena.InvaderID];

            while (defenderCombatStrength > 0 &&
                   transports.Count != 0)
            {
                //GameLog.Core.Combat.DebugFormat("GroundCombat - LandingTroops? - BEFORE: defenderCombatStrength = {0}, attacking Transports = {1}",
                //    defenderCombatStrength, transports.Count);

                var transport = transports[0];

                var transportCombatStrength = CombatHelper.ComputeGroundCombatStrength(
                    invader,
                    colony.Location,
                    ((Ship)transport.Source).ShipDesign.WorkCapacity);
                    GameLog.Core.Combat.DebugFormat("GroundCombat - LandingTroops - transportCombatStrength BEFORE random = {0}",
                            transportCombatStrength);

                int randomResult = RandomProvider.Shared.Next(1, 21);   //  limits random to 20 %
                transportCombatStrength = transportCombatStrength - (transportCombatStrength * randomResult / 100);
                //                                 100 -             (     100                * 15        / 100)    
                GameLog.Core.Combat.DebugFormat("GroundCombat - LandingTroops? - BEFORE: defenderCombatStrength = {0}, attacking Transports = {1}",
                        defenderCombatStrength, transports.Count);

                GameLog.Core.Combat.DebugFormat("GroundCombat - LandingTroops - transportCombatStrength AFTER random = {0}, random in Percent = {1}",
                    transportCombatStrength, randomResult );

                defenderCombatStrength -= transportCombatStrength;

                if (defenderCombatStrength >= 0)
                {
                    transports.RemoveAt(0);
                    transport.Destroy();
                }

                GameLog.Core.Combat.DebugFormat("GroundCombat - LandingTroops? - AFTER: defenderCombatStrength = {0}, attacking Transports = {1}",
                    defenderCombatStrength, transports.Count);


                //        GameLog.Core.Combat.DebugFormat("GroundCombat - LandingTroops? : Target Name = {0}, ID = {1} Design = {2}, health = {3}",
                //targetUnit.Name, targetUnit.ObjectID, targetUnit.Design, targetUnit.Health);
            }

            if (defenderCombatStrength > 0 || transports.Count == 0)
                return;

            if (transports.Count != 0)
            {
                transports[0].Destroy();
                transports.RemoveAt(0);
            }

            colony.TakeOwnership(_invasionArena.Invader, true);
            _invasionArena.InvasionOccurred = true;
        }

        private void ProcessBombardment()
        {
            // Update Strike Cruiser vs. Shields // Exchange ProcessBombardment
            var chanceTree = GetBaseGroundTargetHitChanceTree();
            var totalPopDamage = 0d;

            foreach (var unit in _invasionArena.InvadingUnits.OfType<InvasionOrbital>().Where(CanBombard))
            {
                var accuracyThreshold = 0d;

                var ship = unit.Source as Ship;
                if (ship != null)
                    // use standard 0.5 if odd number where returned
                    // bug of accuary modifer and targetdamagecontrol needs to be addressed later
                    if (ship.GetAccuracyModifier() < 0.1 || ship.GetAccuracyModifier() > 1)
                    {
                        accuracyThreshold = 1d - 0.5;
                    }
                    else
                    {
                        accuracyThreshold = 1d - ship.GetAccuracyModifier(); //why -4?
                    }

                if (chanceTree.IsEmpty)
                {
                    chanceTree = GetBaseGroundTargetHitChanceTree();
                    if (chanceTree.IsEmpty)
                        break;
                }

                var target = chanceTree.Take();
                var defenseMultiplier = CombatHelper.ComputeGroundDefenseMultiplier(_invasionArena.Colony);

                foreach (var weapon in unit.Weapons.Where(o => o.CanFire))
                {
                    if (target == null)
                        break;

                    var maxDamage = weapon.MaxDamage.CurrentValue;
                  

                    if (ship.ShipType != ShipType.StrikeCruiser) //any non-strike Cruiser
                    {
                        // Weapons reduced to 2% for any ordinary  ship
                        maxDamage = Convert.ToInt32(weapon.MaxDamage.CurrentValue * 0.03); // Insert Result to maxDamage as Integer
                    }
                    else
                    {
                        maxDamage = Convert.ToInt32(weapon.MaxDamage.CurrentValue * 0.30); // StrikeCruiser deal 20% weapondamage. Should work for Borg Strike Diamonds, but untested.
                    }
                    // Now, check for the best Orbital Assault Cruiser
                    if (ship.Owner.Key == "CARD_STRIKE_CRUISER_II"
                        || ship.Owner.Key == "CARD_STRIKE_CRUISER_III"
                        || ship.Owner.Key == "TERRAN_CRUISER_I"
                        || ship.Owner.Key == "TERRAN_STRIKE_CRUISER_I"
                        || ship.Owner.Key == "TERRAN_STRIKE_CRUISER_II"
                        || ship.Owner.Key == "TERRAN_STRIKE_CRUISER_III")
                        maxDamage = Convert.ToInt32(weapon.MaxDamage.CurrentValue * 0.40); // 30 % for best suitable ships
                    // Cardassian Keldon class Strike Cruiser
                    // Terran Constiution Class
                    // Terran Strike Cruiser

                    if (ship.Design.Key.Contains("KLING_CRUISER") && !ship.Design.Key.Contains("HEAVY") && !ship.Design.Key.Contains("STRIKE"))
                        maxDamage = Convert.ToInt32(weapon.MaxDamage.CurrentValue * 0.25); // Klingon BattleCruiser work both as anti-ship-Warship (100%) as well as a StrikeCruiser (17.5%)

                    if (ship.Design.Key.Contains("CUBE"))  // All cubes are okay when it comes to orbital bombardment (15%)
                    {
                        // Change damage to 10% if Borg Cubes are firing
                        maxDamage = Convert.ToInt32(weapon.MaxDamage.CurrentValue * 0.10);
                    }

                    maxDamage -= _invasionArena.ColonyShieldStrength.AdjustCurrent(-maxDamage);

                    weapon.Discharge();

                    if (maxDamage <= 0)
                        continue;

                    var confirmedHit = (RandomProvider.Shared.NextDouble() >= accuracyThreshold);
                    if (confirmedHit)
                    {
                        var targetUnit = target as InvasionUnit;
                        if (targetUnit != null)
                        {
                            var actualDamage = (int)Math.Ceiling(maxDamage / defenseMultiplier);

                            targetUnit.TakeDamage(maxDamage);   // units, aka buildings, do not beneficiate from Ground Defense bonuses

                            if (targetUnit.IsDestroyed)
                            {
                                if (chanceTree.IsEmpty)
                                    chanceTree = GetBaseGroundTargetHitChanceTree();
                                GameLog.Core.Combat.DebugFormat("Bombardment: Target Name = {0}, ID = {1} Design = {2}, health = {3}",
                                    targetUnit.Name, targetUnit.ObjectID, targetUnit.Design, targetUnit.Health);
                            }

                            if (_orders.TargetingStrategy == InvasionTargetingStrategy.MaximumDamage ||
                                _orders.Action == InvasionAction.UnloadAllOrdinance)     // here UnloadAllOrdinance is used
                            {
                                totalPopDamage += 1.0 * actualDamage; //from 0.5 to 1.0
                                //GameLog.Core.Combat.DebugFormat("Bombardment - MAXIMUM DAMAGE: Target Name = {0}, ID = {1} Design = {2}, health = {3}",
                                //    targetUnit.Name, targetUnit.ObjectID, targetUnit.Design, targetUnit.Health);
                            }
                            else if (_orders.TargetingStrategy == InvasionTargetingStrategy.Balanced)
                            {
                                totalPopDamage += 0.25 * actualDamage;
                                //GameLog.Core.Combat.DebugFormat("Bombardment - BALANCED: Target Name = {0}, ID = {1} Design = {2}, health = {3}",
                                //    targetUnit.Name, targetUnit.ObjectID, targetUnit.Design, targetUnit.Health);
                            }
                            else if (_orders.TargetingStrategy == InvasionTargetingStrategy.MaximumPrecision)
                            {
                                totalPopDamage += 0.05 * actualDamage; // from 0.125 to 0.05
                                //GameLog.Core.Combat.DebugFormat("Bombardment - maximum PRECISION: Target Name = {0}, ID = {1} Design = {2}, health = {3}",
                                //    targetUnit.Name, targetUnit.ObjectID, targetUnit.Design, targetUnit.Health);
                            }
                        }
                        //else if (target == colony) // ?
                        //{
                        //    totalPopDamage += maxDamage;
                        //}
                    }
                }
            }

            // If no pop damage occured don't even call AttackPopulation for it will force a minimum of a 1 pop dam
            if (totalPopDamage > 0)
                AttackPopulation(false, totalPopDamage * PopulationDamageMultiplier);

            _invasionArena.BombardmentOccurred = true;
            _invasionArena.AttackOccurred = true;

            if (_orders.Action == InvasionAction.UnloadAllOrdinance)
                _invasionArena.UnloadAllOrdinanceOccurred = true;

            if (_invasionArena.WorstTargetingStrategyUsed < _orders.TargetingStrategy)
                _invasionArena.WorstTargetingStrategyUsed = _orders.TargetingStrategy;
        }

        private void AttackPopulation(bool direct, double damage)
        {
            var colony = _invasionArena.Colony;

            if (!direct)
                damage *= colony.Population.PercentFilled;

            _invasionArena.Population.AdjustCurrent(-Math.Max(1, (int)damage));

            if (_invasionArena.Population.CurrentValue < 5)   // Bombardment ends with pop below 5 to avoid and an never ending bombardment, never going below 1
                _invasionArena.Population.AdjustCurrent(-1 * _invasionArena.Population.CurrentValue);
        }

        private static bool CanBombard(InvasionUnit unit)
        {
            if (unit == null || unit.IsDestroyed)
                return false;

            var orbital = unit as InvasionOrbital;

            return orbital != null &&
                   orbital.Source.IsCombatant &&
                   orbital.Weapons.Any(o => o.CanFire);
        }

        private ChanceTree<object> GetBaseGroundTargetHitChanceTree()
        {
            var chanceTree = new ChanceTree<object>();

            //var colony = _invasionArena.Colony;
            //var hitPopulationChance = colony.Population.CurrentValue / 10;
            var structureHitChance = 1;

            if (_orders.TargetingStrategy == InvasionTargetingStrategy.Balanced)
            {
                structureHitChance *= 2;
            }

            if (_orders.TargetingStrategy == InvasionTargetingStrategy.MaximumPrecision)
            {
                //hitPopulationChance /= 2;
                structureHitChance *= 3;
            }

            //chanceTree.AddChance(hitPopulationChance, colony);

            foreach (var structure in _invasionArena.DefendingUnits.Where(o => !o.IsDestroyed).OfType<InvasionStructure>())
                chanceTree.AddChance(structureHitChance, structure);

            foreach (var facility in _invasionArena.DefendingUnits.Where(o => !o.IsDestroyed).OfType<InvasionFacility>())
                chanceTree.AddChance(structureHitChance, facility);

            return chanceTree;
        }

        private void ProcessSpaceCombat(IList<InvasionOrbital> invadingUnits, IList<InvasionOrbital> defendingUnits)
        {
            // Update 1701M Name: Orbital Re-balancing. Replace full ProcessSpaceCombat with it.
            _invasionArena.AttackOccurred = true;

            var nonRetreatingUnits = (_orders.Action == InvasionAction.StandDown) ? defendingUnits : defendingUnits.Concat(invadingUnits);
            var unitsAbleToAttack = nonRetreatingUnits.Where(o => o.Weapons.Any(w => w.CanFire)).ToList();
            unitsAbleToAttack.RandomizeInPlace();

            var defenderTargets = new LinkedList<InvasionOrbital>(invadingUnits.Randomize());
            var invaderTargets = new LinkedList<InvasionOrbital>(defendingUnits.Randomize());


            var unitsAbleToAttackDef = nonRetreatingUnits // Sort Invasion lists
                .Where(o => o.Design.Key.Contains("ORBITAL"))
                .Where(o => o.Weapons.Any(w => w.CanFire)).ToList();
            unitsAbleToAttackDef.RandomizeInPlace();
            var unitsAbleToAttackInv = nonRetreatingUnits
                .Where(o => !o.Design.Key.Contains("ORBITAL"))
                .Where(o => o.Weapons.Any(w => w.CanFire)).ToList();
            unitsAbleToAttackInv.RandomizeInPlace();
            
            var z = 0; // inserted and initialized

            while (z < 1) // only one rounds now,Orbitals vs. Ships
            {
                if ((unitsAbleToAttack.Count == 0 ||
                   defenderTargets.Count == 0 ||
                   invaderTargets.Count == 0))
                    break; // Only one round if no more unites available

                // 
                int a = 0;
                int b = 0;
                // Sort combat Invasion List
                for (var i = 0; i < unitsAbleToAttack.Count;)
                {

                    if ((a < unitsAbleToAttackDef.Count && i % 2 == 0)
                        || (unitsAbleToAttackDef.Count > unitsAbleToAttackInv.Count
                        && i > unitsAbleToAttackInv.Count
                        && a < unitsAbleToAttackDef.Count))
                    {

                        unitsAbleToAttack[i] = unitsAbleToAttackDef[a];
                        a = a + 1;
                    }


                    if ((b < unitsAbleToAttackInv.Count && i % 2 == 1)
                        || (unitsAbleToAttackDef.Count < unitsAbleToAttackInv.Count
                        && i > unitsAbleToAttackDef.Count
                        && b < unitsAbleToAttackInv.Count))
                    {

                        unitsAbleToAttack[i] = unitsAbleToAttackInv[b];
                        b = b + 1;
                    }
                    i++;
                }

                z = z + 1;
                for (var i = 0; i < unitsAbleToAttack.Count; i++)
                {
                    // Strike vs. Orbital Batterys
                    double shiptypemodifier = 0; // edited, initialized.
                    var attacker = unitsAbleToAttack[i];
                    shiptypemodifier = 0.50; // Low weapons for common ships. Strike Cruiser are 1.5 times better...
                    if (attacker.Design.Key.Contains("STRIKE"))
                        shiptypemodifier = 5.00; // Strike Cruiser are good suited
                                                 //if(attacker.Source.Owner.Race == "CARDASSIANS")
                                                 //   shiptypemodifier = 7.5;
                    if ((attacker.Design.Key.Contains("KLING_CRUISER") && !attacker.Design.Key.Contains("HEAVY") && !attacker.Design.Key.Contains("STRIKE"))
                        || attacker.Design.Key.Contains("CUBE")) // Klingon BattleCruiser and Borg Cubes are 2nd best
                        shiptypemodifier = 4.50;

                    // Now, check for the best Orbital Assault Cruiser
                    if (attacker.Source.Design.Key == ("CARD_STRIKE_CRUISER_II")
                        || attacker.Source.Design.Key == ("CARD_STRIKE_CRUISER_III")
                        || attacker.Source.Design.Key == ("TERRAN_CRUISER_I")
                        || attacker.Source.Design.Key == ("TERRAN_STRIKE_CRUISER_I")
                        || attacker.Source.Design.Key == ("TERRAN_STRIKE_CRUISER_II")
                        || attacker.Source.Design.Key == ("TERRAN_STRIKE_CRUISER_III"))
                        shiptypemodifier = 6.5;
                    // Cardassian Keldon class Strike Cruiser
                    // Terran Constiution Class
                    // Terran Strike Cruiser


                    //shiptypemodifer is also used to modify Orbital Batterys
                    if (attacker.Design.Key.Contains("ORBITAL_BATTERY_I") &&
                        !attacker.Design.Key.Contains("ORBITAL_BATTERY_II") &&
                        !attacker.Design.Key.Contains("ORBITAL_BATTERY_III")) // = Orbital I
                        shiptypemodifier = 4.0;
                    if (attacker.Design.Key.Contains("ORBITAL_BATTERY_II") &&
                        !attacker.Design.Key.Contains("ORBITAL_BATTERY_III")) // Orbital II
                        shiptypemodifier = 6.0;
                    if (attacker.Design.Key.Contains("ORBITAL_BATTERY_III")) // Orbital III
                        shiptypemodifier = 10.0;
                    if (attacker.Design.Key.Contains("MINOR")) // Minors
                        shiptypemodifier = shiptypemodifier * 2.0; // Modifies Minor Orbital Strenght. 
                    if (attacker.Name.Contains("!") && !attacker.Name.Contains("ORBITAL")) // HeroShips get bonus (most ain´t Strike Cruiser however so its not much)
                        shiptypemodifier = shiptypemodifier * 3;



                    var weapon = attacker.Weapons.Where(w => w.CanFire).OrderByDescending(o => o.MaxDamage.CurrentValue * shiptypemodifier).FirstOrDefault();



                    if (weapon == null)
                    {
                        unitsAbleToAttack.RemoveAt(i--);
                        continue;
                    }

                    var isInvader = attacker.OwnerID == _invasionArena.InvaderID;
                    var targetList = isInvader ? invaderTargets : defenderTargets;
                    if (targetList.Count == 0)
                    {
                        unitsAbleToAttack.RemoveAt(i--);
                        continue;
                    }
                    // Orbital Re-balancing -> Changing targets for orbitals
                    var target = targetList.Last.Value; // 

                    //  targetList.RemoveFirst(); // Stay at the same target
                    // targetList.AddLast(target); 

                    var accuracyThreshold = 0d;

                    var ship = attacker.Source as Ship;
                    if (ship != null)
                    {
                        if (ship.GetAccuracyModifier() > 1 || ship.GetAccuracyModifier() < 0.1)
                        {
                            accuracyThreshold = 1d - 0.5;
                        }
                        else
                        {
                            accuracyThreshold = 1d - ship.GetAccuracyModifier(); // why -4? for ships...
                        }                        
                    }

                    var maxDamage = weapon.MaxDamage.CurrentValue;
                    maxDamage = Convert.ToInt32(weapon.MaxDamage.CurrentValue * shiptypemodifier); // added



                    weapon.Discharge();

                    var confirmedHit = (RandomProvider.Shared.NextDouble() >= accuracyThreshold);
                    if (confirmedHit)
                    {
                        target.TakeDamage(maxDamage);
                        GameLog.Core.Combat.DebugFormat("AttackingOrbitals = SpaceCombat: Target Name = {0}, ID = {1} Hull Strength = {2}, health = {3}", target.Name, target.ObjectID, target.HullStrength, target.Health);

                        if (target.IsDestroyed)
                        {
                            var targetIndex = unitsAbleToAttack.IndexOf(target);
                            if (targetIndex < i)
                                --i;
                            if (targetIndex > -1) // if targetIndex is 0 or greater there is a target to remove
                                unitsAbleToAttack.RemoveAt(targetIndex);
                            targetList.Remove(target);
                        }
                    }
                }

                if (_orders.Action == InvasionAction.StandDown)
                    break;
            }
        }

        private void RechargeUnits()
        {
            _invasionArena.DefendingUnits.OfType<InvasionOrbital>().ForEach(o => o.Recharge());
            _invasionArena.InvadingUnits.OfType<InvasionOrbital>().ForEach(o => o.Recharge());
        }

        protected void FinishInvasion()
        {
            foreach (var unit in _invasionArena.DefendingUnits.Concat(_invasionArena.InvadingUnits))
                unit.CommitSourceChanges();

            _invasionArena.Population.UpdateAndReset();
            _invasionArena.ColonyShieldStrength.UpdateAndReset();

            if (_invasionArena.Colony.Population.IsMinimized)
            {
                var civManager = CivilizationManager.For(_invasionArena.Colony.OwnerID);
                _invasionArena.Colony.Destroy();
                civManager.EnsureSeatOfGovernment();
            }

            AddDiplomacyMemories();

            _invasionEndedCallback(this);
        }

        private void AddDiplomacyMemories()
        {

        }

        protected void VerifyInvasionInProgress()
        {
            if (_invasionArena == null)
                throw new InvalidOperationException("No invasion is currently in progress.");
        }

        protected void VerifyNoInvasionInProgress()
        {
            if (_invasionArena != null)
                throw new InvalidOperationException("An invasion is already in progress.");
        }
    }
}
