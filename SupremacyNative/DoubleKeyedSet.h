// DoubleKeyedSet.h
//
// Copyright (c) 2008 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

#pragma once

using namespace System;
using namespace System::Collections::Generic;
using namespace System::ComponentModel;
using namespace System::Runtime::InteropServices;

namespace Supremacy
{
    namespace Types
    {
        generic<typename TKey1, typename TKey2, typename TValue>
        public ref class DoubleKeyedSet
        {
        public:
            DoubleKeyedSet()
            {
                _values = gcnew Dictionary<KeyValuePair<TKey1, TKey2>, TValue>();
            }

            property TValue default[TKey1, TKey2]
            {
                TValue get(TKey1 firstKey, TKey2 secondKey)
                {
                    TValue value;
                    _values->TryGetValue(KeyValuePair<TKey1, TKey2>(firstKey, secondKey), value);
                    return value;
                }
                void set(TKey1 firstKey, TKey2 secondKey, TValue value)
                {
                    _values[KeyValuePair<TKey1, TKey2>(firstKey, secondKey)] = value;
                }
            }

            bool TryGetValue(TKey1 first, TKey2 second, [Out] TValue% value)
            {
                return _values->TryGetValue(KeyValuePair<TKey1,TKey2>(first, second) , value);
            }

        private:
            initonly Dictionary<KeyValuePair<TKey1, TKey2>, TValue>^ _values;
        };
    }
}