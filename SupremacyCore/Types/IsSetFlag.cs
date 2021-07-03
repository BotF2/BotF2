using System;

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

        public bool IsSet => _isSet;

        public void Set()
        {
            _isSet = true;
        }
    }
}
