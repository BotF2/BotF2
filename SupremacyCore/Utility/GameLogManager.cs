// GameLogManager.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System.IO;

using log4net;
using log4net.Config;

namespace Supremacy.Utility
{
    // ToDo: This class should be merged with "GameLog" class. 

    // This class only initializes a third-party library "log4net" used for logging, but the "GameLog" class is actualy the one using that library

    public static class GameLogManager
    {
        private static bool _initialized;
        private static readonly object _syncLock;

        static GameLogManager()
        {
            _syncLock = new object();
            Initialize();
        }

        public static void Initialize()
        {
            lock (_syncLock)
            {
                if (_initialized)
                    return;
                XmlConfigurator.Configure(new FileInfo("LogConfig.log4net"));
                BasicConfigurator.Configure(new ChannelLogAppender());
                _initialized = true;
            }
            LogManager.GetLogger(typeof(GameLogManager)).Info("Log Initialized.");

        }

        /*  
         *  DEAD CODE - not used anywhere
         * 
         
        public static ILog GetLogger(string name)
        {
            return LogManager.GetLogger(name);
        }

        public static ILog GetLogger(Type type)
        {
            return LogManager.GetLogger(type);
        }

        public static GameLog GetLog(string name)
        {
            return GameLog.GetLog(name);
        }

        public static GameLog GetLog(Type type)
        {
            return GameLog.GetLog(type);
        }
        */
    }
}
