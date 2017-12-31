// GameOptions.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.IO;

using Supremacy.Annotations;
using Supremacy.IO;
using Supremacy.Utility;

namespace Supremacy.Game
{
    /// <summary>
    /// Defines the different AI modes available in the game.
    /// </summary>
    public enum AIMode : byte
    {
        /// <summary>
        /// Standard AI
        /// </summary>
        Normal = 0,

        /// <summary>
        /// AI is more agreeable towards human players.
        /// </summary>
        Agreeable = 1,

        /// <summary>
        /// AI is more aggressive towards human players.
        /// </summary>
        Aggressive = 2
    }

    /// <summary>
    /// Defines the galaxy shapes available in the game.
    /// </summary>
    public enum GalaxyShape : byte
    {
        Spiral = 0,
        Ring,
        Elliptical,
        Cluster,
        Irregular
    }

    /// <summary>
    /// Defines the galaxy sizes available in the game.
    /// </summary>
    public enum GalaxySize : byte
    {
        Tiny = 0,
        Small,
        Medium,
        Large,
        Huge
    }

    /// <summary>
    /// Defines the planet densities available in the game.
    /// </summary>
    public enum PlanetDensity : byte
    {
        Sparse = 3,
        Medium = 5,
        Dense = 7
    }

    /// <summary>
    /// Defines the star densities available in the game.
    /// </summary>
    public enum StarDensity : byte
    {
        Sparse = 1,
        Medium = 2,
        Dense = 3
    }

    /// <summary>
    /// Defines the minor race frequencies available in the game.
    /// </summary>
    public enum MinorRaceFrequency : byte
    {
        None = 0,
        Few,
        Some,
        Many
    }

    /// <summary>
    /// Defines the starting tech levels in the game.
    /// </summary>
    public enum StartingTechLevel : byte
    {
        Early = 0,
        Developed,
        Sophisticated,
        Advanced,
        Supreme
    }
    

    /// <summary>
    /// Determines whether the civilization is an Empire or an Expanding Power
    /// </summary>
    public enum EmpirePlayable : byte
    {
        No = 0,
        Yes
    }

    //public string PlayerNameSP
    //{
    //    PlayerNameSP = "choice your name";
    //}


    /// <summary>
    /// Defines the options available at the beginning of a game.
    /// </summary>
    [Serializable]
    public sealed class GameOptions : ICloneable
    {
        #region Constructors
        public GameOptions()
        {
            ModID = Guid.Empty;
            AIMode = AIMode.Normal;
            AITakeover = true;
            CombatTimer = default(TimeSpan);
            GalaxyShape = GalaxyShape.Irregular;
            GalaxySize = GalaxySize.Small;
            PlanetDensity = PlanetDensity.Medium;
            StarDensity = StarDensity.Sparse;
            MinorRaceFrequency = MinorRaceFrequency.Many;
            StartingTechLevel = StartingTechLevel.Developed;
            //PlayerNameSP = "choice your name";
            IntroPlayable = EmpirePlayable.No;  // just place holder
            FederationPlayable = EmpirePlayable.Yes;
            RomulanPlayable = EmpirePlayable.Yes;
            KlingonPlayable = EmpirePlayable.Yes;
            CardassianPlayable = EmpirePlayable.Yes;
            DominionPlayable = EmpirePlayable.Yes;
            BorgPlayable = EmpirePlayable.No;
            TerranEmpirePlayable = EmpirePlayable.No;
        }
        #endregion

        #region Properties
        public bool IsFrozen{ get; private set; }

        /// <summary>
        /// Gets or sets the mod ID.
        /// </summary>
        /// <value>The mod ID.</value>
        public Guid ModID { get; set; }

        /// <summary>
        /// Gets or sets the AI mode.
        /// </summary>
        /// <value>The AI mode.</value>
        public AIMode AIMode { get; set; }

        /// <summary>
        /// Gets or sets whether the AI takes over for players who drop from a multiplayer game.
        /// </summary>
        public bool AITakeover { get; set; }

        /// <summary>
        /// Gets or sets the combat timer (multiplayer games only).
        /// </summary>
        /// <value>The combat timer.</value>
        public TimeSpan CombatTimer { get; set; }

