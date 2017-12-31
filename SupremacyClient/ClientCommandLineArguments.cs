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
using System.IO;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

using Avalon.Windows.Controls;

using Wintellect.CommandArgumentParser;
using Supremacy.Utility;

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
            Description = "Sets the logging level.")]
        public string LogLevel { get; set; }

        [CmdArg(
            RequiredValue = CmdArgRequiredValue.No,
            Description = "Break for the debugger immediately after launch.")]
        public bool Debug { get; set; }

        [CmdArg(
            RequiredValue = CmdArgRequiredValue.No,
            Description = "Allow multiple instances of the game to be run simultaneously.")]
        public bool AllowMultipleInstances { get; set; }

        [CmdArg(
            RequiredValue = CmdArgRequiredValue.No,
            Description = "Disable in-game music.")]
        public bool NoMusic { get; set; }

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
            RequiredValue = CmdArgRequiredValue.No,
            Description = "Disable the automatic update service.")]
        public bool DisableUpdates { get; set; }

        [CmdArg(
            RequiredValue = CmdArgRequiredValue.Yes,
            Description = "Specify the saved game to load on startup.")]
        public string SavedGame { get; set; }

        [CmdArg(
            RequiredValue = CmdArgRequiredValue.No,
            Description = "Allow AI to explore the map.")]
        public bool AiExplore { get; set; }

        [CmdArg(
            RequiredValue = CmdArgRequiredValue.No,
            Description = "traces Audio into Log.txt")]
        public bool TraceAudio { get; set; }

        //[CmdArg(
        //    RequiredValue = CmdArgRequiredValue.No,
        //    Description = "makes Borg a ExpandingPower (= not playable as empire)")]
        //public bool BorgNotEmpire { get; set; }
        #endregion

        #region ICmdArgs Members
        public void ProcessStandAloneArgument(string arg)
        {
            throw new Wintellect.Exception<InvalidCmdArgumentExceptionArgs>(
                String.Format("Unrecognized command line argument: '{0}'.", arg));
        }

        public void Usage(string errorInfo)
        {
            var taskDialog = new TaskDialog();
            var content = new TextBlock();

            FontFamily contentFont = null;

            try { contentFont = new FontFamily("Consolas"); }
            catch
            {
                try { contentFont = new FontFamily("Lucida Console"); }
                catch
                {
                    try
                    {
                        contentFont = new FontFamily("Courier New");
                    }
                    catch(Exception e)
                    {
                        GameLog.LogException(e);
                    }
                }
            }

            if (contentFont != null)
            {
                content.FontFamily = contentFont;
            }

            if (errorInfo != null)
            {
                content.Inlines.Add(new Run(errorInfo));
                content.Inlines.Add(new LineBreak());
                content.Inlines.Add(new LineBreak());
                taskDialog.MainIcon = TaskDialogIconConverter.ConvertFrom(TaskDialogIcon.Error);
                taskDialog.Header = "Command Line Argument Error";
            }
            else
            {
                taskDialog.Header = "Command Line Usage";
            }

            content.Inlines.Add(CmdArgParser.Usage(typeof(ClientCommandLineArguments)));

            taskDialog.Content = content;
            taskDialog.AllowDialogCancellation = true;
            taskDialog.Buttons.Add(TaskDialogButtons.Close);
            taskDialog.Title = Path.GetFileName(Environment.GetCommandLineArgs()[0]);
            taskDialog.Show();

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
            var options = new ClientCommandLineArguments();
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
                    options.Usage("Unrecognized command line argument: '" + e.Args.InvalidCmdArg  + "'.");
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