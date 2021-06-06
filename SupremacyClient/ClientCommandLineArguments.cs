// ClientCommandLineArguments.xaml.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Diagnostics;
using System.Windows.Documents;
using Wintellect.CommandArgumentParser;

namespace Supremacy.Client
{
    public class ClientCommandLineArguments : IClientCommandLineArguments, ICmdArgs
    {
        #region Fields
        private PresentationTraceLevel _traceLevel = PresentationTraceLevel.None;
        #endregion

        #region Properties
        [CmdArg(
           RequiredValue = CmdArgRequiredValue.Yes,
           Description = "Sets the WPF presentation trace level.")]
        public PresentationTraceLevel TraceLevel
        {
            get { return _traceLevel; }
            set { _traceLevel = value; }
        }

        [CmdArg(
            RequiredValue = CmdArgRequiredValue.Yes,
            Description = "Sets which systems you want to output debug information for.")]
        public string Traces { get; set; }

        [CmdArg(
            RequiredValue = CmdArgRequiredValue.No,
            Description = "Allow multiple instances of the game to be run simultaneously.")]
        public bool AllowMultipleInstances { get; set; }

        [CmdArg(
            RequiredValue = CmdArgRequiredValue.No,
            Description = "Print this usage screen and then exit.",
            ArgName = "?")]
        public bool ShowUsage { get; set; }

        [CmdArg(
            RequiredValue = CmdArgRequiredValue.Yes,
            Description = "Specify the location of a mod to load.")]
        public string Mod { get; set; }

        [CmdArg(
            RequiredValue = CmdArgRequiredValue.Yes,
            Description = "Specify the saved game to load on startup.")]
        public string SavedGame { get; set; }

        #endregion

        #region ICmdArgs Members
        public void ProcessStandAloneArgument(string arg)
        {
            throw new Wintellect.Exception<InvalidCmdArgumentExceptionArgs>(
                string.Format("Unrecognized command line argument: '{0}'.", arg));
        }

        public void Usage(string errorInfo)
        {
            if (errorInfo != null)
            {
                Console.WriteLine(new Run(errorInfo));
            }

            Console.WriteLine(CmdArgParser.Usage(typeof(ClientCommandLineArguments)));

            Environment.Exit(Environment.ExitCode);
        }

        public void Validate()
        {
            if (ShowUsage)
                return;
        }
        #endregion

        #region Methods
        public static ClientCommandLineArguments Parse(string[] args)
        {
            ClientCommandLineArguments options = new ClientCommandLineArguments();
            Parse(options, args);
            return options;
        }

        public static void Parse(ClientCommandLineArguments options, string[] args)
        {
            try
            {
                CmdArgParser.Parse(options, args);
            }
            catch (Wintellect.Exception<InvalidCmdArgumentExceptionArgs> e)
            {
                if (!options.ShowUsage)
                {
                    options.Usage("Unrecognized command line argument: '" + e.Args.InvalidCmdArg + "'.");
                    ClientApp.Current.Shutdown();
                }
            }

            if (!options.ShowUsage)
                return;

            options.Usage(null);
            ClientApp.Current.Shutdown();
        }
        #endregion
    }
}