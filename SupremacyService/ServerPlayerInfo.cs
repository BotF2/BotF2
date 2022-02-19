using System;
using System.Concurrency;
using System.ServiceModel;

using Supremacy.Game;

namespace Supremacy.WCF
{
    internal sealed class ServerPlayerInfo
    {
        private readonly Lazy<IScheduler> _scheduler;

        internal ServerPlayerInfo(Player player, ISupremacyCallback callback, OperationContext session, IScheduler scheduler)
        {
            Player = player;
            Callback = callback;
            Session = session;
            _scheduler = new Lazy<IScheduler>(
                () => scheduler ??
                      new EventLoopScheduler("ServerCallbackScheduler[" + player.Name + "]"));
        }

        public Player Player { get; }

        public ISupremacyCallback Callback { get; }

        public OperationContext Session { get; }

        public IScheduler Scheduler => _scheduler.Value;
    }
}