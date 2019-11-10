// GameMod.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Supremacy.Game
{
    [Serializable]
    public sealed class GameMod
    {
        private Guid _uniqueIdentifier;
        private string _name;
        private Version _version;
        private string _rootPath;

        public Guid UniqueIdentifier
        {
            get { return _uniqueIdentifier; }
            set { _uniqueIdentifier = value; }
        }

        public string Name
        {
            get { return _name; }
            internal set { _name = value; }
        }

        public Version Version
        {
            get { return _version; }
            internal set { _version = value; }
        }

        public string RootPath
        {
            get { return _rootPath; }
            internal set { _rootPath = value; }
        }

        public GameMod() { }

        public GameMod(string uniqueIdentifier, string name, string version, string rootPath)
            : this(new Guid(uniqueIdentifier), name, new Version(version), rootPath) { }

        public GameMod(Guid uniqueIdentifier, string name, Version version, string rootPath)
        {
            _uniqueIdentifier = uniqueIdentifier;
            _name = name;
            _version = version;
            _rootPath = rootPath;
        }
    }

    public static class GameModLoader
    {
        private const string ModDirectoryName = "Mods";

        public static GameMod[] FindMods()
        {
            string modRootPath = Path.Combine(Environment.CurrentDirectory, ModDirectoryName);
            List<GameMod> mods = new List<GameMod>();

            if (Directory.Exists(modRootPath))
            {
                foreach (string modPath in Directory.GetDirectories(modRootPath))
                {
                    string configFile = GetModConfigFilePath(modPath);
                    try
                    {
                        mods.Add(ReadModConfig(configFile));
                    }
                    catch
                    {
                        continue;
                    }
                }
            }

            return mods.ToArray();
        }

        internal static string GetModConfigFilePath(string modPath)
        {
            if (Directory.Exists(modPath))
            {
                string[] configFiles = Directory.GetFiles(modPath, "*.modconfig");
                if (configFiles.Length > 0)
                    return configFiles[0];
            }
            return null;
        }

        internal static GameMod ReadModConfig(string modConfigPath)
        {
            GameMod mod = new GameMod();
            XmlDocument modConfigDoc = new XmlDocument();
            modConfigDoc.Load(modConfigPath);
            mod.UniqueIdentifier = new Guid(modConfigDoc.SelectSingleNode("./ModConfig/UniqueIdentifier").InnerText);
            mod.Name = modConfigDoc.SelectSingleNode("./ModConfig/Name").InnerText.Trim();
            mod.Version = new Version(modConfigDoc.SelectSingleNode("./ModConfig/Version").InnerText);
            mod.RootPath = Path.GetDirectoryName(modConfigPath);
            return mod;
        }

        public static GameMod GetModFromCommandLine()
        {
            string commandLine = Environment.CommandLine;
            int modNameIndex = commandLine.IndexOf("-Mod:");
            if (modNameIndex < 0)
                modNameIndex = commandLine.IndexOf("/Mod:");
            if (modNameIndex != -1)
            {
                string modDir = commandLine.Substring(modNameIndex + 5).Trim();
                try
                {
                    return ReadModConfig(GetModConfigFilePath(modDir));
                }
                catch (Exception e)
                {
                    GameLog.Core.GameData.Error(e);
                }
            }
            return null;
        }
    }
}
