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
        public static Civilization NewSpyCiv
        {
            get { return IntelHelper.NewSpyCiv; }
        }
        public static Civilization NewTargetCiv
        {
            get {return IntelHelper.NewTargetCiv; }
        }
        public static UniverseObjectList<Colony> NewSpiedColonies
        {
            get { return IntelHelper.NewSpiedColonies; }
        }
        public static Dictionary<Civilization, List<Civilization>> SpiedByDictionary
        {
            get { return IntelHelper.SpiedDictionary; }
        }
        public static Civilization CivZero
        {
            get { return GameContext.Current.CivilizationManagers[0].Civilization; }
        }
        public static Civilization CivOne
        {
            get { return GameContext.Current.CivilizationManagers[1].Civilization; }
        }
        public static Civilization CivTwo
        {
            get { return GameContext.Current.CivilizationManagers[2].Civilization; }
        }
        public static Civilization CivThree
        {
            get { return GameContext.Current.CivilizationManagers[3].Civilization; }
        }
        public static Civilization CivFour
        {
            get { return GameContext.Current.CivilizationManagers[4].Civilization; }
        }
        public static Civilization CivFive
        {
            get { return GameContext.Current.CivilizationManagers[5].Civilization; }
        }
        public static Civilization CivSix
        {
            get { return GameContext.Current.CivilizationManagers[6].Civilization; }
        }
        public static bool IsSpiedZero(Civilization localCivFromScreen)
        {
            if (DesignTimeObjects.SubedZero)
                return false;
            else
            {
                try { var civList = SpiedByDictionary[localCivFromScreen]; }
                catch { return false; }
                if (SpiedByDictionary[localCivFromScreen].Contains(CivZero))                   
                    return true;
            }
            return false;

        }
        public static bool IsSpiedOne(Civilization localCivFromScreen)
        {
            if (DesignTimeObjects.SubedOne)
                return false;
            else
            {
                try { var civList = SpiedByDictionary[localCivFromScreen]; }
                catch { return false; }
                if (SpiedByDictionary[localCivFromScreen].Contains(CivOne))
                    return true;
            }
            return false;
        }
        public static bool IsSpiedTwo(Civilization localCivFromScreen)
        {
            if (DesignTimeObjects.SubedTwo)
                return false;
            else
            {
                try { var civList = SpiedByDictionary[localCivFromScreen]; }
                catch { return false; }
                if (SpiedByDictionary[localCivFromScreen].Contains(CivTwo))
                    return true;
            }
            return false;
        }
        public static bool IsSpiedThree(Civilization localCivFromScreen)
        {
            if (DesignTimeObjects.SubedThree)
                return false;
            else
            {
                try { var civList = SpiedByDictionary[localCivFromScreen]; }
                catch { return false; }
                if (SpiedByDictionary[localCivFromScreen].Contains(CivThree))
                    return true;
            }
            return false;
        }
        public static bool IsSpiedFour(Civilization localCivFromScreen)
        {
            if (DesignTimeObjects.SubedFour)
                return false;
            else
            {
                try { var civList = SpiedByDictionary[localCivFromScreen]; }
                catch { return false; }
                if (SpiedByDictionary[localCivFromScreen].Contains(CivFour))
                    return true;
            }
            return false;
        }
        public static bool IsSpiedFive(Civilization localCivFromScreen)
        {
            if (DesignTimeObjects.SubedFive)
                return false;
            else
            {
                try { var civList = SpiedByDictionary[localCivFromScreen]; }
                catch { return false; }
                if (SpiedByDictionary[localCivFromScreen].Contains(CivFive))
                    return true;
            }
            return false;
        }
        public static bool IsSpiedSix(Civilization localCivFromScreen)
        {
            if (DesignTimeObjects.SubedSix)
                return false;
            else
            {
                try { var civList = SpiedByDictionary[localCivFromScreen]; }
                catch { return false; }
                if (SpiedByDictionary[localCivFromScreen].Contains(CivSix))
                    return true;
            }
            return false;
        }
    }
}

