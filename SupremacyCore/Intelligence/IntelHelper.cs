using Supremacy.Collections;
using Supremacy.Diplomacy;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Universe;
using Supremacy.Utility;
using System;
using System.Collections.Generic;


namespace Supremacy.Intelligence
{
    public static class IntelHelper
    {
        private static CivilizationManager _localPlayer;
        private static CivilizationManager _spiedCivManagerOne;
        private static CivilizationManager _spiedCivManagerTwo;
        private static CivilizationManager _spiedCivManagerThree;
        private static CivilizationManager _spiedCivManagerFour;
        private static CivilizationManager _spiedCivManagerFive;
        private static CivilizationManager _spiedCivManagerSix;
        private static Civilization _newTargetCiv;
        private static Civilization _newSpyCiv;
        private static UniverseObjectList<Colony> _newSpiedColonies;
        //private static Civilization _spiedCivTwo;
        //private static Civilization _spiedCivThree;
        //private static Civilization _spiedCivFour;
        //private static Civilization _spiedCivFive;
        //private static Civilization _spiedCivSix;
        public static CivilizationManager LocalPlayerCivManager
        {
            get { return _localPlayer; }
        }
        public static CivilizationManager SpiedOneCivManager
        {
            get { return _spiedCivManagerOne; }
        }
        public static CivilizationManager SpiedTwoCivManager
        {
            get { return _spiedCivManagerTwo; }
        }
        public static CivilizationManager SpiedThreeCivManager
        {
            get { return _spiedCivManagerThree; }
        }

        public static CivilizationManager SpiedFourCivManager
        {
            get { return _spiedCivManagerFour; }
        }
        public static CivilizationManager SpiedFiveCivManager
        {
            get { return _spiedCivManagerFive; }
        }
        public static CivilizationManager SpiedSixCivManager
        {
            get { return _spiedCivManagerSix; }
        }

        //        public static Dictionary<Civilization, List<Colony>> SpiedOneInfiltrated
        //        {
        //            get { return SpiedOneCivManager.InfiltratedColonies; }
        //        }
        //        public static Dictionary<Civilization, List<Colony>> SpiedTwoInfiltrated
        //        {
        //            get { return SpiedTwoCivManager.InfiltratedColonies; }
        //        }
        //        public static Dictionary<Civilization, List<Colony>> SpiedThreeInfiltrated
        //        {
        //            get { return SpiedThreeCivManager.InfiltratedColonies; }
        //        }
        //        public static Dictionary<Civilization, List<Colony>> SpiedFourInfiltrated
        //        {
        //            get { return SpiedFourCivManager.InfiltratedColonies; }
        //        }
        //        public static Dictionary<Civilization, List<Colony>> SpiedFiveInfiltrated
        //        {
        //            get { return SpiedFiveCivManager.InfiltratedColonies; }
        //        }
        //        public static Dictionary<Civilization, List<Colony>> SpiedSixInfiltrated
        //        {
        //            get { return SpiedSixCivManager.InfiltratedColonies; }
        //        }

        public static CivilizationManager SendLocalPlayer(CivilizationManager localPlayer)
        {
            _localPlayer = localPlayer;
            return localPlayer;
        }
        public static CivilizationManager SendSpiedCivOne(CivilizationManager spiedCivOne)
        {
            _spiedCivManagerOne = spiedCivOne;
            return _spiedCivManagerOne;
        }
        public static CivilizationManager SendSpiedCivTwo(CivilizationManager spiedCivTwo)
        {
            _spiedCivManagerTwo = spiedCivTwo;
            return _spiedCivManagerTwo;
        }
        public static CivilizationManager SendSpiedCivThree(CivilizationManager spiedCivThree)
        {
            _spiedCivManagerThree = spiedCivThree;
            return _spiedCivManagerThree;
        }
        public static CivilizationManager SendSpiedCivFour(CivilizationManager spiedCivFour)
        {
            _spiedCivManagerFour = spiedCivFour;
            return _spiedCivManagerFour;
        }
        public static CivilizationManager SendSpiedCivFive(CivilizationManager spiedCivFive)
        {
            _spiedCivManagerFive = spiedCivFive;
            return _spiedCivManagerFive;
        }
        public static CivilizationManager SendSpiedCivSix(CivilizationManager spiedCivSix)
        {
            _spiedCivManagerSix = spiedCivSix;
            return _spiedCivManagerSix;
        }
        //        //public static bool IsInfiltrated(Civilization source, Civilization target)
        //        //{
        //        //    if (source == null)
        //        //        throw new ArgumentNullException("source");
        //        //    if (target == null)
        //        //        //return false;
        //        //        throw new ArgumentNullException("target");

        public static UniverseObjectList<Colony> NewSpiedColonies
        {
            get { return _newSpiedColonies; }
        }
        public static Civilization NewSpyCiv
        {
            get { return _newSpyCiv; }
        }
        public static Civilization NewTargetCiv
        {
            get { return _newTargetCiv; }
        }

        public static void SendXSpiedY(Civilization spyCiv, Civilization spiedCiv, UniverseObjectList<Colony> colonies)
        {      
            if (spyCiv == null)
                throw new ArgumentNullException("spyCiv");
            if (spiedCiv == null)  
                throw new ArgumentNullException("spiedCiv");

            _newSpyCiv = spyCiv;
            _newTargetCiv = spiedCiv;
            _newSpiedColonies = colonies;
        }
    }
}
