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
        static bool alreadyZero = false;
        static bool alreadyOne = false;
        static bool alreadyTwo = false;
        static bool alreadyThree = false;
        static bool alreadyFour = false;
        static bool alreadyFive = false;
        static bool alreadySix = false;
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
            get { return GameContext.Current.CivilizationManagers[4].Civilization; }
        }
        public static bool IsSpiedZero(Civilization targetFromScreen)
        {
            if (NewSpyCiv == null)
                return false;
            else if (alreadyZero)
                return true;
            else if (CivZero == NewTargetCiv && SpiedByDictionary[NewSpyCiv].Contains(NewTargetCiv))
            {
                alreadyZero = true;
                return true;
            }
            return false;
        }
        public static bool IsSpiedOne(Civilization targetFromScreen)
        {
            if (NewSpyCiv == null)
                return false;
            else if (alreadyOne)
                return true;
            else if (CivOne == NewTargetCiv && SpiedByDictionary[NewSpyCiv].Contains(NewTargetCiv))
            {
                alreadyOne = true;
                return true;
            }           
            return false;
        }
        public static bool IsSpiedTwo(Civilization targetFromScreen)
        {
            if (NewSpyCiv == null)
                return false;
            else if (alreadyTwo)
                return true;
            else if (CivTwo == NewTargetCiv && SpiedByDictionary[NewSpyCiv].Contains(NewTargetCiv))
            {
                alreadyTwo = true;
                return true;
            }
            return false;
        }
        public static bool IsSpiedThree(Civilization targetFromScreen)
        {
            if (NewSpyCiv == null)
                return false;
            else if (alreadyThree)
                return true;
            else if (CivThree == NewTargetCiv && SpiedByDictionary[NewSpyCiv].Contains(NewTargetCiv))
            {
                alreadyThree = true;
                return true;
            }
            return false;
        }
        public static bool IsSpiedFour(Civilization targetFromScreen)
        {
            if (NewSpyCiv == null)
                return false;
            else if (alreadyFour)
                return true;
            else if (CivFour == NewTargetCiv && SpiedByDictionary[NewSpyCiv].Contains(NewTargetCiv))
            {
                alreadyFour = true;
                return true;
            }
            return false;
        }
        public static bool IsSpiedFive(Civilization targetFromScreen)
        {
            if (NewSpyCiv == null)
                return false;
            else if (alreadyFive)
                return true;
            else if (CivFive == NewTargetCiv && SpiedByDictionary[NewSpyCiv].Contains(NewTargetCiv))
            {
                alreadyFive = true;
                return true;
            }
            return false;
        }
        public static bool IsSpiedSix(Civilization targetFromScreen)
        {
            if (NewSpyCiv == null)
                return false;
            else if (alreadySix)
                return true;
            else if (CivSix == NewTargetCiv && SpiedByDictionary[NewSpyCiv].Contains(NewTargetCiv))
            {
                alreadySix = true;
                return true;
            }
            return false;
        }
    }
}

