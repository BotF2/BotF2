using log4net.Appender;
using log4net.Core;

using Supremacy.Messaging;
using System;
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
                    string _text = "check for missing > System.Reactive.dll (version 1.0.2563) + xaml-file in your \\lib-Folder"
                        + Environment.NewLine
                        + "check for missing Microsoft.Practices.ServiceLocation.dll in your \\lib-Folder"
                        + Environment.NewLine
                        + Environment.NewLine
                        + "On first run in VS this crashes - do a manual copy from an old \\lib-folder into the new one !";
                    MessageBox.Show(_text);
                }

            }
        }

        public string Name { get; set; }
    }
}