using Supremacy.Entities;
using Supremacy.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Supremacy.Intelligence
{
    public static class IntelHelper
    {
        private static CivilizationManager _localPlayer;
        //private static CivilizationManagerMap _spyedCivs; 
        private static CivilizationManager _spyCiv;
        private static CivilizationManager _spiedCivOne;
        private static CivilizationManager _spiedCivTwo;
        private static CivilizationManager _spiedCivThree;
        private static CivilizationManager _spiedCivFour;
        private static CivilizationManager _spiedCivFive;
        private static CivilizationManager _spiedCivSix;

        //public static List<Civilization> SpyCivs
        //{
        //    get
        //    {
        //        List<Civilization> myCivs = new List<Civilization>();
        //        foreach (CivilizationManager civManager in _spyedCivs)
        //        {
        //            myCivs.Add(civManager.Civilization);
        //        }
        //        return myCivs;
        //    }
        //}

        //public static CivilizationManagerMap SpyCivsMap // turn off when 1 to 6 works
        //{
        //    get
        //    {
        //        CivilizationManagerMap myCivs = new CivilizationManagerMap();
        //        foreach (CivilizationManager civManager in _spyedCivs)
        //        {
        //            myCivs.Add(civManager);
        //        }
        //        return myCivs;
        //    }
        //}

        public static CivilizationManager LocalPlayerCivManager
        {
            get { return _localPlayer; }
        }
        public static CivilizationManager SpyCivManager
        {
            get { return _spyCiv; }
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
        //public static CivilizationManagerMap SendSpiedCivilizations(CivilizationManagerMap civManageList) // turn off when 1 to 6 works
        //{
        //    _spyedCivs = civManageList;
        //    return civManageList;
        //}
        public static CivilizationManager SendLocalPlayer(CivilizationManager localPlayer)
        {
            _localPlayer = localPlayer;
            return localPlayer; 
        }
        public static CivilizationManager SendSpyCiv(CivilizationManager spyCiv)
        {
            _spyCiv = spyCiv;
            return spyCiv;
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
    }
}
