// DelegatingWeakEventListener.h
//
// Copyright (c) 2008 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

#pragma once

using namespace System;
using namespace System::Collections;
using namespace System::Collections::Generic;
using namespace System::ComponentModel;
using namespace System::Windows;

namespace Supremacy
{
    namespace Utility
    {
        generic <typename THandler> where THandler: Delegate
        public ref class DelegatingWeakEventListener : public IWeakEventListener
        {
        public:
            DelegatingWeakEventListener(THandler handler)
            {
                if (handler == nullptr)
                    throw gcnew ArgumentNullException("handler");
                _handler = handler;
            };

        private:
            virtual bool ReceiveWeakEvent(Type^ managerType, Object^ sender, EventArgs^ e) sealed = IWeakEventListener::ReceiveWeakEvent
            {
                _handler->DynamicInvoke(sender, e);
                return true;
            };

            THandler _handler;
        };
    }
}