// FrugalMap.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections;
using System.Diagnostics;

namespace Supremacy.Effects
{
    // These classes implement a frugal storage model for key/value pair data
    // structures. The keys are integers, and the values objects. 
    // Performance measurements show that Avalon has many maps that contain a
    // single key/value pair. Therefore these classes are structured to prefer
    // a map that contains a single key/value pair and uses a conservative
    // growth strategy to minimize the steady state memory footprint. To enforce 
    // the slow growth the map does not allow the user to set the capacity.
    // Also note that the map uses one fewer objects than the BCL HashTable and 
    // does no allocations at all until an item is inserted into the map. 
    //
    // The code is also structured to perform well from a CPU standpoint. Perf 
    // analysis of DependencyObject showed that we used a single entry 63% of
    // the time and growth tailed off quickly. Average access times are 8 to 16
    // times faster than a BCL Hashtable.
    // 
    // FrugalMap is appropriate for small maps or maps that grow slowly. Its
    // primary focus is for maps that contain fewer than 64 entries and that 
    // usually start with no entries, or a single entry. If you know your map 
    // will always have a minimum of 64 or more entires FrugalMap *may* not
    // be the best choice. Choose your collections wisely and pay particular 
    // attention to the growth patterns and search methods.

    // This enum controls the growth to successively more complex storage models
    internal enum FrugalMapStoreState
    {
        Success,
        ThreeObjectMap,
        SixObjectMap,
        Array,
        SortedArray,
        Hashtable
    }

    internal abstract class FrugalMapBase
    {
        internal static ArgumentException TargetMapTooSmall(string paramName)
        {
            return new ArgumentException(
                "FrugalMap cannot be promoted because the target map is too small.",
                paramName);
        }

        public abstract FrugalMapStoreState InsertEntry(int key, Object value);

        public abstract void RemoveEntry(int key);

        /// <summary>
        /// Looks for an entry that contains the given key, null is returned if the
        /// key is not found. 
        /// </summary>
        public abstract Object Search(int key);

        /// <summary> 
        /// A routine used by enumerators that need a sorted map
        /// </summary>
        public abstract void Sort();

        /// <summary>
        /// A routine used by enumerators to iterate through the map 
        /// </summary> 
        public abstract void GetKeyValuePair(int index, out int key, out Object value);

        /// <summary>
        /// A routine used to iterate through all the entries in the map
        /// </summary>
        public abstract void Iterate(ArrayList list, FrugalMapIterationCallback callback);

        /// <summary> 
        /// Promotes the key/value pairs in the current collection to the next larger 
        /// and more complex storage model.
        /// </summary> 
        public abstract void Promote(FrugalMapBase newMap);

        /// <summary>
        /// Size of this data store 
        /// </summary>
        public abstract int Count { get; }

        protected const int InvalidKey = 0x7FFFFFFF;

        internal struct Entry
        {
            public int Key;
            public Object Value;
        }
    }

    /// <summary>
    /// A simple class to handle a single key/value pair
    /// </summary> 
    internal sealed class SingleObjectMap : FrugalMapBase
    {
        public SingleObjectMap()
        {
            _loneEntry.Key = InvalidKey;
            _loneEntry.Value = DynamicProperty.UnsetValue;
        }

        public override FrugalMapStoreState InsertEntry(int key, Object value)
        {
            // If we don't have any entries or the existing entry is being overwritten, 
            // then we can use this map.  Otherwise we have to promote. 
            if ((InvalidKey != _loneEntry.Key) && (key != _loneEntry.Key))
            {
                // Entry already used, move to an ThreeObjectMap 
                return FrugalMapStoreState.ThreeObjectMap;
            }

            Debug.Assert(InvalidKey != key);

            _loneEntry.Key = key;
            _loneEntry.Value = value;

            return FrugalMapStoreState.Success;
        }

        public override void RemoveEntry(int key)
        {
            // Wipe out the info in the only entry if it matches the key. 
            if (key == _loneEntry.Key)
            {
                _loneEntry.Key = InvalidKey;
                _loneEntry.Value = DynamicProperty.UnsetValue;
            }
        }

        public override Object Search(int key)
        {
            if (key == _loneEntry.Key)
            {
                return _loneEntry.Value;
            }
            return DynamicProperty.UnsetValue;
        }

        public override void Sort()
        {
            // Single items are already sorted.
        }

        public override void GetKeyValuePair(int index, out int key, out Object value)
        {
            if (0 == index)
            {
                value = _loneEntry.Value;
                key = _loneEntry.Key;
            }
            else
            {
                value = DynamicProperty.UnsetValue;
                key = InvalidKey;
                throw new ArgumentOutOfRangeException("index");
            }
        }

        public override void Iterate(ArrayList list, FrugalMapIterationCallback callback)
        {
            if (Count == 1)
            {
                callback(list, _loneEntry.Key, _loneEntry.Value);
            }
        }

        public override void Promote(FrugalMapBase newMap)
        {
            if (FrugalMapStoreState.Success != newMap.InsertEntry(_loneEntry.Key, _loneEntry.Value))
            {
                throw TargetMapTooSmall("newMap");
            }
        }

        // Size of this data store
        public override int Count
        {
            get
            {
                if (InvalidKey != _loneEntry.Key)
                    return 1;
                return 0;
            }
        }

        private Entry _loneEntry;
    }

