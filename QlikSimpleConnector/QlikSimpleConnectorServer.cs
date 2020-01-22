using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using QlikView.Qvx.QvxLibrary;
using System.Reflection;
using System.IO;
using System.Windows.Forms;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System.Xml;
using System.Xml.Linq;

namespace QlikSimpleConnector
{

    public class QvDataContractPreviewResponse : QvDataContractResponse
    {
        public QvDataContractPreviewResponse()
            : base()
        {
            this.qPreview = new List<MyQvxDataRow>();
        }

        public List<MyQvxDataRow> qPreview { get; set; }
    }

    public class QvInfo : Info
    {
        public QvInfo()
            : base()
        {

        }

        public bool qOk;

    }

    public class QvDataContractOwnerListResponse : QvDataContractResponse
    {
        public QvDataContractOwnerListResponse() : base()
        {
            this.qOwners = new List<string>();
        }

        public List<string> qOwners { get; set; }
    }

    public class QvDataContractDriverListResponse : QvDataContractResponse
    {
        public QvDataContractDriverListResponse()
            : base()
        {
            this.qDrivers = new List<string>();
        }

        public List<string> qDrivers { get; set; }
    }

    public class QvDataContractDriverParamListResponse : QvDataContractResponse
    {
        public QvDataContractDriverParamListResponse()
            : base()
        {
            this.qDriverParamList = new List<DriverParam>();
        }

        public List<DriverParam> qDriverParamList { get; set; }
    }

    public class QlikSimpleConnectorServer : QvxServer, IQlikSimpleConnectorHost
    {
        private Dictionary<string, IQlikSimpleConnector> ConnectorMap = new Dictionary<string, IQlikSimpleConnector>();

        public QlikSimpleConnectorServer()
            : base()
        {
            QvxLog.SetLogLevels(true, true);

            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "+ QlikSimpleConnectorServer()");

