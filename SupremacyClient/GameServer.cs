//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Annotations;
using Supremacy.Game;
using Supremacy.Types;
using Supremacy.Utility;
using Supremacy.WCF;
using System;
using System.Threading;

namespace Supremacy.Client
{
    [UsedImplicitly]
    public class GameServer : MarshalByRefObject, IGameServer
    {
        #region Constants
        protected const string ServiceDomainName = "Supremacy Service Domain";
        #endregion

        #region Fields
        private readonly object _serviceLock;
        private readonly IUnhandledExceptionHandler _unhandledExceptionHandler;

        private bool _isDisposed;
        private bool _isServiceLoaded;
        private AppDomain _serviceDomain;
        private SupremacyServiceHost _serviceHost;
        #endregion

        #region Constructors and Finalizers
        public GameServer(
            [NotNull] IUnhandledExceptionHandler unhandledExceptionHandler)
        {
            _serviceLock = new object();
            _unhandledExceptionHandler = unhandledExceptionHandler ?? throw new ArgumentNullException("unhandledExceptionHandler");
        }
        #endregion

        #region Public and Protected Methods
        protected void CheckDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException("GameServer");
            }
        }

        protected void CreateServiceHost()
        {
            CheckDisposed();

            lock (_serviceLock)
            {
                if (_isServiceLoaded)
                {
                    return;
                }

                try
                {
                    SupremacyServiceHost serviceHost = new SupremacyServiceHost();
                    HookServiceHostEventHandlers(serviceHost);
                    _ = Interlocked.CompareExchange(ref _serviceHost, serviceHost, null);
                    _isServiceLoaded = true;
                }
                catch (Exception e)
                {
                    GameLog.Server.General.ErrorFormat("Exception occurred while creating WCF service host: {0}", e.Message);
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
                {
                    return;
                }

                SupremacyServiceHost serviceHost = Interlocked.Exchange(ref _serviceHost, null);
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
                    GameLog.Server.General.ErrorFormat("Exception occurred while stopping WCF service: {0}", e.Message);
                }
                finally
                {
                    _isServiceLoaded = false;
                }

                AppDomain serviceDomain = Interlocked.CompareExchange(ref _serviceDomain, null, null);
                if (serviceDomain == null)
                {
                    return;
                }

                try
                {
                    serviceDomain.UnhandledException -= OnServiceDomainUnhandledException;
                    GCHelper.Collect();
                    AppDomain.Unload(serviceDomain);
                }
                catch (Exception e)
                {
                    GameLog.Server.General.ErrorFormat("Exception occurred while unloading WCF service AppDomain: {0}", e.Message);
                }
            }
        }

        protected void HookServiceHostEventHandlers([NotNull] SupremacyServiceHost serviceHost)
        {
            if (serviceHost == null)
            {
                throw new ArgumentNullException("serviceHost");
            }

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
                if (!IsRunning)
                {
                    return;
                }
            }

            Started?.Invoke(EventArgs.Empty);
        }

        protected void UnhookServiceHostEventHandlers(SupremacyServiceHost serviceHost)
        {
            if (serviceHost == null)
            {
                throw new ArgumentNullException("serviceHost");
            }

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
            {
                return;
            }

            try
            {
                Stop();
            }
            catch (Exception e)
            {
                GameLog.Client.General.Error(e);
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

        public bool IsRunning { get; private set; }

        public void Start([CanBeNull] GameOptions gameOptions, bool allowRemoteConnections)
        {
            CheckDisposed();

            if (IsRunning)
            {
                return;
            }

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
                {
                    return;
                }

                if (!IsRunning)
                {
                    raiseStopped = false;
                }

                IsRunning = false;

                try
                {
                    DestroyServiceHost();
                }
                catch (Exception e)
                {
                    GameLog.Client.General.Error(e);
                }
            }

            if (!raiseStopped)
            {
                return;
            }

            Stopped?.Invoke(EventArgs.Empty);
        }
        #endregion

        #region Private Methods
        private void OnServiceClosed(EventArgs args)
        {
            if (!IsRunning)
            {
                return;
            }

            Stop();
        }

        private void OnServiceFaulted(EventArgs args)
        {
            lock (_serviceLock)
            {
                if (!IsRunning)
                {
                    return;
                }

                IsRunning = false;
            }

            Faulted?.Invoke(EventArgs.Empty);
        }

        private void OnServiceOpened(EventArgs args)
        {
            OnStarted();
        }
        #endregion
    }
}