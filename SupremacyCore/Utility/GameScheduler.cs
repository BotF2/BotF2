// GameScheduler.cs
// 
// Copyright (c) 2012 Mike Strobel
// 
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
// 
// All other rights reserved.

using System;
using System.Concurrency;

using Supremacy.Annotations;
using Supremacy.Game;

namespace Supremacy.Utility
{
    public sealed class GameScheduler : IScheduler
    {
        private readonly IScheduler _baseScheduler;
        private readonly Func<GameContext> _gameContextCallback;

        public GameScheduler([NotNull] IScheduler baseScheduler, [NotNull] Func<GameContext> gameContextCallback)
        {
            _baseScheduler = baseScheduler ?? throw new ArgumentNullException("baseScheduler");
            _gameContextCallback = gameContextCallback ?? throw new ArgumentNullException("gameContextCallback");
        }

        public IDisposable Schedule(Action action)
        {
            return _baseScheduler.Schedule(
                () =>
                {
                    var gameContext = _gameContextCallback();
                    if (gameContext != null)
                        GameContext.PushThreadContext(gameContext);

                    try
                    {
                        action();
                    }
                    finally
                    {
                        if (gameContext != null)
                            GameContext.PopThreadContext();
                    }
                });
        }

        public IDisposable Schedule(Action action, TimeSpan dueTime)
        {
            return _baseScheduler.Schedule(
                () =>
                {
                    var gameContext = _gameContextCallback();
                    if (gameContext != null)
                        GameContext.PushThreadContext(gameContext);

                    try
                    {
                        action();
                    }
                    finally
                    {
                        if (gameContext != null)
                            GameContext.PopThreadContext();
                    }
                },
                dueTime);
        }

        public DateTimeOffset Now => _baseScheduler.Now;
    }

    public static class SchedulerExtensions
    {
        public static GameScheduler AsGameScheduler([NotNull] this IScheduler scheduler, [NotNull] Func<GameContext> gameContextCallback)
        {
            if (scheduler == null)
                throw new ArgumentNullException("scheduler");
            if (gameContextCallback == null)
                throw new ArgumentNullException("gameContextCallback");

            return new GameScheduler(scheduler, gameContextCallback);
        }
    }
}