    /// <summary>
    /// A simple class to handle a single object with 3 key/value pairs.  The pairs are stored unsorted 
    /// and uses a linear search.  Perf analysis showed that this yielded better memory locality and
    /// perf than an object and an array. 
    /// </summary> 
    /// <remarks>
    /// This map inserts at the last position.  Any time we add to the map we set _sorted to false. If you need 
    /// to iterate through the map in sorted order you must call Sort before using GetKeyValuePair.
    /// </remarks>
    internal sealed class ThreeObjectMap : FrugalMapBase
    {
        public override FrugalMapStoreState InsertEntry(int key, Object value)
        {
            // Check to see if we are updating an existing entry 
            Debug.Assert(InvalidKey != key);

            // First check if the key matches the key of one of the existing entries.
            // If it does, overwrite the existing value and return success.
            switch (_count)
            {
                case 1:
                    if (_entry0.Key == key)
                    {
                        _entry0.Value = value;
                        return FrugalMapStoreState.Success;
                    }
                    break;

                case 2:
                    if (_entry0.Key == key)
                    {
                        _entry0.Value = value;
                        return FrugalMapStoreState.Success;
                    }
                    if (_entry1.Key == key)
                    {
                        _entry1.Value = value;
                        return FrugalMapStoreState.Success;
                    }
                    break;

                case 3:
                    if (_entry0.Key == key)
                    {
                        _entry0.Value = value;
                        return FrugalMapStoreState.Success;
                    }
                    if (_entry1.Key == key)
                    {
                        _entry1.Value = value;
                        return FrugalMapStoreState.Success;
                    }
                    if (_entry2.Key == key)
                    {
                        _entry2.Value = value;
                        return FrugalMapStoreState.Success;
                    }
                    break;

                default:
                    break;
            }

            // If we got past the above switch, that means this key
            // doesn't exist in the map already so we should add it. 
            // Only add it if we're not at the size limit; otherwise
            // we have to promote. 
            if (Size > _count)
            {
                // Space still available to store the value. Insert 
                // into the entry at _count (the next available slot).
                switch (_count)
                {
                    case 0:
                        _entry0.Key = key;
                        _entry0.Value = value;
                        _sorted = true;
                        break;

                    case 1:
                        _entry1.Key = key;
                        _entry1.Value = value;
                        // We have added an entry to the array, so we may not be sorted any longer 
                        _sorted = false;
                        break;

                    case 2:
                        _entry2.Key = key;
                        _entry2.Value = value;
                        // We have added an entry to the array, so we may not be sorted any longer
                        _sorted = false;
                        break;
                }
                ++_count;

                return FrugalMapStoreState.Success;
            }

            // Array is full, move to a SixObjectMap
            return FrugalMapStoreState.SixObjectMap;
        }

        public override void RemoveEntry(int key)
        {
            // If the key matches an existing entry, wipe out the last
            // entry and move all the other entries up.  Because we only
            // have three entries we can just unravel all the cases.
            switch (_count)
            {
                case 1:
                    if (_entry0.Key == key)
                    {
                        _entry0.Key = InvalidKey;
                        _entry0.Value = DynamicProperty.UnsetValue;
                        --_count;
                        return;
                    }
                    break;

                case 2:
                    if (_entry0.Key == key)
                    {
                        _entry0 = _entry1;
                        _entry1.Key = InvalidKey;
                        _entry1.Value = DynamicProperty.UnsetValue;
                        --_count;
                        break;
                    }
                    if (_entry1.Key == key)
                    {
                        _entry1.Key = InvalidKey;
                        _entry1.Value = DynamicProperty.UnsetValue;
                        --_count;
                    }
                    break;

                case 3:
                    if (_entry0.Key == key)
                    {
                        _entry0 = _entry1;
                        _entry1 = _entry2;
                        _entry2.Key = InvalidKey;
                        _entry2.Value = DynamicProperty.UnsetValue;
                        --_count;
                        break;
                    }
                    if (_entry1.Key == key)
                    {
                        _entry1 = _entry2;
                        _entry2.Key = InvalidKey;
                        _entry2.Value = DynamicProperty.UnsetValue;
                        --_count;
                        break;
                    }
                    if (_entry2.Key == key)
                    {
                        _entry2.Key = InvalidKey;
                        _entry2.Value = DynamicProperty.UnsetValue;
                        --_count;
                        break;
                    }
                    break;

                default:
                    break;
            }
        }

        public override Object Search(int key)
        {
            Debug.Assert(InvalidKey != key);
            if (_count > 0)
            {
                if (_entry0.Key == key)
                {
                    return _entry0.Value;
                }
                if (_count > 1)
                {
                    if (_entry1.Key == key)
                    {
                        return _entry1.Value;
                    }
                    if ((_count > 2) && (_entry2.Key == key))
                    {
                        return _entry2.Value;
                    }
                }
            }
            return DynamicProperty.UnsetValue;
        }

        public override void Sort()
        {
            // If we're unsorted and we have entries to sort, do a simple
            // sort.  Sort the pairs (0,1), (1,2) and then (0,1) again.
            if ((false == _sorted) && (_count > 1))
            {
                Entry temp;
                if (_entry0.Key > _entry1.Key)
                {
                    temp = _entry0;
                    _entry0 = _entry1;
                    _entry1 = temp;
                }
                if (_count > 2)
                {
                    if (_entry1.Key > _entry2.Key)
                    {
                        temp = _entry1;
                        _entry1 = _entry2;
                        _entry2 = temp;

                        if (_entry0.Key > _entry1.Key)
                        {
                            temp = _entry0;
                            _entry0 = _entry1;
                            _entry1 = temp;
                        }
                    }
                }
                _sorted = true;
            }
        }

        public override void GetKeyValuePair(int index, out int key, out Object value)
        {
            if (index < _count)
            {
                switch (index)
                {
                    case 0:
                        key = _entry0.Key;
                        value = _entry0.Value;
                        break;

                    case 1:
                        key = _entry1.Key;
                        value = _entry1.Value;
                        break;

                    case 2:
                        key = _entry2.Key;
                        value = _entry2.Value;
                        break;

                    default:
                        key = InvalidKey;
                        value = DynamicProperty.UnsetValue;
                        break;
                }
            }
            else
            {
                key = InvalidKey;
                value = DynamicProperty.UnsetValue;
                throw new ArgumentOutOfRangeException("index");
            }
        }

        public override void Iterate(ArrayList list, FrugalMapIterationCallback callback)
        {
            if (_count > 0)
            {
                if (_count >= 1)
                {
                    callback(list, _entry0.Key, _entry0.Value);
                }
                if (_count >= 2)
                {
                    callback(list, _entry1.Key, _entry1.Value);
                }
                if (_count == 3)
                {
                    callback(list, _entry2.Key, _entry2.Value);
                }
            }
        }

        public override void Promote(FrugalMapBase newMap)
        {
            if (FrugalMapStoreState.Success != newMap.InsertEntry(_entry0.Key, _entry0.Value))
            {
                // newMap is smaller than previous map 
                throw new ArgumentException(
                    "FrugalMap cannot be promoted because the target map is too small.",
                    "newMap");
            }
            if (FrugalMapStoreState.Success != newMap.InsertEntry(_entry1.Key, _entry1.Value))
            {
                throw new ArgumentException(
                    "FrugalMap cannot be promoted because the target map is too small.",
                    "newMap");
            }
            if (FrugalMapStoreState.Success != newMap.InsertEntry(_entry2.Key, _entry2.Value))
            {
                throw new ArgumentException(
                    "FrugalMap cannot be promoted because the target map is too small.",
                    "newMap");
            }
        }

