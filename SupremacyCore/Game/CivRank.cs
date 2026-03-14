// File:CivRank.cs
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

//using C5;

namespace Supremacy.Game
{
public partial class GameEngine
    {
        public class CivRank
        {
            //public int AA_CIV_ID;
            public string CIV_KEY;
            //public int TOT_POP;
            //public int MOR;
            //public int TOT_VAL;
            //public int CRED;

            //public int RES;
            //public int IPROD;
            public int R_CRED;
            public int R_MAINT;
            public int R_RESEARCH;
            public int R_INTEL_ATTACK;

            public CivRank(
                //int aa_civ_ID
                //, 
                string civ_key
                //, int tot_pop
                //, int mor
                //, int tot_val
                //, int cred

                //, int res
                //, int iprod
                , int r_cred
                , int r_maint
                , int r_research
                , int r_intel_attack
                )
            {
                //AA_CIV_ID = aa_civ_ID;
                CIV_KEY = civ_key;
                //TOT_POP = tot_pop;
                //MOR = mor;
                //TOT_VAL = tot_val;
                //CRED = cred;

                //RES = res;
                //IPROD = iprod;
                R_CRED = r_cred;
                R_MAINT = r_maint;
                R_RESEARCH = r_research;
                R_INTEL_ATTACK = r_intel_attack;
            }
        }
    }





    //public void GetAcceptReject(ForeignPower _diplomatForeignPower_Civ2)
    //{
    //    if (_diplomatForeignPower_Civ2.PendingAction == PendingDiplomacyAction.AcceptProposal)
    //        AcceptProposalVisitor.Visit(_diplomatForeignPower_Civ2.LastProposalReceived);
    //    else RejectProposalVisitor.Visit(_diplomatForeignPower_Civ2.LastProposalReceived); 
    //}
}

