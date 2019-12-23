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
        public static Civilization CivZero
        {
            get { return DesignTimeObjects.SpiedCivMangers[0].Civilization; }
        }
        public static Civilization CivOne
        {
            get { return DesignTimeObjects.SpiedCivMangers[1].Civilization; }
        }
        public static Civilization CivTwo
        {
            get { return DesignTimeObjects.SpiedCivMangers[2].Civilization; }
        }
        public static Civilization CivThree
        {
            get { return DesignTimeObjects.SpiedCivMangers[3].Civilization; }
        }
        public static Civilization CivFour
        {
            get { return DesignTimeObjects.SpiedCivMangers[4].Civilization; }
        }
        public static Civilization CivFive
        {
            get { return DesignTimeObjects.SpiedCivMangers[5].Civilization; }
        }
        public static Civilization CivSix
        {
            get { return DesignTimeObjects.SpiedCivMangers[6].Civilization; }
        }
        public static bool IsSpiedZero(Civilization targetFromScreen)
        {
            if (NewSpyCiv == null)
                return false;
            else if (alreadyZero)
                return true;
            else if (CivZero == NewTargetCiv)
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
            else if (CivOne == NewTargetCiv)
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
            else if (CivTwo == NewTargetCiv)
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
            else if (CivThree == NewTargetCiv)
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
            else if (CivFour == NewTargetCiv)
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
            else if (CivFive == NewTargetCiv)
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
            else if (CivSix == NewTargetCiv)
            {
                alreadySix = true;
                return true;
            }
            return false;
        }
    }
    //public class IntelHelperNotStatic
    //{
    //    public List<CivilizationManager> ManagerList
    //    {
    //        get { return DesignTimeObjects.SpiedCivMangers; }
    //    }


    //    public class SpiedCivList
    //    {
    //        get         
    //        {         

    //        return IntelHelper.NewSpiedColonies;         

    //         }

    //    }

    //public class SaveSpiedCivs

    //{

    //    private ArrayList SpiedCivManagersArray;



    //    SpiedCivManagersArray = _spie;          

    //#region Implementation of IOwnedDataSerializable          

    //public void DeserializeOwnedData(SerializationReader reader, object context)
    //    }
}