        // Size of this data store 
        public override int Count => _count;

        private const int Size = 3;

        // The number of items in the map.
        private UInt16 _count;

        private bool _sorted;
        private Entry _entry0;
        private Entry _entry1;
        private Entry _entry2;
    }

    /// <summary>
    /// A simple class to handle a single object with 6 key/value pairs.  The pairs are stored unsorted 
    /// and uses a linear search.  Perf analysis showed that this yielded better memory locality and
    /// perf than an object and an array.
    /// </summary>
    /// <remarks> 
    /// This map inserts at the last position.  Any time we add to the map we set _sorted to false. If you need
    /// to iterate through the map in sorted order you must call Sort before using GetKeyValuePair. 
    /// </remarks> 
    internal sealed class SixObjectMap : FrugalMapBase
    {
        public override FrugalMapStoreState InsertEntry(int key, Object value)
        {
            // Check to see if we are updating an existing entry
            Debug.Assert(InvalidKey != key);

            // First check if the key matches the key of one of the existing entries. 
            // If it does, overwrite the existing value and return success. 
            if (_count > 0)
            {
                if (_entry0.Key == key)
                {
                    _entry0.Value = value;
                    return FrugalMapStoreState.Success;
                }
                if (_count > 1)
                {
                    if (_entry1.Key == key)
                    {
                        _entry1.Value = value;
                        return FrugalMapStoreState.Success;
                    }
                    if (_count > 2)
                    {
                        if (_entry2.Key == key)
                        {
                            _entry2.Value = value;
                            return FrugalMapStoreState.Success;
                        }
                        if (_count > 3)
                        {
                            if (_entry3.Key == key)
                            {
                                _entry3.Value = value;
                                return FrugalMapStoreState.Success;
                            }
                            if (_count > 4)
                            {
                                if (_entry4.Key == key)
                                {
                                    _entry4.Value = value;
                                    return FrugalMapStoreState.Success;
                                }
                                if ((_count > 5) && (_entry5.Key == key))
                                {
                                    _entry5.Value = value;
                                    return FrugalMapStoreState.Success;
                                }
                            }
                        }
                    }
                }
            }

            // If we got past the above switch, that means this key 
            // doesn't exist in the map already so we should add it.
            // Only add it if we're not at the size limit; otherwise
            // we have to promote.
            if (Size > _count)
            {
                // We are adding an entry to the array, so we may not be sorted any longer 
                _sorted = false;

                // Space still available to store the value. Insert 
                // into the entry at _count (the next available slot).
                switch (_count)
                {
                    case 0:
                        _entry0.Key = key;
                        _entry0.Value = value;

                        // Single entries are always sorted
                        _sorted = true;
                        break;

                    case 1:
                        _entry1.Key = key;
                        _entry1.Value = value;
                        break;

                    case 2:
                        _entry2.Key = key;
                        _entry2.Value = value;
                        break;

                    case 3:
                        _entry3.Key = key;
                        _entry3.Value = value;
                        break;

                    case 4:
                        _entry4.Key = key;
                        _entry4.Value = value;
                        break;

                    case 5:
                        _entry5.Key = key;
                        _entry5.Value = value;
                        break;
                }
                ++_count;

                return FrugalMapStoreState.Success;
            }

            // Array is full, move to a Array 
            return FrugalMapStoreState.Array;
        }

        public override void RemoveEntry(int key)
        {
            // If the key matches an existing entry, wipe out the last
            // entry and move all the other entries up.  Because we only 
            // have three entries we can just unravel all the cases. 
            switch (_count)
            {
                case 1:
                    if (_entry0.Key == key)
                    {
                        _entry0.Key = InvalidKey;
                        _entry0.Value = DynamicProperty.UnsetValue;
                        --_count;
                        return;
                    }
                    break;

                case 2:
                    if (_entry0.Key == key)
                    {
                        _entry0 = _entry1;
                        _entry1.Key = InvalidKey;
                        _entry1.Value = DynamicProperty.UnsetValue;
                        --_count;
                        break;
                    }
                    if (_entry1.Key == key)
                    {
                        _entry1.Key = InvalidKey;
                        _entry1.Value = DynamicProperty.UnsetValue;
                        --_count;
                    }
                    break;

                case 3:
                    if (_entry0.Key == key)
                    {
                        _entry0 = _entry1;
                        _entry1 = _entry2;
                        _entry2.Key = InvalidKey;
                        _entry2.Value = DynamicProperty.UnsetValue;
                        --_count;
                        break;
                    }
                    if (_entry1.Key == key)
                    {
                        _entry1 = _entry2;
                        _entry2.Key = InvalidKey;
                        _entry2.Value = DynamicProperty.UnsetValue;
                        --_count;
                        break;
                    }
                    if (_entry2.Key == key)
                    {
                        _entry2.Key = InvalidKey;
                        _entry2.Value = DynamicProperty.UnsetValue;
                        --_count;
                        break;
                    }
                    break;

                case 4:
                    if (_entry0.Key == key)
                    {
                        _entry0 = _entry1;
                        _entry1 = _entry2;
                        _entry2 = _entry3;
                        _entry3.Key = InvalidKey;
                        _entry3.Value = DynamicProperty.UnsetValue;
                        --_count;
                        break;
                    }
                    if (_entry1.Key == key)
                    {
                        _entry1 = _entry2;
                        _entry2 = _entry3;
                        _entry3.Key = InvalidKey;
                        _entry3.Value = DynamicProperty.UnsetValue;
                        --_count;
                        break;
                    }
                    if (_entry2.Key == key)
                    {
                        _entry2 = _entry3;
                        _entry3.Key = InvalidKey;
                        _entry3.Value = DynamicProperty.UnsetValue;
                        --_count;
                        break;
                    }
                    if (_entry3.Key == key)
                    {
                        _entry3.Key = InvalidKey;
                        _entry3.Value = DynamicProperty.UnsetValue;
                        --_count;
                        break;
                    }
                    break;

                case 5:
                    if (_entry0.Key == key)
                    {
                        _entry0 = _entry1;
                        _entry1 = _entry2;
                        _entry2 = _entry3;
                        _entry3 = _entry4;
                        _entry4.Key = InvalidKey;
                        _entry4.Value = DynamicProperty.UnsetValue;
                        --_count;
                        break;
                    }
                    if (_entry1.Key == key)
                    {
                        _entry1 = _entry2;
                        _entry2 = _entry3;
                        _entry3 = _entry4;
                        _entry4.Key = InvalidKey;
                        _entry4.Value = DynamicProperty.UnsetValue;
                        --_count;
                        break;
                    }
                    if (_entry2.Key == key)
                    {
                        _entry2 = _entry3;
                        _entry3 = _entry4;
                        _entry4.Key = InvalidKey;
                        _entry4.Value = DynamicProperty.UnsetValue;
                        --_count;
                        break;
                    }
                    if (_entry3.Key == key)
                    {
                        _entry3 = _entry4;
                        _entry4.Key = InvalidKey;
                        _entry4.Value = DynamicProperty.UnsetValue;
                        --_count;
                        break;
                    }
                    if (_entry4.Key == key)
                    {
                        _entry4.Key = InvalidKey;
                        _entry4.Value = DynamicProperty.UnsetValue;
                        --_count;
                        break;
                    }
                    break;

                case 6:
                    if (_entry0.Key == key)
                    {
                        _entry0 = _entry1;
                        _entry1 = _entry2;
                        _entry2 = _entry3;
                        _entry3 = _entry4;
                        _entry4 = _entry5;
                        _entry5.Key = InvalidKey;
                        _entry5.Value = DynamicProperty.UnsetValue;
                        --_count;
                        break;
                    }
                    if (_entry1.Key == key)
                    {
                        _entry1 = _entry2;
                        _entry2 = _entry3;
                        _entry3 = _entry4;
                        _entry4 = _entry5;
                        _entry5.Key = InvalidKey;
                        _entry5.Value = DynamicProperty.UnsetValue;
                        --_count;
                        break;
                    }
                    if (_entry2.Key == key)
                    {
                        _entry2 = _entry3;
                        _entry3 = _entry4;
                        _entry4 = _entry5;
                        _entry5.Key = InvalidKey;
                        _entry5.Value = DynamicProperty.UnsetValue;
                        --_count;
                        break;
                    }
                    if (_entry3.Key == key)
                    {
                        _entry3 = _entry4;
                        _entry4 = _entry5;
                        _entry5.Key = InvalidKey;
                        _entry5.Value = DynamicProperty.UnsetValue;
                        --_count;
                        break;
                    }
                    if (_entry4.Key == key)
                    {
                        _entry4 = _entry5;
                        _entry5.Key = InvalidKey;
                        _entry5.Value = DynamicProperty.UnsetValue;
                        --_count;
                        break;
                    }
                    if (_entry5.Key == key)
                    {
                        _entry5.Key = InvalidKey;
                        _entry5.Value = DynamicProperty.UnsetValue;
                        --_count;
                        break;
                    }
                    break;

                default:
                    break;
            }
        }

