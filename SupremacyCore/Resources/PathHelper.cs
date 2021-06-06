// PathHelper.cs
//
// Copyright (c) 2008 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Utility;
using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows;

namespace Supremacy.Resources
{
    public static class PathHelper
    {
        private static readonly Lazy<string> _workingDirectory = new Lazy<string>(GetWorkingDirectoryCore);
        private static readonly Lazy<bool> _isInDesignMode = new Lazy<bool>(() => DesignerProperties.GetIsInDesignMode(new DependencyObject()), false);

        public static string GetWorkingDirectory()
        {
            return _workingDirectory.Value;
        }

        private static string GetWorkingDirectoryCore()
        {
            try
            {
                if (_isInDesignMode.Value)
                {
                    string homePath = Environment.GetEnvironmentVariable(
                        "SupremacyRoot",
                        EnvironmentVariableTarget.User);

                    if (!string.IsNullOrWhiteSpace(homePath))
                        return homePath;

                    string currentDirectory = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
                    string workingDirectory = Path.Combine(
                        currentDirectory,
                        "..",
                        "..",
                        "..",
                        "SupremacyClient",
                        "bin",
                        "debug");

                    DirectoryInfo workingDirectoryInfo = new DirectoryInfo(workingDirectory);
                    if (workingDirectoryInfo.Exists)
                        return workingDirectoryInfo.FullName;
                }
            }
            catch (Exception e)
            {
                GameLog.Core.General.Error(e);
            }

            return Environment.CurrentDirectory;
        }
    }
}
