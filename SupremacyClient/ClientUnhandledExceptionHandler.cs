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
        private IGameErrorService _errorService;
        //private const string _reportErrorURL = "https://www.exultantmonkey.co.uk/BotF2/report-error.php";
        

        #region Fields
        private readonly object _syncLock = new object();
        private string _text;
        private readonly string newline = Environment.NewLine;

        #endregion

        #region Implementation of IUnhandledExceptionHandler
        public void HandleError(Exception exception)
        {
            if (exception is AppDomainUnloadedException)
            {
                return;
            }

            _errorService = null;

            if (exception is SupremacyException supremacyException)
            {
                _errorService.HandleError(supremacyException);
                Environment.Exit(Environment.ExitCode);
            }
            else
            {
                string errors = "";
                lock (_syncLock)
                {
                    Exception innerException = exception;

                    while (innerException != null)
                    {
                        errors += string.Format("{0}\r\n", innerException.Message);
                        errors += string.Format("{0}\r\n", innerException.StackTrace);
                        errors += "----------------------------------------\r\n";
                        innerException = innerException.InnerException;
                    }

                    Console.Error.WriteLine(errors);
                    Console.Error.Flush();
                    _ = MessageBox.Show(
                        "9999: An unhandled exception has occurred.  Detailed error information is "
                        + "available in the 'Log.txt' file. Missing files could be the reason as well."
                        + " Press OK and have a look to Log.txt",
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
            using (WebClient client = new WebClient())
            {
                NameValueCollection values = new NameValueCollection
                {
                    ["Version"] = ClientApp.ClientVersion.ToString(),
                    ["Title"] = stackTrace.Split('\n')[0],
                    ["StackTrace"] = stackTrace
                };

                _text = newline + newline
                    + DateTime.Now + " #### ERROR " /*+ Environment.NewLine*/
                    + "GAME-VERSION:;" + ClientApp.ClientVersion.ToString() + newline + newline
                    + "ERROR TITLE:;" + stackTrace.Split('\n')[0] + newline + newline
                    + "StackTrace complete:;" + stackTrace;

                GameLog.Core.General.ErrorFormat(_text, ""); // "" for avoiding message "argument missing" for log4net
                Console.WriteLine(_text);

                try
                {
                    _ = System.Diagnostics.Process.Start(".\\Resources\\reportError.bat");
                    // batch opens a web site with email function, 
                    // but just if batch file is activated = rem is removed before web adress
                    // opens Win10 mail function which might be connected to Google Mail account or any other
                }
                catch (Exception ex) 
                {
                    _text = newline + newline
                        + DateTime.Now + " #### ERROR 9998 - a missing file could be the reason as well" + newline; 
                    Console.WriteLine(_text);
                    GameLog.Core.General.ErrorFormat(_text, ""); // "" for avoiding message "argument missing" for log4net


                    throw ex; 
                }

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