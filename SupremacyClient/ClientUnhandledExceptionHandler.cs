// File:ClientUnhandledExceptionHandler.cs
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Client.Services;
using Supremacy.Types;
using Supremacy.Utility;
using System;
using System.Collections.Specialized;
using System.Net;
using System.Windows;

namespace Supremacy.Client
{
    internal sealed class ClientUnhandledExceptionHandler : IUnhandledExceptionHandler
    {
        private readonly IGameErrorService _errorService;
        //private const string _reportErrorURL = "https://www.exultantmonkey.co.uk/BotF2/report-error.php";

        #region Fields
        private readonly object _syncLock = new object();
        #endregion

        #region Implementation of IUnhandledExceptionHandler
        public void HandleError(Exception exception)
        {
            if (exception is AppDomainUnloadedException)
                return;

            var supremacyException = exception as SupremacyException;
            if (supremacyException != null)
            {
                _errorService.HandleError(supremacyException);
                Environment.Exit(Environment.ExitCode);
            }
            else
            {
                string errors = "";
                lock (_syncLock)
                {
                    var innerException = exception;

                    while (innerException != null)
                    {
                        errors += string.Format("{0}\r\n", innerException.Message);
                        errors += string.Format("{0}\r\n", innerException.StackTrace);
                        errors += "----------------------------------------\r\n";
                        innerException = innerException.InnerException;
                    }

                    Console.Error.WriteLine(errors);
                    Console.Error.Flush();
                    MessageBox.Show(
                        "An unhandled exception has occurred.  Detailed error information is "
                        + "available in the 'Log.txt' file.",
                        "Unhandled Exception",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                        );

                    //if (ClientSettings.Current.ReportErrors)
                    //{
                    ReportError(errors);
                    //}

                    Environment.Exit(Environment.ExitCode);
                }
            }
        }

        /// <summary>
        /// If error reporting is enabled, submits the error information to the developers
        /// </summary>
        public void ReportError(string stackTrace)
        {
            using (var client = new WebClient())
            {
                var values = new NameValueCollection();

                values["Version"] = ClientApp.ClientVersion.ToString();
                values["Title"] = stackTrace.Split('\n')[0];
                values["StackTrace"] = stackTrace;

                string _text = Environment.NewLine + Environment.NewLine
                    + DateTime.Now + " #### ERROR " /*+ Environment.NewLine*/
                    + "GAME-VERSION:;" + ClientApp.ClientVersion.ToString() + Environment.NewLine
                    + "ERROR TITLE:;" + stackTrace.Split('\n')[0] + Environment.NewLine
                    + "StackTrace complete:;" + stackTrace;

                GameLog.Core.General.ErrorFormat(_text, ""); // "" for avoiding message "argument missing" for log4net
                Console.WriteLine(_text);

                try 
                {
                    System.Diagnostics.Process.Start(".\\Resources\\reportError.bat");  
                    // batch opens a web site with email function, 
                    // but just if batch file is activated = rem is removed before web adress
                    // opens Win10 mail function which might be connected to Google Mail account or any other
                }
                catch (Exception ex) { throw ex;}

                // old code
                //try
                //{
                //    //var response = client.UploadValues(_reportErrorURL, "POST", values);
                //    //var responseString = Encoding.UTF8.GetString(response, 0, response.Length);
                //    foreach (var item in values)
                //    {
                //    GameLog.Core.General.ErrorFormat(" #### ERROR" + Environment.NewLine + "{0}", item.ToString());
                //    }

                //}
                //catch (Exception e)
                //{
                //    foreach (var item in values)
                //    {
                //        GameLog.Core.General.ErrorFormat(" #### ERROR" + Environment.NewLine + "{0}", e);
                //    }
                //    //Don't bother doing anything
                //}
            }
        }
        #endregion
    }
}