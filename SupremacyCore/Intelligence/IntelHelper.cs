using Supremacy.Diplomacy;
using Supremacy.Entities;
//using Supremacy.Entities;
using Supremacy.Game;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

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
        //public static bool IsContactedMade(Civilization otherCiv)
        //{
        //    return DiplomacyHelper.IsContactMade(_localPlayer.Civilization, otherCiv);
        //}
    }
}
