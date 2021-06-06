using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Xml;

using Supremacy.Annotations;
using Supremacy.Collections;
using Supremacy.Utility;

// ReSharper disable InvocationIsSkipped

namespace Supremacy.Text
{
    [TypeConverter(typeof(RichTextConverter))]
    public class RichText : ICloneable
    {
        private readonly ArrayList _data = new ArrayList();
        private List<RichString> _parts = new List<RichString>(1);
        private string _string = string.Empty;

        public static RichText Empty => new RichText();

        public Brush BackgroundBrush
        {
            get
            {
                Brush color = default(Brush);
                foreach (RichString richString in _parts)
                {
                    if (color == default(Brush))
                        color = richString.Style.Background;
                    else if (color != richString.Style.Background)
                        return default(Brush);
                }
                return color;
            }
        }

        public Brush ForegroundBrush
        {
            get
            {
                Brush color = default(Brush);
                foreach (RichString richString in _parts)
                {
                    if (color == default(Brush))
                        color = richString.Style.Foreground;
                    else if (color != richString.Style.Foreground)
                        return default(Brush);
                }
                return color;
            }
        }

        public bool IsEmpty => string.IsNullOrEmpty(_string);

        public int Length => _string.Length;

        public string Text
        {
            get { return _string; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                
                AssertValid();

                _string = string.Empty;
                _parts.Clear();

                Append(value);
            }
        }

        public RichText(string text, TextStyle style)
        {
            if (!string.IsNullOrEmpty(text))
            {
                _string = text;
                _parts.Add(new RichString(0, text.Length, style, this));
            }

            AssertValid();
        }

        public RichText(string text)
            : this(text, TextStyle.Default) { }

        public RichText()
            : this("", TextStyle.Default) { }

        private RichText(string text, ICollection<RichString> parts)
        {
            if (text == null)
                throw new ArgumentNullException("text");
            if (parts == null)
                throw new ArgumentNullException("parts");

            _string = text;
            _parts = new List<RichString>(parts.Count);

            int offset = 0;

            foreach (RichString part in parts)
            {
                RichString partCopy = new RichString(offset, part.Length, part.Style, this);
                _parts.Add(partCopy);
                offset += partCopy.Length;
            }

            AssertValid();
        }

        [CanBeNull]
        public static implicit operator string([CanBeNull] RichText richtext)
        {
            if (richtext == null)
                return null;
            return richtext.Text;
        }

        [CanBeNull]
        public static implicit operator RichText([CanBeNull] string text)
        {
            if (text == null)
                return null;
            return new RichText(text);
        }

        [CanBeNull]
        public static RichText operator +([CanBeNull] RichText a, [CanBeNull] RichText b)
        {
            if (a == null)
            {
                if (b != null)
                    return b.Clone();
                return null;
            }
            RichText result = a.Clone();
            if (b != null)
                result.Append(b);
            return result;
        }

        [CanBeNull]
        public static RichText operator +([CanBeNull] RichText a, [CanBeNull] string b)
        {
            if (a == null)
            {
                if (b != null)
                    return new RichText(b);
                return null;
            }
            RichText result = a.Clone();
            if (b != null)
                result.Append(b);
            return result;
        }

        [CanBeNull]
        public static RichText operator +([CanBeNull] string a, [CanBeNull] RichText b)
        {
            if (b == null)
            {
                if (a != null)
                    return new RichText(a);
                return null;
            }
            RichText result = b.Clone();
            if (a != null)
                result.Prepend(a);
            return result;
        }

        public static bool IsNullOrEmpty(RichText richtext)
        {
            if (richtext == null)
                return true;
            return richtext.IsEmpty;
        }

        public RichText Append(string s, TextStyle style)
        {
            if (s == null)
                throw new ArgumentNullException("s");

            if (s.Length == 0)
                return this;

            int originalLength = _string.Length;

            _string = _string + s;
            _parts.Add(new RichString(originalLength, s.Length, style, this));

            AssertValid();

            return this;
        }

