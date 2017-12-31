using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using System.Windows.Media;

using Supremacy.Client.Input;

using log4net.Core;
using log4net.Layout;

using Supremacy.Client.Commands;
using Supremacy.Messaging;
using Supremacy.Utility;

namespace Supremacy.Client
{
    public partial class ClientConsole
    {
        private readonly PatternLayout _patternLayout;
        private readonly ClientConsoleWriter _errorWriter;
        private readonly ClientConsoleWriter _warningWriter;
        private readonly ClientConsoleWriter _defaultWriter;
        private readonly Paragraph _paragraph;
        private readonly DelegateCommand<string> _consoleCommand;

        public ClientConsole()
        {
            InitializeComponent();

            _patternLayout = new PatternLayout("%timestamp [%thread] %-5level %logger - %message%newline" /*+ "%exception"*/);
            _errorWriter = new ClientConsoleWriter(this, Brushes.Red);
            _warningWriter = new ClientConsoleWriter(this, Brushes.Yellow);
            _defaultWriter = new ClientConsoleWriter(this, Brushes.White);

            _paragraph = (Paragraph)ConsoleText.Document.Blocks.LastBlock;

            Channel<LoggingEvent>.Public.ObserveOnDispatcher().Subscribe(OnLoggingEvent);
            Channel<ConsoleEvent>.Public.ObserveOnDispatcher().Subscribe(OnConsoleEvent);

            _consoleCommand = new DelegateCommand<string>(_ => CommandText.Clear());

            ClientCommands.ConsoleCommand.RegisterCommand(_consoleCommand);

            IsVisibleChanged += (sender, args) =>
                                     {
                                         if ((bool)args.NewValue)
                                             CommandText.Focus();
                                     };
        }

        private void OnConsoleEvent(ConsoleEvent consoleEvent)
        {
            var sw = new StringWriter();
            var writer = new IndentedTextWriter(sw);

            ObjectDumper.DumpObject(consoleEvent.Output, writer);

            _paragraph.Inlines.Add(new Run(sw.ToString()));
            _paragraph.Inlines.Add(new LineBreak());

            ConsoleText.ScrollToEnd();
        }

        private void OnLoggingEvent(LoggingEvent loggingEvent)
        {
            var writer = _defaultWriter;

            if (loggingEvent.Level.Value >= Level.Error.Value)
                writer = _errorWriter;
            else if (loggingEvent.Level.Value >= Level.Warn.Value)
                writer = _warningWriter;

            _patternLayout.Format(writer, loggingEvent);
        }

        private sealed class ClientConsoleWriter : TextWriter
        {
            private readonly ClientConsole _console;
            private readonly Brush _foreground;

            public ClientConsoleWriter(ClientConsole console, Brush foreground)
            {
                if (console == null)
                    throw new ArgumentNullException("console");

                _console = console;
                _foreground = foreground;
            }

            public override Encoding Encoding
            {
                get { return Encoding.Default; }
            }

            public override void Write(char value)
            {
                AppendText(value.ToString());
            }

            public override void Write(char[] buffer, int index, int count)
            {
                AppendText(new string(buffer, index, count));
                _console.ConsoleText.ScrollToEnd();
            }

            private void AppendText(string value)
            {
                _console._paragraph.Inlines.Add(new Run(value) { Foreground = _foreground });
            }
        }
    }
}
