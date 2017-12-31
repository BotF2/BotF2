using System.Text;
using System.Xml;

namespace Supremacy.Utility
{
    public static class XmlWriterEx
    {
        public static readonly XmlWriterSettings WriterSettings = new XmlWriterSettings
                                                                  {
                                                                      ConformanceLevel = ConformanceLevel.Auto,
                                                                      Indent = true,
                                                                      IndentChars = "  ",
                                                                      CloseOutput = false,
                                                                      Encoding = Encoding.UTF8
                                                                  };
/*
        static XmlWriterEx() {}

        public static TWriter Attr<TWriter>([NotNull] this TWriter writer, [NotNull] string name, [NotNull] object value) where TWriter : XmlWriter
        {
            if (writer == null)
                throw new ArgumentNullException("writer");
            if (name == null)
                throw new ArgumentNullException("name");
            if (value == null)
                throw new ArgumentNullException("value");
            writer.WriteAttributeString(name, value.ToString());
            return writer;
        }

        public static XmlWriter CreateWriter([NotNull] Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");
            return XmlWriter.Create(stream, WriterSettings);
        }

        public static void Element([NotNull] this XmlWriter writer, [NotNull] string name, [NotNull] string value)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");
            if (name == null)
                throw new ArgumentNullException("name");
            if (value == null)
                throw new ArgumentNullException("value");
            var length = name.LastIndexOf("::", StringComparison.Ordinal);
            if (length < 0)
                writer.WriteElementString(name, value);
            else
                writer.WriteElementString(name.Substring(length + 2), name.Substring(0, length), value);
        }

        public static void InElement([NotNull] this XmlWriter writer, [NotNull] string name, [InstantHandle] [NotNull] Action nested)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");
            if (name == null)
                throw new ArgumentNullException("name");
            if (nested == null)
                throw new ArgumentNullException("nested");
            var length = name.LastIndexOf("::", StringComparison.Ordinal);
            if (length < 0)
            {
                using (writer.PushElement(name))
                    GameLog.Client.Catch(nested);
            }
            else
            {
                using (writer.PushElementNs(name.Substring(length + 2), name.Substring(0, length)))
                    GameLog.Client.Catch(nested);
            }
        }

        public static void InElementNs([NotNull] this XmlWriter writer, [NotNull] string name, [NotNull] string xmlns, [NotNull] Action nested)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");
            if (name == null)
                throw new ArgumentNullException("name");
            if (xmlns == null)
                throw new ArgumentNullException("xmlns");
            if (nested == null)
                throw new ArgumentNullException("nested");
            using (writer.PushElementNs(name, xmlns))
                GameLog.Client.Catch(nested);
        }

        public static IDisposable PushElement([NotNull] this XmlWriter writer, [NotNull] string name)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");
            if (name == null)
                throw new ArgumentNullException("name");
            return Disposable.CreateBracket((() => writer.WriteStartElement(name)), writer.WriteEndElement);
        }

        public static IDisposable PushElementNs([NotNull] this XmlWriter writer, [NotNull] string name, [NotNull] string xmlns)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");
            if (name == null)
                throw new ArgumentNullException("name");
            if (xmlns == null)
                throw new ArgumentNullException("xmlns");
            return Disposable.CreateBracket((() => writer.WriteStartElement(name, xmlns)), writer.WriteEndElement);
        }

        public static void WriteXml([NotNull] this Stream stream, [NotNull] Action<XmlWriter> writeAction)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");
            if (writeAction == null)
                throw new ArgumentNullException("writeAction");
            using (var writer = CreateWriter(stream))
                writeAction(writer);
        }

        public static void WriteXml([NotNull] this Stream stream, [NotNull] XmlWriterSettings settings, [NotNull] Action<XmlWriter> writeAction)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");
            if (writeAction == null)
                throw new ArgumentNullException("writeAction");
            if (settings == null)
                throw new ArgumentNullException("settings");
            using (var xmlWriter = XmlWriter.Create(stream, settings))
                writeAction(xmlWriter);
        }*/
    }
}