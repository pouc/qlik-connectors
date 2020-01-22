using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QlikView.Qvx.QvxLibrary;

namespace QlikSimpleConnector
{
    public enum DriverParamType { label, list };
    public enum DriverParamValueType { i, s };

    public class DriverParam
    {
        public DriverParamType paramType;
        public string paramName;
        public DriverParamValueType paramValueType;
        public Dictionary<string, List<DriverParam>> paramValues;
        public Dictionary<string, List<DriverParam>> selectedValues;
    }

    public interface IQlikSimpleConnectorHost
    {
        bool Register(IQlikSimpleConnector ipi);
    }

    public interface IQlikSimpleConnector
    {
        string Name { get; }
        IQlikSimpleConnectorHost Host { get; set; }

        bool SimplifiedParams { get; }
        List<DriverParam> getDriverParams();

        bool testDriver(Dictionary<string, string> args);
        bool testConnection(Dictionary<string, string> args);

        List<Database> getDatabases(Dictionary<string, string> args);
        List<string> getOwners(Database database, Dictionary<string, string> args);
        List<QvxTable> getTables(Database database, string owner, Dictionary<string, string> args);
        List<QvxField> getFields(Database database, string owner, QvxTable table, Dictionary<string, string> args);
        List<List<string>> getPreview(Database database, string owner, QvxTable table, Dictionary<string, string> args);

        void Init(Dictionary<string, string> args);
    }
}
