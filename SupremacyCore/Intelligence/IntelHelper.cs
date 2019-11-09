using Supremacy.Diplomacy;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Universe;
using System;
using System.Collections.Generic;


namespace Supremacy.Intelligence
{
    public static class IntelHelper
    {
        private static CivilizationManager _localPlayer;
        private static CivilizationManager _spiedCivOne;
        private static CivilizationManager _spiedCivTwo;
        private static CivilizationManager _spiedCivThree;
        private static CivilizationManager _spiedCivFour;
        private static CivilizationManager _spiedCivFive;
        private static CivilizationManager _spiedCivSix;

        public static CivilizationManager LocalPlayerCivManager
        {
            get { return _localPlayer; }
        }
        public static CivilizationManager SpiedOneCivManager
        {
            get { return _spiedCivOne; }
        }
        public static CivilizationManager SpiedTwoCivManager
        {
            get { return _spiedCivTwo; }
        }
        public static CivilizationManager SpiedThreeCivManager
        {
            get { return _spiedCivThree; }
        }

        public static CivilizationManager SpiedFourCivManager
        {
            get { return _spiedCivFour; }
        }
        public static CivilizationManager SpiedFiveCivManager
        {
            get { return _spiedCivFive; }
        }
        public static CivilizationManager SpiedSixCivManager
        {
            get { return _spiedCivSix; }
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
            _spiedCivOne = spiedCivOne;
            return _spiedCivOne;
        }
        public static CivilizationManager SendSpiedCivTwo(CivilizationManager spiedCivTwo)
        {
            _spiedCivTwo = spiedCivTwo;
            return _spiedCivTwo;
        }
        public static CivilizationManager SendSpiedCivThree(CivilizationManager spiedCivThree)
        {
            _spiedCivThree = spiedCivThree;
            return _spiedCivThree;
        }
        public static CivilizationManager SendSpiedCivFour(CivilizationManager spiedCivFour)
        {
            _spiedCivFour = spiedCivFour;
            return _spiedCivFour;
        }
        public static CivilizationManager SendSpiedCivFive(CivilizationManager spiedCivFive)
        {
            _spiedCivFive = spiedCivFive;
            return _spiedCivFive;
        }
        public static CivilizationManager SendSpiedCivSix(CivilizationManager spiedCivSix)
        {
            _spiedCivSix = spiedCivSix;
            return _spiedCivSix;
        }
        //        //public static bool IsInfiltrated(Civilization source, Civilization target)
        //        //{
        //        //    if (source == null)
        //        //        throw new ArgumentNullException("source");
        //        //    if (target == null)
        //        //        //return false;
        //        //        throw new ArgumentNullException("target");

        //        //    if (source == target)
        //        //        return false;
        //        //    var infiltrated = AssetsScreenPresentationModel;
        //        //    //GameLog.Core.Test.DebugFormat("Diplomacy: source = {0} target = {1}",source.Key, target.Key);
        //        //    GameContext.Current.DiplomacyData[source, target].IsContactMade()
        //        //    return ;
        //        //}
    }
}
