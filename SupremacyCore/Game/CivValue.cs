// File:CivValue.cs
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
        // new list/stuff to find out 'Best' and 'Places' values for each _civ
        public class CivValue
        {
            public int AA_CIV_ID;
            public string CIV_KEY;
            public int TOT_POP;
            public int MOR;
            public int TOT_VAL;
            public int CRED;
            public int MAINT;
            public int RES;
            public int IPROD;
            public int R_CRED;
            public int R_MAINT;


            public CivValue(
                int aa_civ_ID
                , string civ_key
                , int tot_pop
                , int mor
                , int tot_val
                , int cred
                , int maint
                , int res
                , int iprod
                , int r_cred
                , int r_maint
                )
            {
                AA_CIV_ID = aa_civ_ID;
                CIV_KEY = civ_key;
                TOT_POP = tot_pop;
                MOR = mor;
                TOT_VAL = tot_val;
                CRED = cred;
                MAINT = maint;
                RES = res;
                IPROD = iprod;
                R_CRED = r_cred;
                R_MAINT = r_maint;
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

