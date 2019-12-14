using Supremacy.Client.Context;
using Supremacy.Client.Views;
using Supremacy.Collections;
using Supremacy.Diplomacy;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Universe;
using Supremacy.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Supremacy.Intelligence
{
    public static class AssetsHelper
    {  
        public static Dictionary<Civilization, UniverseObjectList<Colony>> Spied = new Dictionary<Civilization, UniverseObjectList<Colony>>();

        public static bool IsSpiedOn(Civilization targetFromScreen)
        {

            if (IntelHelper.NewSpyCiv == null)
                return false;
            else if (true)
              { 
                var newSpyCiv = IntelHelper.NewSpyCiv;
                var newTargetCiv = IntelHelper.NewTargetCiv;
                var newSpiedColonies = IntelHelper.NewSpiedColonies;

                Civilization civOne = new Civilization();
                Civilization civTwo = new Civilization();
                Civilization civThree = new Civilization();
                Civilization civFour = new Civilization(); 
                Civilization civFive = new Civilization();
                Civilization civSix = new Civilization();

                Civilization civLocalPlayer = DesignTimeObjects.GetCivLocalPlayer().Civilization; 

                //if (true)
                {
                    try { civOne = DesignTimeObjects.GetSpiedCivilizationOne().Civilization; }
                    catch { civOne = civLocalPlayer; }
                }
                //if (true)
                {
                    try { civTwo = DesignTimeObjects.GetSpiedCivilizationTwo().Civilization; }
                    catch { civTwo = civLocalPlayer; }
                }
                //if (true)
                {
                    try { civThree = DesignTimeObjects.GetSpiedCivilizationThree().Civilization; }
                    catch { civThree = civLocalPlayer; }
                }
                //if (true)
                {
                    try { civFour = DesignTimeObjects.GetSpiedCivilizationFour().Civilization; }
                    catch { civFour = civLocalPlayer; }
                }
                //if (true)
                {
                    try { civFive = DesignTimeObjects.GetSpiedCivilizationFive().Civilization; }
                    catch { civFive = civLocalPlayer; }
                }
                //if (true)
                {
                    try { civSix = DesignTimeObjects.GetSpiedCivilizationSix().Civilization; }
                    catch { civSix = civLocalPlayer; }
                }

                GameLog.Client.UI.DebugFormat("Civs are populated .... ");

                if (newSpyCiv == DesignTimeAppContext.Instance.LocalPlayerEmpire.Civilization)
                {
                    GameLog.Client.UI.DebugFormat("building up the screen stuff for the local player .... ");
                    if (civOne == IntelHelper.NewTargetCiv) //civOne.CivID != -1 && 
                    {
                        GameLog.Client.UI.DebugFormat("building up spy report for civOne = {0} .... ", civOne.Key);
                        if (!Spied.Where(o => o.Key == civOne).Any())
                        {
                            GameLog.Client.UI.DebugFormat("before adding all colonies of civOne = {0} .... ", civOne.Key);
                            Spied.Add(civOne, newSpiedColonies);
                            //GameLog.Client.UI.DebugFormat("before adding all colonies of civOne = {0} .... ", civOne.Key);
                            foreach (var col in newSpiedColonies)
                            {
                                GameLog.Client.UI.DebugFormat("before adding all colony {0} for spiedcolonies of civOne = {1} .... ", civOne.Key, col.Name);
                            }
                        }
                    }
                    if (civTwo == IntelHelper.NewTargetCiv)
                    {
                        if (!Spied.Where(o => o.Key == civTwo).Any())
                        {
                            Spied.Add(civTwo, newSpiedColonies);
                        }
                    }
                    if (civThree == IntelHelper.NewTargetCiv)
                    {
                        if (!Spied.Where(o => o.Key == civThree).Any())
                        {
                            Spied.Add(civThree, newSpiedColonies);
                        }
                    }
                    if (civFour == IntelHelper.NewTargetCiv)
                    {
                        if (!Spied.Where(o => o.Key == civFour).Any())
                        {
                            Spied.Add(civFour, newSpiedColonies);
                        }
                    }
                    if (civFive == IntelHelper.NewTargetCiv)
                    {
                        if (!Spied.Where(o => o.Key == civFive).Any())
                        {
                            Spied.Add(civFive, newSpiedColonies);
                        }
                    }
                    if (civSix == IntelHelper.NewTargetCiv)
                    {
                        if (!Spied.Where(o => o.Key == civSix).Any())
                        {
                            Spied.Add(civSix, newSpiedColonies);
                        }
                    }
                    var anySpied = Spied.Where(s => s.Key == targetFromScreen).Any();
                    return anySpied;
                }
            }
            return false;
        }
    }
}

