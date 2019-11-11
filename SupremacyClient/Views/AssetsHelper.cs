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
        //private static CivilizationManager _localPlayer;
        //private static CivilizationManager _spiedCivOne;
        //private static CivilizationManager _spiedCivTwo;
        //private static CivilizationManager _spiedCivThree;
        //private static CivilizationManager _spiedCivFour;
        //private static CivilizationManager _spiedCivFive;
        //private static CivilizationManager _spiedCivSix;

        //public static CivilizationManager LocalPlayerCivManager
        //{
        //    get { return _localPlayer; }
        //}
        //public static CivilizationManager SpiedOneCivManager
        //{
        //    get { return _spiedCivOne; }
        //}
        //public static CivilizationManager SpiedTwoCivManager
        //{
        //    get { return _spiedCivTwo; }
        //}
        //public static CivilizationManager SpiedThreeCivManager
        //{
        //    get { return _spiedCivThree; }
        //}

        //public static CivilizationManager SpiedFourCivManager
        //{
        //    get { return _spiedCivFour; }
        //}
        //public static CivilizationManager SpiedFiveCivManager
        //{
        //    get { return _spiedCivFive; }
        //}
        //public static CivilizationManager SpiedSixCivManager
        //{
        //    get { return _spiedCivSix; }
        //}

        //public static Colony SpiedOneHomeColony
        //{
        //    get
        //    {
        //        return _spiedCivOne.HomeColony;
        //    }
        //}
        //public static Colony SpiedTwoHomeColony
        //{
        //    get
        //    {
        //        return _spiedCivTwo.HomeColony;
        //    }
        //}
        //public static Colony SpiedThreeHomeColony
        //{
        //    get
        //    {
        //        return _spiedCivThree.HomeColony;
        //    }
        //}
        //public static Colony SpiedFourHomeColony
        //{
        //    get
        //    {
        //        return _spiedCivFour.HomeColony;
        //    }
        //}
        //public static Colony SpiedFiveHomeColony
        //{
        //    get
        //    {
        //        return _spiedCivFive.HomeColony;
        //    }
        //}
        //public static Colony SpiedSixHomeColony
        //{
        //    get
        //    {
        //        return _spiedCivSix.HomeColony;
        //    }
        //}
        //public static Dictionary<Civilization, List<Colony>> SpiedOneInfiltrated
        //{
        //    get { return SpiedOneCivManager.InfiltratedColonies; }
        //}
        //public static Dictionary<Civilization, List<Colony>> SpiedTwoInfiltrated
        //{
        //    get { return SpiedTwoCivManager.InfiltratedColonies; }
        //}
        //public static Dictionary<Civilization, List<Colony>> SpiedThreeInfiltrated
        //{
        //    get { return SpiedThreeCivManager.InfiltratedColonies; }
        //}
        //public static Dictionary<Civilization, List<Colony>> SpiedFourInfiltrated
        //{
        //    get { return SpiedFourCivManager.InfiltratedColonies; }
        //}
        //public static Dictionary<Civilization, List<Colony>> SpiedFiveInfiltrated
        //{
        //    get { return SpiedFiveCivManager.InfiltratedColonies; }
        //}
        //public static Dictionary<Civilization, List<Colony>> SpiedSixInfiltrated
        //{
        //    get { return SpiedSixCivManager.InfiltratedColonies; }
        //}

        //public static CivilizationManager SendLocalPlayer(CivilizationManager localPlayer)
        //{
        //    _localPlayer = localPlayer;
        //    return localPlayer;
        //}
        //public static CivilizationManager SendSpiedCivOne(CivilizationManager spiedCivOne)
        //{
        //    _spiedCivOne = spiedCivOne;
        //    return _spiedCivOne;
        //}
        //public static CivilizationManager SendSpiedCivTwo(CivilizationManager spiedCivTwo)
        //{
        //    _spiedCivTwo = spiedCivTwo;
        //    return _spiedCivTwo;
        //}
        //public static CivilizationManager SendSpiedCivThree(CivilizationManager spiedCivThree)
        //{
        //    _spiedCivThree = spiedCivThree;
        //    return _spiedCivThree;
        //}
        //public static CivilizationManager SendSpiedCivFour(CivilizationManager spiedCivFour)
        //{
        //    _spiedCivFour = spiedCivFour;
        //    return _spiedCivFour;
        //}
        //public static CivilizationManager SendSpiedCivFive(CivilizationManager spiedCivFive)
        //{
        //    _spiedCivFive = spiedCivFive;
        //    return _spiedCivFive;
        //}
        //public static CivilizationManager SendSpiedCivSix(CivilizationManager spiedCivSix)
        //{
        //    _spiedCivSix = spiedCivSix;
        //    return _spiedCivSix;
        //}
  
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
                //GameLog.Client.Intel.DebugFormat("civOne Name={0}", DesignTimeAppContext.Instance.SpiedOneEmpire.Civilization.Name);
                //GameLog.Client.Intel.DebugFormat("civTwo Name={0}", DesignTimeAppContext.Instance.SpiedTwoEmpire.Civilization.Name);
                var civOne = IntelHelper.SpiedOneCivManager.Civilization; //DesignTimeAppContext.Instance.SpiedOneEmpire.Civilization;
                var civTwo = IntelHelper.SpiedTwoCivManager.Civilization;//DesignTimeAppContext.Instance.SpiedTwoEmpire.Civilization;
                var civThree = IntelHelper.SpiedThreeCivManager.Civilization; // DesignTimeAppContext.Instance.SpiedThreeEmpire.Civilization;
                var civFour = IntelHelper.SpiedFourCivManager.Civilization; //DesignTimeAppContext.Instance.SpiedFourEmpire.Civilization;
                var civFive = IntelHelper.SpiedFiveCivManager.Civilization;  //DesignTimeAppContext.Instance.SpiedFiveEmpire.Civilization;
                var civSix = IntelHelper.SpiedSixCivManager.Civilization;  //DesignTimeAppContext.Instance.SpiedSixEmpire.Civilization;
                //var homeColonyOne = _spiedCivOne.HomeColony;
                //AssetsScreenPresentationModel helper = new AssetsScreenPresentationModel();

                if (newSpyCiv == DesignTimeAppContext.Instance.LocalPlayerEmpire.Civilization)//helper.)// (Civilization)DesignTimeAppContext.Instance.LocalPlayer)//_localPlayer.Civilization)
                {
                    //int[] counter = new int[6] { 0, 0, 0, 0, 0, 0 };
                    if (civOne.Name != "Empty" && civOne == IntelHelper.NewTargetCiv)
                    {
                        if (!Spied.Where(o => o.Key == civOne).Any())
                        {
                            Spied.Add(civOne, newSpiedColonies);
                        }
                    }
                    if (civTwo.Name != "Empty" && civTwo == IntelHelper.NewTargetCiv)
                    {
                        if (!Spied.Where(o => o.Key == civTwo).Any())
                        {
                            Spied.Add(civTwo, newSpiedColonies);
                        }
                    }
                    if (civThree.Name != "Empty" && civThree == IntelHelper.NewTargetCiv)
                    {
                        if (!Spied.Where(o => o.Key == civThree).Any())
                        {
                            Spied.Add(civThree, newSpiedColonies);
                        }
                    }
                    if (civFour.Name != "Empty" && civFour == IntelHelper.NewTargetCiv)
                    {
                        if (!Spied.Where(o => o.Key == civFour).Any())
                        {
                            Spied.Add(civFour, newSpiedColonies);
                        }
                    }
                    if (civFive.Name != "Empty" && civFive == IntelHelper.NewTargetCiv)
                    {
                        if (!Spied.Where(o => o.Key == civFive).Any())
                        {
                            Spied.Add(civFive, newSpiedColonies);
                        }
                    }
                    if (civSix.Name != "Empty" && civSix == IntelHelper.NewTargetCiv)
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
        //    return anySpied;

        //{
        //    if (source == null)
        //        throw new ArgumentNullException("source");
        //    if (target == null)
        //        //return false;
        //        throw new ArgumentNullException("target");

        //    if (source == target)
        //        return false;

        //    //var otherCivManagers = DesignTimeObjects.OtherMajorEmpires;
        //    //foreach (var CivManager in otherCivManagers)
        //    //{
        //    var SpiedOneInfiltrated = new Dictionary<Civilization, List<Colony>>()
        //    {
        //       // { _spiedCivOne.Civilization, DesignTimeAppContext.Instance.SpiedOneColonies }

        //    };
        //    //}
        //    List <Dictionary<Civilization, List<Colony>>> spiedDictionaries = new List<Dictionary<Civilization, List<Colony>>>();
        //    spiedDictionaries.Add(SpiedOneInfiltrated);
        //    spiedDictionaries.Add(SpiedTwoInfiltrated);
        //    spiedDictionaries.Add(SpiedThreeInfiltrated);
        //    spiedDictionaries.Add(SpiedFourInfiltrated);
        //    spiedDictionaries.Add(SpiedFiveInfiltrated);
        //    spiedDictionaries.Add(SpiedSixInfiltrated);
    }
}

