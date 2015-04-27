using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QlikView.Qvx.QvxLibrary;

namespace QlikConnector
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

    public interface IQlikConnectorHost
    {
        bool Register(IQlikConnector ipi);
    }

    public interface IQlikConnector
    {
        string Name { get; }
        IQlikConnectorHost Host { get; set; }

        List<DriverParam> getDriverParams();

        bool testDriver(Dictionary<string, string> args);
        bool testConnection(Dictionary<string, string> args);

        List<Database> getDatabases(Dictionary<string, string> args);
        List<QvxTable> getTables(Database database, Dictionary<string, string> args);
        List<QvxField> getFields(Database database, QvxTable table, Dictionary<string, string> args);
        List<List<string>> getPreview(Database database, QvxTable table, Dictionary<string, string> args);

        void Init(Dictionary<string, string> args);
    }
}
