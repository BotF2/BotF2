using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace Supremacy.Client.Controls
{
    public class ReadOnlyInfoCardCollection : ReadOnlyObservableCollection<InfoCard>
    {
        private const int InvalidIndex = -1;

        public ReadOnlyInfoCardCollection(ObservableCollection<InfoCard> list) : base(list) {}

        public bool Contains(Guid uniqueId)
        {
            return (IndexOf(uniqueId) != InvalidIndex);
        }

        public int IndexOf(string name)
        {
            for (var index = 0; index < Count; index++)
            {
                if (this[index].Name == name)
                    return index;
            }
            return InvalidIndex;
        }
        
        public int IndexOf(Guid uniqueId)
        {
            for (var index = 0; index < Count; index++)
            {
                if (this[index].UniqueId == uniqueId)
                    return index;
            }
            return InvalidIndex;
        }

        public InfoCard this[string name]
        {
            get
            {
                var index = IndexOf(name);
                if (index != InvalidIndex)
                    return this[index];
                return null;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1043:UseIntegralOrStringArgumentForIndexers")]
        public InfoCard this[Guid uniqueId]
        {
            get
            {
                var index = IndexOf(uniqueId);
                if (index != InvalidIndex)
                    return this[index];
                return null;
            }
        }

        public InfoCard[] ToArray()
        {
            var result = new InfoCard[Count];
            CopyTo(result, 0);
            return result;
        }

        public InfoCard[] ToArray(bool sortByLastFocusedDateTime)
        {
            var result = new InfoCard[Count];

            CopyTo(result, 0);

            if (sortByLastFocusedDateTime)
                Array.Sort(result, new InfoCard.LastFocusedComparer(result));

            return result;
        }
    }
}