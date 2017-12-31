// ClientUnhandledExceptionHandler.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Windows;

using Supremacy.Annotations;
using Supremacy.Client.Services;
using Supremacy.Types;

namespace Supremacy.Client
{
    internal sealed class ClientUnhandledExceptionHandler : IUnhandledExceptionHandler
    {
        private readonly IGameErrorService _errorService;

        #region Fields
        private readonly object _syncLock = new object();
        #endregion

        public ClientUnhandledExceptionHandler([NotNull] IGameErrorService errorService)
        {
            if (errorService == null)
                throw new ArgumentNullException("errorService");
            _errorService = errorService;
        }

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
                lock (_syncLock)
                {
                    var innerException = exception;

                    while (innerException != null)
                    {
                        Console.Error.WriteLine(innerException.Message);
                        Console.Error.WriteLine();
                        Console.Error.WriteLine(innerException.StackTrace);
                        Console.Error.WriteLine();
                        Console.Error.WriteLine("----------------------------------------");
                        Console.Error.WriteLine();
                        Console.Error.Flush();
                        innerException = innerException.InnerException;
                    }

                    MessageBox.Show(
                        "An unhandled exception has occurred.  Detailed error information is "
                        + "available in the 'Error.txt' file.",
                        "Unhandled Exception",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);

                    Environment.Exit(Environment.ExitCode);
                }
            }
        }
        #endregion
    }
}