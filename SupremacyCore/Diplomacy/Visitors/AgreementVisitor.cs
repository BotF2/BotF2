using System;

using Supremacy.Annotations;

namespace Supremacy.Diplomacy.Visitors
{
    public abstract class AgreementVisitor : ProposalVisitor, IAgreementVisitor
    {
        #region Implementation of IAgreementVisitor

        public virtual void Visit([NotNull] IAgreement agreement)
        {
            if (agreement == null)
                throw new ArgumentNullException("agreement");
            
            agreement.Proposal.Accept(this);
        }

        #endregion
    }
}