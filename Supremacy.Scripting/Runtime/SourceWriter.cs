using System.IO;
using System.Text;

using Microsoft.Scripting;

namespace Supremacy.Scripting.Runtime
{
    public class SourceWriter : TextWriter
    {
        public const string DefaultTabString = "    ";

        private readonly string _tabString;
        private int _indentLevel;
        private int _line;
        private int _lineIndex;
        private int _newLineMatchIndex;
        private bool _tabsPending;
        private int _writerIndex;

        public SourceWriter(TextWriter writer)
            : this(writer, "    ") { }

        public SourceWriter(TextWriter writer, string tabString)
        {
            _line = 1;
            InnerWriter = writer;
            _tabString = tabString;
        }

        public override Encoding Encoding => InnerWriter.Encoding;

        public int Indent
        {
            get => _indentLevel;
            set => _indentLevel = (value < 0) ? 0 : value;
        }

        public TextWriter InnerWriter { get; }

        public override string NewLine
        {
            get => InnerWriter.NewLine;
            set => InnerWriter.NewLine = value;
        }

        public SourceLocation Location => new SourceLocation(_writerIndex, _line, _writerIndex - _lineIndex + 1);

        public override void Close()
        {
            InnerWriter.Close();
        }

        public override void Flush()
        {
            InnerWriter.Flush();
        }

        public void OutputTabs()
        {
            if (!_tabsPending)
            {
                return;
            }

            for (int i = 0; i < _indentLevel; i++)
            {
                InnerWriter.Write(_tabString);
            }

            _writerIndex += _indentLevel * _tabString.Length;
            _tabsPending = false;
        }

        public override void Write(char value)
        {
            OutputTabs();

            InnerWriter.Write(value);
            _writerIndex++;

            if (value == CoreNewLine[_newLineMatchIndex])
            {
                _newLineMatchIndex++;
                if (_newLineMatchIndex == CoreNewLine.Length)
                {
                    _line++;
                    _lineIndex = _writerIndex;
                    _newLineMatchIndex = 0;
                }
            }
            else
            {
                _newLineMatchIndex = 0;
            }
        }

        public override void WriteLine()
        {
            base.WriteLine();
            _tabsPending = true;
        }

        public override void WriteLine(string value)
        {
            Write(value);
            WriteLine();
        }

        public void Write(char c, int count)
        {
            for (int i = 0; i < count; i++)
            {
                Write(c);
            }
        }
    }
}