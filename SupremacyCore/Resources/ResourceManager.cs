// ResourceManager.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Game;
using Supremacy.Orbitals;
using Supremacy.Utility;
using Supremacy.VFS;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace Supremacy.Resources
{
    public static class ResourceManager
    {
        public const char PathSeparatorCharacter = ':';
        public const int MaxStringKeyLength = 255;

        public static readonly CultureInfo NeutralCulture;
        public static readonly CultureInfo CurrentCulture;
        //public static readonly CultureInfo GermanCulture;
        //public static readonly CultureInfo FrenchCulture;
        public static readonly string NeutralLocale;
        public static readonly string CurrentLocale;
        //public static readonly string GermanLocale;
        //public static readonly string FrenchLocale;

        private static readonly StringTable _localStrings;
        private static readonly StringTable _defaultStrings;
        //private static readonly StringTable _GermanStrings;
        //private static readonly StringTable _FrenchStrings;
        private static readonly GameMod _commandLineMod;
        private static readonly IVfsService _vfsService;
        private static string _text;

        public static string WorkingDirectory { get; private set; }

        // checks whether commandLineArguments are done and more
        public static GameMod CurrentMod
        {
            get
            {

                if (_commandLineMod != null)
                {
                    return _commandLineMod;
                }

                GameContext gameContext = GameContext.Peek();
                if (gameContext != null)
                {
                    _text += _text + gameContext.GameMod.ToString(); // Dummy, do not remove
                    return gameContext.GameMod;
                }
                //GameLog.Print("GameMod CurrentMod is null");
                return null;
            }
        }

        public static IVfsService VfsService => _vfsService;

        static ResourceManager()
        {
            WorkingDirectory = PathHelper.GetWorkingDirectory();
            _vfsService = new VfsService();

            ReadOnlyHardDiskSource defaultSource = new ReadOnlyHardDiskSource(
                "Default",
                WorkingDirectory);

            ReadOnlyHardDiskSource modSource = new ReadOnlyHardDiskSource("CurrentMod", null)
            {
                PathResolver = ResolveModVfsRoot
            };

            /*
             * We want to probe the currently loaded mod's directory structure before the
             * main game's directory structure, so we add the mod VFS source first.
             */
            _vfsService.AddSource(modSource);
            _vfsService.AddSource(defaultSource);

            AppDomain.CurrentDomain.SetData("DataDirectory", WorkingDirectory);

            NeutralCulture = CultureInfo.GetCultureInfo("en");
            CurrentCulture = CultureInfo.CurrentCulture;
            //GermanCulture = CultureInfo.GetCultureInfo("de");
            //FrenchCulture = CultureInfo.GetCultureInfo("fr");

            string _enTXT = GetResourcePath(@"Resources\Data\en.txt");
            try { if (File.Exists(_enTXT)) { File.Copy(_enTXT, GetResourcePath(@"Resources\Data\de.txt"), true); } }
            catch
            {
                _text = GameEngine.Newline + "Step_0131:; ############### PROBLME - is _strings.csv or .txt in use ?";
                Console.WriteLine(_text);
            }

            while (!CurrentCulture.IsNeutralCulture &&
                   CurrentCulture.Parent != null &&
                   CurrentCulture.Parent != CurrentCulture)
            {
                CurrentCulture = CurrentCulture.Parent;
            }

            NeutralLocale = NeutralCulture.Name;
            CurrentLocale = CurrentCulture.Name;
            //GermanLocale = GermanCulture.Name;
            //FrenchLocale = FrenchCulture.Name;

            VfsWebRequestFactory.EnsureRegistered();

            _commandLineMod = GameModLoader.GetModFromCommandLine();

            _defaultStrings = StringTable.Load(GetResourcePath(@"Resources\Data\" + NeutralLocale + ".txt"));


            //try { _GermanStrings = StringTable.Load(GetResourcePath(@"Resources\Data\de_added.txt")); } catch { }


            //try { _FrenchStrings = StringTable.Load(GetResourcePath(@"Resources\Data\fr.txt")); } catch { }

            // output en.txt + de.txt + fr.txt

            // now in en.tx with #>DE: >>> remove "#" or add by Replace ">DE: " by "#>DE: "

            //try
            //{
            //    int c = 0;
            //    _text = "";
            //    string _key = "";
            //    foreach (var item in _defaultStrings.Keys)
            //    {
            //        c += 1;
            //        _key = item.ToString();

            //        string _entry = GameEngine.Do_4_Digit(c.ToString()) + " ; " + _key;
            //        _entry += " ; ";
            //        _entry += _defaultStrings[_key].ToString();
            //        _entry += " ; ";
            //        _entry += _GermanStrings[_key].ToString();
            //        _entry += " ; ";
            //        _entry += _FrenchStrings[_key].ToString();

            //        _entry = _entry.Replace("\r\n", GameEngine.Blank);

            //        _text += _entry + GameEngine.Newline;

            //        //Console.WriteLine("Step_0135:; " + _text);
            //        //if (_key == "Blackhole")
            //    }
            //    _text = "Entry;Key;EN  ;DE  ;FR  " + GameEngine.Newline + _text;
            //    //Console.WriteLine("Step_0146:; " + GameEngine.Newline + _text);
            //    Console.WriteLine("Step_0146:; " + GameEngine.Newline + "No output of strings in EN / DE /FR");

            //    string file = Path.Combine(ResourceManager.GetResourcePath(".\\lib"), "_Strings.txt");
            //    string file_csv = Path.Combine(ResourceManager.GetResourcePath(".\\lib"), "_Strings.csv");


            //    if (!string.IsNullOrEmpty(file) && File.Exists(file))
            //    {
            //        StreamWriter streamWriter = new StreamWriter(file);
            //        streamWriter.Write(_text);
            //        streamWriter.Close();
            //        _text = "output of _Strings done to " + file;
            //        if (true) Console.WriteLine(_text);
            //        try { if (File.Exists(file)) { File.Copy(file, file_csv, true); } } catch
            //        {
            //            _text = GameEngine.Newline + "Step_0131:; ############### PROBLME - is _strings.csv or .txt in use ?";
            //            Console.WriteLine(_text);
            //        }
            //    }
            //}
            //catch
            //{
            //    _text = "Step_0139:; en.txt and de.txt and fr.txt are structered in different amount of keys"
            //        + GameEngine.Newline + "or _strings.txt or csv is in use"
            //        ;
            //    Console.WriteLine(_text);
            //}


            if (CurrentLocale == NeutralLocale)
            {
                _localStrings = null;
            }
            else
            {
                try
                {
                    _localStrings = StringTable.Load(
                        GetResourcePath(@"Resources\Data\" + CurrentLocale + ".txt"));

                    //if (true)
                    //{
                    //    foreach (var item in _localStrings.Values)
                    //    {
                    //        if (item == "STAR_TYPE_BLACKHOLE_DESCRIPTION"
                    //            || item == "STAR_TYPE_WORMHOLE_DESCRIPTION"
                    //            || item == "STAR_TYPE_BLACKHOLE_DESCRIPTION")
                    //        {
                    //            //item.
                    //            Debugger.Break();
                    //        }
                    //    }
                    //}
                }
                catch (Exception e)
                {
                    _text = "Step_0140: Hint: local file \\Data\\xx.txt not available - at the moment only English (en.txt) is ingame. French and German are already done"
                        + GameEngine.Newline + "exception" + e.Message
                        + GameEngine.Newline + e.StackTrace + e.Source
                        ;
                    Console.WriteLine(_text);
                    GameLog.Core.GameData.Info(_text);
                    GameLog.Core.GameData.DebugFormat("exception {0} {1}", e.Message, e.StackTrace);
                }
            }
        }

        private static string ResolveModVfsRoot()
        {
            GameMod currentMod = CurrentMod;
            if (currentMod == null)
            {
                return null;
            }

            string rootPath = currentMod.RootPath;
            if (Directory.Exists(rootPath))
            {
                return rootPath;
            }

            return null;
        }

        public static void Initialize()
        {
            return;
        }

        public static string GetString(string key)
        {
            string result = null;
            if (_localStrings != null)
            {
                result = _localStrings[key];
            }

            if (result == null)
            {
                result = _defaultStrings[key];
            }

            return result ?? key;
        }

        //public static string Get_DE_String(string key)
        //{
        //    string result = null;
        //    if (_GermanStrings != null)
        //    {
        //        result = _GermanStrings[key];
        //    }

        //    if (result == null)
        //    {
        //        result = _defaultStrings[key];
        //    }

        //    return result ?? key;
        //}

        //public static string Get_FR_String(string key)
        //{
        //    string result = null;
        //    //if (_FrenchStrings != null)
        //    //{
        //    //    result = _FrenchStrings[key];
        //    //}

        //    if (result == null)
        //    {
        //        result = _defaultStrings[key];
        //    }

        //    return result ?? key;
        //}

        public static string GetResourcePath(string path)
        {
            return GetSystemResourcePath(path);
        }

        public static string GetInternalResourcePath(string path)
        {
            if (path == null)
            {
                return null;
            }

            path = EvaluateRelativePath(WorkingDirectory, GetSystemPathFormat(path));
            if (CurrentMod != null)
            {
                string modRelativePath = GetSystemPathFormat(CurrentMod.RootPath);
                string modAbsolutePath;
                if (Path.IsPathRooted(modRelativePath))
                {
                    modAbsolutePath = modRelativePath;
                    modRelativePath = EvaluateRelativePath(WorkingDirectory, modRelativePath);
                }
                else
                {
                    modAbsolutePath = Path.Combine(WorkingDirectory, modRelativePath);
                }
                if (File.Exists(modAbsolutePath))
                {
                    return GetInternalPathFormat(modRelativePath);
                }
            }
            return GetInternalPathFormat(Path.Combine(WorkingDirectory, path));
        }

        public static string GetSystemResourcePath(string path)
        {
            if (path == null)
            {
                return null;
            }

            path = Uri.TryCreate(path, UriKind.Absolute, out Uri uri)
                ? uri.GetComponents(UriComponents.Path, UriFormat.Unescaped)
                : GetSystemPathFormat(path);

            if (Path.IsPathRooted(path))
            {
                path = EvaluateRelativePath(WorkingDirectory, path);
            }

            if (CurrentMod != null)
            {
                string modPath = GetSystemPathFormat(CurrentMod.RootPath);

                if (!Path.IsPathRooted(modPath))
                {
                    modPath = Path.Combine(WorkingDirectory, modPath);
                }

                if (Directory.Exists(modPath))
                {
                    string modFile = EvaluateRelativePath(WorkingDirectory, Path.Combine(modPath, path));
                    if (File.Exists(modFile))
                    {
                        return modFile;
                    }
                }
            }

            if (!Path.IsPathRooted(path))
            {
                path = Path.Combine(WorkingDirectory, path);
            }

            path = EvaluateRelativePath(Environment.CurrentDirectory, path);

            // works   GameLog.Client.General.DebugFormat("File Path = {0}", path);

            return path;
        }

        public static Uri GetResourceUri(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            if (Uri.TryCreate(path, UriKind.Absolute, out Uri uri))
            {
                if (uri.Scheme == "vfs")
                {
                    return uri;
                }

                path = uri.GetComponents(UriComponents.Path, UriFormat.Unescaped);
            }

            if (path[0] != '/')
            {
                path = '/' + path;
            }

            return new UriBuilder
            {
                Scheme = "vfs",
                Path = path,
                Host = null
            }.Uri;
        }

        private static string GetInternalPathFormat(string path)
        {
            path = path.Replace(Path.DirectorySeparatorChar, PathSeparatorCharacter);
            path = path.Replace(Path.AltDirectorySeparatorChar, PathSeparatorCharacter);
            return path;
        }

        private static string GetSystemPathFormat(string path)
        {
            path = path.Replace(PathSeparatorCharacter, Path.DirectorySeparatorChar);
            path = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            return path;
        }

        private static string EvaluateRelativePath(string mainDirPath, string absoluteFilePath)
        {
            mainDirPath = mainDirPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            absoluteFilePath = absoluteFilePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

            string[] firstPathParts = mainDirPath.Trim(Path.DirectorySeparatorChar).Split(Path.DirectorySeparatorChar);
            string[] secondPathParts = absoluteFilePath.Trim(Path.DirectorySeparatorChar).Split(Path.DirectorySeparatorChar);

            int sameCounter = 0;
            for (int i = 0; i < Math.Min(firstPathParts.Length, secondPathParts.Length); i++)
            {
                if (!firstPathParts[i].ToLower().Equals(secondPathParts[i].ToLower()))
                {
                    break;
                }
                sameCounter++;
            }

            if (sameCounter == 0)
            {
                while (absoluteFilePath.StartsWith("." + Path.DirectorySeparatorChar))
                {
                    absoluteFilePath = absoluteFilePath.Substring(2);
                }

                return absoluteFilePath;
            }

            string newPath = string.Empty;
            for (int i = sameCounter; i < firstPathParts.Length; i++)
            {
                if (i > sameCounter)
                {
                    newPath += Path.DirectorySeparatorChar;
                }
                newPath += "..";
            }
            if (newPath.Length == 0)
            {
                newPath = ".";
            }
            for (int i = sameCounter; i < secondPathParts.Length; i++)
            {
                newPath += Path.DirectorySeparatorChar;
                newPath += secondPathParts[i];
            }

            while (newPath.StartsWith("." + Path.DirectorySeparatorChar))
            {
                newPath = newPath.Substring(2);
            }

            return newPath;
        }
    }
}