        public override Object Search(int key)
        {
            Debug.Assert(InvalidKey != key);
            if (_count > 0)
            {
                if (_entry0.Key == key)
                {
                    return _entry0.Value;
                }
                if (_count > 1)
                {
                    if (_entry1.Key == key)
                    {
                        return _entry1.Value;
                    }
                    if (_count > 2)
                    {
                        if (_entry2.Key == key)
                        {
                            return _entry2.Value;
                        }
                        if (_count > 3)
                        {
                            if (_entry3.Key == key)
                            {
                                return _entry3.Value;
                            }
                            if (_count > 4)
                            {
                                if (_entry4.Key == key)
                                {
                                    return _entry4.Value;
                                }
                                if ((_count > 5) && (_entry5.Key == key))
                                {
                                    return _entry5.Value;
                                }
                            }
                        }
                    }
                }
            }
            return DynamicProperty.UnsetValue;
        }

        public override void Sort()
        {
            // If we're unsorted and we have entries to sort, do a simple 
            // bubble sort. Sort the pairs, 0..5, and then again until we no
            // longer do any swapping.
            if ((false == _sorted) && (_count > 1))
            {
                bool swapped;

                do
                {
                    swapped = false;

                    Entry temp;
                    if (_entry0.Key > _entry1.Key)
                    {
                        temp = _entry0;
                        _entry0 = _entry1;
                        _entry1 = temp;
                        swapped = true;
                    }
                    if (_count > 2)
                    {
                        if (_entry1.Key > _entry2.Key)
                        {
                            temp = _entry1;
                            _entry1 = _entry2;
                            _entry2 = temp;
                            swapped = true;
                        }
                        if (_count > 3)
                        {
                            if (_entry2.Key > _entry3.Key)
                            {
                                temp = _entry2;
                                _entry2 = _entry3;
                                _entry3 = temp;
                                swapped = true;
                            }
                            if (_count > 4)
                            {
                                if (_entry3.Key > _entry4.Key)
                                {
                                    temp = _entry3;
                                    _entry3 = _entry4;
                                    _entry4 = temp;
                                    swapped = true;
                                }
                                if (_count > 5)
                                {
                                    if (_entry4.Key > _entry5.Key)
                                    {
                                        temp = _entry4;
                                        _entry4 = _entry5;
                                        _entry5 = temp;
                                        swapped = true;
                                    }
                                }
                            }
                        }
                    }
                }
                while (swapped);
                _sorted = true;
            }
        }

        public override void GetKeyValuePair(int index, out int key, out Object value)
        {
            if (index < _count)
            {
                switch (index)
                {
                    case 0:
                        key = _entry0.Key;
                        value = _entry0.Value;
                        break;

                    case 1:
                        key = _entry1.Key;
                        value = _entry1.Value;
                        break;

                    case 2:
                        key = _entry2.Key;
                        value = _entry2.Value;
                        break;

                    case 3:
                        key = _entry3.Key;
                        value = _entry3.Value;
                        break;

                    case 4:
                        key = _entry4.Key;
                        value = _entry4.Value;
                        break;

                    case 5:
                        key = _entry5.Key;
                        value = _entry5.Value;
                        break;

                    default:
                        key = InvalidKey;
                        value = DynamicProperty.UnsetValue;
                        break;
                }
            }
            else
            {
                key = InvalidKey;
                value = DynamicProperty.UnsetValue;
                throw new ArgumentOutOfRangeException("index");
            }
        }