        public RichText Append(string s)
        {
            return Append(s, _parts.Count > 0 ? _parts[_parts.Count - 1].Style : TextStyle.Default);
        }

        public RichText Append(RichText richText)
        {
            if (richText == null)
                throw new ArgumentNullException("richText");

            richText.AssertValid();

            int originalLength = _string.Length;

            _string = _string + richText._string;

            foreach (RichString richString in richText._parts)
                _parts.Add(new RichString(originalLength + richString.Offset, richString.Length, richString.Style, this));

            AssertValid();

            return this;
        }

        [Conditional("DEBUG")]
        public void AssertValid()
        {
            int start = 0;
            int previousEnd = 0;

            foreach (RichString richString in _parts)
            {
                if (!ReferenceEquals(richString.RichText, this))
                    throw new InvalidOperationException(string.Format("Invalid RichText: the part #{0} has a questionable parentage.", start));

                if (richString.Offset != previousEnd)
                {
                    if (richString.Offset < previousEnd)
                    {
                        throw new InvalidOperationException(
                            string.Format(
                                "Invalid RichText: the part #{0} starts at {1}, before the end of the previous part at {2}.",
                                start,
                                richString.Offset,
                                previousEnd));
                    }

                    throw new InvalidOperationException(
                        string.Format(
                            "Invalid RichText: the part #{0} has empty space before its start at {1}, after the end of the previous part at {2}.",
                            start,
                            richString.Offset,
                            previousEnd));
                }

                previousEnd += richString.Length;
                richString.AssertValid();
                ++start;
            }

            if (previousEnd == Text.Length)
                return;

            if (previousEnd < Text.Length)
            {
                throw new InvalidOperationException(
                    string.Format(
                        "Invalid RichText: there is some space uncovered by parts at the end, {0} chars long.",
                        Text.Length - previousEnd));
            }

            throw new InvalidOperationException(
                string.Format(
                    "Invalid RichText: the last part overhands the string end by {0} chars.",
                    previousEnd - Text.Length));
        }

        public void Clear()
        {
            Text = string.Empty;
        }

        public RichText Clone()
        {
            return new RichText(_string, _parts);
        }

        public string DumpToString()
        {
            StringBuilder output = new StringBuilder();

            using (XmlWriter writer = XmlWriter.Create(output, XmlWriterEx.WriterSettings))
                DumpToXaml(writer);

            return (output).ToString();
        }

