using System;
using System.Globalization;

using Supremacy.Collections;

namespace Supremacy.Text
{
    [Serializable]
    public class LocalizedStringValueCollection : KeyedCollectionBase<CultureInfo, LocalizedStringValue>
    {
        public LocalizedStringValueCollection()
            : base(o => o.Language) { }
    }
}