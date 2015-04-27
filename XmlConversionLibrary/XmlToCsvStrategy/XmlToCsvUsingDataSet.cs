using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace Moor.XmlConversionLibrary.XmlToCsvStrategy
{
    public class XmlToCsvUsingDataSet : XmlToCsvStrategyBase, IDisposable
    {
        private string _csvDestinationFilePath;
        private DataTable _workingTable;


        public XmlToCsvUsingDataSet(string xmlSourceFilePath)
            : this(xmlSourceFilePath, false)
        {

        }


        public XmlToCsvUsingDataSet(string xmlSourceFilePath, bool autoRenameWhenNamingConflict)
        {
            XmlDataSet = new DataSet();
            try
            {
                XmlDataSet.ReadXml(xmlSourceFilePath);

                foreach (DataTable table in XmlDataSet.Tables)
                {
                    TableNameCollection.Add(table.TableName);
                }
            }
            catch (DuplicateNameException)
            {
                if (autoRenameWhenNamingConflict)
                {
                    XmlDataSet.ReadXml(xmlSourceFilePath, XmlReadMode.IgnoreSchema);

                    foreach (DataTable table in XmlDataSet.Tables)
                    {
                        TableNameCollection.Add(table.TableName);
                    }

                    RenameDuplicateColumn();
                }
                else
                {
                    throw;
                }
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