        public override void Iterate(ArrayList list, FrugalMapIterationCallback callback)
        {
            if (_count > 0)
            {
                if (_count >= 1)
                {
                    callback(list, _entry0.Key, _entry0.Value);
                }
                if (_count >= 2)
                {
                    callback(list, _entry1.Key, _entry1.Value);
                }
                if (_count >= 3)
                {
                    callback(list, _entry2.Key, _entry2.Value);
                }
                if (_count >= 4)
                {
                    callback(list, _entry3.Key, _entry3.Value);
                }
                if (_count >= 5)
                {
                    callback(list, _entry4.Key, _entry4.Value);
                }
                if (_count == 6)
                {
                    callback(list, _entry5.Key, _entry5.Value);
                }
            }
        }

        public override void Promote(FrugalMapBase newMap)
        {
            if (FrugalMapStoreState.Success != newMap.InsertEntry(_entry0.Key, _entry0.Value))
                throw TargetMapTooSmall("newMap");
            if (FrugalMapStoreState.Success != newMap.InsertEntry(_entry1.Key, _entry1.Value))
                throw TargetMapTooSmall("newMap");
            if (FrugalMapStoreState.Success != newMap.InsertEntry(_entry2.Key, _entry2.Value))
                throw TargetMapTooSmall("newMap");
            if (FrugalMapStoreState.Success != newMap.InsertEntry(_entry3.Key, _entry3.Value))
                throw TargetMapTooSmall("newMap");
            if (FrugalMapStoreState.Success != newMap.InsertEntry(_entry4.Key, _entry4.Value))
                throw TargetMapTooSmall("newMap");
            if (FrugalMapStoreState.Success != newMap.InsertEntry(_entry5.Key, _entry5.Value))
                throw TargetMapTooSmall("newMap");
        }

        // Size of this data store 
        public override int Count => _count;

        private const int Size = 6;

        // The number of items in the map.
        private UInt16 _count;

        private bool _sorted;
        private Entry _entry0;
        private Entry _entry1;
        private Entry _entry2;
        private Entry _entry3;
        private Entry _entry4;
        private Entry _entry5;
    }

    /// <summary>
    /// A simple class to handle an array of between 6 and 12 key/value pairs.  It is unsorted 
    /// and uses a linear search.  Perf analysis showed that this was the optimal size for both 
    /// memory and perf.  The values may need to be adjusted as the CLR and Avalon evolve.
    /// </summary> 
    internal sealed class ArrayObjectMap : FrugalMapBase
    {
        public override FrugalMapStoreState InsertEntry(int key, Object value)
        {
            // Check to see if we are updating an existing entry
            for (int index = 0; index < _count; ++index)
            {
                Debug.Assert(InvalidKey != key);

                if (_entries[index].Key == key)
                {
                    _entries[index].Value = value;
                    return FrugalMapStoreState.Success;
                }
            }

            // New key/value pair
            if (MaxSize > _count)
            {
                // Space still available to store the value
                if (null != _entries)
                {
                    // We are adding an entry to the array, so we may not be sorted any longer
                    _sorted = false;

                    if (_entries.Length > _count)
                    {
                        // Have empty entries, just set the first available
                    }
                    else
                    {
                        Entry[] destEntries = new Entry[_entries.Length + Growth];

                        // Copy old array 
                        Array.Copy(_entries, 0, destEntries, 0, _entries.Length);
                        _entries = destEntries;
                    }
                }
                else
                {
                    _entries = new Entry[MinSize];

                    // No entries, must be sorted 
                    _sorted = true;
                }

                // Stuff in the new key/value pair
                _entries[_count].Key = key;
                _entries[_count].Value = value;

                // Bump the count for the entry just added. 
                ++_count;

                return FrugalMapStoreState.Success;
            }

            // Array is full, move to a SortedArray 
            return FrugalMapStoreState.SortedArray;
        }

        public override void RemoveEntry(int key)
        {
            for (int index = 0; index < _count; ++index)
            {
                if (_entries[index].Key == key)
                {
                    // Shift entries down 
                    int numToCopy = (_count - index) - 1;
                    if (numToCopy > 0)
                    {
                        Array.Copy(_entries, index + 1, _entries, index, numToCopy);
                    }

                    // Wipe out the last entry 
                    _entries[_count - 1].Key = InvalidKey;
                    _entries[_count - 1].Value = DynamicProperty.UnsetValue;
                    --_count;
                    break;
                }
            }
        }

        public override Object Search(int key)
        {
            for (int index = 0; index < _count; ++index)
            {
                if (key == _entries[index].Key)
                {
                    return _entries[index].Value;
                }
            }
            return DynamicProperty.UnsetValue;
        }

        public override void Sort()
        {
            if ((false == _sorted) && (_count > 1))
            {
                QSort(0, (_count - 1));
                _sorted = true;
            }
        }

        public override void GetKeyValuePair(int index, out int key, out Object value)
        {
            if (index < _count)
            {
                value = _entries[index].Value;
                key = _entries[index].Key;
            }
            else
            {
                value = DynamicProperty.UnsetValue;
                key = InvalidKey;
                throw new ArgumentOutOfRangeException("index");
            }
        }

        public override void Iterate(ArrayList list, FrugalMapIterationCallback callback)
        {
            if (_count > 0)
            {
                for (int i = 0; i < _count; i++)
                {
                    callback(list, _entries[i].Key, _entries[i].Value);
                }
            }
        }

        public override void Promote(FrugalMapBase newMap)
        {
            for (int index = 0; index < _entries.Length; ++index)
            {
                if (FrugalMapStoreState.Success == newMap.InsertEntry(_entries[index].Key, _entries[index].Value))
                    continue;
                throw TargetMapTooSmall("newMap");
            }
        }

        // Size of this data store
        public override int Count => _count;

        // Compare two Entry nodes in the _entries array
        private int Compare(int left, int right)
        {
            return (_entries[left].Key - _entries[right].Key);
        }

        // Partition the _entries array for QuickSort 
        private int Partition(int left, int right)
        {
            int pivot = right;
            int i = left - 1;
            int j = right;
            Entry temp;

            for (; ; )
            {
                while (Compare(++i, pivot) < 0)
                    continue;

                while (Compare(pivot, --j) < 0)
                {
                    if (j == left)
                        break;
                }

                if (i >= j)
                    break;

                temp = _entries[j];
                _entries[j] = _entries[i];
                _entries[i] = temp;
            }

            temp = _entries[right];
            _entries[right] = _entries[i];
            _entries[i] = temp;

            return i;
        }

