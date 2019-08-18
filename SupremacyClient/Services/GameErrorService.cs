//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Avalon.Windows.Converters;
using Avalon.Windows.Utility;
using Supremacy.Annotations;
using Supremacy.Client.Commands;
using Supremacy.Client.Dialogs;
using Supremacy.Resources;
using Supremacy.Types;
using Supremacy.Utility;
using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Supremacy.Client.Services
{
    public class GameErrorService : IGameErrorService
    {
        private readonly IResourceManager _resourceManager;
        private readonly IDispatcherService _dispatcherService;

        public GameErrorService(
            [NotNull] IResourceManager resourceManager,
            [NotNull] IDispatcherService dispatcherService)
        {
            if (resourceManager == null)
                throw new ArgumentNullException("resourceManager");
            if (dispatcherService == null)
                throw new ArgumentNullException("dispatcherService");
            _resourceManager = resourceManager;
            _dispatcherService = dispatcherService;
        }

        protected static string BuildErrorMessage(Exception exception)
        {
            var innerException = exception;
            var errorMessage = new StringBuilder();

            while (innerException != null)
            {
                errorMessage.AppendLine(innerException.Message);
                errorMessage.AppendLine();
                errorMessage.AppendLine(innerException.StackTrace);
                errorMessage.AppendLine();
                errorMessage.AppendLine("----------------------------------------");
                errorMessage.AppendLine();
                innerException = innerException.InnerException;
            }

            return errorMessage.ToString();
        }

        public void HandleError([NotNull] Exception exception)
        {
            if (exception == null)
                throw new ArgumentNullException("exception");

            GameLog.Server.General.Error(BuildErrorMessage(exception));

            string header;
            string message;

            var gameDataException = exception as GameDataException;
            if (gameDataException != null)
            {
                header = _resourceManager.GetString("GAME_DATA_ERROR_HEADER");
                message = _resourceManager.GetStringFormat(
                    "GAME_DATA_ERROR_MESSAGE_FORMAT",
                    gameDataException.Message,
                    gameDataException.FileName);
            }
            else
            {
                header = _resourceManager.GetString("GENERIC_ERROR_HEADER");
                message = _resourceManager.GetStringFormat(
                    "GENERIC_ERROR_MESSAGE_FORMAT",
                    exception.Message);
            }

            _dispatcherService.Invoke(
                (Action)
                (() =>
                 {
                     var formattedTextConverter = new FormattedTextConverter();
                     var messageText = new TextBlock { TextWrapping = TextWrapping.Wrap };

                     BindingHelpers.SetInlines(
                         messageText,
                         (Inline[])formattedTextConverter.Convert(message));
                     
                     MessageDialog.Show(
                         header,
                         messageText,
                         MessageDialogButtons.Ok);

                     var supremacyException = exception as SupremacyException;
                     if (supremacyException == null)
                     {
                         ClientCommands.Exit.Execute(false);
                         return;

                     }
                     switch (supremacyException.Action)
                     {
                         case SupremacyExceptionAction.Continue:
                             break;

                         case SupremacyExceptionAction.Exit:
                             ClientCommands.Exit.Execute(false);
                             break;

                         default:
                         case SupremacyExceptionAction.Undefined:
                         case SupremacyExceptionAction.Disconnect:
                             ClientCommands.EndGame.Execute(false);
                             break;
                     }
                 }));
        }
    }
}