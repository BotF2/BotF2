// IClientCommandLineArguments.xaml.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Utility;
using System.Diagnostics;

namespace Supremacy.Client
{
    public interface IClientCommandLineArguments
    {
        PresentationTraceLevel TraceLevel { get; }
        SupremacyLogLevel LogLevel { get; }
        bool AllowMultipleInstances { get; }
        bool ShowUsage { get; }
        string Mod { get; }
        string SavedGame { get; set; }
    }
}