        // Sort the _entries array using an index based QuickSort
        private void QSort(int left, int right)
        {
            if (left < right)
            {
                int pivot = Partition(left, right);
                QSort(left, pivot - 1);
                QSort(pivot + 1, right);
            }
        }

        // MinSize and Growth chosen to minimize memory footprint
        private const int MinSize = 9;
        private const int MaxSize = 15;
        private const int Growth = 3;

        // The number of items in the map.
        private UInt16 _count;

        private bool _sorted;
        private Entry[] _entries;
    }

    // A sorted array of key/value pairs. A binary search is used to minimize the cost of insert/search.

    internal sealed class SortedObjectMap : FrugalMapBase
    {
        public override FrugalMapStoreState InsertEntry(int key, Object value)
        {
            bool found;

            Debug.Assert(InvalidKey != key);

            // Check to see if we are updating an existing entry 
            int index = FindInsertIndex(key, out found);
            if (found)
            {
                _entries[index].Value = value;
                return FrugalMapStoreState.Success;
            }
            // New key/value pair 
            if (MaxSize > _count)
            {
                // Less than the maximum array size
                if (null != _entries)
                {
                    if (_entries.Length > _count)
                    {
                        // Have empty entries, just set the first available
                    }
                    else
                    {
                        Entry[] destEntries = new Entry[_entries.Length + Growth];

                        // Copy old array
                        Array.Copy(_entries, 0, destEntries, 0, _entries.Length);
                        _entries = destEntries;
                    }
                }
                else
                {
                    _entries = new Entry[MinSize];
                }

                // Inserting into the middle of the existing entries? 
                if (index < _count)
                {
                    // Move higher valued keys to make room for the new key 
                    Array.Copy(_entries, index, _entries, index + 1, (_count - index));
                }
                else
                {
                    _lastKey = key;
                }

                // Stuff in the new key/value pair
                _entries[index].Key = key;
                _entries[index].Value = value;
                ++_count;
                return FrugalMapStoreState.Success;
            }

            // SortedArray is full, move to a hashtable 
            return FrugalMapStoreState.Hashtable;
        }

        public override void RemoveEntry(int key)
        {
            bool found;

            Debug.Assert(InvalidKey != key);

            int index = FindInsertIndex(key, out found);

            if (found)
            {
                // Shift entries down
                int numToCopy = (_count - index) - 1;
                if (numToCopy > 0)
                {
                    Array.Copy(_entries, index + 1, _entries, index, numToCopy);
                }
                else
                {
                    // If we're not copying anything, then it means we are 
                    //  going to remove the last entry.  Update _lastKey so
                    //  that it reflects the key of the new "last entry" 
                    if (_count > 1)
                    {
                        // Next-to-last entry will be the new last entry 
                        _lastKey = _entries[_count - 2].Key;
                    }
                    else
                    {
                        // Unless there isn't a next-to-last entry, in which
                        //  case the key is reset to INVALIDKEY. 
                        _lastKey = InvalidKey;
                    }
                }

                // Wipe out the last entry
                _entries[_count - 1].Key = InvalidKey;
                _entries[_count - 1].Value = DynamicProperty.UnsetValue;

                --_count;
            }
        }

        public override Object Search(int key)
        {
            bool found;

            int index = FindInsertIndex(key, out found);
            if (found)
            {
                return _entries[index].Value;
            }
            return DynamicProperty.UnsetValue;
        }

        public override void Sort()
        {
            // Always sorted. 
        }

        public override void GetKeyValuePair(int index, out int key, out Object value)
        {
            if (index < _count)
            {
                value = _entries[index].Value;
                key = _entries[index].Key;
            }
            else
            {
                value = DynamicProperty.UnsetValue;
                key = InvalidKey;
                throw new ArgumentOutOfRangeException("index");
            }
        }

        public override void Iterate(ArrayList list, FrugalMapIterationCallback callback)
        {
            if (_count > 0)
            {
                for (int i = 0; i < _count; i++)
                {
                    callback(list, _entries[i].Key, _entries[i].Value);
                }
            }
        }

        public override void Promote(FrugalMapBase newMap)
        {
            for (int index = 0; index < _entries.Length; ++index)
            {
                if (FrugalMapStoreState.Success == newMap.InsertEntry(_entries[index].Key, _entries[index].Value))
                    continue;
                throw TargetMapTooSmall("newMap");
            }
        }

        private int FindInsertIndex(int key, out bool found)
        {
            int iLo = 0;

            // Only do the binary search if there is a chance of finding the key 
            // This also speeds insertion because we tend to insert at the end.
            if ((_count > 0) && (key <= _lastKey))
            {
                // The array index used for insertion is somewhere between 0
                //  and _count-1 inclusive
                int iHi = _count - 1;

                // Do a binary search to find the insertion point 
                do
                {
                    int iPv = (iHi + iLo) / 2;
                    if (key <= _entries[iPv].Key)
                    {
                        iHi = iPv;
                    }
                    else
                    {
                        iLo = iPv + 1;
                    }
                }
                while (iLo < iHi);
                found = (key == _entries[iLo].Key);
            }
            else
            {
                // Insert point is at the end 
                iLo = _count;
                found = false;
            }
            return iLo;
        }

        public override int Count => _count;

        // MinSize chosen to be larger than MaxSize of the ArrayObjectMap with some extra space for new values
        // The MaxSize and Growth are chosen to minimize memory usage as we grow the array 
        private const int MinSize = 16;
        private const int MaxSize = 128;
        private const int Growth = 8;

        // The number of items in the map. 
        private int _count;

        private int _lastKey = InvalidKey;
        private Entry[] _entries;
    }

    internal sealed class HashObjectMap : FrugalMapBase
    {
        public override FrugalMapStoreState InsertEntry(int key, Object value)
        {
            Debug.Assert(InvalidKey != key);

            if (null != _entries)
            {
                // This is done because forward branches 
                // default prediction is not to be taken 
                // making this a CPU win because insert
                // is a common operation. 
            }
            else
            {
                _entries = new Hashtable(MinSize);
            }

            _entries[key] = ((value != NullValue) && (value != null)) ? value : NullValue;
            return FrugalMapStoreState.Success;
        }

        public override void RemoveEntry(int key)
        {
            _entries.Remove(key);
        }

