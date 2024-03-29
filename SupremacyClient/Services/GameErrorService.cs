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
            _resourceManager = resourceManager ?? throw new ArgumentNullException("resourceManager");
            _dispatcherService = dispatcherService ?? throw new ArgumentNullException("dispatcherService");
        }

        protected static string BuildErrorMessage(Exception exception)
        {
            Exception innerException = exception;
            StringBuilder errorMessage = new StringBuilder();

            while (innerException != null)
            {
                _ = errorMessage.AppendLine(innerException.Message);
                _ = errorMessage.AppendLine();
                _ = errorMessage.AppendLine(innerException.StackTrace);
                _ = errorMessage.AppendLine();
                _ = errorMessage.AppendLine("----------------------------------------");
                _ = errorMessage.AppendLine();
                innerException = innerException.InnerException;
            }

            return errorMessage.ToString();
        }

        public void HandleError([NotNull] Exception exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }

            GameLog.Server.General.Error(BuildErrorMessage(exception));

            string header;
            string message;

            if (exception is GameDataException gameDataException)
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
                     FormattedTextConverter formattedTextConverter = new FormattedTextConverter();
                     TextBlock messageText = new TextBlock { TextWrapping = TextWrapping.Wrap };

                     BindingHelpers.SetInlines(
                         messageText,
                         (Inline[])formattedTextConverter.Convert(message));

                     _ = MessageDialog.Show(
                         header,
                         messageText,
                         MessageDialogButtons.Ok);

                     if (!(exception is SupremacyException supremacyException))
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