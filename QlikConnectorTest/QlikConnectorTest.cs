using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using QlikView.Qvx.QvxLibrary;
using QlikConnector;

namespace QlikConnectorTest
{
    public class QlikConnectorTest : IQlikConnector
    {
        public QlikConnectorTest() { }

        public string Name
        {
            get { return "Test Driver"; }
        }

        public IQlikConnectorHost Host
        {
            get
            {
                return null;
            }
            set
            {
                return;
            }
        }

        public bool testDriver(Dictionary<string, string> args)
        {
            return true;
        }

        public bool testConnection(Dictionary<string, string> args)
        {
            return args.ContainsKey("test label") && args.ContainsKey("test list");
        }

        public List<DriverParam> getDriverParams()
        {
            return new List<DriverParam>
            {
                new DriverParam() {
                    paramType = DriverParamType.label,
                    paramName = "test label",
                    paramValueType = DriverParamValueType.s,
                    paramValues = new Dictionary<string, List<DriverParam>> {
                        { "test value", null }
                    }
                },
                new DriverParam() {
                    paramType = DriverParamType.list,
                    paramName = "test list",
                    paramValueType = DriverParamValueType.s,
                    paramValues = new Dictionary<string, List<DriverParam>> {
                        {"test value 1", null },
                        {"test value 2", new List<DriverParam> {
                                new DriverParam() {
                                    paramType = DriverParamType.label,
                                    paramName = "test sub label 1",
                                    paramValueType = DriverParamValueType.s,
                                    paramValues = new Dictionary<string, List<DriverParam>> {
                                        { "test sub value l1", null }
                                    }
                                },
                                new DriverParam() {
                                    paramType = DriverParamType.label,
                                    paramName = "test sub label 2",
                                    paramValueType = DriverParamValueType.s,
                                    paramValues = new Dictionary<string, List<DriverParam>> {
                                        { "test sub value l2", null }
                                    }
                                }
                            }
                        },
                        {"test value 3", null }
                    }
                }
            };
        }

        public List<Database> getDatabases(Dictionary<string, string> args)
        {
            return new List<Database>() { new Database() { qName = "test database" } };
        }

        public List<QvxTable> getTables(Database database, Dictionary<string, string> args)
        {
            List<QvxTable> lt = new List<QvxTable>();

            QvxTable t = new QvxTable();

            t.TableName = "test table";
            t.Fields = this.getFields(database, t, args).ToArray();
            t.GetRows = () => { return getRows(t); };

            lt.Add(t);
            return lt;
        }

        public List<QvxField> getFields(Database database, QvxTable table, Dictionary<string, string> args)
        {
            return new List<QvxField> { new QvxField("test field", QvxFieldType.QVX_TEXT, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.ASCII) };
        }

        public List<List<string>> getPreview(Database database, QvxTable table, Dictionary<string, string> args)
        {
            return new List<List<string>> () {
                new List<string> () { "test value" }
            };
        }

        public void Init(Dictionary<string, string> args)
        {
            
        }

        private IEnumerable<QvxDataRow> getRows(QvxTable t)
        {
            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "+ getRows()");

            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, String.Format("getRows() : {0}, {1}", t.TableName, t.Fields[0].FieldName));

            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "- getRows()");

            yield return MakeEntry(t.Fields[0].FieldName, "test value", t);
        }

        private QvxDataRow MakeEntry(string col, string value, QvxTable table)
        {
            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "+ MakeEntry()");

            var row = new QvxDataRow();

            row[table[col]] = value.ToString();

            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "- MakeEntry()");

            return row;
        }



    }
}