        public override Object Search(int key)
        {
            object value = _entries[key];

            return ((value != NullValue) && (value != null)) ? value : DynamicProperty.UnsetValue;
        }

        public override void Sort()
        {
            // Always sorted. 
        }

        public override void GetKeyValuePair(int index, out int key, out Object value)
        {
            if (index < _entries.Count)
            {
                IDictionaryEnumerator enumerator = _entries.GetEnumerator();

                // Move to first valid value 
                enumerator.MoveNext();

                for (int i = 0; i < index; ++i)
                    enumerator.MoveNext();

                key = (int)enumerator.Key;

                if (enumerator.Value != NullValue &&
                    enumerator.Value != null)
                {
                    value = enumerator.Value;
                }
                else
                {
                    value = DynamicProperty.UnsetValue;
                }
            }
            else
            {
                value = DynamicProperty.UnsetValue;
                key = InvalidKey;

                throw new ArgumentOutOfRangeException("index");
            }
        }

        public override void Iterate(ArrayList list, FrugalMapIterationCallback callback)
        {
            IDictionaryEnumerator enumerator = _entries.GetEnumerator();

            while (enumerator.MoveNext())
            {
                int key = (int)enumerator.Key;
                object value;

                if (enumerator.Value != NullValue &&
                    enumerator.Value != null)
                {
                    value = enumerator.Value;
                }
                else
                {
                    value = DynamicProperty.UnsetValue;
                }

                callback(list, key, value);
            }
        }

        public override void Promote(FrugalMapBase newMap)
        {
            throw new InvalidOperationException("FrugalMap cannot promote beyond Hashtable.");
        }

        // Size of this data store
        public override int Count => _entries.Count;

        // 163 is chosen because it is the first prime larger than 128, the MAXSIZE of SortedObjectMap
        internal const int MinSize = 163;

        // Hashtable will return null from its indexer if the key is not
        // found OR if the value is null.  To distinguish between these 
        // two cases we insert NullValue instead of null.
        private static readonly object NullValue = new object();

        private Hashtable _entries;
    }

    internal struct FrugalMap
    {
        public object this[int key]
        {
            get
            {
                // If no entry, EntityField.UnsetValue is returned
                if (_mapStore != null)
                    return _mapStore.Search(key);

                return DynamicProperty.UnsetValue;
            }

            set
            {
                if (value != DynamicProperty.UnsetValue)
                {
                    // If not unset value, ensure write success
                    if (_mapStore != null)
                    {
                        // This is done because forward branches
                        // default prediction is not to be taken
                        // making this a CPU win because set is 
                        // a common operation.
                    }
                    else
                    {
                        _mapStore = new SingleObjectMap();
                    }

                    FrugalMapStoreState myState = _mapStore.InsertEntry(key, value);
                    if (FrugalMapStoreState.Success == myState)
                        return;

                    // Need to move to a more complex storage
                    FrugalMapBase newStore;

                    switch (myState)
                    {
                        case FrugalMapStoreState.ThreeObjectMap:
                            newStore = new ThreeObjectMap();
                            break;

                        case FrugalMapStoreState.SixObjectMap:
                            newStore = new SixObjectMap();
                            break;

                        case FrugalMapStoreState.Array:
                            newStore = new ArrayObjectMap();
                            break;

                        case FrugalMapStoreState.SortedArray:
                            newStore = new SortedObjectMap();
                            break;

                        case FrugalMapStoreState.Hashtable:
                            newStore = new HashObjectMap();
                            break;

                        default:
                            throw new InvalidOperationException("FrugalMap cannot promote beyond Hashtable.");
                    }

                    // Extract the values from the old store and insert them into the new store
                    _mapStore.Promote(newStore);

                    // Insert the new value
                    _mapStore = newStore;
                    _mapStore.InsertEntry(key, value);
                }
                else
                {
                    // EntityField.UnsetValue means remove the value
                    if (_mapStore != null)
                    {
                        _mapStore.RemoveEntry(key);

                        if (_mapStore.Count == 0)
                        {
                            // Map Store is now empty ... throw it away 
                            _mapStore = null;
                        }
                    }
                }
            }
        }

        public void Sort()
        {
            if (_mapStore == null)
                return;

            _mapStore.Sort();
        }

        public void GetKeyValuePair(int index, out int key, out Object value)
        {
            if (_mapStore == null)
                throw new ArgumentOutOfRangeException("index");

            _mapStore.GetKeyValuePair(index, out key, out value);
        }

        public void Iterate(ArrayList list, FrugalMapIterationCallback callback)
        {
            if (callback == null)
                throw new ArgumentNullException("callback");

            if (list == null)
                throw new ArgumentNullException("list");

            if (_mapStore != null)
                _mapStore.Iterate(list, callback);
        }

        public int Count
        {
            get
            {
                if (_mapStore != null)
                    return _mapStore.Count;

                return 0;
            }
        }

        private volatile FrugalMapBase _mapStore;
    }

    // A sorted array of key/value pairs. A binary search is used to minimize the cost of insert/search.

    internal sealed class LargeSortedObjectMap : FrugalMapBase
    {
        public override FrugalMapStoreState InsertEntry(int key, Object value)
        {
            bool found;

            Debug.Assert(InvalidKey != key);

            // Check to see if we are updating an existing entry
            int index = FindInsertIndex(key, out found);
            if (found)
            {
                _entries[index].Value = value;
                return FrugalMapStoreState.Success;
            }
            // New key/value pair
            if (null != _entries)
            {
                if (_entries.Length > _count)
                {
                    // Have empty entries, just set the first available
                }
                else
                {
                    int size = _entries.Length;
                    Entry[] destEntries = new Entry[size + (size >> 1)];

                    // Copy old array 
                    Array.Copy(_entries, 0, destEntries, 0, _entries.Length);
                    _entries = destEntries;
                }
            }
            else
            {
                _entries = new Entry[MinSize];
            }

            // Inserting into the middle of the existing entries? 
            if (index < _count)
            {
                // Move higher valued keys to make room for the new key
                Array.Copy(_entries, index, _entries, index + 1, (_count - index));
            }
            else
            {
                _lastKey = key;
            }

            // Stuff in the new key/value pair 
            _entries[index].Key = key;
            _entries[index].Value = value;
            ++_count;
            return FrugalMapStoreState.Success;
        }

