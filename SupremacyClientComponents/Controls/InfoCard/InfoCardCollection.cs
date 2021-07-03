using System;
using System.Diagnostics.CodeAnalysis;

namespace Supremacy.Client.Controls
{
    /// <summary>
    /// Represents the base class for an observable collection of <see cref="InfoCard"/> objects.
    /// </summary>
    public class InfoCardCollection : DeferrableObservableCollection<InfoCard>
    {
        private const int InvalidIndex = -1;

        /// <summary>
        /// Determines whether an item with the specified <c>UniqueId</c> value is in the collection.
        /// </summary>
        /// <param name="uniqueId">The unique ID value to locate in the collection.</param>
        /// <returns>
        /// <c>true</c> if an item with the specified <c>UniqueId</c> value is found in the collection; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(Guid uniqueId)
        {
            return IndexOf(uniqueId) != InvalidIndex;
        }

        /// <summary>
        /// Searches for an item with the specified <c>Name</c> value and 
        /// returns the zero-based index of the first occurrence within the entire collection.
        /// </summary>
        /// <param name="name">The name value to locate in the collection.</param>
        /// <returns>
        /// The zero-based index of the first occurrence of an item with the specified <c>Name</c> value
        /// within the entire collection, if found; otherwise, <c>-1</c>.
        /// </returns>
        public int IndexOf(string name)
        {
            for (int index = 0; index < Count; index++)
            {
                if (this[index].Name == name)
                {
                    return index;
                }
            }
            return InvalidIndex;
        }

        /// <summary>
        /// Searches for an item with the specified <c>UniqueId</c> value and 
        /// returns the zero-based index of the first occurrence within the entire collection.
        /// </summary>
        /// <param name="uniqueId">The unique ID value to locate in the collection.</param>
        /// <returns>
        /// The zero-based index of the first occurrence of an item with the specified <c>UniqueId</c> value
        /// within the entire collection, if found; otherwise, <c>-1</c>.
        /// </returns>
        public int IndexOf(Guid uniqueId)
        {
            for (int index = 0; index < Count; index++)
            {
                if (this[index].UniqueId == uniqueId)
                {
                    return index;
                }
            }
            return InvalidIndex;
        }

        /// <summary>
        /// Gets the item with the specified <c>UniqueId</c> value. 
        /// <para>
        /// [C#] In C#, this property is the indexer for the collection. 
        /// </para>
        /// </summary>
        /// <param name="uniqueId">The unique ID value to locate in the collection.</param>
        /// <value>
        /// The item with the specified <c>UniqueId</c> value. 
        /// </value>
        [SuppressMessage("Microsoft.Design", "CA1043:UseIntegralOrStringArgumentForIndexers")]
        public InfoCard this[Guid uniqueId]
        {
            get
            {
                int index = IndexOf(uniqueId);
                if (index != InvalidIndex)
                {
                    return this[index];
                }

                return null;
            }
        }

        /// <summary>
        /// Returns a strongly-typed array containing the items in this collection, optionally sorted by last focused date/time.
        /// </summary>
        /// <param name="sortByLastFocusedDateTime">Whether to sort by the last focused date/time.</param>
        /// <returns>A strongly-typed array containing the items in this collection.</returns>
        public InfoCard[] ToArray(bool sortByLastFocusedDateTime)
        {
            InfoCard[] result = new InfoCard[Count];
            CopyTo(result, 0);

            if (sortByLastFocusedDateTime)
            {
                Array.Sort(result, new InfoCard.LastFocusedComparer(result));
            }

            return result;
        }


    }
}