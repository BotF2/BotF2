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
        private static CivilizationManagerMap _spyedCivs;

        public static List<Civilization> SpyCivs
        {
            get 
            {
                List<Civilization> myCivs = new List<Civilization>();
                foreach (CivilizationManager civManager in _spyedCivs)
                {
                    myCivs.Add(civManager.Civilization);
                }
                return myCivs;
            }
        }

        public static CivilizationManager LocalPlayerCivManager
        {
            get { return _localPlayer; }
        }

        public static CivilizationManagerMap SendSpiedCivilizations(CivilizationManagerMap civManageList)
        {
            _spyedCivs = civManageList;// hope we get one major empire that is not local player
            return  civManageList; 
        }
        public static CivilizationManager SendLocalPlayer(CivilizationManager localPlayer)
        {
            _localPlayer = localPlayer;
            return localPlayer; // hope we get one major empire that is not local player
        }

        //public static CivilizationManagerMap GetSpiedCivilizations()
        //{
        //    throw new NotImplementedException();
        //}
    }
}