        public override void RemoveEntry(int key)
        {
            bool found;

            Debug.Assert(InvalidKey != key);

            int index = FindInsertIndex(key, out found);

            if (found)
            {
                // Shift entries down 
                int numToCopy = (_count - index) - 1;
                if (numToCopy > 0)
                {
                    Array.Copy(_entries, index + 1, _entries, index, numToCopy);
                }
                else
                {
                    // If we're not copying anything, then it means we are
                    //  going to remove the last entry.  Update _lastKey so 
                    //  that it reflects the key of the new "last entry"
                    if (_count > 1)
                    {
                        // Next-to-last entry will be the new last entry 
                        _lastKey = _entries[_count - 2].Key;
                    }
                    else
                    {
                        // Unless there isn't a next-to-last entry, in which 
                        //  case the key is reset to INVALIDKEY.
                        _lastKey = InvalidKey;
                    }
                }

                // Wipe out the last entry 
                _entries[_count - 1].Key = InvalidKey;
                _entries[_count - 1].Value = DynamicProperty.UnsetValue;

                --_count;
            }
        }

        public override Object Search(int key)
        {
            bool found;

            int index = FindInsertIndex(key, out found);
            if (found)
            {
                return _entries[index].Value;
            }
            return DynamicProperty.UnsetValue;
        }

        public override void Sort()
        {
            // Always sorted.
        }

        public override void GetKeyValuePair(int index, out int key, out Object value)
        {
            if (index < _count)
            {
                value = _entries[index].Value;
                key = _entries[index].Key;
            }
            else
            {
                value = DynamicProperty.UnsetValue;
                key = InvalidKey;
                throw new ArgumentOutOfRangeException("index");
            }
        }

        public override void Iterate(ArrayList list, FrugalMapIterationCallback callback)
        {
            if (_count > 0)
            {
                for (int i = 0; i < _count; i++)
                {
                    callback(list, _entries[i].Key, _entries[i].Value);
                }
            }
        }

        public override void Promote(FrugalMapBase newMap)
        {
            for (int index = 0; index < _entries.Length; ++index)
            {
                if (FrugalMapStoreState.Success == newMap.InsertEntry(_entries[index].Key, _entries[index].Value))
                    continue;
                throw TargetMapTooSmall("newMap");
            }
        }

        private int FindInsertIndex(int key, out bool found)
        {
            int iLo = 0;

            // Only do the binary search if there is a chance of finding the key
            // This also speeds insertion because we tend to insert at the end.
            if ((_count > 0) && (key <= _lastKey))
            {
                // The array index used for insertion is somewhere between 0 
                //  and _count-1 inclusive 
                int iHi = _count - 1;

                // Do a binary search to find the insertion point
                do
                {
                    int iPv = (iHi + iLo) / 2;
                    if (key <= _entries[iPv].Key)
                    {
                        iHi = iPv;
                    }
                    else
                    {
                        iLo = iPv + 1;
                    }
                }
                while (iLo < iHi);
                found = (key == _entries[iLo].Key);
            }
            else
            {
                // Insert point is at the end
                iLo = _count;
                found = false;
            }
            return iLo;
        }

        public override int Count => _count;

        // MinSize chosen to be small, growth rate of 1.5 is slow at small sizes, but increasingly agressive as 
        // the array grows
        private const int MinSize = 2;

        // The number of items in the map.
        private int _count;

        private int _lastKey = InvalidKey;
        private Entry[] _entries;
    }

    // This is a variant of FrugalMap that always uses an array as the underlying store. 
    // This avoids the virtual method calls that are present when the store morphs through
    // the size efficient store classes normally used. It is appropriate only when we know the
    // store will always be populated and individual elements will be accessed in a tight loop.
    internal struct InsertionSortMap
    {
        public object this[int key]
        {
            get
            {
                // If no entry, EntityField.UnsetValue is returned
                if (_mapStore != null)
                {
                    return _mapStore.Search(key);
                }
                return DynamicProperty.UnsetValue;
            }

            set
            {
                if (value != DynamicProperty.UnsetValue)
                {
                    // If not unset value, ensure write success 
                    if (_mapStore != null)
                    {
                        // This is done because forward branches 
                        // default prediction is not to be taken
                        // making this a CPU win because set is 
                        // a common operation.
                    }
                    else
                    {
                        _mapStore = new LargeSortedObjectMap();
                    }

                    FrugalMapStoreState myState = _mapStore.InsertEntry(key, value);
                    if (FrugalMapStoreState.Success == myState)
                    {
                        return;
                    }
                    // Need to move to a more complex storage 
                    LargeSortedObjectMap newStore;

                    if (FrugalMapStoreState.SortedArray == myState)
                    {
                        newStore = new LargeSortedObjectMap();
                    }
                    else
                    {
                        throw new InvalidOperationException("FrugalMap cannot promote beyond Hashtable.");
                    }

                    // Extract the values from the old store and insert them into the new store 
                    _mapStore.Promote(newStore);

                    // Insert the new value
                    _mapStore = newStore;
                    _mapStore.InsertEntry(key, value);
                }
                else
                {
                    // EntityField.UnsetValue means remove the value
                    if (_mapStore != null)
                    {
                        _mapStore.RemoveEntry(key);
                        if (_mapStore.Count == 0)
                        {
                            // Map Store is now empty ... throw it away 
                            _mapStore = null;
                        }
                    }
                }
            }
        }

        public void Sort()
        {
            if (_mapStore != null)
            {
                _mapStore.Sort();
            }
        }

        public void GetKeyValuePair(int index, out int key, out Object value)
        {
            if (_mapStore != null)
            {
                _mapStore.GetKeyValuePair(index, out key, out value);
            }
            else
            {
                throw new ArgumentOutOfRangeException("index");
            }
        }

        public void Iterate(ArrayList list, FrugalMapIterationCallback callback)
        {
            if (null != callback)
            {
                if (null != list)
                {
                    if (_mapStore != null)
                    {
                        _mapStore.Iterate(list, callback);
                    }
                }
                else
                {
                    throw new ArgumentNullException("list");
                }
            }
            else
            {
                throw new ArgumentNullException("callback");
            }
        }

        public int Count
        {
            get
            {
                if (_mapStore != null)
                {
                    return _mapStore.Count;
                }
                return 0;
            }
        }

        private LargeSortedObjectMap _mapStore;
    }

    /// <summary> 
    ///     FrugalMapIterationCallback
    /// </summary>
    internal delegate void FrugalMapIterationCallback(ArrayList list, int key, object value);
}