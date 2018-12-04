// GameObjectIDService.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;

using Supremacy.Annotations;
using Supremacy.Client.Events;
using Supremacy.Game;
using Supremacy.Client.Context;

namespace Supremacy.Client.Services
{
    public class GameObjectIDService : IGameObjectIDService
    {
        #region Fields
        private readonly IAppContext _appContext;
        #endregion

        #region Constructors and Finalizers
        public GameObjectIDService([NotNull] IAppContext appContext)
        {
            if (appContext == null)
                throw new ArgumentNullException("appContext");
            _appContext = appContext;
        }
        #endregion


        #region Implementation of IGameObjectIDService
        public int? GetNewObjectID()
        {
            var args = new GameObjectIDRequestEventArgs(
                _appContext,
                _appContext.CurrentGame);

            ClientEvents.GameObjectIDRequested.Publish(args);

            var waitStart = DateTime.Now;
            var timeout = TimeSpan.FromSeconds(5);
            while (!args.WaitHandle.WaitOne(timeout))
            {
                if ((DateTime.Now - waitStart) > timeout)
                    break;
            }

            return args.Value;
        }
        #endregion
    }
}