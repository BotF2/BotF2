using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Supremacy.Types
{
    [Serializable]
    public sealed class IsSetFlag
    {
        private bool _isSet;

        public IsSetFlag(bool isSet)
        {
            _isSet = isSet;
        }

        public bool IsSet
        {
            get { return _isSet; }
        }

        public void Set()
        {
            _isSet = true;
        }
    }
}
