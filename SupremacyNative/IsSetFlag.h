// IsSetFlag.h
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
        [Serializable]
        public value class IsSetFlag sealed
        {
        public:
            IsSetFlag(bool isSet) : _isSet(isSet) {}

            property bool IsSet
            {
                bool get() { return _isSet; }
            }

            void Set() { _isSet = true; }

            operator bool() { return _isSet; }

            static operator bool(IsSetFlag value)
            {
                return value.IsSet;
            }

        private:
            bool _isSet;
        };
    }
}
