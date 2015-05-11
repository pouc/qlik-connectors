using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using System.Xml.Xsl;

using System.Xml.Linq;

using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace Moor.XmlConversionLibrary.XmlToCsvStrategy
{
    public static class DocumentExtensions
    {
        public static XmlDocument ToXmlDocument(this XDocument xDocument)
        {
            var xmlDocument = new XmlDocument();
            using (var xmlReader = xDocument.CreateReader())
            {
                xmlDocument.Load(xmlReader);
            }
            return xmlDocument;
        }

        public static XDocument ToXDocument(this XmlDocument xmlDocument)
        {
            using (var nodeReader = new XmlNodeReader(xmlDocument))
            {
                nodeReader.MoveToContent();
                return XDocument.Load(nodeReader);
            }
        }
    }

    public class XmlToCsvUsingDataSetFromString : XmlToCsvStrategyBase, IDisposable
    {
        private string _csvDestinationFilePath;
        private DataTable _workingTable;

        public XmlToCsvUsingDataSetFromString(XmlDocument xmlSource)
            : this(xmlSource, "_", "_")
        {

        }

        public XmlToCsvUsingDataSetFromString(XmlDocument xmldoc, string qualifySep, string prefix)
        {
            XmlDataSet = new DataSet();

            this.prefix(xmldoc.FirstChild, prefix);

            if (qualifySep != null)
            {
                XDocument xdoc = xmldoc.ToXDocument();
                this.qualify(xdoc.Elements().First(), qualifySep);
                xmldoc = xdoc.ToXmlDocument();
            }

            byte[] byteArray = Encoding.ASCII.GetBytes(xmldoc.OuterXml);
            MemoryStream stream = new MemoryStream(byteArray);

            try
            {
                XmlDataSet.ReadXml(stream);

                foreach (DataTable table in XmlDataSet.Tables)
                {
                    TableNameCollection.Add(table.TableName);
                }
            }
            catch (ArgumentException)
            {
                stream.Position = 0;
                XmlDataSet.ReadXml(stream, XmlReadMode.IgnoreSchema);

                foreach (DataTable table in XmlDataSet.Tables)
                {
                    TableNameCollection.Add(table.TableName);
                }

                RenameDuplicateColumn();
            }
        }

        public Dictionary<string, string> exploreXSD(XmlSchemaElement e)
        {
            Dictionary<string, string> l = new Dictionary<string, string>();


            if (e.ElementSchemaType is XmlSchemaComplexType)
            {
                XmlSchemaComplexType ctype = (XmlSchemaComplexType)e.ElementSchemaType;

                if (ctype.ContentTypeParticle is XmlSchemaSequence)
                {
                    XmlSchemaSequence seq = (XmlSchemaSequence)ctype.ContentTypeParticle;

                    foreach (XmlSchemaElement childElement in seq.Items)
                    {
                        Console.WriteLine("Element: {0}", childElement.Name);
                    }
                }

            }

            return l;
        }



        public void qualify(XElement n, string sep)
        {
            Dictionary<XmlNode, XmlNode> rename = new Dictionary<XmlNode, XmlNode>();

            if (n.Parent != null)
            {
                n.Name = n.Parent.Name + sep + n.Name;

            }

            foreach (XElement cn in n.Elements())
            {
                this.qualify(cn, sep);
            }
        }

        public void prefix(XmlNode n, string sep)
        {
            if (n.NodeType == XmlNodeType.Element && n.Name != null && !Regex.Match(n.Name, "^[a-zA-Z]").Success)
            {
                XmlNode nn = n.OwnerDocument.CreateElement(sep + n.Name);
                while (n.HasChildNodes) nn.AppendChild(n.FirstChild);
                n.ParentNode.ReplaceChild(nn, n);

                nn.ChildNodes.Cast<XmlNode>().ToList().ForEach(cn => this.prefix(cn, sep));
            }
            else
            {
                n.ChildNodes.Cast<XmlNode>().ToList().ForEach(cn => this.prefix(cn, sep));
            }
        }



        public DataSet XmlDataSet { get; private set; }

        /// <summary>
        /// Check for duplicates names in XML. Rename the table in case a clash with a column name is found.
        /// </summary>
        /// <returns>True if a duplicate XML name was found and renames the name clash. Otherwise returns false.</returns>
        private void RenameDuplicateColumn()
        {
            foreach (DataTable table in XmlDataSet.Tables)
            {
                bool hasDuplicate = XmlDataSet.Tables[0].Columns.Contains(table.TableName);

                if (hasDuplicate)
                {
                    TableNameCollection.Remove(table.TableName);
                    TableNameCollection.Add(table.TableName + "_Renamed");
                    table.TableName = table.TableName + "_Renamed";
                }
            }
        }

        public override void ExportToCsv(string xmlTableName, string csvDestinationFilePath, Encoding encoding)
        {
            StreamWriter sw = CreateStreamWriter(xmlTableName, csvDestinationFilePath, encoding);

            using (sw)
            {
                WriteHeaderToCsv(sw);

                foreach (DataRow row in XmlDataSet.Tables[xmlTableName].Rows)
                {
                    WriteRowToCsv(xmlTableName, sw, row);
                }

                sw.Flush();
                sw.Close();
            }
        }

        private StreamWriter CreateStreamWriter(string xmlTableName, string csvDestinationFilePath, Encoding encoding)
        {
            if (string.IsNullOrEmpty(xmlTableName))
            {
                throw new NotSupportedException("Table name for table to export is not specified");
            }

            HeaderColumnNameCollection.Clear();

            _csvDestinationFilePath = csvDestinationFilePath;
            _workingTable = XmlDataSet.Tables[xmlTableName];
            ColumnCount = _workingTable.Columns.Count;

            foreach (DataColumn column in _workingTable.Columns)
            {
                HeaderColumnNameCollection.Add(column.Ordinal, column.ColumnName);
            }

            var fs = new FileStream(_csvDestinationFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
            var sw = new StreamWriter(fs, encoding);
            return sw;
        }

        [Obsolete("Use XmlDataSet.Tables[xmlTableName].Columns instead.")]
        public List<DataColumn> GetColumnList(string xmlSourceFilePath, string xmlTableName)
        {
            //var ds = new DataSet("ds");
            //ds.ReadXml(xmlSourceFilePath);
            //var dt = XmlDataSet.Tables[xmlTableName];
            List<DataColumn> list = XmlDataSet.Tables[xmlTableName].Columns.Cast<DataColumn>().ToList();
            return list;
        }

        private void WriteRowToCsv(string xmlTableName, StreamWriter sw, DataRow row)
        {
            int colNr = 0;

            string rowValue = string.Empty;

            foreach (DataColumn column in XmlDataSet.Tables[xmlTableName].Columns)
            {
                bool isString = (column.DataType == typeof(string));
                string columnValue;

                if (isString)
                {
                    string stringValue = row[column].ToString();
                    stringValue = stringValue.Replace(Environment.NewLine, @"\n");
                    columnValue = "\"" + stringValue + "\"";
                }
                else
                {
                    columnValue = row[column].ToString();
                }

                rowValue += columnValue;

                if (colNr < ColumnCount - 1)
                {
                    rowValue += ",";
                }

                colNr++;
            }

            sw.WriteLine(rowValue);
        }

        private void WriteHeaderToCsv(StreamWriter sw)
        {
            string headerLine = string.Empty;

            foreach (KeyValuePair<int, string> pair in HeaderColumnNameCollection)
            {
                headerLine += pair.Value + ",";
            }

            char[] charsToTrim = { ',' };
            sw.WriteLine(headerLine.TrimEnd(charsToTrim));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                XmlDataSet.Dispose();
            }
        }
    }
}