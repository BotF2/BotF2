using Supremacy.Game;
using System;



// THE REAL PROJECT PACE AND EMPIRE POWER
namespace Supremacy.PaceAndEmpirePower
{


    // Project Pace

    // Modifies Game PACE (effects all Empires) and Empires Power (effects specific Major Empires only). Both Pace and Empire Power is used on multiple occasions like Research, Combat etc.
    public class TranslatePaceAndEmpirePower
    {
        // AFTER GAMETESTS THIS CAN BE USED TO BALANCE

        // Called from everywhere to get a power modifier for the corresponding empire
        public double getEmpirePowerModifier(int EmpireID, bool offensiveYes)
        {
            double universalPowerModifier = 1;
            // Depending on the Empire, looks for the corresponding Bonus or Malus... 
            switch (EmpireID)
            {
                case 0:
                    universalPowerModifier = bonusMalus(Convert.ToInt16(GameContext.Current.Options.FederationModifier), offensiveYes); // and based on that (-5 to +5) give a double modifier that can be used everywhere (credis, research etc. to make empire stronger or weaker)
                    break;
                case 1:
                    universalPowerModifier = bonusMalus(Convert.ToInt16(GameContext.Current.Options.TerranEmpireModifier), offensiveYes);
                    break;
                case 2:
                    universalPowerModifier = bonusMalus(Convert.ToInt16(GameContext.Current.Options.RomulanModifier), offensiveYes);
                    break;
                case 3:
                    universalPowerModifier = bonusMalus(Convert.ToInt16(GameContext.Current.Options.KlingonModifier), offensiveYes);
                    break;
                case 4:
                    universalPowerModifier = bonusMalus(Convert.ToInt16(GameContext.Current.Options.CardassianModifier), offensiveYes);
                    break;
                case 5:
                    universalPowerModifier = bonusMalus(Convert.ToInt16(GameContext.Current.Options.DominionModifier), offensiveYes);
                    break;
                case 6:
                    universalPowerModifier = bonusMalus(Convert.ToInt16(GameContext.Current.Options.BorgModifier), offensiveYes);
                    break;
                default:
                    break;
            }
            return universalPowerModifier;
        }

        public double getGamePaceShipCombatModifier()
        {
            // Depending on the Game Pace, the Damage to Ship Combat is reduced or increased.
            double xGamePaceShip = 1;
            switch (Convert.ToInt16(GameContext.Current.Options.GamePace))
            {
                case 0:
                    xGamePaceShip = 0.75; // Slow
                    break;
                case 1:
                    xGamePaceShip = 1.45; // Normal Pace
                    break;
                case 2:
                    xGamePaceShip = 2.0; // fast
                    break;

            }
            return xGamePaceShip;
        }

        // bonusMalus is called from getEmpirePowerModifier not from outside PaceAndEmpirePower.cs
        public double bonusMalus(int minus5toplus5, bool BonusEqualsBigger)
        {
            double _minus5toplus5 = 1; // Returns a Modifier based on the selection of biggestBonus(5) and biggestMalus(-5)
            if (BonusEqualsBigger)// Then Positive Mod for Bonus
            {
                switch (minus5toplus5)
                {
                    case -5:
                        _minus5toplus5 = 0.35; // BIGGEST MALUS In many areas (credits, intel etc.) * 0.25
                        break;
                    case -4:
                        _minus5toplus5 = 0.65;
                        break;
                    case -3:
                        _minus5toplus5 = 0.75;
                        break;
                    case -2:
                        _minus5toplus5 = 0.85;
                        break;
                    case -1:
                        _minus5toplus5 = 0.95;
                        break;
                    case 0:
                        _minus5toplus5 = 1;
                        break;
                    case 1:
                        _minus5toplus5 = 1.05;
                        break;
                    case 2:
                        _minus5toplus5 = 1.15;
                        break;
                    case 3:
                        _minus5toplus5 = 1.25;
                        break;
                    case 4:
                        _minus5toplus5 = 1.45;
                        break;
                    case 5:
                        _minus5toplus5 = 2.5; // BIGGEST Bonus In many areas (credits, intel etc.) 2.5
                        break;
                }
            }
            else // If Mod shell help the defender, bonus = lower, malus = smaller
            {
                switch (minus5toplus5)
                {
                    case -5:
                        _minus5toplus5 = 2.25; // BIGGEST MALUS In many areas (credits, intel etc.) * 0.25
                        break;
                    case -4:
                        _minus5toplus5 = 1.45;
                        break;
                    case -3:
                        _minus5toplus5 = 1.25;
                        break;
                    case -2:
                        _minus5toplus5 = 1.10;
                        break;
                    case -1:
                        _minus5toplus5 = 1.05;
                        break;
                    case 0:
                        _minus5toplus5 = 1;
                        break;
                    case 1:
                        _minus5toplus5 = 0.95;
                        break;
                    case 2:
                        _minus5toplus5 = 0.85;
                        break;
                    case 3:
                        _minus5toplus5 = 0.75;
                        break;
                    case 4:
                        _minus5toplus5 = 0.60;
                        break;
                    case 5:
                        _minus5toplus5 = 0.30; // BIGGEST Bonus In many areas (credits, intel etc.) 2.5
                        break;
                }


            }

            return _minus5toplus5;

        }
        //GameContext.Current.Options.CardassianModifier
        //    -5

