// File:GameOptions.cs
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
        Many,
        Most
    }

    /// <summary>
    /// Defines if races are in their canon quadrants.
    /// </summary>
    public enum GalaxyCanon : byte
    {
        Canon = 0,
        Random
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

    /// <summary>
    /// Gives an Empire a bonus or a malus
    /// </summary>
    /// 
    public enum EmpireModifier : int
    {
        Handicape_Biggest = -5,
        Handicape_Big = -4,
        Handicape_Medium = -3,
        Handicape_Small = -2,
        Handicape_Smallest = -1,
        Standard = 0,
        Bonus_Smallest = 1,
        Bonus_Small = 2,
        Bonus_Medium = 3,
        Bonus_Big = 4,
        Bonus_Biggest = 5
    }

    /// <summary>
    /// for Tooltip-Image-Size
    /// </summary>
    /// 
    //public enum SpecialSize : int  // for Tooltip-Image-Size
    //{
    //    Width1 = 576,
    //    Height1 = 480,
    //    Width2 = 1152,
    //    Height2 = 960,
    //    Width3 = 576,
    //    Height3 = 480
    //}

    public enum EmpireModifierRecurringBalancing : byte
    {
        Run = 0,  // used for DoChecks and more
        Debug
    }

    public enum GamePace : byte
    {
        Slow = 0,
        Normal,
        Fast
    }

    public enum TurnTimerEnum : byte
    {
        Unlimited = 0,
        Sec25,
        Sec50,
        Sec75,
        Sec100,
        Sec150,
        Sec200,
        Sec250,
        Sec300,
        Sec360
    }


    /// <summary>
    /// Defines the options available at the beginning of a game.
    /// </summary>
    [Serializable]
    public sealed class GameOptions : ICloneable
    {
        private string newline = Environment.NewLine;
        #region Constructors
        public GameOptions()
        {
            ModID = Guid.Empty;
            AIMode = AIMode.Normal;
            AITakeover = true;
            CombatTimer = default;
            GalaxyShape = GalaxyShape.Irregular;
            GalaxySize = GalaxySize.Small;
            PlanetDensity = PlanetDensity.Medium;
            StarDensity = StarDensity.Sparse;
            MinorRaceFrequency = MinorRaceFrequency.Many;
            GalaxyCanon = GalaxyCanon.Canon;
            StartingTechLevel = StartingTechLevel.Developed;
            FederationPlayable = EmpirePlayable.Yes;
            RomulanPlayable = EmpirePlayable.Yes;
            KlingonPlayable = EmpirePlayable.Yes;
            CardassianPlayable = EmpirePlayable.Yes;
            DominionPlayable = EmpirePlayable.Yes;
            BorgPlayable = EmpirePlayable.No;
            TerranEmpirePlayable = EmpirePlayable.No;

            FederationModifier = EmpireModifier.Standard;
            RomulanModifier = EmpireModifier.Standard;
            KlingonModifier = EmpireModifier.Standard;
            CardassianModifier = EmpireModifier.Standard;
            DominionModifier = EmpireModifier.Standard;
            BorgModifier = EmpireModifier.Standard;
            TerranEmpireModifier = EmpireModifier.Standard;

            //EmpireModifierRecurringBalancing = EmpireModifierRecurringBalancing.No;
            EmpireModifierRecurringBalancing = EmpireModifierRecurringBalancing.Run;
            GamePace = GamePace.Normal;
            TurnTimerEnum = TurnTimerEnum.Unlimited;
        }
        #endregion

        #region Properties
        public bool IsFrozen { get; private set; }

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
        /// Gets or sets the galaxy canon.
        /// </summary>
        /// <value>The galaxy canon.</value>
        /// 
        public GalaxyCanon GalaxyCanon { get; set; }


        /// <summary>
        /// Gets or sets the starting tech level.
        /// </summary>
        /// <value>The starting tech level.</value>
        public StartingTechLevel StartingTechLevel { get; set; }

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
        /// Gets or sets FederationModifier.
        /// </summary>
        /// <value> Federation Modifier yes or no.</value>
        public EmpireModifier FederationModifier { get; set; }

        /// <summary>
        /// Gets or sets RomulanModifier.
        /// </summary>
        /// <value> Romulan Modifier yes or no.</value>
        public EmpireModifier RomulanModifier { get; set; }

        /// <summary>
        /// Gets or sets KlingonModifier.
        /// </summary>
        /// <value> Klingon Modifier yes or no.</value>
        public EmpireModifier KlingonModifier { get; set; }

        /// <summary>
        /// Gets or sets CardassianModifier.
        /// </summary>
        /// <value> Cardassian Modifier yes or no.</value>
        public EmpireModifier CardassianModifier { get; set; }

        /// <summary>
        /// Gets or sets DominionModifier.
        /// </summary>
        /// <value> Dominion Modifier yes or no.</value>
        public EmpireModifier DominionModifier { get; set; }

        /// <summary>
        /// Gets or sets BorgModifier.
        /// </summary>
        /// <value> borg Modifier yes or no.</value>
        public EmpireModifier BorgModifier { get; set; }

        /// <summary>
        /// Gets or sets TerranEmpireModifier.
        /// </summary>
        /// <value> TerranEmpire Modifier yes or no.</value>
        public EmpireModifier TerranEmpireModifier { get; set; }

        /// <summary>
        /// Gets or sets a boolean whether EmpireModifiers are recurring balanced the longer the game runs
        /// </summary>
        /// <value> TerranEmpire Modifier yes or no.</value>
        public EmpireModifierRecurringBalancing EmpireModifierRecurringBalancing { get; set; }

        ///// <summary>
        ///// Gets or sets Special size for Tooltip image size.
        ///// </summary>
        ///// <value> TerranEmpire Modifier yes or no.</value>
        //public SpecialSize Width1 { get; set; }

        ///// <summary>
        ///// Gets or sets Special size for Tooltip image size.
        ///// </summary>
        ///// <value> TerranEmpire Modifier yes or no.</value>
        //public SpecialSize Height1 { get; set; }

        ///// <summary>
        ///// Gets or sets Special size for Tooltip image size.
        ///// </summary>
        ///// <value> TerranEmpire Modifier yes or no.</value>
        //public SpecialSize Width2 { get; set; }

        ///// <summary>
        ///// Gets or sets Special size for Tooltip image size.
        ///// </summary>
        ///// <value> TerranEmpire Modifier yes or no.</value>
        //public SpecialSize Height2 { get; set; }

        ///// <summary>
        ///// Gets or sets Special size for Tooltip image size.
        ///// </summary>
        ///// <value> TerranEmpire Modifier yes or no.</value>
        //public SpecialSize Width3 { get; set; }

        ///// <summary>
        ///// Gets or sets Special size for Tooltip image size.
        ///// </summary>
        ///// <value> TerranEmpire Modifier yes or no.</value>
        //public SpecialSize Height3 { get; set; }

        /// <summary>
        /// Gets or sets GamePace
        /// </summary>
        /// <value> GamePace slow fast normal.</value>
        public GamePace GamePace { get; set; }

        /// <summary>
        /// Gets or sets TurnTimerEnum
        /// </summary>
        /// <value> TurnTimerEnum from 25 sec ... to unlimited.</value>
        public TurnTimerEnum TurnTimerEnum { get; set; }

        /// <summary>
        /// Gets or sets the turn timer (multiplayer games only).
        /// </summary>
        /// <value>The turn timer.</value>
        /// 
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
            {
                throw new ArgumentNullException("writer");
            }

            // needs the same sorting as Reading

            writer.Write(IsFrozen);
            byte[] modIdBytes = ModID.ToByteArray();
            writer.Write(modIdBytes.Length);
            writer.Write(modIdBytes, 0, modIdBytes.Length);
            writer.Write((byte)AIMode);
            writer.Write((byte)GalaxyShape);
            writer.Write((byte)GalaxySize);
            writer.Write((byte)PlanetDensity);
            writer.Write((byte)StarDensity);
            writer.Write((byte)MinorRaceFrequency);
            writer.Write((byte)GalaxyCanon);
            writer.Write((byte)StartingTechLevel);
            writer.Write((byte)FederationPlayable);
            writer.Write((byte)RomulanPlayable);
            writer.Write((byte)KlingonPlayable);
            writer.Write((byte)CardassianPlayable);
            writer.Write((byte)DominionPlayable);
            writer.Write((byte)BorgPlayable);
            writer.Write((byte)TerranEmpirePlayable);
            writer.Write((int)FederationModifier);   //it is writing a int32 !!!
            writer.Write((int)RomulanModifier);
            writer.Write((int)KlingonModifier);
            writer.Write((int)CardassianModifier);
            writer.Write((int)DominionModifier);
            writer.Write((int)BorgModifier);
            writer.Write((int)TerranEmpireModifier);
            writer.Write((byte)EmpireModifierRecurringBalancing);
            writer.Write((byte)GamePace);
            writer.Write((byte)TurnTimerEnum); // project intel save game
            writer.Write(AITakeover);
            writer.Write(TurnTimer.Ticks);
            writer.Write(CombatTimer.Ticks);

        }

        public void Read([NotNull] BinaryReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            // needs the same sorting as Writing
            string _readingText = "";
            //_readingText += "Step_4030: "/* + reader.BaseStream.*/;

            bool everySingleLine = false;

            IsFrozen = reader.ReadBoolean();
            //_readingText += "; IsFrozen=" + IsFrozen; 
            if (everySingleLine) Console.WriteLine(_readingText);

            int modIdBytesLength = reader.ReadInt32();
            //_readingText += "; modIdBytesLength=" + modIdBytesLength; 
            if (everySingleLine) Console.WriteLine(_readingText);

            //modIdBytesLength = 16;  // must be

            ModID = new Guid(reader.ReadBytes(modIdBytesLength));
            //_readingText += "; ModID=" + ModID; 
            if (everySingleLine) Console.WriteLine(_readingText);

            AIMode = (AIMode)reader.ReadByte();
            //_readingText += "; AIMode=" + AIMode; 
            if (everySingleLine) Console.WriteLine(_readingText);


            //_readingText += newline;
            _readingText += "Step_4031:";

            GalaxyShape = (GalaxyShape)reader.ReadByte(); 
            _readingText += "; GalaxyShape=" + GalaxyShape; 
            if (everySingleLine) Console.WriteLine(_readingText);

            GalaxySize = (GalaxySize)reader.ReadByte();
            _readingText += "; GalaxySize=" + GalaxySize; 
            if (everySingleLine) Console.WriteLine(_readingText);

            PlanetDensity = (PlanetDensity)reader.ReadByte();
            _readingText += "; PlanetDensity=" + PlanetDensity; 
            if (everySingleLine) Console.WriteLine(_readingText);

            StarDensity = (StarDensity)reader.ReadByte();
            _readingText += "; StarDensity=" + StarDensity; 
            if (everySingleLine) Console.WriteLine(_readingText);

            MinorRaceFrequency = (MinorRaceFrequency)reader.ReadByte();
            _readingText += "; MinorRaceFrequency=" + MinorRaceFrequency; 
            if (everySingleLine) Console.WriteLine(_readingText);

            GalaxyCanon = (GalaxyCanon)reader.ReadByte();
            _readingText += "; GalaxyCanon=" + GalaxyCanon; 
            if (everySingleLine) Console.WriteLine(_readingText);

            StartingTechLevel = (StartingTechLevel)reader.ReadByte();
            _readingText += "; StartingTechLevel=" + StartingTechLevel; 
            if (everySingleLine) Console.WriteLine(_readingText);

            _readingText += newline + "Step_4032:";


            FederationPlayable = (EmpirePlayable)reader.ReadByte();
            _readingText += "; Playable: Federation=" + FederationPlayable; 
            if (everySingleLine) Console.WriteLine(_readingText);

            RomulanPlayable = (EmpirePlayable)reader.ReadByte();
            _readingText += "; Romulan=" + RomulanPlayable;
            if (everySingleLine) Console.WriteLine(_readingText);

            KlingonPlayable = (EmpirePlayable)reader.ReadByte();
            _readingText += "; Klingon=" + KlingonPlayable; 
            if (everySingleLine) Console.WriteLine(_readingText);

            CardassianPlayable = (EmpirePlayable)reader.ReadByte();
            _readingText += "; Cardassian=" + CardassianPlayable; 
            if (everySingleLine) Console.WriteLine(_readingText);

            DominionPlayable = (EmpirePlayable)reader.ReadByte();
            _readingText += "; Dominion=" + DominionPlayable; 
            if (everySingleLine) Console.WriteLine(_readingText);

            BorgPlayable = (EmpirePlayable)reader.ReadByte();
            _readingText += "; Borg=" + BorgPlayable; 
            if (everySingleLine) Console.WriteLine(_readingText);

            TerranEmpirePlayable = (EmpirePlayable)reader.ReadByte();
            _readingText += "; Terran=" + TerranEmpirePlayable; 
            if (everySingleLine) Console.WriteLine(_readingText);


            //_readingText += newline + "Step_4033:"; ;

            FederationModifier = (EmpireModifier)reader.ReadInt32();
            //_readingText += "; Modifier: Federation=" + FederationModifier; 
            if (everySingleLine) Console.WriteLine(_readingText);

            RomulanModifier = (EmpireModifier)reader.ReadInt32();
            //_readingText += "; Romulan=" + RomulanModifier; 
            if (everySingleLine) Console.WriteLine(_readingText);

            KlingonModifier = (EmpireModifier)reader.ReadInt32();
            //_readingText += "; Klingon=" + KlingonModifier; 
            if (everySingleLine) Console.WriteLine(_readingText);

            CardassianModifier = (EmpireModifier)reader.ReadInt32();
            //_readingText += "; Cardassian=" + CardassianModifier; 
            if (everySingleLine) Console.WriteLine(_readingText);

            DominionModifier = (EmpireModifier)reader.ReadInt32();
            //_readingText += "; Dominion=" + DominionModifier; 
            if (everySingleLine) Console.WriteLine(_readingText);

            BorgModifier = (EmpireModifier)reader.ReadInt32();
            //_readingText += "; Borg=" + BorgModifier; 
            if (everySingleLine) Console.WriteLine(_readingText);

            TerranEmpireModifier = (EmpireModifier)reader.ReadInt32();
            //_readingText += "; TerranEmpireModifier=" + TerranEmpireModifier; 
            if (everySingleLine) Console.WriteLine(_readingText);


            //_readingText += newline + "Step_4034:"; ;


            EmpireModifierRecurringBalancing = (EmpireModifierRecurringBalancing)reader.ReadByte();
            //_readingText += "; EmpireModifierRecurringBalancing=" + EmpireModifierRecurringBalancing; 
            if (everySingleLine) Console.WriteLine(_readingText);
            
            GamePace = (GamePace)reader.ReadByte();
            //_readingText += "; GamePace=" + GamePace; 
            if (everySingleLine) Console.WriteLine(_readingText);

            TurnTimerEnum = (TurnTimerEnum)reader.ReadByte(); // project intel read save game
            //_readingText += "; TurnTimerEnum=" + TurnTimerEnum; 
            if (everySingleLine) Console.WriteLine(_readingText);

            AITakeover = reader.ReadBoolean();
            //_readingText += "; AITakeover=" + AITakeover; 
            if (everySingleLine) Console.WriteLine(_readingText);

            TurnTimer = TimeSpan.FromTicks(reader.ReadInt64());
            //_readingText += "; TurnTimer=" + TurnTimer; 
            if (everySingleLine) Console.WriteLine(_readingText);

            CombatTimer = TimeSpan.FromTicks(reader.ReadInt64());
            //_readingText += "; CombatTimer=" + CombatTimer; 
            Console.WriteLine(_readingText);

            ; // breakpoint
        }

        internal static bool Validate(GameOptions options)
        {
            if (options == null)
            {
                return false;
            }

            if (!Enum.IsDefined(typeof(AIMode), options.AIMode))
            {
                return false;
            }

            if (!Enum.IsDefined(typeof(GalaxyShape), options.GalaxyShape))
            {
                return false;
            }

            if (!Enum.IsDefined(typeof(GalaxySize), options.GalaxySize))
            {
                return false;
            }

            if (!Enum.IsDefined(typeof(PlanetDensity), options.PlanetDensity))
            {
                return false;
            }

            if (!Enum.IsDefined(typeof(StarDensity), options.StarDensity))
            {
                return false;
            }

            if (!Enum.IsDefined(typeof(GalaxyCanon), options.GalaxyCanon))
            {
                return false;
            }

            return true;
        }

        public GameOptions Clone()
        {
            GameOptions clone = (GameOptions)MemberwiseClone();
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
                    _ = SaveDefaults(defaults);
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
            bool success = false;
            try
            {
                StorageManager.WriteSetting("DefaultGameOptions", defaults.Clone());
                success = true;
            }
            catch (Exception e)
            {
                GameLog.Core.General.Error(e);
            }
            return success;
        }
        #endregion
    }
}