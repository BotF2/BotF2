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
        private static Civilization _newTargetCiv;
        private static Civilization _newSpyCiv;
        private static UniverseObjectList<Colony> _newSpiedColonies;

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
