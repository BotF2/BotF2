// File:StorageManager.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Resources;
using Supremacy.Utility;
using System;
using System.Collections;
using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization.Formatters.Binary;

namespace Supremacy.IO
{
    public static class StorageManager
    {
        //private const string UserProfileFolderName = "Star Trek Supremacy";
        private const string UserProfileFolderName = ".";  // . = current folder
        private const string SettingsFilename = "Settings.dat";

        public static string UserLocalProfileFolder
        {
            get
            {
                return Path.Combine(
                    //Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    ResourceManager.GetResourcePath(""),
                    UserProfileFolderName);
                //return UserProfileFolderName;
            }
        }

        public static string UserRoamingProfileFolder
        {
            get
            {
                return Path.Combine(
                    //Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    ResourceManager.GetResourcePath(""),
                    UserProfileFolderName);
            }
        }

        private static IsolatedStorageFile OpenIsolatedStorage()
        {
            return IsolatedStorageFile.GetUserStoreForAssembly();
        }

        public static void WriteSetting<TKey, TValue>(TKey key, TValue value)
        {
            try
            {
                IsolatedStorageFile storage = OpenIsolatedStorage();

                BinaryFormatter formatter = new BinaryFormatter();
                Hashtable settings = null;

                using (Stream loadStream = new IsolatedStorageFileStream(
                    SettingsFilename, FileMode.OpenOrCreate, storage))
                {
                    try
                    {
                        settings = (Hashtable)formatter.Deserialize(loadStream);
                    }
                    catch (Exception e)
                    {
                        GameLog.Core.General.Error(e);
                    }
                }

                if (settings == null)
                {
                    settings = new Hashtable();
                }

                settings[key] = value;

                using (Stream writeStream = new IsolatedStorageFileStream(
                    SettingsFilename, FileMode.Create, storage))
                {
                    try
                    {
                        formatter.Serialize(writeStream, settings);
                    }
                    catch (Exception e)
                    {
                        GameLog.Core.General.Error(e);
                    }
                }
            }
            catch (Exception e)
            {
                GameLog.Core.General.Error(e);
            }
        }

        public static TValue ReadSetting<TKey, TValue>(TKey key)
        {
            try
            {
                IsolatedStorageFile storage = OpenIsolatedStorage();

                BinaryFormatter formatter = new BinaryFormatter();
                Hashtable settings = null;

                using (Stream loadStream = new IsolatedStorageFileStream(
                    SettingsFilename, FileMode.OpenOrCreate, storage))
                {
                    try
                    {
                        settings = (Hashtable)formatter.Deserialize(loadStream);
                    }
                    catch (Exception e)
                    {
                        GameLog.Core.General.Error(e);
                    }
                }

                return (TValue)settings[key];
            }
            catch
            {
                throw;
            }
        }
    }
}
