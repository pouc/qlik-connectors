using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using Moor.XmlConversionLibrary.XmlToCsvHelpers;

namespace Moor.XmlConversionLibrary.XmlToCsvStrategy
{
    public class XmlToCsvUsingLinq : XmlToCsvStrategyBase
    {
        private string _csvDestinationFilePath;
        private readonly string _xmlSourceFilePath;

        public XmlToCsvUsingLinq(string xmlSourceFilePath)
        {
            _xmlSourceFilePath = @xmlSourceFilePath;

            var ds = new DataSet("ds");
            ds.ReadXmlSchema(@_xmlSourceFilePath);

            foreach (DataTable table in ds.Tables)
            {
                TableNameCollection.Add(table.TableName);
            }
        }

        public override void ExportToCsv(string xmlTableName, string csvDestinationFilePath, Encoding encoding)
        {
            _csvDestinationFilePath = csvDestinationFilePath;

            HeaderColumnNameCollection.Clear();

            _csvDestinationFilePath = csvDestinationFilePath;

            using (XmlReader reader = XmlReader.Create(_xmlSourceFilePath))
            {
                IEnumerable<XElement> workingTable =
                    from el in reader.StreamElements(xmlTableName).DescendantsAndSelf()
                    where el.Descendants().Count() > 0
                    select el;

                IEnumerable<XElement> list = workingTable.ToList();

                var fs = new FileStream(_csvDestinationFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
                var sw = new StreamWriter(fs, encoding);

                string headerLine = string.Empty;

                foreach (XElement x in list.Take(1).Descendants())
                {
                    HeaderColumnNameCollection.Add(ColumnCount, x.Name.ToString());
                    headerLine += x.Name + ",";
                    ColumnCount++;
                }

                using (sw)
                {
                    char[] charsToTrim = { ',' };
                    sw.WriteLine(headerLine.TrimEnd(charsToTrim));

                    foreach (XElement element in list)
                    {
                        string rowString = string.Empty;
                        string columnString = string.Empty;

                        foreach (var obj in element.Descendants())
                        {
                            columnString += obj.Value + ",";
                        }

                        rowString += columnString;
                        rowString = rowString.Replace(Environment.NewLine, @"-");
                        sw.WriteLine(rowString.TrimEnd(charsToTrim));
                    }

                    sw.Close();
                }

                reader.Close();
            }
        }
    }
}