// File:CivilizationManager.cs
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.AI;
using Supremacy.Annotations;
using Supremacy.Collections;
using Supremacy.Diplomacy;
using Supremacy.Economy;
using Supremacy.Entities;
using Supremacy.Orbitals;
using Supremacy.Tech;
using Supremacy.Types;
using Supremacy.Universe;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace Supremacy.Game
{
    /// <summary>
    /// Contains data and logic for managing an individual <see cref="Civilization"/> and its assets.
    /// </summary>
    [Serializable]
    public class CivilizationManager : INotifyPropertyChanged, ICivIdentity
    {
        #region Fields
        private readonly int _civId;
        private readonly Meter _credits;

        private readonly List<Bonus> _globalBonuses;
        private readonly CivilizationMapData _mapData;
        private readonly ResearchPool _research;
        private readonly ResourcePool _resources;
        private readonly List<SitRepEntry> _sitRepEntries;
        private readonly Meter _totalPopulation;
        private readonly Meter _totalValue;
        private readonly Meter _totalResearch;
        private readonly Treasury _treasury;
        private int _maintenanceCostLastTurn;

        //private int z_shipColonyNeeded;
        private int z_ship_Colony_Ordered;
        //private int z_shipColonyAvailable;
        //private int z_shipConstructionNeeded;
        private int z_ship_Construction_Ordered;
        //private int z_shipConstructionAvailable;
        //private int z_shipMedicalNeeded;
        private int z_ship_Medical_Ordered;
        //private int z_shipMedicalAvailable;
        //private int z_shipTransportNeeded;
        private int z_ship_Transport_Ordered;
        private int z_Ship_Transport_Needed_For_Assaults = 0;
        //private int z_shipTransportAvailable;
        //private int z_shipSpyNeeded;
        private int z_ship_Spy_Ordered;
        //private int z_shipSpyAvailable;
        //private int z_shipDiplomaticNeeded;
        private int z_ship_Diplomatic_Ordered;
        //private int z_shipDiplomaticAvailable;
        //private int z_shipScienceNeeded;
        private int z_ship_Science_Ordered;
        //private int z_shipScienceAvailable;
        //private int z_shipScoutNeeded;
        private int z_ship_Scout_Ordered;
        //private int z_shipScoutAvailable;
        //private int z_shipFastAttackNeeded;
        private int z_ship_FastAttack_Ordered;
        //private int z_shipFastAttackAvailable;
        //private int z_ShipCombatantNeeded;

        private int z_ship_Combatant_Ordered = -2;  // to avoid "is never assigned"
        //private int z_ShipCombatantAvailable;
        //private int z_shipCruiserNeeded;
        private int z_ship_Cruiser_Ordered;
        //private int z_shipCruiserAvailable;
        //private int z_shipHeavyCruiserNeeded;
        private int z_ship_HeavyCruiser_Ordered;
        //private int z_shipHeavyCruiserAvailable;
        //private int z_shipStrikeCruiserNeeded;
        private int z_ship_StrikeCruiser_Ordered;
        //private int z_shipStrikeCruiserAvailable;
        //private int z_shipCommandNeeded;
        private int z_ship_Command_Ordered;
        //private int z_shipCommandAvailable;
        //private int _buyCostLastTurn;
        //private int _rankCredits;
        private readonly UniverseObjectList<Colony> _colonies;
        public List<CivHistory> _civHist_List = new List<CivHistory>();

        //#pragma warning disable IDE0044 // Add readonly modifier
        private List<Civilization> _spiedCivList;
        private List<Civilization> _targetCivList;
        //#pragma warning restore IDE0044 // Add readonly modifier

        public List<string> _neededShiptypesList;


        private int _homeColonyId;
        private List<int> _IntelIDs;
        private MapLocation? _homeColonyLocation;
        private MapLocation _accumulateLocation;
        private Sector _accumulateSector;

        private Civilization _assault_targetCiv;
        private MapLocation _assault_accumulate_location_1;
        private Sector _assault_Accumulate_Sector_1;
        //private int _systemAssaultPower_1;


        //private MapLocation _systemAssault_Accumulate_Location_2;
        //private Sector _systemAssault_Accumulate_Sector_2;
        //private int _systemAssaultPower_2;



        //#pragma warning disable IDE0052 // Remove unread private members
        private Sector _strandedShipsSector;



        //private int _buyCostLastTurn;
        //#pragma warning restore IDE0052 // Remove unread private members
        private int _seatOfGovernmentId = -1;
        private readonly Meter _totalIntelligenceAttackingAccumulated;
        private readonly Meter _totalIntelligenceDefenseAccumulated;
        //private int _rankMaint;
        //private int _rankResearch;
        //private int _rankIntelAttack;
        private bool _destroyOfShipOrdered;
        //private string _text;
        private int _fire_power_space;


        //private int bc;  // buildingCosts
        //private readonly string _newline = Environment.NewLine;

        #endregion Fields

        //private AppContext _appContext => _appContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="CivilizationManager"/> class.
        /// </summary>
        public CivilizationManager()// new CivilizationManage
        {
            _credits = new Meter(5000, Meter.MinValue, Meter.MaxValue);
            _treasury = new Treasury(5000);
            _maintenanceCostLastTurn = 0;


            //_buyCostLastTurn = 0;

            _resources = new ResourcePool();
            _colonies = new UniverseObjectList<Colony>();

            _globalBonuses = new List<Bonus>();
            _mapData = new CivilizationMapData(
                GameContext.Current.Universe.Map.Width, GameContext.Current.Universe.Map.Height);

            _totalPopulation = new Meter();
            _totalPopulation.PropertyChanged += OnTotalPopulationPropertyChanged;

            _totalValue = new Meter();
            _totalValue.PropertyChanged += OnTotalValuePropertyChanged;

            _totalResearch = new Meter();
            _totalResearch.PropertyChanged += OnTotalResearchPropertyChanged;

            _totalIntelligenceAttackingAccumulated = new Meter(0, 0, Meter.MaxValue);
            _totalIntelligenceAttackingAccumulated.PropertyChanged += OnTotalIntelligenceAttackingAccumulatedPropertyChanged;
            _totalIntelligenceDefenseAccumulated = new Meter(0, 0, Meter.MaxValue);
            _totalIntelligenceDefenseAccumulated.PropertyChanged += OnTotalIntelligenceDefenseAccumulatedPropertyChanged;

            _sitRepEntries = new List<SitRepEntry>();
            _spiedCivList = new List<Civilization>();
            _targetCivList = new List<Civilization>();
            _civHist_List = new List<CivHistory>();

            _resources.Deuterium.BaseValue = 100;
            _resources.Deuterium.Reset();
            _resources.Dilithium.BaseValue = 10;
            _resources.Dilithium.Reset();
            _resources.Duranium.BaseValue = 1000;
            _resources.Duranium.Reset();
            _resources.UpdateAndReset();

            //_stealCreditsSpyOperation = new List<StealCredits>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CivilizationManager"/> class.
        /// </summary>
        /// <param name="game">The game context.</param>
        /// <param name="civilization">The civilization.</param>
        public CivilizationManager(IGameContext game, Civilization civilization) : this()
        {
            if (civilization == null)
            {
                throw new ArgumentNullException("civilization");
            }

            _civId = civilization.CivID;
            _research = new ResearchPool(civilization, game.ResearchMatrix);
            _targetCivList = new List<Civilization>();
        }

        //public CivilizationManager(List<StealCredits> stealCreditsSpyOperation)
        //{
        //    _stealCreditsSpyOperation = stealCreditsSpyOperation;
        //}

        #region Constructors

        [Serializable]
        public class CivHistory
        {
            public string CivIDHistAndTurn;
            public int CivIDHist;
            public string CivKeyHist;
            public int CreditsHist;
            public int CreditsHist_LT;
            public int CreditsHist_Maint;
            public int ColoniesHist;
            public int PopulationHist;
            public int MoraleHist;
            public int MoraleGlobalHist;
            public int DilithiumHist;
            public int DeuteriumHist;
            public int DuraniumHist;
            public int TotalValueHist;
            public int ResearchHist;
            public int IntelProdHist;
            public int IDefHist;
            public int IAttHist;
            public int R_CredHist;
            public int R_MaintHist;
            public int R_ResearchHist;
            public int R_IntelAttackHist;

            public CivHistory
                (
                string civIDHistAndTurn  // Index of civID and Turn
                , int civIDHist   // just civID
                , string civKeyHist
                , int creditsHist
                , int creditsHist_lt
                , int creditsHist_maint
                , int coloniesHist
                , int populationHist
                , int moraleHist
                , int moraleGlobalHist
                , int diHist
                , int deHist
                , int duHist
                , int totalValueHist
                , int researchHist
                , int intelProdHist
                , int iDefHist
                , int iAttHist
                , int r_CredHist
                , int r_MaintHist
                , int r_ResearchHist
                , int r_IntelAttackHist
                //, string sitrepsHist
                )
            {
                CivIDHistAndTurn = civIDHistAndTurn;
                CivIDHist = civIDHist;
                CivKeyHist = civKeyHist;
                CreditsHist = creditsHist;
                CreditsHist_LT = creditsHist_lt;
                CreditsHist_Maint = creditsHist_maint;
                ColoniesHist = coloniesHist;
                PopulationHist = populationHist;
                MoraleHist = moraleHist;
                MoraleGlobalHist = moraleGlobalHist;
                DilithiumHist = diHist;
                DeuteriumHist = deHist;
                DuraniumHist = duHist;
                TotalValueHist = totalValueHist;
                ResearchHist = researchHist;
                IntelProdHist = intelProdHist;
                IDefHist = iDefHist;
                IAttHist = iAttHist;
                R_CredHist = r_CredHist;
                R_MaintHist = r_MaintHist;
                R_ResearchHist = r_ResearchHist;
                R_IntelAttackHist = r_IntelAttackHist;
                //SitRepsHist = sitrepsHist;
            }
        }

        public void ZZ_AddCivHist(int civIDHist
            , string civKeyHist
            , int creditsHist
            , int creditsHist_lt
            , int creditsHist_maint
            , int coloniesHist
            , int populationHist
            , int moraleHist
            , int moraleGlobalHist
            , int dilithiumHist
            , int deHist
            , int duHist
            , int totalValueHist
            , int researchHist
            , int intelProdHist
            , int iDefHist
            , int iAttHist
            , int r_CredHist
            , int r_MaintHist
            , int r_ResearchHist
            , int r_IntelAttackHist
            //, string sitrepsHist
            )
        {
            string _tn;
            _tn = GameContext.Current.TurnNumber.ToString();
            string civIDHistAndTurn = civIDHist + "-" + _tn;
            CivHistory civHist_New = new CivHistory(
                civIDHistAndTurn

                , civIDHist
                , civKeyHist
                , creditsHist
                , creditsHist_lt
                , creditsHist_maint
                , coloniesHist
                , populationHist
                , moraleHist
                , moraleGlobalHist
                , dilithiumHist
                , deHist
                , duHist
                , totalValueHist
                , researchHist
                , intelProdHist
                , iDefHist
                , iAttHist
                , r_CredHist
                , r_MaintHist
                , r_ResearchHist
                , r_IntelAttackHist
                //, sitrepsHist  // not here
                //, blank, blank, blank, blank, blank, blank, blank  // 11
                );

            //_text = _newline; // dummy - do not remove

            _civHist_List?.Add(civHist_New);

        }

        #endregion

        #region Properties and Indexers
        /// <summary>
        /// Gets the civilization ID.
        /// </summary>
        /// <value>The civilization ID.</value>
        public int CivilizationID => _civId;

        /// <summary>
        /// Gets the civilization.
        /// </summary>
        /// <value>The civilization.</value>
        public Civilization Civilization => GameContext.Current.Civilizations[_civId];

        /// <summary>
        /// Gets the total population of all the civilization's colonies.
        /// </summary>
        /// <value>The total population.</value>
        public Meter TotalPopulation => _totalPopulation;

        /// <summary>
        /// Gets the total value of all the civilization's colonies for compare issues
        /// </summary>
        /// <value>The total value for compare issues.</value>
        public Meter TotalValue => _totalValue;

        /// <summary>
        /// Gets the total research of all the civilization's colonies.
        /// </summary>
        /// <value>The total population.</value>
        public Meter TotalResearch => _totalResearch;

        /// <summary>
        /// Gets the credits in the civilization's treasury.
        /// </summary>
        /// <value>The credits.</value>
        public Meter Credits
        {
            get
            {
                Meter _credits1 = _credits;
                if (this.Civilization.Key == "BORG")
                {
                    _credits1 = new Meter(1, 1);
                }
                return _credits1;
            }
        }

        /// <summary>
        /// Gets the civilization's treasury.
        /// </summary>
        public Treasury Treasury => _treasury;

        /// <summary>
        /// Gets the civilization's MaintenanceCostLastTurn.
        /// </summary>
        public int MaintenanceCostLastTurn
        {
            get => _maintenanceCostLastTurn;
            set => _maintenanceCostLastTurn = value;
        }


        // Ship Colony
        public int Z_Ship_Colony_Needed
        {
            get
            {
                int z_shipNeeded = 2 - ((int)GameContext.Current.TurnNumber / 10); // no common need after Turn 55

                z_shipNeeded -= (Z_Ship_Colony_Available + Z_Ship_Colony_Ordered);
                if (z_shipNeeded > 0 && Civilization.Key != "BORG")
                {
                    _neededShiptypesList.Add("Colony");
                }

                return z_shipNeeded;
            }
        }
        public int Z_Ship_Colony_Ordered
        {
            get => z_ship_Colony_Ordered;
            //set => z_ship_Colony_Ordered = value;
        }
        public int Z_Ship_Colony_Available
        {
            get
            {
                return GameContext.Current.Universe.FindOwned<Fleet>(Civilization).Where(s => s.IsColonizer).Count();
                //return _ships.Count();
                //return z_shipColonyAvailable1;
            }
            //set => z_shipColonyAvailable = value;
        }



        // Ship Construction
        public int Z_Ship_Construction_Needed
        {
            get
            {
                int z_shipNeeded = 4 - ((int)GameContext.Current.TurnNumber / 10); // no common need after Turn 55
                
                z_shipNeeded -= (Z_Ship_Construction_Available + z_ship_Construction_Ordered);
                if (z_shipNeeded > 0)
                {
                    _neededShiptypesList.Add("Construction");
                }
                
                return z_shipNeeded;
            }
            //set
            //{
            //    z_shipConstructionNeeded = value;
            //    if (z_shipConstructionNeeded > 3)
            //    {
            //        z_shipConstructionNeeded = 3;
            //    }
            //}
        }

        public int Z_Ship_Construction_Ordered
        {
            get => z_ship_Construction_Ordered;
            //set => z_ship_Construction_Ordered = value;
        }

        public int Z_Ship_Construction_Available
        {
            get => GameContext.Current.Universe.FindOwned<Fleet>(Civilization).Where(s => s.IsConstructor).Count();
            //set => z_shipConstructionAvailable = value;
        }

        // Ship Medical
        public int Z_Ship_Medical_Needed
        {
            get
            {
                int z_shipNeeded = 1 + ((int)GameContext.Current.TurnNumber / 20) - (Z_Ship_Medical_Available + z_ship_Medical_Ordered);
                if (z_shipNeeded > 0 && Civilization.Key != "BORG")
                {
                    _neededShiptypesList.Add("Medical");
                }

                return z_shipNeeded;
            }
            //set => z_shipMedicalNeeded = value;
        }
        //public int Z_ShipMedicalOrdered
        //{
        //    get => z_ship_Medical_Ordered;
        //    //set => z_ship_Medical_Ordered = value;
        //}

        public int Z_Ship_Medical_Available
        {
            get => GameContext.Current.Universe.FindOwned<Fleet>(Civilization).Where(s => s.IsMedical).Count();
            //set => z_shipMedicalAvailable = value;
        }

        // Ship Transport

        public int Z_Ship_Transport_Needed_For_Assaults
        {
            get
            {
                return z_Ship_Transport_Needed_For_Assaults;
            }
            set => z_Ship_Transport_Needed_For_Assaults = value;
        }
        public int Z_Ship_Transport_Needed
        {
            get
            {
                if (this.Civilization.IsHuman) 
                {
                    //Debugger.Break();
                }

                int _ships_per_turn = ((int)GameContext.Current.TurnNumber / 10);
                if (_ships_per_turn > 5)
                {
                    _ships_per_turn = 5;
                }

                int z_shipNeeded = 2 + _ships_per_turn + Z_Ship_Transport_Needed_For_Assaults - (Z_Ship_Transport_Available + z_ship_Transport_Ordered);

                if (z_shipNeeded > 0)
                {
                    _neededShiptypesList.Add("Transport");
                }

                return z_shipNeeded;
            }
            //set => z_shipTransportNeeded = value;
        }
        //public int Z_ShipTransportOrdered
        //{
        //    get => z_ship_Transport_Ordered;
        //    //set => z_ship_Transport_Ordered = value;
        //}

        public int Z_Ship_Transport_Available
        {
            get => GameContext.Current.Universe.FindOwned<Fleet>(Civilization).Where(s => s.IsTransport).Count();
            //set => z_shipTransportAvailable = value;
        }

        // Ship Spy
        public int Z_Ship_Spy_Needed
        {
            //get => 2 + (int)GameContext.Current.TurnNumber / 20 - (Z_Ship_Medical_Available + z_ship_Medical_Ordered);
            get
            {
                int z_shipNeeded = 3 - ((int)GameContext.Current.TurnNumber / 20); // no common need after Turn 60
                z_shipNeeded -= (Z_Ship_Spy_Available + z_ship_Spy_Ordered);
                if (z_shipNeeded > 0 && Civilization.Key != "BORG")
                {
                    _neededShiptypesList.Add("Spy");
                }

                return z_shipNeeded;
            }
            //set => z_shipSpyNeeded = value;
        }
        //public int Z_ShipSpyOrdered
        //{
        //    get => z_ship_Spy_Ordered;
        //    //set => z_ship_Spy_Ordered = value;
        //}

        public int Z_Ship_Spy_Available
        {
            get => GameContext.Current.Universe.FindOwned<Fleet>(Civilization).Where(s => s.IsSpy).Count();
            //set => z_shipSpyAvailable = value;
        }

        // Ship Diplomatic
        public int Z_Ship_Diplomatic_Needed
        {
            get
            {
                int z_shipNeeded = 2 - (Z_Ship_Diplomatic_Available + z_ship_Diplomatic_Ordered);
                //z_shipNeeded -= (Z_Ship_Construction_Available + z_ship_Construction_Ordered);
                if (z_shipNeeded > 0 && Civilization.Key != "BORG")
                {
                    _neededShiptypesList.Add("Diplomatic");
                }

                return z_shipNeeded;
            }
        }

        //public int Z_ShipDiplomaticOrdered
        //{
        //    get => z_ship_Diplomatic_Ordered;
        //    //set => z_ship_Diplomatic_Ordered = value;
        //}
        public int Z_Ship_Diplomatic_Available
        {
            get => GameContext.Current.Universe.FindOwned<Fleet>(Civilization).Where(s => s.IsDiplomatic).Count();
            //set => z_shipDiplomaticAvailable = value;
        }

        // Ship Science
        public int Z_Ship_Science_Needed
        {

            get
            {
                int z_shipNeeded = 2; //*+ (int)GameContext.Current.TurnNumber / 10*/ - (Z_Ship_Science_Available + z_ship_Science_Ordered);
                                    
                    z_shipNeeded -= (Z_Ship_Science_Available + z_ship_Science_Ordered);
                if (z_shipNeeded > 0 && Civilization.Key != "BORG")
                {
                    _neededShiptypesList.Add("Science");
                }

                return z_shipNeeded;
               
            }
            //set => z_shipScienceNeeded = value;
        }
        //public int Z_ShipScienceOrdered
        //{
        //    get => z_ship_Science_Ordered;
        //    //set => z_ship_Science_Ordered = value;
        //}

        public int Z_Ship_Science_Available
        {
            get => GameContext.Current.Universe.FindOwned<Fleet>(Civilization).Where(s => s.IsScience).Count();
            //set => z_shipScienceAvailable = value;
        }

        // Ship Scout
        public int Z_Ship_Scout_Needed
        {
            get
            {
                int z_shipNeeded = 2 /*+ (int)GameContext.Current.TurnNumber / 10*/ - (Z_Ship_Scout_Available + z_ship_Scout_Ordered);

                if (z_shipNeeded > 0)
                {
                    if (this.Civilization.IsHuman)
                    {
                        //Debugger.Break();
                    }

                    _neededShiptypesList.Add("Scout");
                }

                return z_shipNeeded;

            }
            //set => z_shipScoutNeeded = value;
        }


        //public int Z_ShipScoutOrdered
        //{
        //    get => z_ship_Scout_Ordered;
        //    //set => z_ship_Scout_Ordered = value;
        //}

        public int Z_Ship_Scout_Available
        {
            get => GameContext.Current.Universe.FindOwned<Fleet>(Civilization).Where(s => s.IsScout).Count();
            //set => z_shipScoutAvailable = value;
        }

        // Ship FastAttack
        public int Z_Ship_FastAttack_Needed
        {
            get => -1;
            //set => z_shipFastAttackNeeded = value;
        }
        //public int Z_ShipFastAttackOrdered
        //{
        //    get => z_ship_FastAttack_Ordered;
        //    //set => z_ship_FastAttack_Ordered = value;
        //}

        public int Z_Ship_FastAttack_Available
        {
            get => GameContext.Current.Universe.FindOwned<Fleet>(Civilization).Where(s => s.IsFastAttack).Count();
            //set => z_shipFastAttackAvailable = value;
        }

        // Ship Combatant
        public int Z_Ship_Combatant_Needed
        {
            get
            {
                //int z_shipNeeded = 4 - ((int)GameContext.Current.TurnNumber / 10); // no common need after Turn 55

                //z_shipNeeded -= (Z_Ship_Construction_Available + z_ship_Construction_Ordered);
                //if (z_shipNeeded > 0)
                //{
                    _neededShiptypesList.Add("FastAttack");
                //}

                return 5;
            }
        }
        //public int Z_ShipCombatantOrdered
        //{
        //    get => z_ShipCombatantOrdered;
        //    //set => z_ShipCombatantOrdered = value;
        //}

        public int Z_Ship_Combatant_Available
        {
            get => GameContext.Current.Universe.FindOwned<Fleet>(Civilization).Where(s => s.IsCombatant).Count();
            //set => z_ShipCombatantAvailable = value;
        }

        // Ship Cruiser
        public int Z_Ship_Cruiser_Needed
        {
            get => -1;// GameContext.Current.Universe.FindOwned<Fleet>(Civilization).Where(s => s.IsCombatant).Count();
        }
        //public int Z_ShipCruiserOrdered
        //{
        //    get => z_ship_Cruiser_Ordered;
        //    //set => z_ship_Cruiser_Ordered = value;
        //}

        public int Z_Ship_Cruiser_Available
        {
            get => -1;
            //set => z_shipCruiserAvailable = value;
        }

        // Ship HeavyCruiser
        public int Z_Ship_HeavyCruiser_Needed
        {
            get => -1;// GameContext.Current.Universe.FindOwned<Fleet>(Civilization).Where(s => s.IsCombatant).Count();
        }
        //public int Z_ShipHeavyCruiserOrdered
        //{
        //    get => z_ship_HeavyCruiser_Ordered;
        //    //set => z_ship_HeavyCruiser_Ordered = value;
        //}

        public int Z_Ship_HeavyCruiser_Available
        {
            get => -1;// GameContext.Current.Universe.FindOwned<Fleet>(Civilization).Where(s => s.IsCombatant).Count();
            //set => z_shipHeavyCruiserAvailable = value;
        }

        // Ship StrikeCruiser
        public int Z_Ship_StrikeCruiser_Needed
        {
            get => -1;
            //set => z_shipStrikeCruiserNeeded = value;
        }
        //public int Z_ShipStrikeCruiserOrdered
        //{
        //    get => z_ship_StrikeCruiser_Ordered;
        //    //set => z_ship_StrikeCruiser_Ordered = value;
        //}

        public int Z_Ship_StrikeCruiser_Available
        {
            get => -1;
            //set => z_shipStrikeCruiserAvailable = value;
        }

        // Ship Command
        public int Z_Ship_Command_Needed
        {
            get => -1;
            //set => z_shipCommandNeeded = value;
        }
        //public int Z_ShipCommandOrdered
        //{
        //    get => z_ship_Command_Ordered;
        //    //set => z_ship_Command_Ordered = value;
        //}

        public int Z_Ship_Command_Available
        {
            get => -1;
            //set => z_shipCommandAvailable = value;
        }

        /// <summary>
        /// Gets whether a destroy of a ship (one per turn) was ordered to reduce MaintenanceCostLastTurn.
        /// </summary>
        public bool DestroyOfShipOrdered
        {
            get => _destroyOfShipOrdered;
            set => _destroyOfShipOrdered = value;
        }

        ///// <summary>
        ///// Gets the civilization's MaintenanceCostLastTurn.
        ///// </summary>
        public int CurrentChange
        {
            get
            {
                return Credits.CurrentChange;
            }
        }

        /// <summary>
        /// Gets the civilization's BuyCostLastTurn. .... when it's working
        /// </summary>
        public int BuyCostLastTurn
        {
            get
            {
                //_buyCostLastTurn += 1;  // dummy
                //int bc = 0;
                //if (_credits.LastValue - _maintenanceCostLastTurn + _credits.LastChange > _credits.CurrentValue) 
                int bc = TaxIncome - ((_credits.LastChange + _maintenanceCostLastTurn) /** -1*/);
                ////TotalPopulation
                if (bc < 4)
                    bc = 0;
                return bc;
                //else
                //return 0 - ((_credits.LastChange + _maintenanceCostLastTurn) * -1);
            }
            //set => _buyCostLastTurn += value;
        }

        /// <summary>
        /// Gets the civilization's TaxIncome. .... when it's working
        /// </summary>
        public int TaxIncome
        {
            get
            {
                
                return Colonies.Sum(colony => colony.TaxCredits);
            }
        }

        public int FirePowerSpace
        {
            get
            {
                //List<Ship> _combatant_ships = GameContext.Current.Universe.FindOwned<Ship>(Civilization).Where(s => s.IsCombatant).ToList();
                //int _int = 0;
                //foreach (Ship ship in _combatant_ships)
                //{
                //    _int += ship.Fire_Power_Ship;
                //    //
                //    Debugger.Break();
                //}

                return _fire_power_space;
            }
            set
            {
                _fire_power_space = value;
            }
        }

        public int IncomeFromTrade
        {
            get
            {
                return Colonies.Sum(colony => colony.CreditsFromTrade.CurrentValue);
            }
        }

        /// <summary>
        /// Gets the civilization's ranking for Credits.
        /// </summary>
        public int Z_RankingCredits  // AI has credit advantage: Minors 4x, AI 2x ... so ranking doesn't is realistic anymore
        {
            get
            {
                int _rankCredits = -1;
                var CivHist = GameContext.Current.CivilizationManagers[CivilizationID]._civHist_List;

                if (CivHist.Count != 0)
                {
                    _rankCredits = CivHist[CivHist.Count - 1].R_CredHist;
                }
                return _rankCredits;
            }
        }

        /// <summary>
        /// Gets the civilization's ranking for Credits.
        /// </summary>
        public int Z_RankingMaint
        {
            get
            {
                int _rankMaint = -1;
                var CivHist = GameContext.Current.CivilizationManagers[CivilizationID]._civHist_List;
                if (CivHist.Count != 0)
                {
                    _rankMaint = CivHist[CivHist.Count - 1].R_MaintHist;
                }
                return _rankMaint;
            }
        }

        /// <summary>
        /// Gets the civilization's ranking for Credits.
        /// </summary>
        public int Z_RankingResearch
        {
            get
            {
                int _rankResearch = -1;
                var CivHist = GameContext.Current.CivilizationManagers[CivilizationID]._civHist_List;
                if (CivHist.Count != 0)
                {
                    _rankResearch = CivHist[CivHist.Count - 1].R_ResearchHist;
                }
                return _rankResearch;
            }
        }

        /// <summary>
        /// Gets the civilization's ranking for Intelligence_Attacking.
        /// </summary>
        public int Z_RankingIntelAttack
        {
            get
            {
                int _rankIntelAttack = -1;
                var CivHist = GameContext.Current.CivilizationManagers[CivilizationID]._civHist_List;
                if (CivHist.Count != 0)
                {
                    _rankIntelAttack = CivHist[CivHist.Count - 1].R_IntelAttackHist;
                }
                return _rankIntelAttack;
            }
        }

        /// <summary>
        /// Gets the civilization's resource pool.
        /// </summary>
        /// <value>The resource pool.</value>
        [NotNull]
        public ResourcePool Resources => _resources;

        /// <summary>
        /// Gets the civilization's research pool.
        /// </summary>
        /// <value>The research pool.</value>
        [NotNull]
        public ResearchPool Research => _research;

        /// <summary>
        /// Gets a list of the civilization's colonies.
        /// </summary>
        /// <value>The colonies.</value>
        [NotNull]
        public UniverseObjectList<Colony> Colonies => _colonies;

        [NotNull]
        public Colony SeatOfGovernment
        {
            get
            {
                if (_seatOfGovernmentId == -1)
                {
                    return null;
                }

                return GameContext.Current.Universe.Objects[_seatOfGovernmentId] as Colony;
            }
        }

        /// <summary>
        /// Gets the list of SitRep entries for the civilization.
        /// </summary>
        /// <value>The SitRep entries.</value>
        [NotNull]
        public IList<SitRepEntry> SitRepEntries
        {
            get
            {
                if (LocalPlayer != null)
                {
                    foreach (SitRepEntry _rep in _sitRepEntries)  // foreachsitrep
                    {

                        CivilizationManager _playerCivManager = GameContext.Current.CivilizationManagers[LocalPlayer.CivID];
                        if (_playerCivManager != null && _rep.Owner.ToString() == _playerCivManager.ToString())
                        {
                            string _text = "Step_3337:; SitRep Turn "
                                + GameContext.Current.TurnNumber
                                + " Cat= " + _rep.Categories
                                + " " + _rep.Priority
                                + " Action= " + _rep.Action
                                + " for " + _rep.Owner
                                + ":" + Environment.NewLine
                                + "                    SitRep: " + _rep.SummaryText
                                + " Cat= " + _rep.Categories
                                + Environment.NewLine
                                ;

                            Console.WriteLine(_text);
                            //GameLog.Core.SitReps.DebugFormat(_text);
                            //GameLog.Core.SitReps.DebugFormat("SitRep Turn {4} Cat={2} Action {3} for {1}:" + Environment.NewLine + // splitted in 2 lines for better reading
                            //     "                    SitRep: {0}" + Environment.NewLine, _rep.SummaryText, _rep.Owner, _rep.Categories, _rep.Action, GameContext.Current.TurnNumber);

                        }
                    }

                }
                _ = _sitRepEntries.Distinct();
                _sitRepEntries.OrderBy(o => o.SummaryText);
                return _sitRepEntries;
            }
        }


        public List<Civilization> SpiedCivList => _spiedCivList;
        //public List<Civilization> TargetCivList => _targetCivList;
        public List<Civilization> TargetCivList { get; /*private*/ set; } = new List<Civilization>();

        public List<MapLocation> Locations_To_Explore { get; /*private*/ set; } = new List<MapLocation>();

        public List<MapLocation> Locations_To_Build_Stations { get; /*private*/ set; } = new List<MapLocation>();

        public List<MapLocation> Locations_To_NOT_Build_Stations { get; /*private*/ set; } = new List<MapLocation>();

        public List<Sector> Sectors_To_Enter_Carefully { get; /*private*/ set; } = new List<Sector>();



        /// <summary>
        /// Gets the average morale of all the civilization's colonies.
        /// </summary>
        /// <value>The average morale.</value>
        public int AverageMorale
        {
            get
            {
                int totalPopulation = _totalPopulation.CurrentValue;
                double totalMorale = Colonies.Sum(colony => colony.Morale.CurrentValue * (1d / totalPopulation * colony.Population.CurrentValue));
                return (int)totalMorale;
            }
        }

        /// <summary>
        /// Gets the average techlevel of research fields.
        /// </summary>
        /// <value>The average morale.</value>
        public int AverageTechLevel
        {
            get
            {
                int _averageTechlevel =
                    Research.GetTechLevel(TechCategory.BioTech)
                    + Research.GetTechLevel(TechCategory.Computers)
                    + Research.GetTechLevel(TechCategory.Construction)
                    + Research.GetTechLevel(TechCategory.Energy)
                    + Research.GetTechLevel(TechCategory.Propulsion)
                    + Research.GetTechLevel(TechCategory.Weapons)
                    ;

                return _averageTechlevel / 6;
            }
        }

        /// <summary>
        /// Gets the sum intelligence generated by all the colonies
        /// </summary>
        /// <value>The total intelligence.</value>
        public int TotalIntelligenceProduction
        {
            get
            {
                int baseIntel = Colonies.Sum(colony => colony.Intelligence_Net) + _globalBonuses.Where(b => b.BonusType == BonusType.Intelligence).Sum(b => b.Amount);
                foreach (Bonus bonus in _globalBonuses.Where(b => b.BonusType == BonusType.PercentTotalIntelligence))
                {
                    baseIntel *= bonus.Amount;
                }
                //works   GameLog.Client.Intel.DebugFormat("TotalIntelProduction = {0}", baseIntel);
                return baseIntel;
            }
        }
        public Meter TotalIntelligenceAttackingAccumulated
        {
            get
            {
                Meter updateMeter = _totalIntelligenceAttackingAccumulated;

                if (_totalIntelligenceAttackingAccumulated.CurrentValue == 0)
                {
                    updateMeter.CurrentValue = 0;
                }
                //works
                //_text = "Step_3113:; TotalIntelAttackingAccumulated = " + updateMeter.CurrentValue.ToString();
                //Console.WriteLine(_text);   
                //GameLog.Client.Intel.DebugFormat("TotalIntelAttackingAccumulated = {0}", updateMeter.CurrentValue);
                return _totalIntelligenceAttackingAccumulated;
            }
        }

        public Meter TotalIntelligenceDefenseAccumulated
        {
            get
            {
                Meter updateMeter = _totalIntelligenceDefenseAccumulated;
                //works   GameLog.Client.Intel.DebugFormat("TotalIntelDefenseAccumulated = {0}", updateMeter.CurrentValue);
                if (_totalIntelligenceDefenseAccumulated.CurrentValue == 0)
                {
                    updateMeter.CurrentValue = 0;
                }
                //_text = "Step_3114:; TotalIntelligenceDefenseAccumulated = " + updateMeter.CurrentValue;
                //Console.WriteLine(_text);
                //GameLog.Client.Intel.DebugFormat("TotalIntelAttackingAccumulated = {0}", updateMeter.CurrentValue);
                return _totalIntelligenceDefenseAccumulated;
            }
        }

        public bool ControlsHomeSystem
        {
            get
            {
                StarSystem homeSystem = HomeSystem;
                if (homeSystem == null)
                {
                    return false;
                }

                return homeSystem.OwnerID == CivilizationID;
            }
        }

        public bool IsHomeColonyDestroyed
        {
            get
            {
                StarSystem homeSystem = HomeSystem;
                if (homeSystem == null)
                {
                    return false;
                }

                Colony colony = homeSystem.Colony;
                return colony == null ||
                       colony.ObjectID != _homeColonyId;
            }
        }

        /// <summary>
        /// Gets the civilization's home colony.
        /// </summary>
        /// <value>The home colony.</value>
        public Colony HomeColony
        {
            get => GameContext.Current.Universe.Get<Colony>(_homeColonyId);
            internal set
            {
                _homeColonyId = (value != null) ? value.ObjectID : -1;

                if (value != null)
                {
                    _homeColonyLocation = value.Location;
                }
            }
        }

        /// <summary>
        /// Gets the civilization's home system.
        /// </summary>
        /// <value>The home system.</value>
        public StarSystem HomeSystem
        {
            get
            {
                if (!_homeColonyLocation.HasValue)
                {
                    return null;
                }

                return GameContext.Current.Universe.Map[_homeColonyLocation.Value].System;
            }
        }

        public MapLocation AccumulateLocation
        {
            get
            {
                //MapLocation _accumulateLocation = HomeSystem.Location; 
                if (_accumulateLocation == null || _accumulateLocation.ToString() == "(0, 0)")
                {
                    _accumulateLocation = HomeSystem.Location;
                    string _text = "Step_3338:; Turn "
                            + GameContext.Current.TurnNumber
                            + " > AccumulateLocation for " + this.Civilization
                            + " is set to " + _accumulateLocation.ToString()
                            ;

                    Console.WriteLine(_text);
                }
                return _accumulateLocation;
            }
            internal set
            {
                //MapLocation _accumulateLocation;
                _accumulateLocation = value;

            }
        }

        public Sector AccumulateSector
        {
            get
            {
                Sector _accumulateSector = new Sector(AccumulateLocation);
                //new Sector()
                if (_accumulateSector == null || _accumulateSector.Location.ToString() == "(0, 0)")
                {
                    _accumulateSector = this.HomeSystem.Sector;
                    string _text = "Step_3341:; "
                            + GameContext.Current.TurnNumber
                            + " _accumulateSector for " + this.Civilization
                            + " is set to " + _accumulateSector.ToString()
                            + " ( HomeSystem ) "
                            ;

                    Console.WriteLine(_text);
                }
                return _accumulateSector;
            }
            internal set
            {

                _accumulateSector = value;

            }
        }

        public Sector StrandedShipsSector
        {
            get
            {
                //Sector _strandedShipsSector = new Sector(AccumulateLocation);
                //new Sector()
                if (_strandedShipsSector == null || _strandedShipsSector.Location.ToString() == "(0, 0)")
                {
                    _strandedShipsSector = this.HomeSystem.Sector;
                    //string _text = "Step_3343:; Turn= "
                    //        + GameContext.Current.TurnNumber
                    //        + " >  _strandedShipsSector for " + this.Civilization
                    //        + " is set to " + _strandedShipsSector.ToString()
                    //        + " ( HomeSystem ) "
                    //        ;

                    //Console.WriteLine(_text);
                    string _text = "Stranded Ship Sector is set to " + _strandedShipsSector.ToString();

                    if (_strandedShipsSector.Location != this.HomeSystem.Sector.Location)
                    {
                        this.SitRepEntries.Add(new ReportEntry_CoS(Civilization, _strandedShipsSector.Location, _text, "", "", SitRepPriority.Red));
                    }


                }
                return _strandedShipsSector;
            }
            internal set
            {


                if (value != null)
                {
                _strandedShipsSector = value;
                string _text = "Step_3343:; Turn= "
                        + GameContext.Current.TurnNumber
                        + " >  _strandedShipsSector for " + this.Civilization
                        + " is set to " + _strandedShipsSector.ToString()
                        + " ( HomeSystem ) "
                        ;

                //Console.WriteLine(_text);
                //_text = "Stranded Ship Sector is set to " + _strandedShipsSector.ToString();
            }

            }
        }

        public Civilization Assault_TargetCiv
        {
            get => _assault_targetCiv;
            set
            {
                if (value != null /*&& _assault_targetCiv == null*/)  //always overwrite with new values
                {
                    _assault_targetCiv = value;
                    //Assault_TargetCiv = _assault_targetCiv;   // no no no !!
                }
                if (Civilization == Assault_TargetCiv)
                {
                    _assault_targetCiv = null;
                    Assault_TargetCiv = null;
                }
            }
        }

        public MapLocation Assault_Location // for systems and outposts as well
        {
            get => _assault_location;
            set
            {
                string _text = "Step_8789:; " + this.Civilization.Key + " against " + value.ToString();
                //Console.WriteLine(_text);

                //if (value != null /*&& _assault_targetCiv == null*/)  //always overwrite with new values
                //{
                    _assault_location = value;
                    //Assault_TargetCiv = _assault_targetCiv;   // no no no !!
                //}
                //if (Civilization == Assault_TargetCiv)
                //{
                //    _assault_location = null;
                //    Assault_Location = null;
                //}
            }
        }

        public string Assault_A_Info // for systems and outposts as well
        {
            get => _assault_location 
                + " " + Assault_TargetCiv
                + ", Defense= " + Assault_Value_Defense
                + ", Attack= " + Assault_Attack_Value
                + " for " + Civilization.Key

                ;
        }

        public string A_Info_CivM // for systems and outposts as well
        {
            get => 
                Civilization.Key
                //+ " " + Assault_TargetCiv
                //+ ", Defense= " + Assault_Value_Defense
                //+ ", Attack= " + Assault_Attack_Value


                ;
        }

        public int Assault_Value_Defense
        {
            get => _assault_DefenseValue;

            set
            {
                _assault_DefenseValue = value;
            }
        }

        public int Assault_Value_Defense_and_Distance
        {
            get => _assault_Value_Defense_and_Distance;

            set
            {
                _assault_Value_Defense_and_Distance = value;
            }
        }


        public int Assault_Attack_Value
        {
            get => _assault_attack_value;

            set
            {
                _assault_attack_value = value;
            }
        }

        public int Assault_GroundCombat_Attack_Value
        {
            get => _assault_groundcombat_value;

            set
            {
                _assault_groundcombat_value = value;
            }
        }

        //public string TargetCiv1Status = "why1";

        //public string TargetCiv2Status = "why2";

        //private Civilization _assault_targetCiv;
        private MapLocation _assault_location;
        private int _assault_attack_value = 0;
        private int _assault_groundcombat_value = 0;

        private int _assault_DefenseValue;
        private int _assault_Value_Defense_and_Distance = 999992;
        //private int z_Ship_Transport_Needed_For_Assaults = 0;

        public MapLocation Assault_Accumulate_Location_1
        {
            get
            {
                if (_assault_accumulate_location_1 == null && _assault_accumulate_location_1.ToString() == "( 0, 0)")
                {
                    _assault_accumulate_location_1 = HomeSystem.Location;
                    string _text = "Step_3351:; Turn "
                            + GameContext.Current.TurnNumber
                            + " > AccumulateLocation for " + this.Civilization
                            + " is set to " + _assault_accumulate_location_1.ToString()
                            ;

                    Console.WriteLine(_text);
                }
                return _assault_accumulate_location_1;
            }
            set
            {
                _assault_accumulate_location_1 = value;
            }
        }

        public Sector Assault_Accumulate_Sector_1
        {
            get
            {
                Sector _assault_Accumulate_Sector_1 = new Sector(Assault_Accumulate_Location_1);
                //new Sector()
                if (_assault_Accumulate_Sector_1 == null || _assault_Accumulate_Sector_1.Location.ToString() == "{(0, 0)}")
                {
                    //_assault_Accumulate_Sector_1 = this.HomeSystem.Sector;
                    _assault_Accumulate_Sector_1 = this.Assault_Accumulate_Sector_1;
                    string _text = "Step_3345:; "
                            //+ GameContext.Current.TurnNumber
                            + " _accumulateSector for " + this.Civilization
                            + " is set to " + _assault_Accumulate_Sector_1.ToString()
                            + " ( HomeSystem ) "
                            + " in Turn " + GameContext.Current.TurnNumber
                            ;

                    Console.WriteLine(_text);
                }
                return _assault_Accumulate_Sector_1;
            }
            set
            {
                _assault_Accumulate_Sector_1 = value;
                //Assault_Accumulate_Sector_1 = value;
            }
        }

        //public int SystemAssaultPower_1
        //{
        //    get
        //    {
        //        //Sector _systemAssaultPower_1 = new Sector(Assault_Accumulate_Location_1);
        //        ////new Sector()
        //        //if (_systemAssaultPower_1 == null || _systemAssaultPower_1.Location.ToString() == "{(0, 0)}")
        //        //{
        //        //    //_systemAssaultPower_1 = this.HomeSystem.Sector;
        //        //    _systemAssaultPower_1 = this.Assault_Accumulate_Sector_1;
        //        //    string _text = "Step_3345:; "
        //        //            //+ GameContext.Current.TurnNumber
        //        //            + " _accumulateSector for " + this.Civilization
        //        //            + " is set to " + _systemAssaultPower_1.ToString()
        //        //            + " ( HomeSystem ) "
        //        //            + " in Turn " + GameContext.Current.TurnNumber
        //        //            ;

        //        //    Console.WriteLine(_text);
        //        //}
        //        return _systemAssaultPower_1;
        //    }
        //    set
        //    {

        //        _systemAssaultPower_1 = value;
        //        //Assault_Accumulate_Sector_1 = value;

        //    }
        //}

        //public MapLocation SystemAssault_Accumulate_Location_2
        //{
        //    get
        //    {
        //        //Sector _systemAssault_Accumulate_Sector_2 = new Sector(SystemAssault_Accumulate_Location_2);
        //        if (_systemAssault_Accumulate_Location_2 == null || _systemAssault_Accumulate_Location_2.ToString() == "( 0, 0)")
        //        {
        //            //_systemAssault_Accumulate_Location_2 = HomeSystem.Location;
        //            //if (SystemAssault_Accumulate_Location_2.ToString() != "(0, 0)")
        //            //{
        //            _systemAssault_Accumulate_Location_2 = this.SystemAssault_Accumulate_Location_2;
        //            string _text = "Step_3338:; "

        //                    + " > AccumulateLocation for " + this.Civilization
        //                    + " is set to " + _systemAssault_Accumulate_Location_2.ToString()
        //                    + " in Turn " + GameContext.Current.TurnNumber
        //                    ;

        //            Console.WriteLine(_text);
        //            //}

        //        }
        //        return _systemAssault_Accumulate_Location_2;
        //    }
        //    set
        //    {

        //        _systemAssault_Accumulate_Location_2 = value;
        //        //SystemAssault_Accumulate_Location_2 = value;

        //    }
        //}

        //public Sector SystemAssault_Accumulate_Sector_2
        //{
        //    get
        //    {
        //        Sector _systemAssault_Accumulate_Sector_2 = new Sector(SystemAssault_Accumulate_Location_2);
        //        //new Sector()
        //        if (_systemAssault_Accumulate_Sector_2 == null || _systemAssault_Accumulate_Sector_2.Location.ToString() == "{(0, 0)}")
        //        {
        //            //_systemAssault_Accumulate_Sector_2 = this.HomeSystem.Sector;
        //            _systemAssault_Accumulate_Sector_2 = this.SystemAssault_Accumulate_Sector_2;
        //            string _text = "Step_3347:; "

        //                    + " _accumulateSector for " + this.Civilization
        //                    + " is set to " + _systemAssault_Accumulate_Sector_2.ToString()
        //                    + " ( HomeSystem ) "
        //                    + " in Turn " + GameContext.Current.TurnNumber
        //                    ;

        //            Console.WriteLine(_text);
        //        }
        //        return _systemAssault_Accumulate_Sector_2;
        //    }
        //    set
        //    {

        //        _systemAssault_Accumulate_Sector_2 = value;
        //        //SystemAssault_Accumulate_Sector_2 = value;

        //    }
        //}

        /// <summary>
        /// Gets the civilization's tech tree.
        /// </summary>
        /// <value>The tech tree.</value>
        public TechTree TechTree
        {
            get => GameContext.Current.TechTrees[_civId];
            internal set => GameContext.Current.TechTrees[_civId] = value;
        }

        /// <summary>
        /// Gets the civilization's global bonuses.
        /// </summary>
        /// <value>The global bonuses.</value>
        public IList<Bonus> GlobalBonuses => _globalBonuses;

        /// <summary>
        /// Gets the map data for the civilization.
        /// </summary>
        /// <value>The map data.</value>
        public CivilizationMapData MapData => _mapData;

        /// <summary>
        /// Gets the desired borders for the civilization.
        /// </summary>
        /// <value>The desired borders.</value>
        public ConvexHullSet DesiredBorders { get; internal set; }
        #endregion

        #region Methods

        public void SpiedList_Update(List<Civilization> civList)
        {
            _spiedCivList.AddRange(civList);
            //foreach (var item in _civList)
            //{
            //    GameLog.Client.Intel.DebugFormat("Updated the spied list = {0}", item);
            //}
        }

        //public void TargetList_Update(List<Civilization> _civList)
        //{
        //    _targetCivList?.AddRange(_civList);

        //    if (_targetCivList != null)
        //    {
        //        _targetCivList.Distinct();
        //    }
        //    //
        //    //foreach (var item in _civList)
        //    //{
        //    //    GameLog.Client.Intel.DebugFormat("Updated the TargetCivList = {0}", item);
        //    //}
        //}

        public void ShipsOrdered_Check()
        {
            _neededShiptypesList = new List<string> { "dummy" };

            z_ship_Colony_Ordered = 0;
            z_ship_Construction_Ordered = 0;
            z_ship_Medical_Ordered = 0;
            z_ship_Spy_Ordered = 0;
            z_ship_Diplomatic_Ordered = 0;
            z_ship_Science_Ordered = 0;
            z_ship_Scout_Ordered = 0;
            z_ship_FastAttack_Ordered = 0;
            z_ship_Cruiser_Ordered = 0;
            z_ship_StrikeCruiser_Ordered = 0;
            z_ship_HeavyCruiser_Ordered = 0;
            z_ship_Command_Ordered = 0;
            z_ship_Transport_Ordered = 0;

            foreach (var colony in Colonies)
            {
                if (colony.Shipyard == null) continue;
                if (colony.Shipyard.BuildQueue.Count == 0) continue;

                foreach (var item in colony.Shipyard.BuildQueue)
                {
                    if (item.Project.BuildDesign.Key.Contains("COLONY")) z_ship_Colony_Ordered += 1;
                    if (item.Project.BuildDesign.Key.Contains("CONSTRUCTION")) z_ship_Construction_Ordered += 1;
                    if (item.Project.BuildDesign.Key.Contains("MEDICAL")) z_ship_Medical_Ordered += 1;
                    if (item.Project.BuildDesign.Key.Contains("SPY")) z_ship_Spy_Ordered += 1;
                    if (item.Project.BuildDesign.Key.Contains("DIPLO")) z_ship_Diplomatic_Ordered += 1;
                    if (item.Project.BuildDesign.Key.Contains("SCIENCE")) z_ship_Science_Ordered += 1;
                    if (item.Project.BuildDesign.Key.Contains("SCOUT")) z_ship_Scout_Ordered += 1;
                    if (item.Project.BuildDesign.Key.Contains("DESTROYER") || item.Project.BuildDesign.Key.Contains("FRIGATE")) z_ship_FastAttack_Ordered += 1;
                    if (item.Project.BuildDesign.Key.Contains("_CRUISER_")) z_ship_Cruiser_Ordered += 1;
                    if (item.Project.BuildDesign.Key.Contains("STRIKE_CRUISER")) z_ship_StrikeCruiser_Ordered += 1;
                    if (item.Project.BuildDesign.Key.Contains("HEAVY_CRUISER")) z_ship_HeavyCruiser_Ordered += 1;
                    if (item.Project.BuildDesign.Key.Contains("COMMAND")) z_ship_Command_Ordered += 1;
                    if (item.Project.BuildDesign.Key.Contains("TRANSPORT")) z_ship_Transport_Ordered += 1;

                }

            }
            //CheckFor_SystemsToColonizeProject(Colonies.FirstOrDefault);

            //IEnumerable<Fleet> scienceShips = GameContext.Current.Universe.FindOwned<Fleet>(_fleet.Owner).Where(s => s.IsScience);

            Report_Ships_Demand();
        }

        private void CheckFor_SystemsToColonizeProject(Func<Colony> firstOrDefault)
        {
            string _text;
            CivilizationManager _civM = this;
            // need a fleet for getting a range for IsSectorWithinFuelRange
            Fleet fleet = GameContext.Current.Universe.FindOwned<Fleet>(_civM.Civilization).Where(f => f.IsColonizer).FirstOrDefault();
            if (fleet == null)
                return;

            _text = "Step_5392:; " + GameEngine.LocationString(fleet.Location.ToString()) + " using " + fleet.Ships[0].ObjectID + " " + fleet.Ships[0].Design + " > CheckFor_SystemsToColonizeProject..."
                    //+ " - Not Habited: Habitation Aim= "
                    //+ item.HasColony
                    //+ " at " + item.Location
                    //+ " - " + item.Owner
                    ;
            //if (_writeDirectly_Colony)
            //{
            //    Console.WriteLine(_text);
            //}


            var possibleSystems = GameContext.Current.Universe.Find<StarSystem>()
                .Where(c => c.Sector != null && c.IsInhabited == false && c.IsHabitable(_civM.Civilization.Race) == true
                && FleetHelper.IsSectorWithinFuelRange(c.Sector, fleet) && DiplomacyHelper.IsTravelAllowed(_civM.Civilization, c.Sector)) /*&& mapData.IsScanned(c.Location)*/
                //&& mapData.IsExplored(c.Location) && FleetHelper.IsSectorWithinFuelRange(c.Sector, fleet)
                //)//Where other science ship is not already going
                //.Where(d => !otherFleets.Any(f => f.Route.Waypoints.LastOrDefault() == d.Location || d.Location == f.Location))
                .ToList();

            foreach (var item in possibleSystems)
            {
                string _ownerText = "No Owner";
                if (item.Owner != null)
                {
                    _ownerText = item.Owner.Key;
                }
                _text = "Step_5396:; " + GameEngine.LocationString(item.Location.ToString()) + " Check for possible Colonies " // at " + _name_col
                    + " - possible: " + possibleSystems.Count
                    + " - inhabited ? > " + item.IsInhabited //" for HasColony"


                    + " > at " + GameEngine.LocationString(item.Location.ToString())
                    + " - " + _ownerText
                    ;
                //if (_writeDirectly_Colony) 
                Console.WriteLine(_text);
            }
            //neededColonizer = possibleSystems.Count;
           //  _civM.Z_Ship_Colony_Needed = possibleSystems.Count - _civM.Z_Ship_Colony_Available - _civM.Z_Ship_Colony_Ordered;  // set _civM.z_ShipColonyNeeded


            //Report_Ships_Demand();

            if (Civilization.IsHuman)
            {
                Debugger.Break();  // Chekc Colonize shis needed
            }


        }



        public void Report_Ships_Demand()
        {
            string _newline = Environment.NewLine;

            string _text = _newline + "Step_7455:; " + Civilization + "-Ships-Overview ( Available / Needed / Ordered ) for "
                    + _newline + GameEngine.Do_x_Digit_String( 2, Z_Ship_Colony_Available.ToString()) 
                    + "  #  " + GameEngine.Do_x_Digit_String( 2, Z_Ship_Colony_Needed.ToString())
                    + "  #  " + GameEngine.Do_x_Digit_String( 2, Z_Ship_Colony_Ordered.ToString())
                    + " > Colonizer"
                    //+ _newline + Z_Ship_Construction_Available + "  #  " + Z_Ship_Construction_Needed + "  #  " + Z_Ship_Construction_Ordered + " > Constructor"
                    + _newline + GameEngine.Do_x_Digit_String( 2, Z_Ship_Medical_Available.ToString()) 
                    + "  #  " + GameEngine.Do_x_Digit_String( 2, Z_Ship_Medical_Needed.ToString())
                    + "  #  " + GameEngine.Do_x_Digit_String( 2, z_ship_Medical_Ordered.ToString())
                    + " > Medical Ship"
                    + _newline + GameEngine.Do_x_Digit_String( 2, Z_Ship_Spy_Available.ToString()) 
                    + "  #  " + GameEngine.Do_x_Digit_String( 2, Z_Ship_Spy_Needed.ToString())
                    + "  #  " + GameEngine.Do_x_Digit_String( 2, z_ship_Spy_Ordered.ToString())
                    + " > Spy Ship"
                    + _newline + GameEngine.Do_x_Digit_String( 2, Z_Ship_Diplomatic_Available.ToString()) 
                    + "  #  " + GameEngine.Do_x_Digit_String( 2, Z_Ship_Diplomatic_Needed.ToString())
                    + "  #  " + GameEngine.Do_x_Digit_String( 2, z_ship_Diplomatic_Ordered.ToString())
                    + " > Diplomatic Ship"
                    + _newline + GameEngine.Do_x_Digit_String( 2, Z_Ship_Science_Available.ToString()) 
                    + "  #  " + GameEngine.Do_x_Digit_String( 2, Z_Ship_Science_Needed.ToString())
                    + "  #  " + GameEngine.Do_x_Digit_String( 2, z_ship_Science_Ordered.ToString())
                    + " > Science Ship"
                    + _newline + GameEngine.Do_x_Digit_String( 2, Z_Ship_Scout_Available.ToString()) 
                    + "  #  " + GameEngine.Do_x_Digit_String( 2, Z_Ship_Scout_Needed.ToString())
                    + "  #  " + GameEngine.Do_x_Digit_String( 2, z_ship_Scout_Ordered.ToString())
                    + " > Scout Ship"
                    + _newline + GameEngine.Do_x_Digit_String( 2, Z_Ship_Transport_Available.ToString())
                    + "  #  " + GameEngine.Do_x_Digit_String( 2, Z_Ship_Transport_Needed.ToString())
                    + "  #  " + GameEngine.Do_x_Digit_String( 2, z_ship_Transport_Ordered.ToString())
                    + " > Transport Ship Fleet"
                    + _newline + GameEngine.Do_x_Digit_String( 2, Z_Ship_Construction_Available.ToString())
                    + "  #  " + GameEngine.Do_x_Digit_String( 2, Z_Ship_Construction_Needed.ToString())
                    + "  #  " + GameEngine.Do_x_Digit_String( 2, z_ship_Construction_Ordered.ToString())
                    + " > Constructor"
                    + _newline + GameEngine.Do_x_Digit_String( 2, Z_Ship_Combatant_Available.ToString()) 
                    + "  #  " + GameEngine.Do_x_Digit_String( 2, Z_Ship_Combatant_Needed.ToString())
                    + "  #  " + GameEngine.Do_x_Digit_String( 2, z_ship_Combatant_Ordered.ToString())
                    + " > Combatant Ship ###"
                    + _newline + GameEngine.Do_x_Digit_String( 2, Z_Ship_FastAttack_Available.ToString()) 
                    + "  #  " + GameEngine.Do_x_Digit_String( 2, Z_Ship_FastAttack_Needed.ToString())
                    + "  #  " + GameEngine.Do_x_Digit_String( 2, z_ship_FastAttack_Ordered.ToString())
                    + " > Fast Attack Ship"
                    + _newline + GameEngine.Do_x_Digit_String( 2, Z_Ship_Cruiser_Available.ToString()) 
                    + "  #  " + GameEngine.Do_x_Digit_String( 2, Z_Ship_Cruiser_Needed.ToString())
                    + "  #  " + GameEngine.Do_x_Digit_String( 2, z_ship_Cruiser_Ordered.ToString())
                    + " > Cruiser"
                    + _newline + GameEngine.Do_x_Digit_String( 2, Z_Ship_HeavyCruiser_Available.ToString()) 
                    + "  #  " + GameEngine.Do_x_Digit_String( 2, Z_Ship_HeavyCruiser_Needed.ToString())
                    + "  #  " + GameEngine.Do_x_Digit_String( 2, z_ship_HeavyCruiser_Ordered.ToString())
                    + " > HeavyCruiser"
                    + _newline + GameEngine.Do_x_Digit_String( 2, Z_Ship_StrikeCruiser_Available.ToString()) 
                    + "  #  " + GameEngine.Do_x_Digit_String( 2, Z_Ship_StrikeCruiser_Needed.ToString())
                    + "  #  " + GameEngine.Do_x_Digit_String( 2, z_ship_StrikeCruiser_Ordered.ToString())
                    + " > StrikeCruiser"
                    + _newline + GameEngine.Do_x_Digit_String( 2, Z_Ship_Command_Available.ToString()) 
                    + "  #  " + GameEngine.Do_x_Digit_String( 2, Z_Ship_Command_Needed.ToString())
                    + "  #  " + GameEngine.Do_x_Digit_String( 2, z_ship_Command_Ordered.ToString())
                    + " > Command Ship"
                    ;
            
            if (true)
            {
                //Console.WriteLine(_text);
            }

            if (Civilization.IsHuman)
            {
                Console.WriteLine(_text);
                //Debugger.Break();  // Check all ships > Available / Needed / Ordered
                //Console.WriteLine(_text);
            }
        }

        /// <summary>
        /// Applies the specified morale event.
        /// </summary>
        /// <param name="eventType">Type of the event.</param>
        public void ApplyMoraleEvent(MoraleEvent eventType)
        {
            ApplyMoraleEvent(eventType, SeatOfGovernment.Location);
        }

        /// <summary>
        /// Applies the specified morale event.
        /// </summary>
        /// <param name="eventType">Type of the event.</param>
        /// <param name="location">The location at which the event occurred.</param>
        public void ApplyMoraleEvent(MoraleEvent eventType, MapLocation location)
        {
            Data.Table moraleTable = GameContext.Current.Tables.MoraleTables["MoraleEventResults"];
            if (moraleTable == null)
            {
                return;
            }

            const float multiplier = 1.0f;

            string tableValue = moraleTable[eventType.ToString()][_civId] ??
                             moraleTable[eventType.ToString()][0];

            if (tableValue == null)
            {
                return;
            }

            if (!int.TryParse(tableValue, out int change))
            {
                return;
            }

            foreach (Colony colony in Colonies)
            {
                _ = colony.Morale.AdjustCurrent((int)(multiplier * change));
            }
        }

        /// <summary>
        /// Compacts this instance for serialization.
        /// </summary>
        public void Compact()
        {
            _colonies.TrimExcess();
            _globalBonuses.TrimExcess();
            _sitRepEntries.TrimExcess();
        }

        public void OnTurnBeginn()
        {
            ShipsOrdered_Check(); // to have the list of needed ships updated
            CheckFor_SystemsToColonizeProject(Colonies.FirstOrDefault);
        }

        /// <summary>
        /// Called when the current game turn is finished.
        /// </summary>
        public void OnTurnFinished()
        {
            OnPropertyChanged("AverageMorale");
        }

        public void EnsureSeatOfGovernment()
        {
            Colony seatOfGovernment = SeatOfGovernment;
            if (seatOfGovernment == null || seatOfGovernment.OwnerID != CivilizationID)
            {
                MapLocation? homeColonyLocation = _homeColonyLocation;

                double rankHueristic(Colony c)
                {
                    if (!homeColonyLocation.HasValue)
                    {
                        return 1d;
                    }

                    double distanceFactor = Math.Min(
                        0.2,
                        Math.Max(
                            1d,
                            2d / MapLocation.GetDistance(c.Location, homeColonyLocation.Value)));

                    return c.ColonyValue() * distanceFactor;
                }

                seatOfGovernment = (
                                       from c in Colonies
                                       where c.OwnerID == CivilizationID
                                       orderby rankHueristic(c) descending
                                       select c
                                   ).FirstOrDefault();

                _seatOfGovernmentId = seatOfGovernment != null ? seatOfGovernment.ObjectID : -1;
            }

            Diplomacy.Diplomat diplomat = GameContext.Current.Diplomats[_civId];
            if (diplomat != null)
            {
                diplomat.SeatOfGovernment = seatOfGovernment;
            }
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">Name of the property that changed.</param>
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Handles the PropertyChanged event of the TotalPopulation property.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void OnTotalPopulationPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentValue")
            {
                OnPropertyChanged("TotalPopulation");
            }
        }
        private void OnTotalValuePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentValue")
            {
                OnPropertyChanged("TotalValue");
            }
        }
        private void OnTotalResearchPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentValue")
            {
                OnPropertyChanged("TotalResearch");
            }
        }
        private void OnInstallingSpyNetworkPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentValue")
            {
                OnPropertyChanged("InstallingSpyNetwork");
            }
        }
        private void OnTotalIntelligenceAttackingAccumulatedPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //GameLog.Client.IntelDetails.DebugFormat("OnTotalIntelAttackingAccumulated sender ={0} property changed ={1}", sender.ToString(), e.PropertyName.ToString());
            if (e.PropertyName == "CurrentValue")
            {
                OnPropertyChanged("TotalIntelligenceAttackingAccumulated");
            }
        }

        private void OnTotalIntelligenceDefenseAccumulatedPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //GameLog.Client.IntelDetails.DebugFormat("OnTotalIntelDefenceAccumulated sender ={0} property changed ={1}", sender.ToString(), e.PropertyName.ToString());
            if (e.PropertyName == "CurrentValue")
            {
                OnPropertyChanged("TotalIntelligenceDefenseAccumulated");
            }
        }

        #endregion

        #region INotifyPropertyChanged Members
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Static Accessors

        public static CivilizationManager For([NotNull] Civilization civ)
        {
            if (civ == null)
            {
                throw new ArgumentNullException("civ");
            }

            return GameContext.Current.CivilizationManagers[civ];
        }

        public static CivilizationManager For([NotNull] string civKey)
        {
            if (civKey == null)
            {
                throw new ArgumentNullException("civKey");
            }

            return GameContext.Current.CivilizationManagers[civKey];
        }

        public static CivilizationManager For(int civId)
        {
            return GameContext.Current.CivilizationManagers[civId];
        }

        #endregion

        #region Implementation of ICivIdentity

        int ICivIdentity.CivID => _civId;

        public List<int> IntelIDs { get => _IntelIDs; set => _IntelIDs = value; }
        public object AppContextProperty { get; private set; }
        public Civilization LocalPlayer { get; private set; }


        #endregion
    }

    /// <summary>
    /// A collection of <typeparamref name="TValue"/> instances, keyed by
    /// civilization ID and indexed by civilization ID or civilization.
    /// </summary>
    [Serializable]
    public class CivilizationKeyedMap<TValue> : KeyedCollectionBase<int, TValue>
    {
        #region Constructors

        public CivilizationKeyedMap(Func<TValue, int> keyRetriever)
            : base(keyRetriever) { }

        #endregion

        #region Properties and Indexers

        /// <summary>
        /// Gets the <typeparamref name="TValue"/> for the specified civilization.
        /// </summary>
        /// <value>The <typeparamref name="TValue"/>.</value>
        public TValue this[ICivIdentity civilization]
        {
            get
            {
                if (civilization == null)
                {
                    throw new ArgumentNullException("civilization");
                }

                _ = TryGetValue(civilization.CivID, out TValue value);
                return value;
            }
        }

        /// <summary>
        /// Gets the <typeparamref name="TValue"/> for the specified civilization.
        /// </summary>
        /// <value>The <typeparamref name="TValue"/>.</value>
        public TValue this[string civKey]
        {
            get
            {
                _ = TryGetValue(civKey, out TValue value);
                return value;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Tries to get the <typeparamref name="TValue"/> for the Civilization with the key <paramref name="civKey"/>.
        /// </summary>
        /// <param name="civKey">The Civilization's key.</param>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> if the value was successfully retrieved; otherwise, <c>false</c></returns>
        public bool TryGetValue(string civKey, out TValue value)
        {
            if (GameContext.Current.Civilizations.TryGetValue(civKey, out Civilization civ))
            {
                if (civ != null)
                {
                    value = this[civ.CivID];
                    return true;
                }
            }
            value = typeof(TValue).IsValueType ? Activator.CreateInstance<TValue>() : default;
            return false;
        }

        #endregion
    }

    /// <summary>
    /// A collection of <see cref="CivilizationManager"/> instances, keyed by
    /// civilization ID and indexed by civilization ID or civilization.
    /// </summary>
    [Serializable]
    public class CivilizationManagerMap : CivilizationKeyedMap<CivilizationManager>
    {
        #region Constructors

        public CivilizationManagerMap()
            : base(o => o.CivilizationID) { }

        #endregion
    }

    /// <summary>
    /// A collection of Colonies keyed by Object ID.
    /// </summary>
    [Serializable]
    public sealed class ColonyMap : KeyedCollectionBase<int, Colony>
    {
        #region Constructors

        public ColonyMap()
            : base(o => o.ObjectID) { }

        #endregion
    }

}