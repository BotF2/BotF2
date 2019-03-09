// GameLog.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using log4net;
using log4net.Config;
using log4net.Core;
using System;
using System.IO;


namespace Supremacy.Utility
{
    public class GameLog
    {
        private readonly string _name;
        private static bool _initialized;
        private static readonly object _syncLock;

        static GameLog()
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
            Core.General.Info("Log Initialized");
            string now = "Possible file name prepared... (see next line)" + Environment.NewLine + DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day + "_" + DateTime.Now.Hour + "-" + DateTime.Now.Minute + "-" + DateTime.Now.Second + " Gamelog.txt" + Environment.NewLine;
            Core.General.Info(now);  // new line for saving under Date
        }

        public static GameLog Client
        {
            get { return GetLog("Client"); }
        }

        public static GameLog Server
        {
            get { return GetLog("Server"); }
        }

        public static GameLog Core
        {
            get { return GetLog("Core"); }
        }

        protected static class Repositories
        {
            public const string AI = "AI";
            public const string Audio = "Audio";
            public const string Colonies = "Colonies";
            public const string Combat = "Combat";
            public const string CombatDetails = "CombatDetails";
            public const string Diplomacy = "Diplomacy";
            public const string Energy = "Energy";
            public const string Events = "Events";
            public const string Intel = "Intel";
            public const string GalaxyGenerator = "GalaxyGenerator";
            public const string GameData = "GameData";
            public const string General = "General";
            public const string Multiplay = "Multiplay";
            public const string Production = "Production";
            public const string Research = "Research";
            public const string SaveLoad = "SaveLoad";
            public const string ShipProduction = "ShipProduction";
            public const string Structures = "Structures";
            public const string SystemAssault = "SystemAssault";
            public const string SystemAssaultDetails = "SystemAssaultDetails";
            public const string TradeRoutes = "TradeRoutes";
            public const string UI = "UI";
        }

        public ILog AI
        {
            get { return LogManager.GetLogger(Repositories.AI); }
        }

        public ILog Audio
        {
            get { return LogManager.GetLogger(Repositories.Audio); }
        }

        public ILog Colonies
        {
            get { return LogManager.GetLogger(Repositories.Colonies); }
        }

        public ILog Combat
        {
            get { return LogManager.GetLogger(Repositories.Combat); }
        }

        public ILog CombatDetails
        {
            get { return LogManager.GetLogger(Repositories.CombatDetails); }
        }

        public ILog Diplomacy
        {
            get { return LogManager.GetLogger(Repositories.Diplomacy); }
        }

        public ILog Energy
        {
            get { return LogManager.GetLogger(Repositories.Energy); }
        }

        public ILog Events
        {
            get { return LogManager.GetLogger(Repositories.Events); }
        }

        public ILog Intel
        {
            get { return LogManager.GetLogger(Repositories.Intel); }
        }

        public ILog GalaxyGenerator
        {
            get { return LogManager.GetLogger(Repositories.GalaxyGenerator); }
        }

        public ILog GameData
        {
            get { return LogManager.GetLogger(Repositories.GameData); }
        }

        public ILog General
        {
            get { return LogManager.GetLogger(Repositories.General); }
        }

        public ILog Multiplay
        {
            get { return LogManager.GetLogger(Repositories.Multiplay); }
        }

        public ILog SaveLoad
        {
            get { return LogManager.GetLogger(Repositories.SaveLoad); }
        }

        public ILog Production
        {
            get { return LogManager.GetLogger(Repositories.Production); }
        }

        public ILog Research
        {
            get { return LogManager.GetLogger(Repositories.Research); }
        }

        public ILog ShipProduction
        {
            get { return LogManager.GetLogger(Repositories.ShipProduction); }
        }

        public ILog SystemAssault
        {
            get { return LogManager.GetLogger(Repositories.SystemAssault); }
        }

        public ILog SystemAssaultDetails
        {
            get { return LogManager.GetLogger(Repositories.SystemAssaultDetails); }
        }

        public ILog TradeRoutes
        {
            get { return LogManager.GetLogger(Repositories.TradeRoutes); }
        }

        public ILog UI
        {
            get { return LogManager.GetLogger(Repositories.UI); }
        }

        protected GameLog(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            _name = type.FullName;
        }

        protected GameLog(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name must be a non-null, non-empty string");
            _name = name;
        }

        public static GameLog GetLog(Type type)
        {
            Initialize();
            return new GameLog(type);
        }

        public static GameLog GetLog(string name)
        {
            Initialize();
            return new GameLog(name);
        }

        public static void SetRepositoryToDebug(string repository)
        {
            ((log4net.Repository.Hierarchy.Logger)LogManager.GetLogger(repository).Logger).Level = Level.Debug;
            GameLog.Client.General.InfoFormat("    Log.txt: Trace is set to      DEBUG for '{0}' ", repository);
        }

        public static void SetRepositoryToErrorOnly(string repository)
        {
            ((log4net.Repository.Hierarchy.Logger)LogManager.GetLogger(repository).Logger).Level = Level.Error;
            GameLog.Client.General.InfoFormat("Log.txt: Trace is set to ERROR only for '{0}' ", repository);
        }
    }
}