        //swtich 
        //    case ^1 Federation
        //    Small bonus = 5 -5

        //GameContext.Current.... Stanard, Small Bonus.....



        //  int modifier = 2;


        public double getGamePaceStationModifier()
        {
            // Depending on the Game Pace, the Damage to Stations is reduced or increased.
            double xGamePaceStation = 1;
            switch (Convert.ToInt16(GameContext.Current.Options.GamePace))
            {
                case 0:
                    xGamePaceStation = 0.55; // Slow
                    break;
                case 1:
                    xGamePaceStation = 1; // Normal Pace
                    break;
                case 2:
                    xGamePaceStation = 2.25; // fast
                    break;

            }

            return xGamePaceStation;

        }

        public double getGamePaceOrbitalModifier(bool FastIsMore)
        {
            // Depending on the Game Pace, the Damage to Orbitals is reduced or increased.
            double xGamePaceOrbital = 1;
            if (FastIsMore)
            {
                switch (Convert.ToInt16(GameContext.Current.Options.GamePace))
                {
                    case 0:
                        xGamePaceOrbital = 0.7; // Slow
                        break;
                    case 1:
                        xGamePaceOrbital = 1; // Normal Pace
                        break;
                    case 2:
                        xGamePaceOrbital = 1.35; // fast
                        break;

                }
            }
            else
            {
                switch (Convert.ToInt16(GameContext.Current.Options.GamePace))
                {
                    case 0:
                        xGamePaceOrbital = 1.3; // Slow
                        break;
                    case 1:
                        xGamePaceOrbital = 1; // Normal Pace
                        break;
                    case 2:
                        xGamePaceOrbital = 0.65; // fast
                        break;

                }
            }
            return xGamePaceOrbital;

        }




        public double getGamePaceInvasionModifier() // Also used for Science Ship research
        {
            // Depending on the Game Pace, the Damage to Stations is reduced or increased.
            double xGamePaceInvasion = 1;
            switch (Convert.ToInt16(GameContext.Current.Options.GamePace))
            {
                case 0:
                    xGamePaceInvasion = 0.40; // Slow
                    break;
                case 1:
                    xGamePaceInvasion = 1; // Normal Pace
                    break;
                case 2:
                    xGamePaceInvasion = 1.6; // fast
                    break;

            }

            return xGamePaceInvasion;

        }







        public double getGamePaceResearchModifier()
        {
            // Depending on the Game Pace, the Damage to Stations is reduced or increased.
            double xGamePaceResearch = 1;
            switch (Convert.ToInt16(GameContext.Current.Options.GamePace))
            {
                case 0:
                    xGamePaceResearch = 0.50; // slow
                    break;
                case 1:
                    xGamePaceResearch = 1;
                    break;
                case 2:
                    xGamePaceResearch = 1.80; // fast
                    break;

            }

            return xGamePaceResearch;

        }



    }
    //TranslatePaceAndEmpirePower valueX = new TranslatePaceAndEmpirePower();
}

