using System;

using Supremacy.Annotations;

namespace Supremacy.Diplomacy.Visitors
{
    public static class AgreementFulfillmentVisitor
    {
        public static void Visit([NotNull] IAgreement agreement)
        {
            if (agreement == null)
            {
                throw new ArgumentNullException("agreement");
            }

            CreditObligationFulfillmentVisitor.Visit(agreement);
        }
    }
}