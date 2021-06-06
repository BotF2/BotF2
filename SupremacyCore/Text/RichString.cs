using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Xml;

using Supremacy.Utility;

// ReSharper disable InvocationIsSkipped

namespace Supremacy.Text
{
    public class RichString : IComparable<RichString>, IComparable
    {
        private readonly RichText _richText;

        private int _offset;
        private int _length;
        private TextStyle _style;

        public string Text
        {
            get
            {
                AssertValid();
                return RichText.Text.Substring(Offset, Length);
            }
        }

        public TextStyle Style
        {
            get { return _style; }
            set { _style = value; }
        }

        public int Offset
        {
            get { return _offset; }
            set
            {
                _offset = value;
                AssertValid();
            }
        }

        public int Length
        {
            get { return _length; }
            set
            {
                _length = value;
                AssertValid();
            }
        }

        public RichText RichText => _richText;

        public RichString(int offset, int length, TextStyle style, RichText richtext)
        {
            if (richtext == null)
                throw new ArgumentNullException("richtext");

            if (length < 0)
                throw new ArgumentOutOfRangeException("length", length, "Length must be non-negative.");

            var parentLength = richtext.Length;

            if (offset < 0 || offset > parentLength)
            {
                throw new ArgumentOutOfRangeException(
                    "offset",
                    offset,
                    string.Format(
                        "The offset must be within the parent RichText string of length {1}, “{0}”.",
                        richtext.Text,
                        parentLength));
            }

            if (offset + length > parentLength)
            {
                throw new ArgumentOutOfRangeException(
                    "length",
                    length,
                    string.Format(
                        "The substring (offset={2}, length={3}) must fall within the parent RichText string of length {1}, “{0}”.",
                        (object)richtext.Text,
                        (object)parentLength,
                        (object)offset,
                        (object)length));
            }

            _offset = offset;
            _length = length;
            _style = style;
            _richText = richtext;
        }

        [Conditional("JET_MODE_ASSERT")]
        public void AssertValid()
        {
            if (_richText == null)
                throw new InvalidOperationException("RichText is Null.");

            if (_length < 0)
                throw new ArgumentOutOfRangeException(string.Format("Length {0} must be non-negative.", _length));

            var parentLength = _richText.Length;

            if (_offset < 0 || _offset > parentLength)
            {
                throw new InvalidOperationException(
                    string.Format(
                        "The offset {2} must be within the parent RichText string of length {1}, “{0}”.",
                        _richText.Text,
                        parentLength,
                        _offset));
            }

            if (_offset + _length <= parentLength)
                return;

            throw new ArgumentOutOfRangeException(
                string.Format(
                    "The substring (offset={2}, length={3}) must fall within the parent RichText string of length {1}, “{0}”.",
                    (object)_richText.Text,
                    (object)parentLength,
                    (object)_offset,
                    (object)_length));
        }

        public void Dump(XmlWriter writer)
        {
            writer.WriteStartElement("RichString");
            writer.WriteAttributeString("Length", Length.ToString(CultureInfo.InvariantCulture));
            writer.WriteStartElement("RichString.Style");
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        public int CompareTo(object obj)
        {
            return CompareTo(obj as RichString);
        }

        public int CompareTo(RichString other)
        {
            if (other == null)
                return -1;
            if (other == this)
                return 0;
            if (other.RichText != RichText)
                return -1;
            return other.Offset - Offset;
        }

        public override string ToString()
        {
            var output = new StringBuilder();
            using (var writer = XmlWriter.Create(output, XmlWriterEx.WriterSettings))
                Dump(writer);
            return output.ToString();
        }
    }
}

// ReSharper restore InvocationIsSkipped