        /// <summary>
        /// Gets or sets the galaxy shape.
        /// </summary>
        /// <value>The galaxy shape.</value>
        public GalaxyShape GalaxyShape { get; set; }

        /// <summary>
        /// Gets or sets the size of the galaxy.
        /// </summary>
        /// <value>The size of the galaxy.</value>
        public GalaxySize GalaxySize { get; set; }

        /// <summary>
        /// Gets or sets the planet density.
        /// </summary>
        /// <value>The planet density.</value>
        public PlanetDensity PlanetDensity { get; set; }

        /// <summary>
        /// Gets or sets the star density.
        /// </summary>
        /// <value>The star density.</value>
        public StarDensity StarDensity { get; set; }

        /// <summary>
        /// Gets or sets the minor race frequency.
        /// </summary>
        /// <value>The minor race frequency.</value>
        public MinorRaceFrequency MinorRaceFrequency { get; set; }

        /// <summary>
        /// Gets or sets the starting tech level.
        /// </summary>
        /// <value>The starting tech level.</value>
        public StartingTechLevel StartingTechLevel { get; set; }

        /// <summary>
        /// Gets or sets the starting tech level.
        /// </summary>
        /// <value>The starting tech level.</value>
        //public PlayerNameSP PlayerNameSP { get; set; }


        /// <summary>
        /// Gets or sets IntroPlayable.   // just a place holder
        /// </summary>
        /// <value> Federation playable yes or no.</value>
        public EmpirePlayable IntroPlayable { get; set; }

        /// <summary>
        /// Gets or sets FederationPlayable.
        /// </summary>
        /// <value> Federation playable yes or no.</value>
        public EmpirePlayable FederationPlayable { get; set; }

        /// <summary>
        /// Gets or sets RomulanPlayable.
        /// </summary>
        /// <value> Romulan playable yes or no.</value>
        public EmpirePlayable RomulanPlayable { get; set; }

        /// <summary>
        /// Gets or sets KlingonPlayable.
        /// </summary>
        /// <value> Klingon playable yes or no.</value>
        public EmpirePlayable KlingonPlayable { get; set; }

        /// <summary>
        /// Gets or sets CardassianPlayable.
        /// </summary>
        /// <value> Cardassian playable yes or no.</value>
        public EmpirePlayable CardassianPlayable { get; set; }

        /// <summary>
        /// Gets or sets DominionPlayable.
        /// </summary>
        /// <value> Dominion playable yes or no.</value>
        public EmpirePlayable DominionPlayable { get; set; }

        /// <summary>
        /// Gets or sets BorgPlayable.
        /// </summary>
        /// <value> borg playable yes or no.</value>
        public EmpirePlayable BorgPlayable { get; set; }

        /// <summary>
        /// Gets or sets TerranEmpirePlayable.
        /// </summary>
        /// <value> TerranEmpire playable yes or no.</value>
        public EmpirePlayable TerranEmpirePlayable { get; set; }

