using System;
using System.Concurrency;
using System.ServiceModel;

using Supremacy.Game;

namespace Supremacy.WCF
{
    internal sealed class ServerPlayerInfo
    {
        private readonly Player _player;
        private readonly ISupremacyCallback _callback;
        private readonly OperationContext _session;
        private readonly Lazy<IScheduler> _scheduler;

        internal ServerPlayerInfo(Player player, ISupremacyCallback callback, OperationContext session, IScheduler scheduler)
        {
            _player = player;
            _callback = callback;
            _session = session;
            _scheduler = new Lazy<IScheduler>(
                () => scheduler ??
                      new EventLoopScheduler("ServerCallbackScheduler[" + player.Name + "]"));
        }

        public Player Player => _player;

        public ISupremacyCallback Callback => _callback;

        public OperationContext Session => _session;

        public IScheduler Scheduler => _scheduler.Value;
    }
}