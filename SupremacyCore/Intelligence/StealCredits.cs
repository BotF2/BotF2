using Supremacy.Entities;
using Supremacy.Universe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Supremacy.Intelligence
{
    class StealCredits // save a list of these on host computer in CivManager to exicute in GameEngine just before DoPostTurnOperations
    {
        private Colony _colony;
        private Civilization _attackingCiv;
        private Civilization _attackedCiv;
        private string _blamed;

        public StealCredits(Colony colony, Civilization attackingCiv, Civilization attackedCiv, string blamed)
        {
            _colony = colony;
            _attackingCiv = attackingCiv;
            _attackedCiv = attackedCiv;
            _blamed = blamed;
        }

    }
}
