using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

namespace Moor.XmlConversionLibrary.XmlToCsvHelpers
{
    public static class Helper
    {
        public static IEnumerable<XElement> StreamElements(this XmlReader reader, string tableName)
        {
            while (reader.ReadToFollowing(tableName))
            {
                var el = (XElement)XNode.ReadFrom(reader);
                yield return el;
            }
        }
    }
}
