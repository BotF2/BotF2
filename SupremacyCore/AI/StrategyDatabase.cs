using System;
using System.Linq;
using System.Xml.Linq;

using Supremacy.Collections;
using Supremacy.Utility;

namespace Supremacy.AI
{
    [Serializable]
    public sealed class StrategyDatabase : KeyedCollectionBase<string, Strategy>
    {
        private StrategyDatabase() : base(o => o.Key) { }

        public static StrategyDatabase Load()
        {
            StrategyDatabase db = new StrategyDatabase();

            // TODO: Load Strategy Database
            XDocument doc;

            try
            {
                //doc = XDocument.Load(ResourceManager.GetResourcePath("Resources/Data/Strategies.xml"));
                doc = new XDocument(new XElement("Strategies"));
            }
            catch
            {
                doc = new XDocument(new XElement("Strategies"));
            }

            System.Collections.Generic.IEnumerable<Strategy> strategies = from s in doc.Descendants("Strategy")
                                                                          let key = (string)s.Attribute("Key")
                                                                          where !string.IsNullOrEmpty(key)
                                                                          orderby key ascending
                                                                          select new Strategy(key, (string)s.Element("Parent"))
                                                                          {
                                                                              MinimumMorale = (int?)s.Element("MinimumMorale"),
                                                                              DeficitSpending = (float?)s.Element("DeficitSpending"),
                                                                              MaxSupportCostPercent = (float?)s.Element("MaxSupportCostPercent"),
                                                                              ReadinessLevel = EnumHelper.ParseOrGetDefault<ReadinessLevel>((string)s.Element("ReadinessLevel")),
                                                                              ImproveProductionBonus = (float?)s.Element("ImproveProductionBonus"),
                                                                              ImproveGrowthBonus = (float?)s.Element("ImproveGrowthBonus"),
                                                                              ImproveResourceBonus = (float?)s.Element("ImproveResourceBonus"),
                                                                              ImproveSmallColonyGrowthBonus = (float?)s.Element("ImproveSmallColonyGrowthBonus"),
                                                                              ImproveLargeColonyGrowthBonus = (float?)s.Element("ImproveLargeColonyGrowthBonus"),
                                                                              Goals = (from g in s.Elements("Goal")
                                                                                       where g.Element("Goal") != null
                                                                                       select ParseStrategyGoalEntry(g))
                                                                                        .Where(g => g != null)
                                                                                        .ToArray(),
                                                                              DistanceModifierFactor = (int?)s.Element("DistanceModifierFactor"),
                                                                              DisbandArmyCount = (int?)s.Element("DisbandArmyCount"),
                                                                              MinColonizeDistance = (int?)s.Element("MinColonizeDistance"),
                                                                              MinColonizeScore = (int?)s.Element("MinColonizeScore"),
                                                                              OffensiveTaskForceCount = (int?)s.Element("OffensiveTaskForceCount"),
                                                                              DefensiveTaskForceCount = (int?)s.Element("DefensiveTaskForceCount"),
                                                                              FearInvasion = (int?)s.Element("FearInvasion"),
                                                                              FearColonyDefense = (int?)s.Element("FearColonyDefense"),
                                                                              FearRaiding = (int?)s.Element("FearRaiding"),
                                                                              FearTech = (int?)s.Element("FearTech"),
                                                                              FearSpying = (int?)s.Element("FearSpying"),
                                                                              FearScienceRank = (int?)s.Element("FearScienceRank"),
                                                                              FearMilitaryRank = (int?)s.Element("FearMilitaryRank"),
                                                                              FearEconomyRank = (int?)s.Element("FearEconomyRank"),
                                                                              DesireAttack = (int?)s.Element("DesireAttack"),
                                                                              DesireAttackColony = (int?)s.Element("DesireAttackColony"),
                                                                              DesireTrade = (int?)s.Element("DesireTrade"),
                                                                              DesireGrowth = (int?)s.Element("DesireGrowth"),
                                                                              DesireCredits = (int?)s.Element("DesireCredits"),
                                                                              DesireIntimidate = (int?)s.Element("DesireIntimidate"),
                                                                              DesireMakeFriend = (int?)s.Element("DesireMakeFriend"),
                                                                              DesireEnlistFriend = (int?)s.Element("DesireEnlistFriend"),
                                                                              RaidingMemoryTurns = (int?)s.Element("RaidingMemoryTurns"),
                                                                              MaxRaidingEvents = (int?)s.Element("MaxRaidingEvents"),
                                                                              OffensiveMatch = ParseForceMatch(s.Element("OffensiveMatch")),
                                                                              DefensiveMatch = ParseForceMatch(s.Element("DefensiveMatch")),
                                                                              StealthMatch = ParseForceMatch(s.Element("StealthMatch")),
                                                                              BombardMatch = ParseForceMatch(s.Element("BombardMatch")),
                                                                              HarassMatch = ParseForceMatch(s.Element("HarassMatch")),
                                                                              PreemptiveStrikeRegard = (int?)s.Element("PreemptiveStrikeRegard"),
                                                                              StopBuildingFoodBeforePopulationMaximized = (bool?)s.Element("StopBuildingFoodBeforePopulationMaximized"),
                                                                          };

            db.AddRange(strategies);

            return db;
        }

        private static StrategyGoalEntry ParseStrategyGoalEntry(XContainer element)
        {
            return element == null
                ? null
                : new StrategyGoalEntry((string)element.Element("Goal"))
                {
                    Priority = (int?)element.Element("Priority"),
                    MaxEvaluations = (int?)element.Element("MaxEvaluations"),
                    MaxExecutions = (int?)element.Element("MaxExecutions"),
                    ExecutionsPerColony = (int?)element.Element("ExecutionsPerColony"),
                    EvaluationsPerColony = (int?)element.Element("EvaluationsPerColony"),
                    PerColony = (int?)element.Element("PerCity")
                };
        }

        private static ForceMatch ParseForceMatch(XContainer element)
        {
            ForceMatch result = ForceMatch.Default;
            if (element != null)
            {
                result = new ForceMatch
                {
                    Attack = (float?)element.Element("Attack"),
                    Defense = (float?)element.Element("Defense"),
                    Bombard = (float?)element.Element("Bombard"),
                    Value = (float?)element.Element("Value")
                };
            }
            return result;
        }
    }
}