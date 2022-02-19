// File:Planet.cs
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.IO.Serialization;
using Supremacy.Types;
using Supremacy.Utility;
using System;
using System.Collections.Specialized;
using System.Linq;

namespace Supremacy.Universe
{
    /// <summary>
    /// Represents a planet in a star system.
    /// </summary>
    [Serializable]
    public sealed class Planet : IOwnedDataSerializableAndRecreatable
    {
        #region Constants
        public const int MaxMoonsPerPlanet = 4;
        public const int MaxVariations = 3;
        #endregion

        #region Fields
        private static readonly BitVector32.Section MoonCountSection;
        private static readonly BitVector32.Section[] MoonShapeSections;
        private static readonly BitVector32.Section[] MoonSizeSections;
        private static readonly BitVector32.Section PlanetSizeSection;
        private static readonly BitVector32.Section PlanetTypeSection;
        private static readonly BitVector32.Section VariationSection;

        private PlanetBonus _bonuses;
        private BitVector32 _data;
        private string _name;
        private string _text;
        private readonly string newline = Environment.NewLine;
        #endregion

        #region Constructors
        static Planet()
        {
            Collections.EnumValueCollection<PlanetSize> sizes = EnumUtilities.GetValues<PlanetSize>();
            Collections.EnumValueCollection<PlanetType> types = EnumUtilities.GetValues<PlanetType>();
            MoonShapeSections = new BitVector32.Section[MaxMoonsPerPlanet];
            MoonSizeSections = new BitVector32.Section[MaxMoonsPerPlanet];

            PlanetSizeSection = BitVector32.CreateSection((short)(sizes[sizes.Count - 1] - 1), default);
            PlanetTypeSection = BitVector32.CreateSection((short)(types[types.Count - 1] - 1), PlanetSizeSection);
            VariationSection = BitVector32.CreateSection(3, PlanetTypeSection);
            MoonCountSection = BitVector32.CreateSection(MaxMoonsPerPlanet, VariationSection);

            BitVector32.Section lastSection = MoonCountSection;
            for (int i = 0; i < MaxMoonsPerPlanet; i++)
            {
                MoonShapeSections[i] = BitVector32.CreateSection(3, lastSection);
                lastSection = MoonShapeSections[i];
                MoonSizeSections[i] = BitVector32.CreateSection(3, lastSection);
                lastSection = MoonSizeSections[i];
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Planet"/> class.
        /// </summary>
        public Planet()
        {
            _data = new BitVector32();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets a planet's name.
        /// </summary>
        /// <value>The planet's name.</value>
        public string Name
        {
            get => _name;
            set => _name = value;
        }

        /// <summary>
        /// Gets or sets a planet's bonuses.
        /// </summary>
        /// <value>The bonuses.</value>
        public PlanetBonus Bonuses
        {
            get => _bonuses;
            set => _bonuses = value;
        }

        /// <summary>
        /// Gets or sets the design of the planet.
        /// </summary>
        /// <value>The design of the planet.</value>
        public PlanetType PlanetType
        {
            get => (PlanetType)_data[PlanetTypeSection];
            set => _data[PlanetTypeSection] = (int)value;
        }

        /// <summary>
        /// Gets or sets the size of the planet.
        /// </summary>
        /// <value>The size of the planet.</value>
        public PlanetSize PlanetSize
        {
            get => (PlanetSize)_data[PlanetSizeSection];
            set => _data[PlanetSizeSection] = (int)value;
        }

        /// <summary>
        /// Gets or sets the variation.
        /// </summary>
        /// <value>The variation.</value>
        public int Variation
        {
            get => _data[VariationSection];
            set => _data[VariationSection] = value;
        }

        /// <summary>
        /// Gets or sets the moons.
        /// </summary>
        /// <value>The moons.</value>
        public MoonType[] Moons
        {
            get
            {
                int count = _data[MoonCountSection];
                MoonType[] moons = new MoonType[count];
                for (int i = 0; i < count; i++)
                {
                    moons[i] = MoonHelper.GetType(
                        (MoonSize)_data[MoonSizeSections[i]],
                        (MoonShape)_data[MoonShapeSections[i]]);
                }
                return moons;
            }
            internal set
            {
                if (value == null)
                {
                    value = new MoonType[0];
                }
                else if (value.Length > MaxMoonsPerPlanet)
                {
                    throw new ArgumentException("Number of moons must be <= " + MaxMoonsPerPlanet);
                }
                if (value.Length > 0)
                {
                    value = value.OrderByDescending(mt => MoonHelper.GetSize(mt)).ToArray();
                }
                _data[MoonCountSection] = value.Length;
                for (int i = 0; i < MaxMoonsPerPlanet; i++)
                {
                    if (i < value.Length)
                    {
                        _data[MoonShapeSections[i]] = (int)MoonHelper.GetShape(value[i]);
                        _data[MoonSizeSections[i]] = (int)MoonHelper.GetSize(value[i]);
                    }
                    else
                    {
                        _data[MoonShapeSections[i]] = 0;
                        _data[MoonSizeSections[i]] = 0;
                    }
                }
            }


        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="Planet"/> has a food bonus.
        /// </summary>
        /// <value>
        /// <c>true</c> if this <see cref="Planet"/> has a food bonus; otherwise, <c>false</c>.
        /// </value>
        public bool HasFoodBonus => (_bonuses & PlanetBonus.Food) == PlanetBonus.Food;

        /// <summary>
        /// Gets a value indicating whether this <see cref="Planet"/> has an energy bonus.
        /// </summary>
        /// <value>
        /// <c>true</c> if this <see cref="Planet"/> has an energy bonus; otherwise, <c>false</c>.
        /// </value>
        public bool HasEnergyBonus => (_bonuses & PlanetBonus.Energy) == PlanetBonus.Energy;
        #endregion

        #region Methods
        /// <summary>
        /// Adds a planetary bonus.
        /// </summary>
        /// <param name="bonus">The bonus to add.</param>
        public void AddBonus(PlanetBonus bonus)
        {
            _bonuses |= bonus;
        }

        /// <summary>
        /// Gets the environment of the <see cref="Planet"/> based the home planet design of a <see cref="Race"/>.
        /// </summary>
        /// <param name="homePlanetType">The home planet design.</param>
        /// <returns>The environment.</returns>
        public PlanetEnvironment GetEnvironment(PlanetType homePlanetType)
        {
            _text = "";
            _text = _text + _text + newline;  // dummy - do not remove

            switch (PlanetType)
            {
                case PlanetType.Asteroids:
                case PlanetType.GasGiant:
                case PlanetType.Crystalline:
                case PlanetType.Demon:
                    return PlanetEnvironment.Uninhabitable;

                case PlanetType.Jungle:
                case PlanetType.Oceanic:
                case PlanetType.Terran:
                    return PlanetEnvironment.Ideal;

                case PlanetType.Arctic:
                case PlanetType.Barren:
                case PlanetType.Desert:
                case PlanetType.Rogue:
                case PlanetType.Volcanic:
                    if (homePlanetType == PlanetType.Rogue)
                    {
                        return PlanetEnvironment.Ideal;  // e.g. for Dominion
                    }
                    return PlanetEnvironment.Hostile;

                default:
                    int result;
                    Wheel<PlanetType> envs = new Wheel<PlanetType>();
                    for (int i = 0; i < (int)PlanetType.Rogue; i++)
                    {
                        envs.Insert((PlanetType)i);
                    }

                    result = envs.GetDistance(
                        PlanetType,
                        homePlanetType);
                    if (result >= (int)PlanetEnvironment.Uninhabitable)
                    {
                        return PlanetEnvironment.Uninhabitable;
                    }

                    return (PlanetEnvironment)result;
            }
        }

        /// <summary>
        /// Gets the environment of the <see cref="Planet"/> based the home planet design of a <see cref="Race"/>.
        /// </summary>
        /// <param name="race">The race.</param>
        /// <returns>The environment.</returns>
        public PlanetEnvironment GetEnvironment(Race race)
        {
            return race == null ? throw new ArgumentNullException("race") : GetEnvironment(race.HomePlanetType);
        }

        /// <summary>
        /// Gets the growth rate of the <see cref="Planet"/> based on the home planet
        /// design of a <see cref="Race"/>.
        /// </summary>
        /// <param name="homePlanetType">The home planet design.</param>
        /// <returns>The growth rate.</returns>
        public Percentage GetGrowthRate(PlanetType homePlanetType)
        {
            Data.Table table = GameContext.Current.Tables.UniverseTables["PlanetGrowthRate"];
            return Percentage.Parse(table[GetEnvironment(homePlanetType).ToString()][0]);
        }

        /// <summary>
        /// Gets the growth rate of the <see cref="Planet"/> based on the home planet
        /// design of a <see cref="Race"/>.
        /// </summary>
        /// <param name="race">The race.</param>
        /// <returns>The growth rate.</returns>
        public Percentage GetGrowthRate(Race race)
        {
            Data.Table table = GameContext.Current.Tables.UniverseTables["PlanetGrowthRate"];
            return Percentage.Parse(table[GetEnvironment(race).ToString()][0]);
        }

        /// <summary>
        /// Gets the maximum population of the <see cref="Planet"/> based on the home planet
        /// design of a <see cref="Race"/>.
        /// </summary>
        /// <param name="homePlanetType">The home planet design.</param>
        /// <returns>The maximum population.</returns>
        public int GetMaxPopulation(PlanetType homePlanetType)
        {
            Data.Table table = GameContext.Current.Tables.UniverseTables["PlanetMaxPop"];

            // OK to return null here! Do not need to fix
            // 2021-02-21 reg: well, making trouble time by time - we should keep this coding
            int maxPop = 99;
            try
            {
                maxPop = PlanetSize == PlanetSize.NoWorld
                    ? 0
                    : Number.ParseInt32(table[PlanetSize.ToString()]
                    [GetEnvironment(homePlanetType).ToString()]);
            }
            catch (Exception ex)
            {
                GameLog.Client.GalaxyGenerator.ErrorFormat("Generated at HomeSystem with 99 Population due to avoid crash > GetMaxPopulation");
                GameLog.Client.GalaxyGenerator.ErrorFormat("Message = {0}, stack trace = [1]", ex.Message, ex.StackTrace);
            }
            //_text = /*newline + */"GetMaxPopulation by homePlanetType " + homePlanetType.ToString() + " > " + maxPop;
            ////Console.WriteLine(_text);
            //GameLog.Client.GalaxyGeneratorDetails.DebugFormat(_text);

            return maxPop;
            //return Number.ParseInt32(table[PlanetSize.ToString()]
            //       [GetEnvironment(homePlanetType).ToString()]);
            // Botha are maybe outcommented in HomeSystem.xml - so the game crashes for missing Planet Size
        }

        /// <summary>
        /// Gets the maximum population of the <see cref="Planet"/> based on the home planet
        /// design of a <see cref="Race"/>.
        /// </summary>
        /// <param name="race">The race.</param>
        /// <returns>The maximum population.</returns>
        public int GetMaxPopulation(Race race)
        {
            _text = "GetMaxPopulation by race " + race.Key;
            //Console.WriteLine(_text);
            //GameLog.Client.GalaxyGeneratorDetails.DebugFormat(_text);

            Data.Table table = GameContext.Current.Tables.UniverseTables["PlanetMaxPop"];

            int _pop = Number.ParseInt32(table[PlanetSize.ToString()]
                [GetEnvironment(race).ToString()]);

            return _pop;
        }

        /// <summary>
        /// Determines whether a planet has the specified bonus.
        /// </summary>
        /// <param name="bonus">The bonus.</param>
        /// <returns>
        /// <c>true</c> if the planet has the specified bonus; otherwise, <c>false</c>.
        /// </returns>
        public bool HasBonus(PlanetBonus bonus)
        {
            return (_bonuses & bonus) == bonus;
        }

        /// <summary>
        /// Determines whether the specified home planet design is habitable for a <see cref="Race"/>.
        /// </summary>
        /// <param name="homePlanetType">The home planet design of the <see cref="Race"/>.</param>
        /// <returns>
        /// <c>true</c> if habitable; otherwise, <c>false</c>.
        /// </returns>
        public bool IsHabitable(PlanetType homePlanetType)
        {
            return GetEnvironment(homePlanetType) != PlanetEnvironment.Uninhabitable;
        }

        /// <summary>
        /// Determines whether the specified home planet design is habitable for a <see cref="Race"/>.
        /// </summary>
        /// <param name="race">The race.</param>
        /// <returns>
        /// <c>true</c> if habitable; otherwise, <c>false</c>.
        /// </returns>
        public bool IsHabitable(Race race)
        {
            return GetEnvironment(race) != PlanetEnvironment.Uninhabitable;
        }

        /// <summary>
        /// Removes a planetary bonus.
        /// </summary>
        /// <param name="bonus">The bonus to remove.</param>
        public void RemoveBonus(PlanetBonus bonus)
        {
            _bonuses &= ~bonus;
        }
        #endregion

        public void SerializeOwnedData(SerializationWriter writer, object context)
        {
            writer.Write((byte)_bonuses);
            writer.Write(_data.Data);
            writer.WriteOptimized(_name);
        }

        public void DeserializeOwnedData(SerializationReader reader, object context)
        {
            _bonuses = (PlanetBonus)reader.ReadByte();
            _data = new BitVector32((int)reader.ReadUInt32());
            _name = reader.ReadOptimizedString();
        }
    }
}