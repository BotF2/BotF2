using Supremacy.Client.Context;
using Supremacy.Collections;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Universe;
using Supremacy.Utility;
using System.Collections.Generic;
using System.Linq;

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
        public static Civilization CivZero => GameContext.Current.CivilizationManagers[0].Civilization;
        public static Civilization CivOne => GameContext.Current.CivilizationManagers[1].Civilization;
        public static Civilization CivTwo => GameContext.Current.CivilizationManagers[2].Civilization;
        public static Civilization CivThree => GameContext.Current.CivilizationManagers[3].Civilization;
        public static Civilization CivFour => GameContext.Current.CivilizationManagers[4].Civilization;
        public static Civilization CivFive => GameContext.Current.CivilizationManagers[5].Civilization;
        public static Civilization CivSix => GameContext.Current.CivilizationManagers[6].Civilization;
        public static bool IsSpiedZero(Civilization localCivFromScreen)
        {
            if (DesignTimeObjects.SubedZero)
                return false;
            else
            {
                var civList = GameContext.Current.CivilizationManagers[localCivFromScreen].SpiedCivList;
                if (civList.Contains(CivZero))
                        return true;
                else { return false; }
            }
        }
        public static bool IsSpiedOne(Civilization localCivFromScreen)
        {
            if (DesignTimeObjects.SubedOne)
                return false;
            else
            {
                var civList = GameContext.Current.CivilizationManagers[localCivFromScreen].SpiedCivList;
                if (civList.Contains(CivOne))
                    return true;
                else { return false; }
            }
        }
        public static bool IsSpiedTwo(Civilization localCivFromScreen)
        {
            if (DesignTimeObjects.SubedTwo)
                    return false;
            else
            {
                var civList = GameContext.Current.CivilizationManagers[localCivFromScreen].SpiedCivList;
                if (civList.Contains(CivTwo))
                    return true;
                else { return false; }
            }
        }
        public static bool IsSpiedThree(Civilization localCivFromScreen)
        {
            if (DesignTimeObjects.SubedThree)
                    return false;
            else
            {
                var civList = GameContext.Current.CivilizationManagers[localCivFromScreen].SpiedCivList;
                if (civList.Contains(CivThree))
                    return true;
                else { return false; }
            }
        }
        public static bool IsSpiedFour(Civilization localCivFromScreen)
        {
            if (DesignTimeObjects.SubedFour)
                    return false;
            else
            {
                var civList = GameContext.Current.CivilizationManagers[localCivFromScreen].SpiedCivList;
                if (civList.Contains(CivFour))
                    return true;
                else { return false; }
            }
        }
        public static bool IsSpiedFive(Civilization localCivFromScreen)
        {
            if (DesignTimeObjects.SubedFive)
                return false;
            else
            {
                var civList = GameContext.Current.CivilizationManagers[localCivFromScreen].SpiedCivList;
                if (civList.Contains(CivFive))
                    return true;
                else { return false; }
            }
        }
        public static bool IsSpiedSix(Civilization localCivFromScreen)
        {
            if (DesignTimeObjects.SubedSix)
                    return false;
            else
            {
                var civList = GameContext.Current.CivilizationManagers[localCivFromScreen].SpiedCivList;
                if (civList.Contains(CivSix))
                    return true;
                else { return false; }
            }
        }
    }
}

