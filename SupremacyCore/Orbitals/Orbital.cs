// Orbital.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using Supremacy.Game;
using Supremacy.IO.Serialization;
using Supremacy.Tech;
using Supremacy.Types;
using Supremacy.Universe;
using Supremacy.Utility;

namespace Supremacy.Orbitals
{
    /// <summary>
    /// Represents the base type of any orbital object in the game (i.e. weapon platforms,
    /// ships, and stations).
    /// </summary>
    [Serializable]
    public class Orbital : TechObject
    {
        private Meter _crew;
        private ushort _experienceLevel;
        private Meter _hullStrength;
        private Meter _shieldStrength;
        private Meter _cloakStrength;
        private Meter _camouflagedMeter;

        /// <summary>
        /// Gets the type of the UniverseObject.
        /// </summary>
        /// <value>The type of the UniverseObject.</value>
        public override UniverseObjectType ObjectType
        {
            get { return UniverseObjectType.Orbital; }
        }

        /// <summary>
        /// Gets the crew complement.
        /// </summary>
        /// <value>The crew.</value>
        public Meter Crew
        {
            get { return _crew; }
        }

        /// <summary>
        /// Gets or sets the design.
        /// </summary>
        /// <value>The design.</value>
        public OrbitalDesign OrbitalDesign
        {
            get { return Design as OrbitalDesign; }
            set { Design = value; }
        }

        /// <summary>
        /// Gets the hull strength.
        /// </summary>
        /// <value>The hull strength.</value>
        public Meter HullStrength
        {
            get { return _hullStrength; }
        }

        /// <summary>
        /// Gets the shield strength.
        /// </summary>
        /// <value>The shield strength.</value>
        public Meter ShieldStrength
        {
            get { return _shieldStrength; }
        }

        /// <summary>
        /// Gets the cloak strength.
        /// </summary>
        /// <value>The cloak strength.</value>
        public Meter CloakStrength
        {
            get { return _cloakStrength; }
        }

