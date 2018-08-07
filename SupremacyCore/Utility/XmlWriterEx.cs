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
    }
}