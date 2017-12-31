// GameServer.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Threading;

using Microsoft.Practices.Composite.Logging;

using Supremacy.Annotations;
using Supremacy.Game;
using Supremacy.Types;
using Supremacy.Utility;
using Supremacy.WCF;

namespace Supremacy.Client
{
    [UsedImplicitly]
    public class GameServer : MarshalByRefObject, IGameServer
    {
        #region Constants
        protected const string ServiceDomainName = "Supremacy Service Domain";
        #endregion

        #region Fields
        private readonly ILoggerFacade _logger;
        private readonly object _serviceLock;
        private readonly IUnhandledExceptionHandler _unhandledExceptionHandler;

        private bool _isDisposed;
        private bool _isServiceLoaded;
        private bool _isServiceRunning;
        private AppDomain _serviceDomain;
        private SupremacyServiceHost _serviceHost;
        #endregion

        #region Constructors and Finalizers
        public GameServer(
            [NotNull] IUnhandledExceptionHandler unhandledExceptionHandler,
            [NotNull] ILoggerFacade logger)
        {
            if (unhandledExceptionHandler == null)
                throw new ArgumentNullException("unhandledExceptionHandler");
            if (logger == null)
                throw new ArgumentNullException("logger");
            _serviceLock = new object();
            _unhandledExceptionHandler = unhandledExceptionHandler;
            _logger = logger;
        }
        #endregion

        #region Public and Protected Methods
        protected void CheckDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException("GameServer");
        }

        protected void CreateServiceHost()
        {
            CheckDisposed();

            lock (_serviceLock)
            {
                if (_isServiceLoaded)
                    return;

                try
                {
                    if (Interlocked.CompareExchange(ref _serviceDomain, null, null) == null)
                    {
                        //var serviceDomain = AppDomain.CreateDomain(
                        //    ServiceDomainName,
                        //    null,
                        //    AppDomain.CurrentDomain.SetupInformation);
                        //serviceDomain.UnhandledException += OnServiceDomainUnhandledException;
                        //serviceDomain.Load("SupremacyNative");
                        //serviceDomain.Load("SupremacyCore");
                        //serviceDomain.Load("SupremacyService");
                        //Interlocked.CompareExchange(ref _serviceDomain, serviceDomain, null);
                    }
                }
                catch (Exception e)
                {
                    _logger.Log(
                        "Exception occurred while creating WCF service AppDomain: " + e.Message,
                        Category.Exception,
                        Priority.High);
                    throw new SupremacyException("Failed to create AppDomain for WCF service.", e);
                }

                try
                {
                    //var serviceHost = (SupremacyServiceHost)_serviceDomain.CreateInstanceAndUnwrap(
                    //                                            "SupremacyService",
                    //                                            "Supremacy.WCF.SupremacyServiceHost");
                    var serviceHost = new SupremacyServiceHost();
                    HookServiceHostEventHandlers(serviceHost);
                    Interlocked.CompareExchange(ref _serviceHost, serviceHost, null);
                    _isServiceLoaded = true;
                }
                catch (Exception e)
                {
                    _logger.Log(
                        "Exception occurred while creating WCF service host: " + e.Message,
                        Category.Exception,
                        Priority.High);
                    throw new SupremacyException("Failed to create WCF service host.", e);
                }
            }
        }

