// SetOnceValue.h
//
// Copyright (c) 2008 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

#pragma once

using namespace System;

namespace Supremacy
{
    namespace Types
    {
        generic<typename T>
        [Serializable]
        public value class SetOnceValue
        {
        public:
            SetOnceValue(T value)
                : _isSet(true), 
                  _value(value) {};

            property bool IsSet
            {
                bool get() { return _isSet; };
            };

            virtual property T Value
            {
                T get()
                {
                    if (_isSet)
                        return _value;
                    return (T)(T::typeid->IsValueType ? System::Activator::CreateInstance(T::typeid) : nullptr);
                };
                void set(T value)
                {
                    if (_isSet)
                        throw gcnew InvalidOperationException("Value can only be set once.");    
                    _isSet = true;
                    _value = value;
                };
            };

            static operator T(SetOnceValue<T> source)
            {
                return source._value;
            }

            static operator SetOnceValue<T>(T value)
            {
                return SetOnceValue(value);
            }

        private:
            bool _isSet;
            T _value;
        };
    }
}
