using log4net.Appender;
using log4net.Core;

using Supremacy.Messaging;
using System;

namespace Supremacy.Utility
{
    public class ChannelLogAppender : IAppender
    {
        private readonly object _syncLock;
        private bool _isClosed;

        public ChannelLogAppender()
        {
            _syncLock = new object();
        }

        public void Close()
        {
            lock (_syncLock)
            {
                if (_isClosed)
                    return;

                _isClosed = true;
            }
        }

        public void DoAppend(LoggingEvent loggingEvent)
         {
            lock (_syncLock)
            {
                if (_isClosed)
                    return;

                Channel.Publish(loggingEvent);
                //Console.WriteLine("Log.txt: " + loggingEvent.RenderedMessage); // writes every Log.Txt line additional to Console
            }
        }

        public string Name { get; set; }
    }
}