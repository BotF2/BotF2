using Supremacy.Client.Context;
using Supremacy.Entities;
using Supremacy.Game;
using System.Collections.Generic;

namespace Supremacy.Intelligence
{
    public static class AssetsHelper
    {
        //public static Civilization NewSpyCiv
        //{
        //    get { return IntelHelper.NewSpyCiv; }
        //}
        //public static Civilization NewTargetCiv
        //{
        //    get {return IntelHelper.NewTargetCiv; }
        //}
        //public static UniverseObjectList<Colony> NewSpiedColonies
        //{
        //    get { return IntelHelper.NewSpiedColonies; }
        //}
        public static Civilization Civ0 => GameContext.Current.CivilizationManagers[0].Civilization;
        public static Civilization Civ1 => GameContext.Current.CivilizationManagers[1].Civilization;
        public static Civilization Civ2 => GameContext.Current.CivilizationManagers[2].Civilization;
        public static Civilization Civ3 => GameContext.Current.CivilizationManagers[3].Civilization;
        public static Civilization Civ4 => GameContext.Current.CivilizationManagers[4].Civilization;
        public static Civilization Civ5 => GameContext.Current.CivilizationManagers[5].Civilization;
        public static Civilization Civ6 => GameContext.Current.CivilizationManagers[6].Civilization;
        public static bool IsSpied_0_(Civilization localCivFromScreen)
        {
            if (DesignTimeObjects.Subed_0)
            {
                return false;
            }
            else
            {
                List<Civilization> civList = GameContext.Current.CivilizationManagers[localCivFromScreen].SpiedCivList;
                if (civList.Contains(Civ0))
                {
                    return true;
                }
                else { return false; }
            }
        }
        public static bool IsSpied_1_(Civilization localCivFromScreen)
        {
            if (DesignTimeObjects.Subed_1)
            {
                return false;
            }
            else
            {
                List<Civilization> civList = GameContext.Current.CivilizationManagers[localCivFromScreen].SpiedCivList;
                if (civList.Contains(Civ1))
                {
                    return true;
                }
                else { return false; }
            }
        }
        public static bool IsSpied_2_(Civilization localCivFromScreen)
        {
            if (DesignTimeObjects.Subed_2)
            {
                return false;
            }
            else
            {
                List<Civilization> civList = GameContext.Current.CivilizationManagers[localCivFromScreen].SpiedCivList;
                if (civList.Contains(Civ2))
                {
                    return true;
                }
                else { return false; }
            }
        }
        public static bool IsSpied_3_(Civilization localCivFromScreen)
        {
            if (DesignTimeObjects.Subed_3)
            {
                return false;
            }
            else
            {
                List<Civilization> civList = GameContext.Current.CivilizationManagers[localCivFromScreen].SpiedCivList;
                if (civList.Contains(Civ3))
                {
                    return true;
                }
                else { return false; }
            }
        }
        public static bool IsSpied_4_(Civilization localCivFromScreen)
        {
            if (DesignTimeObjects.Subed_4)
            {
                return false;
            }
            else
            {
                List<Civilization> civList = GameContext.Current.CivilizationManagers[localCivFromScreen].SpiedCivList;
                if (civList.Contains(Civ4))
                {
                    return true;
                }
                else { return false; }
            }
        }
        public static bool IsSpied_5_(Civilization localCivFromScreen)
        {
            if (DesignTimeObjects.Subed_5)
            {
                return false;
            }
            else
            {
                List<Civilization> civList = GameContext.Current.CivilizationManagers[localCivFromScreen].SpiedCivList;
                if (civList.Contains(Civ5))
                {
                    return true;
                }
                else { return false; }
            }
        }
        public static bool IsSpied_6_(Civilization localCivFromScreen)
        {
            if (DesignTimeObjects.Subed_6)
            {
                return false;
            }
            else
            {
                List<Civilization> civList = GameContext.Current.CivilizationManagers[localCivFromScreen].SpiedCivList;
                if (civList.Contains(Civ6))
                {
                    return true;
                }
                else { return false; }
            }
        }
    }
}

