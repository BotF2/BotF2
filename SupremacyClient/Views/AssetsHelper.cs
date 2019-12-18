using Supremacy.Client.Context;
//using Supremacy.Client.Views;
using Supremacy.Collections;
//using Supremacy.Diplomacy;
using Supremacy.Entities;
//using Supremacy.Game;
using Supremacy.Universe;
using Supremacy.Utility;
//using System;
using System.Collections.Generic;
using System.Linq;

namespace Supremacy.Intelligence
{
    public static class AssetsHelper
    {  
        public static Dictionary<Civilization, UniverseObjectList<Colony>> SpiedDictionary = new Dictionary<Civilization, UniverseObjectList<Colony>>();
        
        public static bool IsSpiedOn(Civilization targetFromScreen)
        {
            if (IntelHelper.NewSpyCiv == null)
                return false;
            else if (true)
              { 
                var newSpyCiv = IntelHelper.NewSpyCiv;
                var newTargetCiv = IntelHelper.NewTargetCiv;
                var newSpiedColonies = IntelHelper.NewSpiedColonies;

                Civilization civOne = DesignTimeObjects.SpiedCivMangers[0].Civilization;
                Civilization civTwo = DesignTimeObjects.SpiedCivMangers[1].Civilization;
                Civilization civThree = DesignTimeObjects.SpiedCivMangers[2].Civilization;
                Civilization civFour = DesignTimeObjects.SpiedCivMangers[3].Civilization;
                Civilization civFive = DesignTimeObjects.SpiedCivMangers[4].Civilization;
                Civilization civSix = DesignTimeObjects.SpiedCivMangers[5].Civilization;

                Civilization civLocalPlayer = DesignTimeObjects.GetCivLocalPlayer().Civilization;

                GameLog.Client.UI.DebugFormat("Civs are populated .... ");
      
                if (newSpyCiv == civLocalPlayer)
                {
                    GameLog.Client.UI.DebugFormat("building up the screen stuff for the local player .... ");
                    if (civOne == IntelHelper.NewTargetCiv) //civOne.CivID != -1 && 
                    {
                        GameLog.Client.UI.DebugFormat("building up spy report for civOne = {0} .... ", civOne.Key);
                        if (!SpiedDictionary.Where(o => o.Key == civOne).Any() && civOne != civLocalPlayer)
                        {
                            GameLog.Client.UI.DebugFormat("before adding all colonies of civOne = {0} .... ", civOne.Key);
                            SpiedDictionary.Add(civOne, newSpiedColonies);
                            //GameLog.Client.UI.DebugFormat("before adding all colonies of civOne = {0} .... ", civOne.Key);
                            foreach (var col in newSpiedColonies)
                            {
                                GameLog.Client.UI.DebugFormat("before adding all colony {0} for spiedcolonies of civOne = {1} .... ", civOne.Key, col.Name);
                            }
                        }
                    }
                    if (civTwo == IntelHelper.NewTargetCiv)
                    {
                        if (!SpiedDictionary.Where(o => o.Key == civTwo).Any() && civTwo != civLocalPlayer)
                        {
                            SpiedDictionary.Add(civTwo, newSpiedColonies);
                        }
                    }
                    if (civThree == IntelHelper.NewTargetCiv)
                    {
                        if (!SpiedDictionary.Where(o => o.Key == civThree).Any() && civThree != civLocalPlayer)
                        {
                            SpiedDictionary.Add(civThree, newSpiedColonies);
                        }
                    }
                    if (civFour == IntelHelper.NewTargetCiv)
                    {
                        if (!SpiedDictionary.Where(o => o.Key == civFour).Any() && civFour != civLocalPlayer)
                        {
                            SpiedDictionary.Add(civFour, newSpiedColonies);
                        }
                    }
                    if (civFive == IntelHelper.NewTargetCiv)
                    {
                        if (!SpiedDictionary.Where(o => o.Key == civFive).Any() && civFive != civLocalPlayer)
                        {
                            SpiedDictionary.Add(civFive, newSpiedColonies);
                        }
                    }
                    if (civSix == IntelHelper.NewTargetCiv)
                    {
                        if (!SpiedDictionary.Where(o => o.Key == civSix).Any() && civSix != civLocalPlayer)
                        {
                            SpiedDictionary.Add(civSix, newSpiedColonies);
                        }
                    }
                    var anySpied = SpiedDictionary.Where(s => s.Key == targetFromScreen).Any();
                    return anySpied;
                }
            }
            return false;
        }
    }
}

