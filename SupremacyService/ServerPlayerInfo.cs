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

        public Player Player
        {
            get { return _player; }
        }

        public ISupremacyCallback Callback
        {
            get { return _callback; }
        }

        public OperationContext Session
        {
            get { return _session; }
        }

        public IScheduler Scheduler
        {
            get { return _scheduler.Value; }
        }
    }
}