        private void OnServiceDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (!(e.ExceptionObject is AppDomainUnloadedException))
            {
                _unhandledExceptionHandler.HandleError((Exception)e.ExceptionObject);
            }
        }

        protected void DestroyServiceHost()
        {
            CheckDisposed();

            lock (_serviceLock)
            {
                if (!_isServiceLoaded)
                    return;

                var serviceHost = Interlocked.Exchange(ref _serviceHost, null);
                try
                {
                    if (serviceHost != null)
                    {
                        UnhookServiceHostEventHandlers(serviceHost);
                        serviceHost.StopService();
                    }
                }
                catch (Exception e)
                {
                    _logger.Log(
                        "Exception occurred while stopping WCF service: " + e.Message,
                        Category.Warn,
                        Priority.Low);
                }
                finally
                {
                    _isServiceLoaded = false;
                }

                var serviceDomain = Interlocked.CompareExchange(ref _serviceDomain, null, null);
                if (serviceDomain == null)
                    return;

                try
                {
                    serviceDomain.UnhandledException -= OnServiceDomainUnhandledException;
                    GCHelper.Collect();
                    AppDomain.Unload(serviceDomain);
                }
                catch (Exception e)
                {
                    _logger.Log(
                        "Exception occurred while unloading WCF service AppDomain: " + e.Message,
                        Category.Warn,
                        Priority.Low);
                }
            }
        }

        protected void HookServiceHostEventHandlers([NotNull] SupremacyServiceHost serviceHost)
        {
            if (serviceHost == null)
                throw new ArgumentNullException("serviceHost");

            CheckDisposed();

            serviceHost.ServiceOpened += OnServiceOpened;
            serviceHost.ServiceFaulted += OnServiceFaulted;
            serviceHost.ServiceClosed += OnServiceClosed;
        }

        protected void OnStarted()
        {
            CheckDisposed();

            lock (_serviceLock)
            {
                if (!_isServiceRunning)
                    return;
            }

            var handler = Started;
            if (handler != null)
                handler(EventArgs.Empty);
        }

        protected void UnhookServiceHostEventHandlers(SupremacyServiceHost serviceHost)
        {
            if (serviceHost == null)
                throw new ArgumentNullException("serviceHost");

            CheckDisposed();

            serviceHost.ServiceOpened -= OnServiceOpened;
            serviceHost.ServiceFaulted -= OnServiceFaulted;
            serviceHost.ServiceClosed -= OnServiceClosed;
        }
        #endregion

        #region Implementation of IDisposable
        public void Dispose()
        {
            if (_isDisposed)
                return;

            try
            {
                Stop();
            }
            catch (Exception e) //ToDo: Just log or additional handling necessary?
            {
                GameLog.LogException(e);
            }
            finally
            {
                _isDisposed = true;
            }
        }
        #endregion

        #region Implementation of IGameServer
        public event Action<EventArgs> Faulted;
        public event Action<EventArgs> Started;
        public event Action<EventArgs> Stopped;

        public bool IsRunning
        {
            get { return _isServiceRunning; }
        }

        public void Start([CanBeNull] GameOptions gameOptions, bool allowRemoteConnections)
        {
            CheckDisposed();

            if (_isServiceRunning)
                return;

            try
            {
                CreateServiceHost();
                _serviceHost.StartService(!allowRemoteConnections);
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        public void Stop()
        {
            bool raiseStopped = true;

            lock (_serviceLock)
            {
                if (!_isServiceLoaded)
                    return;

                if (!_isServiceRunning)
                    raiseStopped = false;

                _isServiceRunning = false;

                try
                {
                    DestroyServiceHost();
                }
                catch (Exception e) //ToDo: Just log or additional handling necessary?
                {
                    GameLog.LogException(e);
                }
            }

            if (!raiseStopped)
                return;

            var handler = Stopped;
            if (handler != null)
                handler(EventArgs.Empty);
        }
        #endregion

        #region Private Methods
        private void OnServiceClosed(EventArgs args)
        {
            if (!_isServiceRunning)
                return;
            Stop();
        }

        private void OnServiceFaulted(EventArgs args)
        {
            lock (_serviceLock)
            {
                if (!_isServiceRunning)
                    return;
                _isServiceRunning = false;
            }

            var handler = Faulted;
            if (handler != null)
                handler(EventArgs.Empty);
        }

        private void OnServiceOpened(EventArgs args)
        {
            OnStarted();
        }
        #endregion
    }
}