            this.getPlugins();

            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "- QlikSimpleConnectorServer()");
        }

        private void getPlugins()
        {
            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "+ getPlugins()");

            try
            {

                string path = Application.StartupPath + "\\connectors";
                string[] pluginFiles = new string[] { };

                QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "getPlugins() : path = " + path);

                pluginFiles = Directory.GetFiles(path, "*.DLL");
                QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "getPlugins() : found " + pluginFiles.Length + " dlls...");

                IQlikSimpleConnector[] ipi = new IQlikSimpleConnector[pluginFiles.Length];

                for (int i = 0; i < pluginFiles.Length; i++)
                {

                    Type ObjType = null;

                    Assembly ass = Assembly.LoadFrom(pluginFiles[i]);
                    QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "getPlugins() : Assembly loaded...");
                
                    if (ass != null)
                    {
                        Type[] types = ass.GetTypes();
                        foreach (Type t in types)
                        {
                            if (t.GetInterface(typeof(IQlikSimpleConnector).FullName) != null)
                            {
                                QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "getPlugins() : found plugin: " + t.ToString() + " in " + pluginFiles[i]);
                                ObjType = t;
                            }
                        }
                    }

                    if (ObjType != null)
                    {
                        QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "getPlugins() : creating instance... :)");

                        ipi[i] = (IQlikSimpleConnector)Activator.CreateInstance(ObjType);

                        QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "getPlugins() : registering plugin...");

                        ipi[i].Host = this;
                        this.Register(ipi[i]);
                    }
                }
            }
            catch (Exception e)
            {
                QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Error, "getPlugins() : " + e.Message);
                throw e;
            }

            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "- getPlugins()");
        }

        public bool Register(IQlikSimpleConnector ipi)
        {
            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "+ Register()");

            bool retVal = false;

            try {

                if (!this.ConnectorMap.ContainsKey(ipi.Name))
                {
                    this.ConnectorMap.Add(ipi.Name, ipi);
                    retVal = true;
                }

            }
            catch (Exception e)
            {
                QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Error, "Register() : " + e.Message);
                throw e;
            }

            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "- Register()");

            return retVal;
        }

        public IQlikSimpleConnector Registered(string plugin)
        {
            if (!this.ConnectorMap.ContainsKey(plugin)) return null;
            return this.ConnectorMap[plugin];
        }

        public override QvxConnection CreateConnection()
        {
            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "+ CreateConnection()");

            QvxConnection retVal;

            try {

                retVal = new QlikSimpleConnectorConnection(this);

            }
            catch (Exception e)
            {
                QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Error, "CreateConnection() : " + e.Message);
                throw e;
            }

            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "- CreateConnection()");

            return retVal;

        }

        public override string CreateConnectionString()
        {

            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "+ CreateConnectionString()");

            string retVal = "FOOBAR;";

            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "- CreateConnectionString()");

            return retVal;

        }

        private Dictionary<string, string> driverParamToDictionary(DriverParam p)
        {
            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "+ driverParamToDictionary()");

            Dictionary<string, string> retVal;

            try
            {
                retVal = new Dictionary<string, string>() { { p.paramName, p.selectedValues.Keys.ToArray()[0] } };

                if (p.selectedValues.Values.ToArray()[0] != null)
                    retVal = retVal.Concat(
                            p.selectedValues.Values.ToArray()[0]
                            .SelectMany(
                                f => driverParamToDictionary(f)
                            ).ToDictionary(
                                k => k.Key,
                                v => v.Value
                            )
                        ).ToDictionary(
                             k => k.Key,
                             v => v.Value
                        );

            }
            catch (Exception e)
            {
                QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Error, "driverParamToDictionary() : " + e.Message);
                throw e;
            }

            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "- driverParamToDictionary()");

            return retVal;

        }

        public class DriverParamDeserialKV
        {
            public string Key;
            public List<DriverParamDeserial> Value;
        }

        public class DriverParamDeserial
        {
            public DriverParamType paramType;
            public string paramName;
            public DriverParamValueType paramValueType;
            public List<DriverParamDeserialKV> paramValues;
            public List<DriverParamDeserialKV> selectedValues;
        }

        private List<DriverParam> ToDriverParamList(List<DriverParamDeserial> s)
        {
            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "+ ToDriverParamList()");

            List<DriverParam> retVal;

            try
            {

                if (s == null) retVal = null;
                else
                    retVal = s.Select(f =>
                    {
                        QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "ToDriverParamList() : " + f.paramName);

                        return new DriverParam()
                        {
                            paramName = f.paramName,
                            paramType = f.paramType,
                            paramValueType = f.paramValueType,
                            paramValues = (f.paramValues != null) ? f.paramValues.ToDictionary(g => g.Key, g => ToDriverParamList(g.Value)) : null,
                            selectedValues = (f.selectedValues != null) ? f.selectedValues.ToDictionary(g => g.Key, g => ToDriverParamList(g.Value)) : null
                        };
                    }).ToList();

            }
            catch (Exception e)
            {
                QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Error, "ToDriverParamList() : " + e.Message);
                throw e;
            }

            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "- ToDriverParamList()");

            return retVal;

        }

        public Dictionary<string, string> ToDictionary(string[] param)
        {
            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "+ ToDictionary()");

            Dictionary<string, string> retVal;
            JsonSerializer js = new JsonSerializer();

            try {

                retVal = param.ToList().SelectMany(
                    (f) => {
                        QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "ToDictionary() : " + f);
                        return ToDriverParamList(JsonConvert.DeserializeObject<List<DriverParamDeserial>>(f));
                    }
                ).ToList().SelectMany(f => driverParamToDictionary(f))
                .ToDictionary(f => f.Key, f => f.Value);

            }
            catch (Exception e)
            {
                QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Error, "ToDictionary() : " + e.Message);
                throw e;
            }

            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "- ToDictionary()");

            return retVal;

        }

        public string getConnectionFile(string qConnectionId)
        {
            return Path.Combine(Application.StartupPath, "connections", qConnectionId + ".xml");
        }

        private object _lock = new object();

        public Dictionary<string, string> readParams(string qConnectionId)
        {
            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "+ readParams()");

            XElement rootElement;

            lock (this._lock)
            {
                if (!File.Exists(this.getConnectionFile(qConnectionId)))
                    return null;

                rootElement = XElement.Parse(File.ReadAllText(this.getConnectionFile(qConnectionId)));
            }

            Dictionary<string, string> dict = new Dictionary<string, string>();
            foreach (var el in rootElement.Elements())
            {
                dict.Add(el.Name.LocalName.Replace("___", " "), el.Value);
            }

            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "- readParams()");

            return dict;

        }

        private void writeParams(string qConnectionId, Dictionary<string, string> dict)
        {
            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "+ writeParams()");

            XElement newEl = new XElement(
                "root",
                dict.Select(kv =>
                    {
                        QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "writeParams() : (" + kv.Key + ", " + kv.Value + ")");

                        return new XElement(
                            kv.Key.Replace(" ", "___"),
                            kv.Value
                        );
                    }
                )
            );

            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "writeParams() : dic generated!");

            lock (this._lock)
            {
                newEl.Save(this.getConnectionFile(qConnectionId));
            }

            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "- writeParams()");
        }

        private QvDataContractResponse storeParams(Dictionary<string, string> userParameters)
        {
            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "+ storeParams()");

            if (!userParameters.ContainsKey("qConnectionId"))
                throw new Exception("qConnectionId parameter required");

            string qConnectionId = userParameters["qConnectionId"];
            userParameters.Remove("qConnectionId");

            this.writeParams(qConnectionId, userParameters);

            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "- storeParams()");

            return new Info { qMessage = "Done" };
        }

        public override string HandleJsonRequest(string method, string[] userParameters, QvxConnection connection)
        {
            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "+ HandleJsonRequest() : method = " + method + ", " + String.Join(";", userParameters));

            Dictionary<string, string> parameters = this.ToDictionary(userParameters);

            QvDataContractResponse retVal;

            try {

                switch (method)
                {
                    case "getInfo":
                        retVal = new Info
                        {
                            qMessage = "Connector wrapper for QlikView & QlikSense. Please visit Branch for more information."
                        };

                        break;

                    case "getDrivers":
                        retVal = this.getDrivers();
                        break;

                    case "createConnectionString":
                        retVal = this.createConnectionString(parameters);
                        break;

                    case "storeParams":
                        retVal = this.storeParams(parameters);
                        break;

                    default:

                        IQlikSimpleConnector iqc = null;

                        if (parameters.ContainsKey("qDriver"))
                            iqc = this.ConnectorMap[parameters["qDriver"]];
                        else if (parameters.ContainsKey("qConnectionId"))
                        {
                            parameters = parameters.Concat(this.readParams(parameters["qConnectionId"])).ToDictionary(kv => kv.Key, kv => kv.Value);

                            if (parameters.ContainsKey("qDriver"))
                                iqc = this.ConnectorMap[parameters["qDriver"]];

                        }
                        

                        if(iqc == null)
                            throw new Exception("No parameter telling which driver to use");

                        switch (method)
                        {

                            case "getDriverConnectParams":
                                retVal = this.getDriverConnectParams(iqc);
                                break;

                            case "getDatabases":
                                retVal = this.getDatabases(iqc, parameters);
                                break;

                            case "getOwners":
                                retVal = this.getOwners(iqc, parameters["qDatabase"], parameters);
                                break;

                            case "getTables":
                                retVal = this.getTables(iqc, parameters["qDatabase"], parameters["qOwner"], parameters);
                                break;

                            case "getFields":
                                retVal = this.getFields(iqc, parameters["qDatabase"], parameters["qOwner"], parameters["qTable"], parameters);
                                break;

                            case "getPreview":
                                retVal = this.getPreview(iqc, parameters["qDatabase"], parameters["qOwner"], parameters["qTable"], parameters);
                                break;

                            case "testDriver":
                                retVal = this.testDriver(iqc, parameters);
                                break;

                            case "testConnection":
                                retVal = this.testConnection(iqc, parameters);
                                break;

                            default:
                                retVal = new Info { qMessage = "Unknown command" };
                                break;
                        }

                        break;


                }
      

            }
            catch (Exception e)
            {
                QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Error, "HandleJsonRequest() : " + e.Message);
                throw e;
            }

            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "- HandleJsonRequest() : retVal = " + ToJson(retVal));

            return ToJson(retVal);
        }

        public QvDataContractResponse getDrivers()
        {
            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "+ getDrivers()");

            QvDataContractDriverListResponse retVal = new QvDataContractDriverListResponse();

            try
            {

                retVal.qDrivers.AddRange(this.ConnectorMap.Values.Select(a => a.Name).ToList());

            }
            catch (Exception e)
            {
                QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Error, "getDrivers() : " + e.Message);
                throw e;
            }

            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "- getDrivers()");

            return retVal;
        }

        public QvDataContractResponse getDriverConnectParams(IQlikSimpleConnector driver)
        {
            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "+ getDriverConnectParams()");

            QvDataContractDriverParamListResponse retVal;

            try
            {
                retVal = new QvDataContractDriverParamListResponse
                {
                    qDriverParamList = driver
                        .getDriverParams()
                        .ToList()
                };

            }
            catch (Exception e)
            {
                QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Error, "getDriverConnectParams() : " + e.Message);
                throw e;
            }

            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "- getDriverConnectParams()");

            return retVal;
        }

        public QvDataContractResponse getDatabases(IQlikSimpleConnector driver, Dictionary<string, string> userParameters)
        {
            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "+ getDatabases()");

            QvDataContractDatabaseListResponse retVal;

            try {

                retVal = new QvDataContractDatabaseListResponse
                {
                    qDatabases = driver
                        .getDatabases(userParameters)
                        .ToArray()
                };

            }
            catch (Exception e)
            {
                QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Error, "getDatabases() : " + e.Message);
                throw e;
            }

            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "- getDatabases()");

            return retVal;

        }

        public QvDataContractResponse getOwners(IQlikSimpleConnector driver, string database, Dictionary<string, string> userParameters)
        {
            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "+ getOwners()");

            QvDataContractResponse retVal;

            try
            {
                Database db = driver.getDatabases(userParameters).Find(x => x.qName == database);

                retVal = new QvDataContractOwnerListResponse()
                {
                    qOwners = driver
                        .getOwners(db, userParameters)
                        .ToList()
            };

            }
            catch (Exception e)
            {
                QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Error, "getOwners() : " + e.Message);
                throw e;
            }

            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "- getOwners()");

            return retVal;

        }

        public QvDataContractResponse getTables(IQlikSimpleConnector driver, string database, string owner, Dictionary<string, string> userParameters)
        {

            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "+ getTables()");

            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, String.Format("getTables() : {0}", String.Join(", ", userParameters.Select(kv => String.Format("{0} : {1}", kv.Key, kv.Value)))));

            QvDataContractTableListResponse retVal;

            try {

                Database db = driver.getDatabases(userParameters).Find(x => x.qName == database);
                string ow = driver.getOwners(db, userParameters).Find(x => x == owner);

                retVal = new QvDataContractTableListResponse
                {
                    qTables = driver
                        .getTables(db, ow, userParameters)
                        .ToList()
                };

            }
            catch (Exception e)
            {
                QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Error, "getTables() : " + e.Message);
                throw e;
            }

            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "- getTables()");

            return retVal;
        }

        public QvDataContractResponse getFields(IQlikSimpleConnector driver, string database, string owner, string table, Dictionary<string, string> userParameters)
        {

            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "+ getFields()");

            QvDataContractResponse retVal;

            try {

                Database db = driver.getDatabases(userParameters).Find(x => x.qName == database);
                string ow = driver.getOwners(db, userParameters).Find(x => x == owner);
                QvxTable tb = driver.getTables(db, ow, userParameters).Find(x => x.TableName == table);

                retVal = new QvDataContractFieldListResponse
                {
                    qFields = driver.getFields(db, ow, tb, userParameters).ToArray()
                };

            }
            catch (Exception e)
            {
                QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Error, "getFields() : " + e.Message);
                throw e;
            }

            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "- getFields()");

            return retVal;
        }

        public QvDataContractResponse getPreview(IQlikSimpleConnector driver, string database, string owner, string table, Dictionary<string, string> userParameters)
        {

            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "+ getPreview()");

            QvDataContractPreviewResponse retVal;

            try {

                Database db = driver.getDatabases(userParameters).Find(x => x.qName == database);
                string ow = driver.getOwners(db, userParameters).Find(x => x == owner);
                QvxTable tb = driver.getTables(db, ow, userParameters).Find(x => x.TableName == table);

                retVal = new QvDataContractPreviewResponse()
                {
                    qPreview = new List<MyQvxDataRow>()
                    {
                        new MyQvxDataRow() {
                            qValues = driver.getFields(db, ow, tb, userParameters).Select(a => a.FieldName).ToList()
                        }
                    }
                };

                retVal.qPreview.AddRange(
                    driver.getPreview(db, ow, tb, userParameters).Select(
                        a => new MyQvxDataRow()
                        {
                            qValues = a
                        }
                    ).ToList()
                );

            }
            catch (Exception e)
            {
                QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Error, "getPreview() : " + e.Message);
                throw e;
            }

            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "- getPreview()");

            return retVal;
        }

        public QvDataContractResponse testDriver(IQlikSimpleConnector driver, Dictionary<string, string> userParameters)
        {
            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "+ testDriver()");

            Info retVal;

            try {

                retVal = new QvInfo { qMessage = "Driver not loaded", qOk = false };

                if (driver.testDriver(userParameters))
                {
                    retVal = new QvInfo { qMessage = "Driver loaded OK!", qOk = true };
                }
                

            }
            catch (Exception e)
            {
                QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Error, "testDriver() : " + e.Message);
                throw e;
            }

            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "- testDriver()");

            return retVal;
        }

        public QvDataContractResponse testConnection(IQlikSimpleConnector driver, Dictionary<string, string> userParameters)
        {
            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "+ testConnection()");

            Info retVal;

            try
            {
                retVal = new QvInfo { qMessage = "Connection failed", qOk = false };

                if (driver.testConnection(userParameters))
                {
                    retVal = new QvInfo { qMessage = "Connection OK!", qOk = true };
                }
            }
            catch (Exception e)
            {
                QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Error, "testConnection() : " + e.Message);
                throw e;
            }

            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "- testConnection()");

            return retVal;
        }

        public QvDataContractResponse createConnectionString(Dictionary<string, string> userParameters)
        {
            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "+ createConnectionString()");

            Info retVal;

            try
            {
                retVal = new QvInfo { qMessage = "Connection string creation failed", qOk = false };

                if (userParameters.ContainsKey("qConnectionId"))
                {
                    retVal = new QvInfo { qMessage = "qConnectionId=" + userParameters["qConnectionId"], qOk = true };
                }
            }
            catch (Exception e)
            {
                QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Error, "createConnectionString() : " + e.Message);
                throw e;
            }

            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "- createConnectionString()");

            return retVal;
        }
    }
}