        /// <summary>
        /// Gets or sets the turn timer (multiplayer games only).
        /// </summary>
        /// <value>The turn timer.</value>
        public TimeSpan TurnTimer { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether civilizations should be placed based on their home system quadrant.
        /// </summary>
        /// <value><c>true</c> if civilizations should be placed based on their home system quadrant; otherwise, <c>false</c>.</value>
        public bool UseHomeQuadrants { get; set; }

        #endregion

        #region Methods
        public void Freeze()
        {
            IsFrozen = true;
        }

        public void Write([NotNull] BinaryWriter writer)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");

            // needs the same sorting as Reading

            writer.Write(IsFrozen);
            var modIdBytes = ModID.ToByteArray();
            writer.Write(modIdBytes.Length);
            writer.Write(modIdBytes, 0, modIdBytes.Length);
            writer.Write((byte)AIMode);
            writer.Write((byte)GalaxyShape);
            writer.Write((byte)GalaxySize);
            writer.Write((byte)PlanetDensity);
            writer.Write((byte)StarDensity);
            writer.Write((byte)MinorRaceFrequency);
            writer.Write((byte)StartingTechLevel);
            writer.Write((byte)IntroPlayable);
            writer.Write((byte)FederationPlayable);
            writer.Write((byte)RomulanPlayable);
            writer.Write((byte)KlingonPlayable);
            writer.Write((byte)CardassianPlayable);
            writer.Write((byte)DominionPlayable);
            writer.Write((byte)BorgPlayable);
            writer.Write((byte)TerranEmpirePlayable);
            writer.Write(AITakeover);
            writer.Write(TurnTimer.Ticks);
            writer.Write(CombatTimer.Ticks);

            /*
            GameLog.Print("GameOptions writing: IsFrozen={0}", this.IsFrozen);
            //var modIdBytesLength = reader.ReadInt32();
            GameLog.Print("GameOptions writing: ModID={0}", this.ModID);
            GameLog.Print("GameOptions writing: AIMode={0}", this.AIMode);
            GameLog.Print("GameOptions writing: AITakeover={0}", this.AITakeover);
            GameLog.Print("GameOptions writing: CombatTimer={0}", this.CombatTimer);
            GameLog.Print("GameOptions writing: GalaxyShape={0}", this.GalaxyShape);
            GameLog.Print("GameOptions writing: GalaxySize={0}", this.GalaxySize);
            GameLog.Print("GameOptions writing: PlanetDensity={0}", this.PlanetDensity);
            GameLog.Print("GameOptions writing: StarDensity={0}", this.StarDensity);
            GameLog.Print("GameOptions writing: MinorRaceFrequency={0}", this.MinorRaceFrequency);
            GameLog.Print("GameOptions writing: StartingTechLevel={0}", this.StartingTechLevel);
            GameLog.Print("GameOptions writing: IntroPlayable={0}", this.IntroPlayable);
            GameLog.Print("GameOptions writing: FederationPlayable={0}", this.FederationPlayable);
            GameLog.Print("GameOptions writing: RomulanPlayable={0}", this.RomulanPlayable);
            GameLog.Print("GameOptions writing: KlingonPlayable={0}", this.KlingonPlayable);
            GameLog.Print("GameOptions writing: CardassianPlayable={0}", this.CardassianPlayable);
            GameLog.Print("GameOptions writing: DominionPlayable={0}", this.DominionPlayable);
            GameLog.Print("GameOptions writing: BorgPlayable={0}", this.BorgPlayable);
            GameLog.Print("GameOptions writing: TerranEmpirePlayable={0}", this.TerranEmpirePlayable);

            GameLog.Print("GameOptions writing: TurnTimer={0}", this.TurnTimer);
            */

        }

        public void Read([NotNull] BinaryReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            // needs the same sorting as Writing

            IsFrozen = reader.ReadBoolean();
            var modIdBytesLength = reader.ReadInt32();
            ModID = new Guid(reader.ReadBytes(modIdBytesLength));
            AIMode = (AIMode)reader.ReadByte();
            GalaxyShape = (GalaxyShape)reader.ReadByte();
            GalaxySize = (GalaxySize)reader.ReadByte();

            PlanetDensity = (PlanetDensity)reader.ReadByte();
            //GameLog.Print("PlanetDensity from file = {0}", this.PlanetDensity);
            //this.PlanetDensity = PlanetDensity.Medium;   // enables "Loading a game"

            StarDensity = (StarDensity)reader.ReadByte();
            MinorRaceFrequency = (MinorRaceFrequency)reader.ReadByte();
            StartingTechLevel = (StartingTechLevel)reader.ReadByte();
            IntroPlayable = (EmpirePlayable)reader.ReadByte();
            FederationPlayable = (EmpirePlayable)reader.ReadByte();
            RomulanPlayable = (EmpirePlayable)reader.ReadByte();
            KlingonPlayable = (EmpirePlayable)reader.ReadByte();
            CardassianPlayable = (EmpirePlayable)reader.ReadByte();
            DominionPlayable = (EmpirePlayable)reader.ReadByte();
            BorgPlayable = (EmpirePlayable)reader.ReadByte();
            TerranEmpirePlayable = (EmpirePlayable)reader.ReadByte();

            AITakeover = reader.ReadBoolean();
            TurnTimer = TimeSpan.FromTicks(reader.ReadInt64());
            CombatTimer = TimeSpan.FromTicks(reader.ReadInt64());

            /*
            GameLog.Print("GameOptions reading: IsFrozen={0}", this.IsFrozen);
            //var modIdBytesLength = reader.ReadInt32();
            GameLog.Print("GameOptions reading: ModID={0}", this.ModID);
            GameLog.Print("GameOptions reading: AIMode={0}", this.AIMode);
            GameLog.Print("GameOptions reading: AITakeover={0}", this.AITakeover);
            GameLog.Print("GameOptions reading: CombatTimer={0}", this.CombatTimer);
            GameLog.Print("GameOptions reading: GalaxyShape={0}", this.GalaxyShape);
            GameLog.Print("GameOptions reading: GalaxySize={0}", this.GalaxySize);
            GameLog.Print("GameOptions reading: PlanetDensity={0}", this.PlanetDensity);
            GameLog.Print("GameOptions reading: StarDensity={0}", this.StarDensity);
            GameLog.Print("GameOptions reading: MinorRaceFrequency={0}", this.MinorRaceFrequency);
            GameLog.Print("GameOptions reading: StartingTechLevel={0}", this.StartingTechLevel);
            GameLog.Print("GameOptions reading: IntroPlayable={0}", this.IntroPlayable);
            GameLog.Print("GameOptions reading: FederationPlayable={0}", this.FederationPlayable);
            GameLog.Print("GameOptions reading: RomulanPlayable={0}", this.RomulanPlayable);
            GameLog.Print("GameOptions reading: KlingonPlayable={0}", this.KlingonPlayable);
            GameLog.Print("GameOptions reading: CardassianPlayable={0}", this.CardassianPlayable);
            GameLog.Print("GameOptions reading: DominionPlayable={0}", this.DominionPlayable);
            GameLog.Print("GameOptions reading: BorgPlayable={0}", this.BorgPlayable);
            GameLog.Print("GameOptions reading: TerranEmpirePlayable={0}", this.TerranEmpirePlayable);

            GameLog.Print("GameOptions reading: TurnTimer={0}", this.TurnTimer);
            */

        }