        public void DumpToXaml(XmlWriter writer)
        {
            writer.WriteStartElement("RichText");
            writer.WriteAttributeString("Text", Text);
            writer.WriteStartElement("RichText.Parts");

            foreach (RichString richString in _parts)
                richString.Dump(writer);

            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        [NotNull]
        public IList<RichString> GetFormattedParts()
        {
            return _parts.AsReadOnly();
        }

        public IList<T> GetUserData<T>(int startOffset, int endOffset)
        {
            int length = Length;

            if (endOffset == -1)
                endOffset = length;

            if (startOffset < 0)
                throw new ArgumentOutOfRangeException("startOffset", startOffset, "The starting offset must be non-negative.");

            if (endOffset < 0)
                throw new ArgumentOutOfRangeException("endOffset", endOffset, "The ending offset must be non-negative.");

            if (startOffset > length)
            {
                throw new ArgumentOutOfRangeException(
                    "startOffset",
                    startOffset,
                    string.Format(
                        "The starting offset must fall within the text, whose length is {0}.",
                        length));
            }

            if (endOffset > length)
            {
                throw new ArgumentOutOfRangeException(
                    "endOffset",
                    endOffset,
                    string.Format(
                        "The ending offset must fall within the text, whose length is {0}.",
                        length));
            }

            if (endOffset < startOffset)
            {
                throw new ArgumentException(
                    string.Format(
                        "The starting offset ({0}) must not fall behind the ending offset ({1}).",
                        startOffset,
                        endOffset));
            }

            IList<T> list = (IList<T>)null;

            foreach (TextRangeDataRecord textRangeDataRecord in _data)
            {
                if (textRangeDataRecord.StartOffset <= startOffset &&
                    textRangeDataRecord.EndOffset >= endOffset &&
                    textRangeDataRecord.Object is T)
                {
                    if (list == null)
                        list = new List<T>();
                    list.Add((T)textRangeDataRecord.Object);
                }
            }

            return list ?? ArrayWrapper<T>.Empty;
        }

        public RichText Prepend(string s, TextStyle style)
        {
            if (s == null)
                throw new ArgumentNullException("s");

            if (s.Length == 0)
                return this;

            _string = s + _string;

            int length = s.Length;
            foreach (RichString richString in _parts)
                richString.Offset += length;

            _parts.Insert(0, new RichString(0, s.Length, style, this));

            AssertValid();

            return this;
        }

        public RichText Prepend(string s)
        {
            return Prepend(s, _parts.Count > 0 ? _parts[0].Style : TextStyle.Default);
        }

        public RichText Prepend(RichText richText)
        {
            if (richText == null)
                throw new ArgumentNullException("richText");

            richText.AssertValid();

            _string = richText._string + _string;

            int length = richText._string.Length;
            foreach (RichString richString in _parts)
                richString.Offset += length;

            List<RichString> list = new List<RichString>(richText._parts.Count);
            foreach (RichString richString in richText._parts)
                list.Add(new RichString(richString.Offset, richString.Length, richString.Style, this));

            list.AddRange(_parts);
            _parts = list;

            AssertValid();

            return this;
        }

        public void PutUserData(int startOffset, int endOffset, object data)
        {
            int length = Length;

            if (endOffset == -1)
                endOffset = length;

            if (startOffset < 0)
            {
                throw new ArgumentOutOfRangeException(
                    "startOffset",
                    startOffset,
                    "The starting offset must be non-negative.");
            }

            if (startOffset > length)
            {
                throw new ArgumentOutOfRangeException(
                    "startOffset",
                    startOffset,
                    string.Format(
                        "The starting offset must fall within the text, whose length is {0}.",
                        length));
            }

            if (endOffset < 0)
            {
                throw new ArgumentOutOfRangeException(
                    "endOffset",
                    endOffset,
                    "The ending offset must be non-negative.");
            }

            if (endOffset > length)
            {
                throw new ArgumentOutOfRangeException(
                    "endOffset",
                    endOffset,
                    string.Format(
                        "The ending offset must fall within the text, whose length is {0}.",
                        length));
            }

            if (endOffset < startOffset)
            {
                throw new ArgumentException(
                    string.Format(
                        "The starting offset ({0}) must not fall behind the ending offset ({1}).",
                        startOffset,
                        endOffset));
            }

            _data.Add(new TextRangeDataRecord(startOffset, endOffset, data));
        }

        public void SetBackground(Brush background, int startOffset, int length)
        {
            foreach (RichString richString in GetPartsFromRangeAndSplit(startOffset, length))
            {
                TextStyle style = richString.Style;
                style.Background = background;
                richString.Style = style;
            }
        }

        public void SetBackground(Brush background)
        {
            foreach (RichString richString in _parts)
            {
                TextStyle style = richString.Style;
                style.Background = background;
                richString.Style = style;
            }
        }

        public void SetBrushes(Brush foreground, Brush background)
        {
            AssertValid();

            foreach (RichString richString in _parts)
            {
                TextStyle style = richString.Style;

                style.Background = background;
                style.Foreground = foreground;
                style.EffectBrush = foreground;

                richString.Style = style;
            }
        }

        public void SetBrushes(Brush foreground, Brush background, int startOffset, int length)
        {
            foreach (RichString richString in GetPartsFromRangeAndSplit(startOffset, length))
            {
                TextStyle style = richString.Style;

                style.Background = background;
                style.Foreground = foreground;
                style.EffectBrush = foreground;

                richString.Style = style;
            }
        }

        public void SetForeground(Brush foreground)
        {
            AssertValid();

            foreach (RichString richString in _parts)
            {
                TextStyle style = richString.Style;
                style.Foreground = foreground;
                richString.Style = style;
            }
        }

        public void SetForeground(Brush foreground, int startOffset, int length)
        {
            foreach (RichString richString in GetPartsFromRangeAndSplit(startOffset, length))
            {
                TextStyle style = richString.Style;
                style.Foreground = foreground;
                richString.Style = style;
            }
        }

        public void SetStyle(TextStyle style, int startOffset, int length)
        {
            foreach (RichString richString in GetPartsFromRangeAndSplit(startOffset, length))
                richString.Style = style;
        }

        public void SetStyle(FontStyle style, int startOffset, int length)
        {
            foreach (RichString richString in GetPartsFromRangeAndSplit(startOffset, length))
            {
                richString.Style = new TextStyle(
                    style,
                    richString.Style.FontWeight,
                    richString.Style.Foreground,
                    richString.Style.Background,
                    richString.Style.Effect,
                    richString.Style.EffectBrush);
            }
        }

        public void SetStyle(FontStyle style)
        {
            AssertValid();

            foreach (RichString richString in _parts)
            {
                richString.Style = new TextStyle(
                    style,
                    richString.Style.FontWeight,
                    richString.Style.Foreground,
                    richString.Style.Background,
                    richString.Style.Effect,
                    richString.Style.EffectBrush);
            }
        }

        public void SetWeight(FontWeight weight)
        {
            AssertValid();

            foreach (RichString richString in _parts)
            {
                richString.Style = new TextStyle(
                    richString.Style.FontStyle, 
                    weight,
                    richString.Style.Foreground,
                    richString.Style.Background,
                    richString.Style.Effect,
                    richString.Style.EffectBrush);
            }
        }

        public void SetStyle(TextStyle style)
        {
            string text = Text;
            Clear();
            Append(text, style);
        }

        public void SetStyle(TextEffectStyle effect, Brush effectBrush, int startOffset, int length)
        {
            foreach (RichString richString in GetPartsFromRangeAndSplit(startOffset, length))
            {
                richString.Style = new TextStyle(
                    richString.Style.FontStyle,
                    richString.Style.FontWeight,
                    richString.Style.Foreground,
                    richString.Style.Background,
                    effect,
                    effectBrush);
            }
        }

        public void SetStyle(TextEffectStyle effect, Brush effectBrush)
        {
            AssertValid();
            foreach (RichString richString in _parts)
            {
                richString.Style = new TextStyle(
                    richString.Style.FontStyle,
                    richString.Style.FontWeight,
                    richString.Style.Foreground,
                    richString.Style.Background,
                    effect,
                    effectBrush);
            }
        }

        public RichText[] Split(int offset)
        {
            if (offset < 0 || offset > Length)
            {
                throw new ArgumentOutOfRangeException(
                    "offset",
                    offset,
                    string.Format(
                        "The offset must be non-negative and not above the text length of {0}.",
                        Length));
            }

            if (offset == 0)
            {
                return new[]
                       {
                           new RichText("", TextStyle.Default),
                           Clone()
                       };
            }
            if (offset == Length)
            {
                return new[]
                       {
                           Clone(),
                           new RichText("", TextStyle.Default)
                       };
            }

            AssertValid();

            List<RichString> head = new List<RichString>(_parts.Count);
            List<RichString> tail = new List<RichString>(_parts.Count);

            foreach (RichString part in _parts)
            {
                if (part.Offset + part.Length <= offset)
                {
                    head.Add(part);
                    continue;
                }
                if (part.Offset >= offset)
                {
                    tail.Add(part);
                    continue;
                }

                IList<RichString> brokenPart = BreakString(part, offset - part.Offset, false);

                head.Add(brokenPart[0]);
                tail.Add(brokenPart[1]);
            }

            return new[]
                   {
                       new RichText(Text.Substring(0, offset), head),
                       new RichText(Text.Substring(offset), tail)
                   };
        }

        public RichText Trim(params char[] trimchars)
        {
            return TrimStart(trimchars).TrimEnd(trimchars);
        }

        public RichText TrimEnd(params char[] trimchars)
        {
            string trimmed = Text.TrimEnd(trimchars);
            if (trimmed.Length == Length)
                return this;
            return Split(trimmed.Length)[0];
        }

        public RichText TrimStart(params char[] trimchars)
        {
            string trimmed = Text.TrimStart(trimchars);
            if (trimmed.Length == Length)
                return this;
            return Split(Length - trimmed.Length)[1];
        }

        private IList<RichString> BreakString(RichString part, int nLocalOffset, bool bModifyPartsCollection)
        {
            int length = part.Length;

            if (nLocalOffset < 0 || nLocalOffset > length)
            {
                throw new ArgumentOutOfRangeException(
                    "nLocalOffset",
                    nLocalOffset,
                    string.Format(
                        "The local offset must be non-negative and not above the part length of {0}.",
                        length));
            }

            if (nLocalOffset == 0 || nLocalOffset == length)
                return new[] { part };

            if (!ReferenceEquals(part.RichText, this))
                throw new InvalidOperationException("The given part has a wrong parent.");

            RichString[] richStringArray = new[]
                                  {
                                      new RichString(part.Offset, nLocalOffset, part.Style, this),
                                      new RichString(part.Offset + nLocalOffset, length - nLocalOffset, part.Style, this)
                                  };

            if (bModifyPartsCollection)
            {
                int index = _parts.IndexOf(part);
                _parts.Remove(part);
                _parts.InsertRange(index, richStringArray);
            }

            AssertValid();

            return richStringArray;
        }

        private IEnumerable<RichString> GetPartsFromRangeAndSplit(int startOffset, int length)
        {
            AssertValid();

            if (length == -1)
                length = Length - startOffset;

            if (length == 0)
                return ArrayWrapper<RichString>.Empty;

            if (startOffset < 0)
                throw new ArgumentOutOfRangeException("startOffset", startOffset, "The starting offset must be non-negative.");

            if (length < 0)
                throw new ArgumentOutOfRangeException("length", length, "The length must be non-negative.");

            if (startOffset > Length)
            {
                throw new ArgumentOutOfRangeException(
                    "startOffset",
                    startOffset,
                    string.Format(
                        "The starting offset must fall within the text, whose length is {0}.",
                        Length));
            }

            if (startOffset + length > Length)
            {
                throw new ArgumentException(
                    string.Format(
                        "The offset plus length give {0}+{1}={2}, which is beyond the string length {3}.",
                        (object)startOffset,
                        (object)length,
                        (object)(startOffset + length),
                        (object)Length));
            }

            int end = startOffset + length;
            List<RichString> list = new List<RichString>();

            for (int index = 0; index < _parts.Count; ++index)
            {
                RichString part = _parts[index];
                int offset = part.Offset;
                int partEnd = offset + part.Length;

                if (partEnd <= startOffset || offset >= end)
                    continue;

                if (startOffset > offset && startOffset < partEnd)
                {
                    BreakString(part, startOffset - offset, true);
                    --index;
                    continue;
                }

                if (end > offset && end < partEnd)
                {
                    BreakString(part, end - offset, true);
                    --index;
                    continue;
                }

                list.Add(part);
            }

            return list;
        }

        public override string ToString()
        {
            return _string;
            //return _parts.AggregateString("|", ((builder, part) => builder.Append(part.Text)));
        }

        object ICloneable.Clone()
        {
            return new RichText(_string, _parts);
        }

        private class TextRangeDataRecord
        {
            private readonly int _endOffset;
            private readonly object _object;
            private readonly int _startOffset;

            public int EndOffset => _endOffset;

            public object Object => _object;

            public int StartOffset => _startOffset;

            public TextRangeDataRecord(int startOffset, int endOffset, object @object)
            {
                _startOffset = startOffset;
                _endOffset = endOffset;
                _object = @object;
            }
        }
    }
}

// ReSharper restore InvocationIsSkipped
