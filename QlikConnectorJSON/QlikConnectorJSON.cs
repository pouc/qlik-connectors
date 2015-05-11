using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml;
using System.Net;
using System.IO;
using System.Data;

using System.Globalization;
using System.Runtime.Serialization.Formatters;

using QlikView.Qvx.QvxLibrary;
using QlikConnector;

using Moor.XmlConversionLibrary.XmlToCsvStrategy;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;


namespace QlikConnectorJSON
{
    /*
    public class XmlJSONReader : XmlReader
    {
        private JsonReader reader = null;

        private bool _eof;

        public XmlJSONReader(JsonReader reader)
        {
            this.reader = reader;

            
        }

        public override int AttributeCount
        {
            get { return 0; }
        }

        public override string BaseURI
        {
            get { throw new NotImplementedException(); }
        }

        public override int Depth
        {
            get { return this.reader.Depth; }
        }

        public override bool EOF
        {
            get { return this.reader.Read(); }
        }

        public override string GetAttribute(int i)
        {
            return null;
        }

        public override string GetAttribute(string name, string namespaceURI)
        {
            return null;
        }

        public override string GetAttribute(string name)
        {
            return null;
        }


        public override bool IsEmptyElement
        {
            get { throw new NotImplementedException(); }
        }

        public override string LocalName
        {
            get { throw new NotImplementedException(); }
        }

        public override string LookupNamespace(string prefix)
        {
            throw new NotImplementedException();
        }

        public override bool MoveToAttribute(string name, string ns)
        {
            throw new NotImplementedException();
        }

        public override bool MoveToAttribute(string name)
        {
            throw new NotImplementedException();
        }

        public override bool MoveToElement()
        {
            throw new NotImplementedException();
        }

        public override bool MoveToFirstAttribute()
        {
            throw new NotImplementedException();
        }

        public override bool MoveToNextAttribute()
        {
            throw new NotImplementedException();
        }

        public override XmlNameTable NameTable
        {
            get { throw new NotImplementedException(); }
        }

        public override string NamespaceURI
        {
            get { throw new NotImplementedException(); }
        }

        public override XmlNodeType NodeType
        {
            get { throw new NotImplementedException(); }
        }

        public override string Prefix
        {
            get { throw new NotImplementedException(); }
        }

        public override bool Read()
        {
            throw new NotImplementedException();
        }

        public override bool ReadAttributeValue()
        {
            throw new NotImplementedException();
        }

        public override ReadState ReadState
        {
            get { throw new NotImplementedException(); }
        }

        public override void ResolveEntity()
        {
            throw new NotImplementedException();
        }

        public override string Value
        {
            get { throw new NotImplementedException(); }
        }
    }
    */

    public class MyWebRequest
    {
        private WebRequest request;
        private Stream dataStream;
        private WebResponse response;

        private string status;

        public String Status
        {
            get
            {
                return status;
            }
            set
            {
                status = value;
            }
        }
        public HttpStatusCode StatusCode;

        public MyWebRequest(string url, string auth, string user, string password)
        {
            if (auth != "None") throw new Exception("Invalid Auth Type");

            // Create a request using a URL that can receive a post.
            request = WebRequest.Create(url);
        }

        public MyWebRequest(string url, string method, string auth, string user, string password)
            : this(url, auth, user, password)
        {

            if (method.Equals("GET") || method.Equals("POST"))
            {
                // Set the Method property of the request to POST.
                request.Method = method;
            }
            else
            {
                throw new Exception("Invalid Method Type");
            }
        }

        public MyWebRequest(string url, string method, string data, string auth, string user, string password)
            : this(url, method, auth, user, password)
        {

            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, String.Format("MyWebRequest() : {0}", url));

            if (data != null)
            {
                // Create POST data and convert it to a byte array.
                string postData = data;
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);

                // Set the ContentType property of the WebRequest.
                request.ContentType = "application/x-www-form-urlencoded";

                // Set the ContentLength property of the WebRequest.
                request.ContentLength = byteArray.Length;

                // Get the request stream.
                dataStream = request.GetRequestStream();

                // Write the data to the request stream.
                dataStream.Write(byteArray, 0, byteArray.Length);

