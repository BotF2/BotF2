// GameLog.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;

using log4net;
using System.Runtime.CompilerServices;

namespace Supremacy.Utility
{
    public class GameLog
    {
        private readonly string _name;

        // Prints the message with class and function from where it is called
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Print(string format, params object[] args)
        {
            System.Diagnostics.StackFrame frame = new System.Diagnostics.StackFrame(1);
            var callerMethodName = frame.GetMethod().Name;
            var callerClassName  = frame.GetMethod().ReflectedType.Name;

            // frame.GetFileName() and frame.GetFileLineNumber() are not working
            // If project switches to the .NET 4.5 this function could use:
            //      https://stackoverflow.com/questions/12556767/how-do-i-get-the-current-line-number
            //      https://stackoverflow.com/questions/3095696/how-do-i-get-the-calling-method-name-and-type-using-reflection

            Debug.General.DebugFormat(callerClassName + "." + callerMethodName + "(): " + format, args);
        }

        // Prints all relevant data from a exeception
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void LogException(Exception e)
        {
            System.Diagnostics.StackFrame frame = new System.Diagnostics.StackFrame(1);
            var callerMethodName = frame.GetMethod().Name;
            var callerClassName = frame.GetMethod().ReflectedType.Name;

            // There surely is a better way than to call "Debug.General.DebugFormat" eight times, but this is good enough until thing are sorted out
            Debug.General.DebugFormat("");
            Debug.General.DebugFormat("EXCEPTION OCCURRED in " + callerClassName + "." + callerMethodName + "()");
            Debug.General.DebugFormat("  Exception source:  " + e.Source);
            Debug.General.DebugFormat("  Exception message: " + e.Message);
            Debug.General.DebugFormat("  InnerException:    " + e.InnerException);
            Debug.General.DebugFormat("  TargetSite:        " + e.TargetSite);
            Debug.General.DebugFormat("  Exception StackTrace: " + e.StackTrace);
            Debug.General.DebugFormat("");
        }

        public static GameLog Debug
        {
            get { return GetLog("Debug"); }
        }

        public static GameLog Client
        {
            get { return GetLog("Client"); }
        }
        
        public static GameLog ScriptEngine
        {
            get { return GetLog("ScriptEngine"); }
        }

        public static GameLog Server
        {
            get { return GetLog("Server"); }
        }

        protected static class Repositories
        {
            public const string Combat = "Combat";
            public const string Diplomacy = "Diplomacy";
            public const string General = "General";
            public const string Effects = "Effects";
            public const string GameData = "GameData";
        }

        public ILog GameData
        {
            get { return LogManager.GetLogger(Repositories.GameData); }
        }

        public ILog Effects
        {
            get { return LogManager.GetLogger(Repositories.Effects); }
        }

        public ILog Combat
        {
            get { return LogManager.GetLogger(Repositories.Combat); }
        }

        public ILog Diplomacy
        {
            get { return LogManager.GetLogger(Repositories.Diplomacy); }
        }

        public ILog General
        {
            get { return LogManager.GetLogger(Repositories.General); }
        }

        protected GameLog(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            _name = type.FullName;
        }

        protected GameLog(string name)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentException("name must be a non-null, non-empty string");
            _name = name;
        }

        public static GameLog GetLog(Type type)
        {
            GameLogManager.Initialize();
            return new GameLog(type);
        }

        public static GameLog GetLog(string name)
        {
            GameLogManager.Initialize();
            return new GameLog(name);
        }
    }
}