        /// <summary>
        /// Gets the camouflage strength.
        /// </summary>
        /// <value>The camouflage strength.</value>
        public Meter CamouflagedMeter
        {
            get { return _camouflagedMeter; }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="Orbital"/> is combatant.
        /// </summary>
        /// <value>
        /// <c>true</c> if this <see cref="Orbital"/> is combatant; otherwise, <c>false</c>.
        /// </value>
        public bool IsCombatant
        {
            get { return OrbitalDesign.IsCombatant; }
        }

        /// <summary>
        /// Gets or sets the crew experience level.
        /// </summary>
        /// <value>The crew experience level.</value>
        public int ExperienceLevel
        {
            get { return _experienceLevel; }
            set { _experienceLevel = (ushort)Math.Max(0, Math.Min(value, UInt16.MaxValue)); }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="Orbital"/> is manned.
        /// </summary>
        /// <value><c>true</c> if this <see cref="Orbital"/> is manned; otherwise, <c>false</c>.</value>
        public bool IsManned
        {
            get { return (OrbitalDesign.CrewSize > 0); }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="Orbital"/> is operational.
        /// </summary>
        /// <value>
        /// <c>true</c> if this <see cref="Orbital"/> is operational; otherwise, <c>false</c>.
        /// </value>
        public bool IsOperational
        {
            get { return (!IsManned || !Crew.IsMinimized); }
        }

        /// <summary>
        /// Gets the crew experience rank.
        /// </summary>
        /// <value>The crew experience rank.</value>
        public ExperienceRank ExperienceRank
        {
            get
            {
                var rankTable = GameContext.Current.Tables.ShipTables["ExperienceRanks"];
                for (int i = 0; i < rankTable.Rows.Count; i++)
                {
                    int minimum;
                    if (int.TryParse(rankTable[i][0], out minimum))
                    {
                        if (ExperienceLevel >= minimum)
                        {
                            return (ExperienceRank)Enum.Parse(
                                typeof(ExperienceRank), rankTable[i].Name);
                        }
                    }
                }
                return ExperienceRank.Green;
            }
        }

        /// <summary>
        /// Gets the crew experience rank as a string.
        /// </summary>
        /// <value>The experience rank as a string.</value>
        public string ExperienceRankString
        {
            get
            {
                return GameContext.Current.Tables.EnumTables["ExperienceRank"][ExperienceRank.ToString()][0];
            }
        }

        public virtual bool IsMobile
        {
            get { return false; }
        }

        public Orbital() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Orbital"/> class using the specified design.
        /// </summary>
        /// <param name="design">The design.</param>
        public Orbital(OrbitalDesign design)
            : base(design)
        {
            _crew = new Meter(design.CrewSize, 0, design.CrewSize);
            _crew.CurrentValueChanged += Crew_CurrentValueChanged;
            _hullStrength = new Meter(design.HullStrength, 0, design.HullStrength);
            _shieldStrength = new Meter(design.ShieldStrength, 0, design.ShieldStrength);
            _cloakStrength = new Meter(design.CloakStrength, 0, design.CloakStrength);
            _camouflagedMeter = new Meter(design.CamouflagedStrength, 0, design.CamouflagedStrength);
        }

        /// <summary>
        /// Handles the CurrentValueChanged event of the Crew control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MeterChangedEventArgs"/> instance containing the event data.</param>
        private void Crew_CurrentValueChanged(object sender, MeterChangedEventArgs e)
        {
            if (!e.Cancel)
            {
                if (_crew.IsMinimized)
                {
                    OnPropertyChanged("IsOperational");
                }
                e.Cancel = true;
            }
        }

        /// <summary>
        /// Regenerates the hull.
        /// </summary>
        private void RegenerateHull()
        {
            // TODO Disabling Negative treasury stuff because it's apparently not working properly.
            // To be re-instated when properly fixed.
            // don't regenerate hull damage if empire treasury is in the red
            /*if (Owner != null)
            {
                CivilizationManager civManager = GameContext.Current.CivilizationManagers[Owner];
                if (civManager.Credits.CurrentValue <= 0)
                    return;
            }*/

            double increase = 0.01;

            // repair abilities are better in allied systems with colonies or starbases
            Entities.Civilization claimingCiv = null;
            if(GameContext.Current.SectorClaims != null)
                claimingCiv = GameContext.Current.SectorClaims.GetPerceivedOwner(Sector.Location, Owner);
            if (claimingCiv == Owner)
            {
                // Added that system must be owned by ship´s owner
                if ((Sector.System != null) && Sector.System.HasColony && Sector.System.Owner == Owner)
                {
                    increase = 0.10;
                }// Added that Station must be owned by ship´s owner
                else if (Sector.Station != null)
                {
                    increase = 0.05;
                }
                //else // no more Hull repair without a base
                //{
                //    increase = 0.025;
                //}
            }

            HullStrength.AdjustCurrent((int)Math.Ceiling(increase * HullStrength.Maximum));
        }

        /// <summary>
        /// Regenerates the shields.
        /// </summary>
        public void RegenerateShields()
        {       //Bases regenerate their own shields 4 times quicker. See ship repair in simular line elsewhere
            double increase = (OrbitalDesign.ShieldRechargeRate) * ShieldStrength.Maximum;   // ShieldStrength shown in Encyclopedia

            // recently values were divided by seven and for stations multiplied with 4 - not neccessary anymore
            //if (OrbitalDesign.Key.Contains("BASE") 
            //    || OrbitalDesign.Key.Contains("OUTPOST")
            //    || OrbitalDesign.Key.Contains("TRANSWARP")
            //    || OrbitalDesign.Key.Contains("STATION")
            //    )
            //{
            //    increase = increase * 4;
            //}

            ShieldStrength.AdjustCurrent((int)Math.Ceiling(increase));
        }

        /// <summary>
        /// Resets this <see cref="Orbital"/> at the end of each game turn.
        /// If there are any fields or properties of this <see cref="Orbital"/>
        /// that should be reset or modified at the end of each turn, perform
        /// those operations here.
        /// </summary>
        protected internal override void Reset()
        {
            base.Reset();
            //base.DynamicObjectType // that?
            // No shield regeneration // use Reharge value /7 from it. Here is where the ship regeneration is 
            // next 2 lines not needed they are in Regnerate shields -> no they are needed, its that thats working
            double increase = (OrbitalDesign.ShieldRechargeRate /7) * ShieldStrength.Maximum; // Reduce Oribtal ShieldRecharge to 50% Not yet tested
            ShieldStrength.AdjustCurrent((int)Math.Ceiling(increase));


            // orignal stuff. No longer full recovery after battle.
            // _shieldStrength.Reset(_shieldStrength.Maximum);
            // RegenerateHull();

            // full regeneration at homebases
            //double increase = 0.01; // only minimal hull repair on non-bases
            Entities.Civilization claimingCiv = null;
            if (GameContext.Current.SectorClaims != null)
                claimingCiv = GameContext.Current.SectorClaims.GetPerceivedOwner(Sector.Location, Owner);
            GameLog.Core.MapData.DebugFormat("claimingCiv = {0}, Sector {1}, but owner=newOwner wish to be = {2}", claimingCiv, Sector.Location.ToString(), Owner);
            if (claimingCiv == Owner)
            {
                if ((Sector.System != null) && Sector.System.HasColony && Sector.System.Owner == Owner)
                {
                    _shieldStrength.Reset(_shieldStrength.Maximum);

                    RegenerateHull();
                    increase = 0.07;
                    HullStrength.AdjustCurrent((int)Math.Ceiling(increase * HullStrength.Maximum));
                    _hullStrength.UpdateAndReset();
                    GameLog.Core.MapData.DebugFormat("claiming: Sector has colony = {0}, Sector = {1}, Owner = {2}", Sector.System.Colony.Name, Sector.Location.ToString(), Owner);
                }
                if (Sector.Station != null) 
                {
                    _shieldStrength.Reset(_shieldStrength.Maximum);
                    increase = 0.10;
                    RegenerateHull();
                    HullStrength.AdjustCurrent((int)Math.Ceiling(increase * HullStrength.Maximum));
                    _hullStrength.UpdateAndReset();
                    GameLog.Core.MapData.DebugFormat("claiming: Sector has station = {0}, Sector = {1}, Owner = {2}", Sector.Station.Name, Sector.Location.ToString(), Owner);
                }
            }
            // no more instant-repair outcomment the following line
            //_hullStrength.UpdateAndReset();
        }

        public override void SerializeOwnedData(SerializationWriter writer, object context)
		{
			base.SerializeOwnedData(writer, context);
			writer.WriteObject(_crew);
			writer.Write(_experienceLevel);
			writer.WriteObject(_hullStrength);
			writer.WriteObject(_shieldStrength);
            writer.WriteObject(_cloakStrength);
            writer.WriteObject(_camouflagedMeter);
        }

		public override void DeserializeOwnedData(SerializationReader reader, object context)
		{
			base.DeserializeOwnedData(reader, context);
			_crew = (Meter)reader.ReadObject();
			_experienceLevel = reader.ReadUInt16();
			_hullStrength = (Meter)reader.ReadObject();
			_shieldStrength = (Meter)reader.ReadObject();
            _cloakStrength = (Meter)reader.ReadObject();
            _camouflagedMeter = (Meter)reader.ReadObject();
            _crew.CurrentValueChanged += Crew_CurrentValueChanged;
		}
    }

    /// <summary>
    /// Defines the crew experience rank levels.
    /// </summary>
    public enum ExperienceRank : byte
    {
        Green = 0,
        Regular,
        Veteran,
        Elite,
        Legendary
    }
}

