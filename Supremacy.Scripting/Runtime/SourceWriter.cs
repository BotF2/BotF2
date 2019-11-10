using System.IO;
using System.Text;

using Microsoft.Scripting;

namespace Supremacy.Scripting.Runtime
{
    public class SourceWriter : TextWriter
    {
        public const string DefaultTabString = "    ";

        private readonly string _tabString;
        private readonly TextWriter _writer;

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
            _writer = writer;
            _tabString = tabString;
        }

        public override Encoding Encoding
        {
            get { return _writer.Encoding; }
        }

        public int Indent
        {
            get { return _indentLevel; }
            set { _indentLevel = (value < 0) ? 0 : value; }
        }

        public TextWriter InnerWriter
        {
            get { return _writer; }
        }

        public override string NewLine
        {
            get { return _writer.NewLine; }
            set { _writer.NewLine = value; }
        }

        public SourceLocation Location
        {
            get { return new SourceLocation(_writerIndex, _line, (_writerIndex - _lineIndex) + 1); }
        }

        public override void Close()
        {
            _writer.Close();
        }

        public override void Flush()
        {
            _writer.Flush();
        }

        public void OutputTabs()
        {
            if (!_tabsPending)
                return;
            
            for (var i = 0; i < _indentLevel; i++)
                _writer.Write(_tabString);
            
            _writerIndex += _indentLevel * _tabString.Length;
            _tabsPending = false;
        }

        public override void Write(char value)
        {
            OutputTabs();

            _writer.Write(value);
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
                Write(c);
        }
    }
}