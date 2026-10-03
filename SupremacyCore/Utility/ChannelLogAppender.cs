using log4net.Appender;
using log4net.Core;

using Supremacy.Messaging;
using System.Windows;

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
                {
                    return;
                }

                _isClosed = true;
            }
        }

        public void DoAppend(LoggingEvent loggingEvent)
        {
            lock (_syncLock)
            {
                if (_isClosed)
                {
                    return;
                }

                try
                {
                Channel.Publish(loggingEvent);
                }
                catch
                {
                    MessageBox.Show("On first run in VS this crashes - do a manual copy from an old \\lib-folder into the new one !");
                }

            }
        }

        public string Name { get; set; }
    }
}