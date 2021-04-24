// File:GameLog.cs
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
#pragma warning disable IDE0052 // Remove unread private members
        private readonly string _name;
#pragma warning restore IDE0052 // Remove unread private members
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
            public const string CivsAndRaces = "CivsAndRaces";
            public const string Colonies = "Colonies";
            public const string Combat = "Combat";
            public const string Credits = "Credits";
            public const string Deuterium = "Deuterium";
            public const string Dilithium = "Dilithium";
            public const string Diplomacy = "Diplomacy";
            public const string Duranium = "Duranium";
            public const string Energy = "Energy";
            public const string Events = "Events";
            public const string GalaxyGenerator = "GalaxyGenerator";
            public const string GameData = "GameData";
            public const string GameInitData = "GameInitData";
            public const string General = "General";
            public const string InfoText = "InfoText";
            public const string Intel = "Intel";
            public const string MapData = "MapData";
            public const string Multiplay = "Multiplay";
            public const string Production = "Production";
            //public const string ReportErrorsToEmail = "ReportErrorsToEmail";  // that's no category
            public const string Research = "Research";
            public const string SaveLoad = "SaveLoad";
            public const string Ships = "Ships";
            public const string ShipProduction = "ShipProduction";
            public const string SitReps = "SitReps";
            public const string Stations = "Stations";
            public const string Structures = "Structures";
            public const string SystemAssault = "SystemAssault";
            public const string Test = "Test";
            public const string TradeRoutes = "TradeRoutes";
            public const string UI = "UI";
            public const string XMLCheck = "XMLCheck";
            public const string XML2CSVOutput = "XML2CSVOutput";

            // Details
            public const string AIDetails = "AIDetailsDetailsDetails";
            public const string AudioDetails = "AudioDetails";
            public const string CivsAndRacesDetails = "CivsAndRacesDetails";
            public const string ColoniesDetails = "ColoniesDetails";
            public const string CombatDetails = "CombatDetailsDetails";
            public const string CreditsDetails = "CreditsDetails";
            public const string DeuteriumDetails = "DeuteriumDetails";
            public const string DilithiumDetails = "DilithiumDetails";
            public const string DiplomacyDetails = "DiplomacyDetails";
            public const string DuraniumDetails = "DuraniumDetails";
            public const string EnergyDetails = "EnergyDetails";
            public const string EventsDetails = "EventsDetails";
            public const string GalaxyGeneratorDetails = "GalaxyGeneratorDetails";
            public const string GameDataDetails = "GameDataDetails";
            public const string GameInitDataDetails = "GameInitDataDetails";
            public const string GeneralDetails = "GeneralDetails";
            public const string InfoTextDetails = "InfoTextDetails";
            public const string IntelDetails = "IntelDetails";
            public const string MapDataDetails = "MapDataDetails";
            public const string MultiplayDetails = "MultiplayDetails";
            public const string ProductionDetails = "ProductionDetails";
            //public const string ReportErrorsToEmail = "ReportErrorsToEmailDetails";  // that's no category
            public const string ResearchDetails = "ResearchDetails";
            public const string SaveLoadDetails = "SaveLoadDetails";
            public const string ShipsDetails = "ShipsDetails";
            public const string ShipProductionDetails = "ShipProductionDetails";
            public const string SitRepsDetails = "SitRepsDetails";
            public const string StationsDetails = "StationsDetails";
            public const string StructuresDetails = "StructuresDetails";
            public const string SystemAssaultDetails = "SystemAssaultDetails";
            public const string TestDetails = "TestDetails";
            public const string TradeRoutesDetails = "TradeRoutesDetails";
            public const string UIDetails = "UIDetails";
            public const string XMLCheckDetails = "XMLCheckDetails";
            public const string XML2CSVOutputDetails = "XML2CSVOutputDetails";
        }
        public ILog AI { get { return LogManager.GetLogger(Repositories.AI); } }
        public ILog AIDetails { get { return LogManager.GetLogger(Repositories.AIDetails); } }
        public ILog Audio { get { return LogManager.GetLogger(Repositories.Audio); } }
        public ILog AudioDetails { get { return LogManager.GetLogger(Repositories.AudioDetails); } }
        public ILog CivsAndRaces { get { return LogManager.GetLogger(Repositories.CivsAndRaces); } }
        public ILog CivsAndRacesDetails { get { return LogManager.GetLogger(Repositories.CivsAndRacesDetails); } }
        public ILog Colonies { get { return LogManager.GetLogger(Repositories.Colonies); } }
        public ILog ColoniesDetails { get { return LogManager.GetLogger(Repositories.ColoniesDetails); } }
        public ILog Combat { get { return LogManager.GetLogger(Repositories.Combat); } }
        public ILog CombatDetails { get { return LogManager.GetLogger(Repositories.CombatDetails); } }
        public ILog Credits { get { return LogManager.GetLogger(Repositories.Credits); } }
        public ILog CreditsDetails { get { return LogManager.GetLogger(Repositories.CreditsDetails); } }
        public ILog Deuterium { get { return LogManager.GetLogger(Repositories.Deuterium); } }
        public ILog DeuteriumDetails { get { return LogManager.GetLogger(Repositories.DeuteriumDetails); } }
        public ILog Dilithium { get { return LogManager.GetLogger(Repositories.Dilithium); } }
        public ILog DilithiumDetails { get { return LogManager.GetLogger(Repositories.DilithiumDetails); } }
        public ILog Duranium { get { return LogManager.GetLogger(Repositories.Duranium); } }
        public ILog DuraniumDetails { get { return LogManager.GetLogger(Repositories.DuraniumDetails); } }
        public ILog Diplomacy { get { return LogManager.GetLogger(Repositories.Diplomacy); } }
        public ILog DiplomacyDetails { get { return LogManager.GetLogger(Repositories.DiplomacyDetails); } }
        public ILog Energy { get { return LogManager.GetLogger(Repositories.Energy); } }
        public ILog EnergyDetails { get { return LogManager.GetLogger(Repositories.EnergyDetails); } }
        public ILog Events { get { return LogManager.GetLogger(Repositories.Events); } }
        public ILog EventsDetails { get { return LogManager.GetLogger(Repositories.EventsDetails); } }
        public ILog GalaxyGenerator { get { return LogManager.GetLogger(Repositories.GalaxyGenerator); } }
        public ILog GalaxyGeneratorDetails { get { return LogManager.GetLogger(Repositories.GalaxyGeneratorDetails); } }
        public ILog GameData { get { return LogManager.GetLogger(Repositories.GameData); } }
        public ILog GameDataDetails { get { return LogManager.GetLogger(Repositories.GameDataDetails); } }
        public ILog GameInitData { get { return LogManager.GetLogger(Repositories.GameInitData); } }
        public ILog GameInitDataDetails { get { return LogManager.GetLogger(Repositories.GameInitDataDetails); } }
        public ILog General { get { return LogManager.GetLogger(Repositories.General); } }
        public ILog GeneralDetails { get { return LogManager.GetLogger(Repositories.GeneralDetails); } }
        public ILog InfoText { get { return LogManager.GetLogger(Repositories.InfoText); } }
        public ILog InfoTextDetails { get { return LogManager.GetLogger(Repositories.InfoTextDetails); } }
        public ILog Intel { get { return LogManager.GetLogger(Repositories.Intel); } }
        public ILog IntelDetails { get { return LogManager.GetLogger(Repositories.IntelDetails); } }
        public ILog MapData { get { return LogManager.GetLogger(Repositories.MapData); } }
        public ILog MapDataDetails { get { return LogManager.GetLogger(Repositories.MapDataDetails); } }
        public ILog Multiplay { get { return LogManager.GetLogger(Repositories.Multiplay); } }
        public ILog MultiplayDetails { get { return LogManager.GetLogger(Repositories.MultiplayDetails); } }

        public ILog Production { get { return LogManager.GetLogger(Repositories.Production); } }
        public ILog ProductionDetails { get { return LogManager.GetLogger(Repositories.ProductionDetails); } }
        //public ILog ReportErrorsToEmail // that's no category
        //{
        //    get { return LogManager.GetLogger(Repositories.ReportErrorsToEmail); }
        //}
        public ILog Research { get { return LogManager.GetLogger(Repositories.Research); } }
        public ILog ResearchDetails { get { return LogManager.GetLogger(Repositories.ResearchDetails); } }
        public ILog SaveLoad { get { return LogManager.GetLogger(Repositories.SaveLoad); } }
        public ILog SaveLoadDetails { get { return LogManager.GetLogger(Repositories.SaveLoadDetails); } }
        public ILog Ships { get { return LogManager.GetLogger(Repositories.Ships); } }
        public ILog ShipsDetails { get { return LogManager.GetLogger(Repositories.ShipsDetails); } }
        public ILog ShipProduction { get { return LogManager.GetLogger(Repositories.ShipProduction); } }
        public ILog ShipProductionDetails { get { return LogManager.GetLogger(Repositories.ShipProductionDetails); } }
        public ILog SitReps { get { return LogManager.GetLogger(Repositories.SitReps); } }
        public ILog SitRepsDetails { get { return LogManager.GetLogger(Repositories.SitRepsDetails); } }
        public ILog Stations { get { return LogManager.GetLogger(Repositories.Stations); } }
        public ILog StationsDetails { get { return LogManager.GetLogger(Repositories.StationsDetails); } }
        public ILog Structures { get { return LogManager.GetLogger(Repositories.Structures); } }
        public ILog StructuresDetails { get { return LogManager.GetLogger(Repositories.StructuresDetails); } }
        public ILog SystemAssault { get { return LogManager.GetLogger(Repositories.SystemAssault); } }
        public ILog SystemAssaultDetails { get { return LogManager.GetLogger(Repositories.SystemAssaultDetails); } }
        public ILog Test { get { return LogManager.GetLogger(Repositories.Test); } }
        public ILog TradeRoutes { get { return LogManager.GetLogger(Repositories.TradeRoutes); } }
        public ILog TradeRoutesDetails { get { return LogManager.GetLogger(Repositories.TradeRoutesDetails); } }
        public ILog UI { get { return LogManager.GetLogger(Repositories.UI); } }
        public ILog UIDetails { get { return LogManager.GetLogger(Repositories.UIDetails); } }
        public ILog XMLCheck { get { return LogManager.GetLogger(Repositories.XMLCheck); } }
        public ILog XMLCheckDetails { get { return LogManager.GetLogger(Repositories.XMLCheckDetails); } }
        public ILog XML2CSVOutput { get { return LogManager.GetLogger(Repositories.XML2CSVOutput); } }
        public ILog XML2CSVOutputDetails { get { return LogManager.GetLogger(Repositories.XML2CSVOutputDetails); } }

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
            string _text = "    Log.txt: Trace is set to      DEBUG for > " + repository;
            GameLog.Client.GeneralDetails.DebugFormat(_text);
            Console.WriteLine(_text);
        }

        public static void SetRepositoryToErrorOnly(string repository)
        {
            ((log4net.Repository.Hierarchy.Logger)LogManager.GetLogger(repository).Logger).Level = Level.Error;
            //works 
            string _text = "Log.txt: Trace is set to ERROR only for " + repository;
            GameLog.Client.GeneralDetails.DebugFormat(_text);
            Console.WriteLine(_text);
        }
    }
}