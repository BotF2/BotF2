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
            {
                throw new ArgumentNullException(nameof(instance));
            }

            if (fileName == null)
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            using (XmlReader xmlReader = XmlReader.Create(fileName))
            {
                XamlXmlReader xamlReader = new XamlXmlReader(xmlReader);
                return LoadInto(instance, xamlReader);
            }
        }

        public static object LoadInto([NotNull] object instance, [NotNull] Stream stream)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            using (XmlReader xmlReader = XmlReader.Create(stream))
            {
                XamlXmlReader xamlReader = new XamlXmlReader(xmlReader);
                return LoadInto(instance, xamlReader);
            }
        }

        public static object LoadInto([NotNull] object instance, [NotNull] TextReader textReader)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            if (textReader == null)
            {
                throw new ArgumentNullException(nameof(textReader));
            }

            using (XmlReader reader = XmlReader.Create(textReader))
            {
                XamlXmlReader xamlReader = new XamlXmlReader(reader);
                return LoadInto(instance, xamlReader);
            }
        }

        public static object LoadInto([NotNull] object instance, [NotNull] XmlReader xmlReader)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            if (xmlReader == null)
            {
                throw new ArgumentNullException(nameof(xmlReader));
            }

            using (XamlXmlReader xamlReader = new XamlXmlReader(xmlReader))
            {
                return LoadInto(instance, xamlReader);
            }
        }

        public static object LoadInto([NotNull] object instance, [NotNull] XamlReader xamlReader)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            if (xamlReader == null)
            {
                throw new ArgumentNullException(nameof(xamlReader));
            }

            XamlObjectWriter xamlWriter = new XamlObjectWriter(
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
            {
                throw new ArgumentNullException(nameof(instance));
            }

            if (xaml == null)
            {
                throw new ArgumentNullException(nameof(xaml));
            }

            StringReader input = new StringReader(xaml);

            using (XmlReader xmlReader = XmlReader.Create(input))
            {
                XamlXmlReader xamlReader = new XamlXmlReader(xmlReader);
                _ = LoadInto(instance, xamlReader);
            }
        }
    }
}