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

                if (true)
                {
                    try { civOne = DesignTimeObjects.GetSpiedCivilizationOne().Civilization; }
                    catch { civOne = DesignTimeObjects.GetCivLocalPlayer().Civilization; }
                }
                if (true)
                {
                    try { civTwo = DesignTimeObjects.GetSpiedCivilizationTwo().Civilization; }
                    catch { civTwo = DesignTimeObjects.GetCivLocalPlayer().Civilization; }
                }
                if (true)
                {
                    try { civThree = DesignTimeObjects.GetSpiedCivilizationThree().Civilization; }
                    catch { civThree = DesignTimeObjects.GetCivLocalPlayer().Civilization; }
                }
                if (true)
                {
                    try { civFour = DesignTimeObjects.GetSpiedCivilizationFour().Civilization; }
                    catch { civFour = DesignTimeObjects.GetCivLocalPlayer().Civilization; }
                }
                if (true)
                {
                    try { civFive = DesignTimeObjects.GetSpiedCivilizationFive().Civilization; }
                    catch { civFive = DesignTimeObjects.GetCivLocalPlayer().Civilization; }
                }
                if (true)
                {
                    try { civSix = DesignTimeObjects.GetSpiedCivilizationSix().Civilization; }
                    catch { civSix = DesignTimeObjects.GetCivLocalPlayer().Civilization; }
                }

                if (newSpyCiv == DesignTimeAppContext.Instance.LocalPlayerEmpire.Civilization)
                {
                    if (civOne == IntelHelper.NewTargetCiv) //civOne.CivID != -1 && 
                    {
                        if (!Spied.Where(o => o.Key == civOne).Any())
                        {
                            Spied.Add(civOne, newSpiedColonies);
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

