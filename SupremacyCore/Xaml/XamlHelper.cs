using System;
using System.IO;
using System.Xaml;
using System.Xaml.Permissions;
using System.Xml;

using Supremacy.Annotations;

namespace Supremacy.Xaml
{
    public class XamlHelper
    {
        public static object LoadInto([NotNull] object instance, [NotNull] string fileName)
        {
            if (instance == null)
                throw new ArgumentNullException("instance");
            if (fileName == null)
                throw new ArgumentNullException("fileName");

            using (var xmlReader = XmlReader.Create(fileName))
            {
                var xamlReader = new XamlXmlReader(xmlReader);
                return LoadInto(instance, xamlReader);
            }
        }

        public static object LoadInto([NotNull] object instance, [NotNull] Stream stream)
        {
            if (instance == null)
                throw new ArgumentNullException("instance");
            if (stream == null)
                throw new ArgumentNullException("stream");

            using (var xmlReader = XmlReader.Create(stream))
            {
                var xamlReader = new XamlXmlReader(xmlReader);
                return LoadInto(instance, xamlReader);
            }
        }

        public static object LoadInto([NotNull] object instance, [NotNull] TextReader textReader)
        {
            if (instance == null)
                throw new ArgumentNullException("instance");
            if (textReader == null)
                throw new ArgumentNullException("textReader");

            using (var reader = XmlReader.Create(textReader))
            {
                var xamlReader = new XamlXmlReader(reader);
                return LoadInto(instance, xamlReader);
            }
        }
        
        public static object LoadInto([NotNull] object instance, [NotNull] XmlReader xmlReader)
        {
            if (instance == null)
                throw new ArgumentNullException("instance");
            if (xmlReader == null)
                throw new ArgumentNullException("xmlReader");

            using (var xamlReader = new XamlXmlReader(xmlReader))
            {
                return LoadInto(instance, xamlReader);
            }
        }

        public static object LoadInto([NotNull] object instance, [NotNull] XamlReader xamlReader)
        {
            if (instance == null)
                throw new ArgumentNullException("instance");
            if (xamlReader == null)
                throw new ArgumentNullException("xamlReader");

            var xamlWriter = new XamlObjectWriter(
                xamlReader.SchemaContext,
                new XamlObjectWriterSettings
                {
                    RootObjectInstance = instance,
                    AccessLevel = XamlAccessLevel.AssemblyAccessTo(typeof(XamlHelper).Assembly)
                });

            XamlServices.Transform(xamlReader, xamlWriter);
            return xamlWriter.Result;
        }

        public static void ParseInto([NotNull] object instance, [NotNull] string xaml)
        {
            if (instance == null)
                throw new ArgumentNullException("instance");
            if (xaml == null)
                throw new ArgumentNullException("xaml");

            var input = new StringReader(xaml);

            using (var xmlReader = XmlReader.Create(input))
            {
                var xamlReader = new XamlXmlReader(xmlReader);
                LoadInto(instance, xamlReader);
            }
        }
    }
}