                // Close the Stream object.
                dataStream.Close();
            }

        }

        public StreamReader GetResponseStream()
        {
            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "+ GetResponseStream()");

            // Get the original response.
            response = null;

            try
            {
                this.Status = null;
                this.StatusCode = HttpStatusCode.NotFound;

                response = request.GetResponse();

                this.Status = ((HttpWebResponse)response).StatusDescription;
                this.StatusCode = ((HttpWebResponse)response).StatusCode;
            }
            catch (WebException e)
            {
                this.Status = ((HttpWebResponse)e.Response).StatusDescription;
                this.StatusCode = ((HttpWebResponse)e.Response).StatusCode;
            }

            if (this.StatusCode == HttpStatusCode.OK)
            {
                QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "GetResponseStream() : OK!");

                // Get the stream containing all content returned by the requested server.
                dataStream = response.GetResponseStream();

                // Open the stream using a StreamReader for easy access.
                StreamReader reader = new StreamReader(dataStream);

                QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "GetResponseStream() : stream OK!");

                QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "- GetResponseStream()");

                return reader;

            }

            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "- GetResponseStream()");

            return null;
        }

        public void CloseStreams(StreamReader stream)
        {
            stream.Close();
            dataStream.Close();
            response.Close();
        }

        public string GetResponse()
        {
            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "+ GetResponse()");

            StreamReader reader = this.GetResponseStream();

            if (reader != null)
            {
                // Read the content fully up to the end.
                string responseFromServer = reader.ReadToEnd();

                QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "GetResponse() : convert to string OK!");

                // Clean up the streams.
                this.CloseStreams(reader);

                QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "- GetResponse()");

                return responseFromServer;
            }

            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "- GetResponse()");

            return null;
        }

    }

    public class QlikConnectorJSON : IQlikConnector
    {
        private Dictionary<string, Dictionary<string, List<QvxTable>>> connectParams = new Dictionary<string, Dictionary<string, List<QvxTable>>>();
        public QlikConnectorJSON() { }

        public string Name
        {
            get { return "JSON Driver"; }
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
            return true;
        }

        public List<DriverParam> getDriverParams()
        {
            return new List<DriverParam>
            {
                new DriverParam() {
                    paramType = DriverParamType.label,
                    paramName = "Host",
                    paramValueType = DriverParamValueType.s,
                    paramValues = new Dictionary<string, List<DriverParam>> {
                        { "protocol://server:port", null }
                    }
                },
                new DriverParam() {
                    paramType = DriverParamType.label,
                    paramName = "Method",
                    paramValueType = DriverParamValueType.s,
                    paramValues = new Dictionary<string, List<DriverParam>> {
                        { "ping", null }
                    }
                },
                new DriverParam() {
                    paramType = DriverParamType.list,
                    paramName = "Http Method",
                    paramValueType = DriverParamValueType.s,
                    paramValues = new Dictionary<string, List<DriverParam>> {
                        { "GET", null },
                        { "POST", new List<DriverParam> () {
                            new DriverParam() {
                                paramType = DriverParamType.label,
                                paramName = "Params",
                                paramValueType = DriverParamValueType.s,
                                paramValues = new Dictionary<string, List<DriverParam>> {
                                    { "params", null }
                                }
                            }
                        }}
                    }
                },
                new DriverParam() {
                    paramType = DriverParamType.list,
                    paramName = "Force Refresh",
                    paramValueType = DriverParamValueType.s,
                    paramValues = new Dictionary<string, List<DriverParam>> {
                        { "True", null },
                        { "False", null }
                    }
                }
            };
        }

        public List<Database> getDatabases(Dictionary<string, string> args)
        {
            return new List<Database>() { new Database() { qName = "Host" } };
        }

        public IEnumerable<DataTable> getRawTables(Database database, Dictionary<string, string> args)
        {
            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "+ getRawTables()");

            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, String.Format("getRawTables() : {0}", args.Count));
            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, String.Format("getRawTables() : {0}", String.Join(", ", args.Select(kv => String.Format("{0} : {1}", kv.Key, kv.Value)))));

            MyWebRequest q;

            string param = args["Method"] == "POST" ? args["Params"] : null;

            if (args.ContainsKey("Http Method"))
                q = new MyWebRequest(
                    String.Format(
                        "{0}/{1}",
                        args["Host"],
                        args["Method"]
                    )
                    , args["Http Method"], param, "None", null, null);
            else
                throw new ArgumentOutOfRangeException();

            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "getRawTables() : request prepared");

            //string s = q.GetResponse();
            //QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, String.Format("getRawTables() : response : {0}", s.Substring(0, 50)));

            StreamReader sr = q.GetResponseStream();

            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, String.Format("getRawTables() : {0}", q.StatusCode));

            List<QvxTable> lt = new List<QvxTable>();
            if (q.StatusCode == HttpStatusCode.OK)
            {
                JsonSerializer js = new JsonSerializer();
                JsonTextReader jr = new JsonTextReader(sr);

                QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, String.Format("getRawTables() : deserializing document"));

                XmlNodeConverter converter = new XmlNodeConverter();
                converter.DeserializeRootElementName = "root";
                converter.WriteArrayAttribute = false;

                JsonSerializerSettings settings = new JsonSerializerSettings { Converters = new JsonConverter[] { converter } };

                JsonSerializer jsonSerializer = JsonSerializer.CreateDefault(settings);
                jsonSerializer.CheckAdditionalContent = true;

                XmlDocument doc = (XmlDocument) jsonSerializer.Deserialize(jr, typeof(XmlDocument));

                QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, String.Format("getRawTables() : {0}", doc.InnerXml.Substring(0, Math.Min(5000, doc.InnerXml.Length))));

                jr.Close();
                q.CloseStreams(sr);

                QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, String.Format("getRawTables() : JSON deserialized"));

                XmlToCsvUsingDataSetFromString csvConverter = new XmlToCsvUsingDataSetFromString(doc, null);
                XmlToCsvContext context = new XmlToCsvContext(csvConverter);

                QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "getRawTables() : converted to xml");

                foreach (DataTable dt in csvConverter.XmlDataSet.Tables)
                {
                    QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, String.Format("getRawTables() : table found {0}", dt.TableName));

                    yield return dt;
                }

            }

            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "- getRawTables()");
        }

        public List<QvxTable> getTables(Database database, Dictionary<string, string> args)
        {
            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "+ getTables()");

            string param = args["Method"] == "POST" ? args["Params"] : "default";

            if (
                args.ContainsKey("Force Refresh")
                && (!Convert.ToBoolean(args["Force Refresh"]))
                && this.connectParams.ContainsKey(args["Method"])
                && this.connectParams[args["Method"]].ContainsKey(param)
            )
            {
                QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "- getTables() (from cache)");
                return this.connectParams[args["Method"]][param];
            }

            List<QvxTable> lt = new List<QvxTable>();

            foreach (DataTable dt in this.getRawTables(database, args))
            {
                QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Notice, "Found table " + dt.TableName + ": " + dt.Rows.Count.ToString());

                List<QvxField> l = new List<QvxField>();
                foreach (DataColumn dc in dt.Columns)
                {
                    QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Notice, "Found Column " + dc.ColumnName);

                    l.Add(new QvxField(dc.ColumnName, QvxFieldType.QVX_TEXT, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.ASCII));
                }

                QvxTable mt = new QvxTable();
                mt.TableName = dt.TableName;
                mt.GetRows = delegate() { return GetJSONRows(dt, mt); };
                mt.Fields = l.ToArray();
                    
                lt.Add(mt);
            }

            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "- getTables()");

            if (!this.connectParams.ContainsKey(args["Method"])) this.connectParams.Add(args["Method"], new Dictionary<string, List<QvxTable>>());

            if (this.connectParams[args["Method"]].ContainsKey(param))
                this.connectParams[args["Method"]][param] = lt;
            else
                this.connectParams[args["Method"]].Add(param, lt);

            return lt;

        }

        public List<QvxField> getFields(Database database, QvxTable table, Dictionary<string, string> args)
        {
            return table.Fields.ToList();
        }

        public List<List<string>> getPreview(Database database, QvxTable table, Dictionary<string, string> args)
        {
            DataTable dt = this.getRawTables(database, args).Where(t => t.TableName == table.TableName).FirstOrDefault();

            if (dt == null)
                return null;

            return dt.Rows.Cast<DataRow>().Select(
                row => table.Fields.Select(
                    col => row[dt.Columns.Cast<DataColumn>().Where(
                        rcol => rcol.ColumnName == col.FieldName
                    ).FirstOrDefault()].ToString()
                ).ToList()
            ).ToList();

        }

        public void Init(Dictionary<string, string> args)
        {
            
        }

        private IEnumerable<QvxDataRow> GetJSONRows(DataTable dt, QvxTable table)
        {
            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "+ GetJSONRows()");

            foreach (DataRow dr in dt.Rows)
            {
                yield return MakeEntry(dr, table);
            }

            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "- GetJSONRows()");
        }

        private QvxDataRow MakeEntry(DataRow dr, QvxTable table)
        {
            var row = new QvxDataRow();

            foreach (DataColumn dc in dr.Table.Columns)
            {
                row[table[dc.ColumnName]] = dr[dc].ToString();
            }

            return row;
        }
    }
}
