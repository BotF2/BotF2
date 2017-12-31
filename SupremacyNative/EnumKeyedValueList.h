// EnumIndexedValueList.h
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

#include "EnumHelper.h"

using namespace Supremacy::Utility;

namespace Supremacy
{
    namespace Types
    {
        generic<typename TKey, typename TValue> where TKey : Enum
        [Serializable]
        public ref class EnumKeyedValueList : INotifyPropertyChanged, System::Collections::Generic::IEnumerable<TValue>
        {
        public:
            EnumKeyedValueList()
            {
                _values = gcnew array<TValue>(EnumHelper::GetValues<TKey>()->Length);
            }

            property TValue default[TKey]
            {
                TValue get(TKey key)
                {
                    if (!EnumHelper::IsDefined<TKey>(key))
                        throw gcnew ArgumentOutOfRangeException("Invalid enum value: " + key);

                    int keyIndex = EnumHelper::GetValueOrdinal(key);
                    return _values[keyIndex];
                }
                void set(TKey key, TValue value)
                {
                    if (!EnumHelper::IsDefined<TKey>(key))
                        throw gcnew ArgumentOutOfRangeException("Invalid enum value: " + key);

                    int keyIndex = EnumHelper::GetValueOrdinal(key);
                    _values->SetValue(value, keyIndex);
                    this->OnPropertyChanged("Item[]");
                }
            }

            array<TValue>^ ToArray()
            {
                array<TValue>^ values = gcnew array<TValue>(_values->Length);
                for (int i = 0; i < _values->Length; i++)
                    values[i] = _values[i];
                return values;
            }

            void Clear()
            {
                _values->Initialize();
            }

        protected:
            property array<TValue>^ Values
            {
                array<TValue>^ get()
                {
                    return _values;
                }
            }
            void OnPropertyChanged(String^ propertyName)
            {
                PropertyChangedEventHandler^ handler = _propertyChangedHandler;
                if (handler != nullptr)
                    handler(this, gcnew PropertyChangedEventArgs(propertyName));
            }

        private:
            virtual System::Collections::Generic::IEnumerator<TValue>^ GetEnumerator() sealed = System::Collections::Generic::IEnumerable<TValue>::GetEnumerator
            {
                return static_cast<System::Collections::Generic::IEnumerable<TValue>^>(_values)->GetEnumerator();
            };

            virtual System::Collections::IEnumerator^ GetNonGenericEnumerator() sealed = System::Collections::IEnumerable::GetEnumerator
            {
                return _values->GetEnumerator();
            };

            virtual void AddPropertyChangedHandler(PropertyChangedEventHandler^ value) sealed = INotifyPropertyChanged::PropertyChanged::add
            {
                 _propertyChangedHandler = (PropertyChangedEventHandler^)Delegate::Combine(
                        _propertyChangedHandler,
                        value);
            }

            virtual void RemovePropertyChangedHandler(PropertyChangedEventHandler^ value) sealed = INotifyPropertyChanged::PropertyChanged::remove
            {
                _propertyChangedHandler = (PropertyChangedEventHandler^)Delegate::Remove(
                    _propertyChangedHandler,
                    value);
            }

            [NonSerialized]
            PropertyChangedEventHandler^ _propertyChangedHandler;
            initonly array<TValue>^ _values;
        };
    }
}