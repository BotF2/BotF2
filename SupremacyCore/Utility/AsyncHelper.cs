// AsyncHelper.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Supremacy.Utility
{
    public static class AsyncHelper
    {
        public static readonly TaskScheduler Scheduler;
        public static readonly TaskFactory TaskFactory;

        static AsyncHelper()
        {
            Scheduler = TaskScheduler.Current;
            TaskFactory = new TaskFactory(Scheduler);
        }

        #region Methods
        public static void Invoke(Delegate target, params object[] args)
        {
            ThreadPool.UnsafeQueueUserWorkItem(
                AsyncInvokeCallback,
                new AsyncCallbackData(target, args));
            //Task.Create(
            //    AsyncInvokeCallback,
            //    new AsyncCallbackData(target, args),
            //    TaskManager,
            //    TaskCreationOptions.None);
        }

        public static void Invoke(TaskScheduler taskManager, Delegate target, params object[] args)
        {
            //ThreadPool.UnsafeQueueUserWorkItem(
            //    AsyncInvokeCallback,
            //    new AsyncCallbackData(target, args));
            TaskFactory.StartNew(() => target.DynamicInvoke(args));
        }

        private static void AsyncInvokeCallback(object state)
        {
            try
            {
                var callbackData = (AsyncCallbackData)state;
                callbackData.Target.DynamicInvoke(callbackData.Args);
            }
            catch (ThreadAbortException) {}
            catch (AppDomainUnloadedException) {}
        }
        #endregion

        #region AsyncCallbackData Type
        private class AsyncCallbackData
        {
            #region Fields
            public readonly object[] Args;
            public readonly Delegate Target;
            #endregion

            #region Constructors
            public AsyncCallbackData(Delegate target, object[] args)
            {
                Target = target;
                Args = args;
            }
            #endregion
        }
        #endregion

        internal static void Yield()
        {
            SwitchToThread();
        }

        [DllImport("kernel32.dll")]
        internal static extern int SwitchToThread();
    }
}