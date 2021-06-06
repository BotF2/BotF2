using System;
using System.Concurrency;

namespace Supremacy.Threading
{
    public static class Scheduler
    {
        private static readonly Lazy<EventLoopScheduler> _clientEventLoop;

        public static IScheduler ClientEventLoop => _clientEventLoop.Value;

        static Scheduler()
        {
            _clientEventLoop = new Lazy<EventLoopScheduler>(() => new EventLoopScheduler("ClientEventLoop"));
        }
    }
}