using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.RegularExpressions; 
using QlikView.Qvx.QvxLibrary;


namespace QlikConnector
{
    public class MyQvxDataRow
        {
            public MyQvxDataRow()
            {
                this.qValues = new List<string>();
            }

            public List<string> qValues { get; set; }
        }

    internal class QlikConnectorConnection : QvxConnection
    {
        private QlikConnectorServer parent;
        Dictionary<string, string> args = null;


        public QlikConnectorConnection(QlikConnectorServer qcs)
            : base()
        {
            QvxLog.SetLogLevels(true, true);

            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "+ QlikConnectorConnection()");

            try {

                this.parent = qcs;

            }
            catch (Exception e)
            {
                QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Error, "QlikConnectorConnection() : " + e.Message);
                throw e;
            }

            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "- QlikConnectorConnection()");
        }

        public override void Init()
        {
            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "+ Init()");

            try {

                if (this.MParameters != null)
                {

                    QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "Init() : " + String.Join("|", this.MParameters.Select(k => k.Key + ": " + k.Value).ToArray()));

                    if (this.MParameters.ContainsKey("qConnectionId"))
                    {   
                        this.args = parent.readParams(this.MParameters["qConnectionId"]);

                        if (this.args != null)
                        {
                            parent.Registered(this.args["qDriver"]).Init(this.args);
                            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "Init() : " + this.args["qDriver"]);
                        }
                    }

                }

                QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "Init()");

            }
            catch (Exception e)
            {
                QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Error, "Init() : " + e.Message);
                throw e;
            }

            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "- Init()");
        }

        public override QvxDataTable ExtractQuery(string query, List<QvxTable> tables)
        {
            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "+ ExtractQuery()");

            Dictionary<string, string> myArgs = this.args.Select(kv => kv).ToDictionary(kv => kv.Key, kv => kv.Value);
            QvxDataTable retVal = null;

            try {

                QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "ExtractQuery() : " + query + " " + ((tables != null) ? String.Join("|", tables.Select(t => t.TableName)) : ""));

                IQlikConnector c = this.parent.Registered(myArgs["qDriver"]);

                QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "ExtractQuery() : driver found!");

                string pattern =
                    "^" +
                    "SELECT" +
                        "\\s*(?<select>" +
                            "((\\[(?<field>[^\\[\\]]+)\\]|(?<field>[^\\[\\],\\s]+))(\\s*,\\s*))*" +
                            "((\\[(?<field>[^\\[\\]]+)\\]|(?<field>[^\\[\\],\\s]+)))*" +
                        ")\\s*" +
                    "FROM" +
                        "\\s*(?<from>" +
                            "(\\[(?<db>[^\\[\\]]*?)\\]|(?<db>[^\\[\\]\\.\\s]*?))" +
                            "\\s*\\.\\s*" +
                            "(\\[(?<table>[^\\[\\]]*?)\\]|(?<table>[^\\[\\]\\.\\s]*?))" +
                        ")\\s*" +
                    "(WHERE" +
                        "\\s*(?<where>" +
                            ".*" +
                        ")\\s*" +
                    ")?" +
                    "$"
                ;

                Match m = Regex.Match(query.Trim(), pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline
                );

                if (m.Success)
                {
                    QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "ExtractQuery() : query matched!");

                    string select = m.Groups["select"].Value.Trim();
                    string from = m.Groups["from"].Value.Trim();
                    string where = (m.Groups["where"].Success) ? m.Groups["where"].Value.Trim() : null;

                    QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, String.Format("ExtractQuery() : {0}, {1}, {2}!", select, from, where));

                    if (m.Groups["where"].Success)
                    {
                        bool quoted = false, escaped = false, name = true;

                        Dictionary<string, string> currParam = new Dictionary<string, string>();

                        string currParamName = "";
                        string currParamValue = "";

                        #region //where clause parsing

                        int i = 0;
                        where.ToList().ForEach(chr =>
                        {
                            bool addchar = false;

                            if (!escaped && !quoted && chr == '\\')
                            {
                                throw new ArgumentOutOfRangeException(String.Format("unescaped \\ @{0} in where clause", i));
                            }

                            if (!escaped && quoted && chr == '\\')
                            {
                                escaped = true;
                            }
                            else if (!escaped && !quoted && chr == '=')
                            {
                                name = false;
                                escaped = false;
                            }
                            else if (!escaped && chr == '"')
                            {
                                if (quoted)
                                {
                                    if (!name)
                                    {
                                        currParam.Add(currParamName, currParamValue);

                                        currParamName = "";
                                        currParamValue = "";
                                        name = true;
                                    }
                                    else
                                    {
                                        throw new ArgumentOutOfRangeException(String.Format("quote in param name @{0} in where clause", i));
                                    }
                                }

                                quoted = !quoted;
                                escaped = false;
                            }
                            else
                            {
                                addchar = true;
                                escaped = false;
                            }

                            if (addchar)
                            {
                                if (name) currParamName += chr;
                                else currParamValue += chr;
                            }


                            i++;
                        });

                        #endregion

                        currParam.ToList().ForEach(kv =>
                        {
                            Match mWhere = Regex.Match(kv.Key, "^(\\s*AND)?\\s*((?<param>[^\\s]+)|\\[(?<param>[^\\]]+)\\])\\s*$");

                            if (mWhere.Success)
                            {
                                QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, String.Format("ExtractQuery() : {0}, {1}!", mWhere.Groups["param"].Value, kv.Value));

                                myArgs[mWhere.Groups["param"].Value.Trim().Replace("[", "").Replace("]", "")] = kv.Value.Trim();
                            }
                            else
                            {
                                throw new ArgumentOutOfRangeException();
                            }
                        });
                    }

                    List<string> fields = m.Groups["field"].Captures.Cast<Capture>().Select(cap => cap.Value).ToList();

                    QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, String.Format("ExtractQuery() : {0}!", String.Join("|", fields)));

                    string fromDb = m.Groups["db"].Value.Trim();
                    string fromTable = m.Groups["table"].Value.Trim();

                    QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, String.Format("ExtractQuery() : {0}, {1}!", fromDb, fromTable));

                    Database db = c.getDatabases(myArgs).Where(dbItem => dbItem.qName == fromDb).FirstOrDefault();
                    if (db != null)
                    {
                        QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "ExtractQuery() : found db!");

                        QvxTable table = c.getTables(db, myArgs).Where(tableItem => tableItem.TableName == fromTable).FirstOrDefault();
                        if (table != null)
                        {
                            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "ExtractQuery() : found table!");
                            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, String.Format("ExtractQuery() : {0}", table.GetRows().Count()));

                            QvxDataTable t = new QvxDataTable(table);
                            t.Select(table.Fields.Where(fld => fields.Contains(fld.FieldName)).ToArray());
                            return t;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Error, "ExtractQuery() : " + e.Message);
                throw e;
            }

            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "- ExtractQuery()");

            return retVal;
        }

    }
}