        internal static bool Validate(GameOptions options)
        {
            if (options == null)
                return false;
            if (!Enum.IsDefined(typeof(AIMode), options.AIMode))
                return false;
            if (!Enum.IsDefined(typeof(GalaxyShape), options.GalaxyShape))
                return false;
            if (!Enum.IsDefined(typeof(GalaxySize), options.GalaxySize))
                return false;
            if (!Enum.IsDefined(typeof(PlanetDensity), options.PlanetDensity))
                return false;
            if (!Enum.IsDefined(typeof(StarDensity), options.StarDensity))
                return false;

            //if (!Enum.IsDefined(typeof(BorgPlayable), options.BorgPlayable))
            //    return false;

            return true;
        }

        public GameOptions Clone()
        {
            var clone = (GameOptions)MemberwiseClone();
            clone.IsFrozen = false;
            return clone;
        }
        #endregion

        #region Implementation of ICloneable
        object ICloneable.Clone()
        {
            return Clone();
        }
        #endregion
    }

    /// <summary>
    /// Helper class for managing default game options.
    /// </summary>
    public static class GameOptionsManager
    {
        #region Methods
        /// <summary>
        /// Loads the default game options.
        /// </summary>
        /// <returns>The default game options.</returns>
        public static GameOptions LoadDefaults()
        {
            GameOptions defaults;
            try
            {
                defaults = StorageManager.ReadSetting<string, GameOptions>("DefaultGameOptions");
            }
            catch
            {
                defaults = null;
            }
            if ((defaults == null) || !GameOptions.Validate(defaults))
            {
                defaults = new GameOptions();
                try
                {
                    SaveDefaults(defaults);
                }
                catch
                {
                    return defaults;
                }
            }
            return defaults;
        }

        /// <summary>
        /// Saves the default game options.
        /// </summary>
        /// <param name="defaults">The new defaults.</param>
        /// <returns><c>true</c> if successful; otherwise, <c>false</c>.</returns>
        public static bool SaveDefaults(GameOptions defaults)
        {
            var success = false;
            try
            {
                StorageManager.WriteSetting("DefaultGameOptions", defaults.Clone());
                success = true;
            }
            catch (Exception e)
            {
                GameLog.Debug.General.DebugFormat( "GameOptionsManager.SaveDefaults() encountered an error: {0}", e.Message);
            }
            return success;
        }
        #endregion
    }
}