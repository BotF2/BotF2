using System;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Xaml;
using System.Xml;

using Supremacy.Utility;

namespace Supremacy.Text
{
    public struct TextStyle : IEquatable<TextStyle>
    {
        private static readonly TextStyle _hiddenStyle = new TextStyle(FontStyles.Normal, FontWeights.Normal, null, null, TextEffectStyle.None, null);

        private FontStyle _fontStyle;
        private FontWeight _fontWeight;
        private Brush _background;
        private Brush _effectBrush;

        public FontStyle FontStyle
        {
            get => _fontStyle;
            set => _fontStyle = value;
        }

        public FontWeight FontWeight
        {
            get => _fontWeight;
            set => _fontWeight = value;
        }

        [DefaultValue(null)]
        public Brush Foreground { get; set; }

        [DefaultValue(null)]
        public Brush Background
        {
            get => _background;
            set => _background = value;
        }

        [DefaultValue(TextEffectStyle.None)]
        public TextEffectStyle Effect { get; set; }

        [DefaultValue(null)]
        public Brush EffectBrush
        {
            get => _effectBrush;
            set => _effectBrush = value;
        }

        public static TextStyle Default { get; } = new TextStyle(FontStyles.Normal, FontWeights.Normal, DefaultForeground, DefaultBackground);

        public static TextStyle Hidden => _hiddenStyle;

        public static Brush DefaultForeground => null;

        public static Brush DefaultBackground => null;

        public static Brush DefaultEffectBrush => null;

        static TextStyle() { }

        public TextStyle(FontStyle fontStyle, FontWeight fontWeight, Brush foreground, Brush background)
        {
            _fontStyle = fontStyle;
            _fontWeight = fontWeight;
            Foreground = foreground;
            _background = background;
            Effect = TextEffectStyle.None;
            _effectBrush = null;
        }

        public TextStyle(FontStyle fontStyle, FontWeight fontWeight, Brush foreground)
        {
            _fontStyle = fontStyle;
            _fontWeight = fontWeight;
            Foreground = foreground;
            _background = null;
            Effect = TextEffectStyle.None;
            _effectBrush = null;
        }

        public TextStyle(FontStyle fontStyle, FontWeight fontWeight, Brush foreground, Brush background, TextEffectStyle effect, Brush effectBrush)
        {
            _fontStyle = fontStyle;
            _fontWeight = fontWeight;
            Foreground = foreground;
            _background = background;
            Effect = effect;
            _effectBrush = effectBrush;
        }

        public TextStyle(FontStyle fontStyle, FontWeight fontWeight)
        {
            this = new TextStyle(fontStyle, fontWeight, null, null, TextEffectStyle.None, null);
        }

        public TextStyle(Brush foregroundBrush, Brush backgroundBrush)
        {
            this = new TextStyle(FontStyles.Normal, FontWeights.Normal, foregroundBrush, backgroundBrush);
        }

        public static bool operator ==(TextStyle left, TextStyle right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TextStyle left, TextStyle right)
        {
            return !left.Equals(right);
        }

        public static TextStyle FromBackground(Brush brush)
        {
            return new TextStyle(FontStyles.Normal, FontWeights.Normal, null, brush);
        }

        public static TextStyle FromForeground(Brush brush)
        {
            return new TextStyle(FontStyles.Normal, FontWeights.Normal, brush, null);
        }

        public void Dump(XmlWriter writer)
        {
            XamlServices.Save(writer, this);
            //            writer.WriteStartElement("TextStyle");
            //            writer.WriteAttributeString("FontStyle", this.FontWeight.ToString());
            //            writer.WriteAttributeString("FontWeight", this.FontWeight.ToString());
            //            writer.WriteAttributeString("Foreground", this.Foreground.ToString());
            //            writer.WriteAttributeString("Background", this.Background.ToString());
            //            writer.WriteAttributeString("Effect", ((object)this.Effect).ToString());
            //            writer.WriteAttributeString("EffectBrush", this.EffectBrush.ToString());
            //            writer.WriteEndElement();
        }

        public bool Equals(TextStyle other)
        {
            return _fontStyle == other._fontStyle &&
                   _fontWeight == other._fontWeight &&
                   Foreground == other.Foreground &&
                   _background == other._background &&
                   Effect == other.Effect &&
                   _effectBrush == other._effectBrush;

        }

        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            using (XmlWriter writer = XmlWriter.Create(output, XmlWriterEx.WriterSettings))
            {
                Dump(writer);
            }

            return output.ToString();
        }

        public override bool Equals(object obj)
        {
            TextStyle? other = obj as TextStyle?;
            return other.HasValue && Equals(other.Value);
        }

        public override int GetHashCode()
        {

            return 29 *
                   (29 *
                    (29 *
                     (29 *
                      (29 * _fontStyle.GetHashCode()
                       + _fontWeight.GetHashCode())
                      + Foreground.GetHashCode())
                     + _background.GetHashCode())
                    + Effect.GetHashCode())
                   + _effectBrush.GetHashCode();

        }
    }
}