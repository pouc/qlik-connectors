using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.RegularExpressions;

namespace Tests
{
    class Program
    {
        static void Main(string[] args)
        {

            QlikConnectorJSON.QlikConnectorJSON conn = new QlikConnectorJSON.QlikConnectorJSON();

            //conn.getTables(null, new Dictionary<string, string>()
            //{
            //    {"qDriver", "JSON Driver"},
            //    {"Host", "http://10.76.224.36:9200"},
            //    {"Http Method", "POST"},
            //    {"Method", "twitter/_search"},
            //    {"Params", "{\"query\":{\"bool\":{\"must\":[{\"match_all\":{}}],\"must_not\":[],\"should\":[]}},\"from\":0,\"size\":100000,\"sort\":[],\"facets\":{}}"}
            //});


            IEnumerable<System.Data.DataTable> tl = conn.getRawTables(null, new Dictionary<string, string>()
            {
                {"qDriver", "JSON Driver"},
                {"Host", "http://10.76.224.36:9200"},
                {"Http Method", "POST"},
                {"Method", "twitter/_search?search_type=scan&scroll=10m&size=1"},
                {"Params", "{\"query\":{\"match_all\" : {}}}"}
            });

            System.Data.DataTable t = tl.Where(tItem => tItem.TableName == "root").FirstOrDefault();
            string id = t.Rows[0][0].ToString();

            string query = @"
                SELECT
                    root_hits_hits__source_text
                FROM
                    Host.root_hits_hits__source
                WHERE
                    Method = ""_search/scroll?scroll=10m""
                    AND Params = """ + id + @"""
                ";

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
                string select = m.Groups["select"].Value.Trim();
                string from = m.Groups["from"].Value.Trim();
                string where = (m.Groups["where"].Success) ? m.Groups["where"].Value.Trim() : null;

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
                        Match mWhere = Regex.Match(kv.Key, "(\\s*AND)?\\s*(?<param>[^\\s]+)\\s*");

                        if (mWhere.Success)
                        {
                            
                        }
                        else
                        {
                            throw new ArgumentOutOfRangeException();
                        }
                    });
                }

                List<string> fields = m.Groups["field"].Captures.Cast<Capture>().Select(cap => cap.Value).ToList();

                string fromDb = m.Groups["db"].Value.Trim();
                string fromTable = m.Groups["table"].Value.Trim();

                
            }

            //conn.getTables(null, new Dictionary<string, string>()
            //{
            //    {"qDriver", "JSON Driver"},
            //    {"Host", "http://10.76.224.36:9200"},
            //    {"Http Method", "POST"},
            //    {"Method", "_search/scroll?scroll=10m"},
            //    {"Params", id}
            //});

            //System.Data.DataTable t = tlHit.Where(tItem => tItem.TableName == "root_hits_hits__source").FirstOrDefault();
            //t.Rows.Cast<System.Data.DataRow>().ToList().ForEach(row =>
            //{
            //    foreach (System.Data.DataColumn dc in row.Table.Columns)
            //    {
            //        Console.Write(row[dc].ToString());
            //        Console.Write("\t");
            //    }
            //    Console.WriteLine();

            //});


            return;

//            string query = @"
//            SELECT
//                root_hits_hits__source_message,
//                root_hits_hits__source_user,
//                root_hits_hits__source_client,
//                root_hits_hits__source_retweeted,
//                root_hits_hits__source_source,
//                root_hits_hits__source_type,
//                root_hits_hits__source_tags,
//                [root_hits_hits__source_in-reply-to],
//                root_hits_hits__source_urls,
//                root_hits_hits_Id
//            FROM
//	            Host.root_hits_hits__source
//            WHERE
//	            Method = ""twitter/_search"" AND
//                params = ""{\""query\"":{\""bool\"":{\""must\"":[{\""match_all\"":{}}],\""must_not\"":[],\""should\"":[]}},\""from\"":0,\""size\"":100,\""sort\"":[],\""facets\"":{}}""
//            ";

//            string pattern =
//                "^" +
//                "SELECT" +
//                    "\\s*(?<select>" +
//                        "((\\[(?<field>[^\\[\\]]+)\\]|(?<field>[^\\[\\],\\s]+))(\\s*,\\s*))*" +
//                        "((\\[(?<field>[^\\[\\]]+)\\]|(?<field>[^\\[\\],\\s]+)))*" +
//                    ")\\s*" +
//                "FROM" +
//                    "\\s*(?<from>" +
//                        "(\\[(?<db>[^\\[\\]]*?)\\]|(?<db>[^\\[\\]\\.\\s]*?))" +
//                        "\\s*\\.\\s*" +
//                        "(\\[(?<table>[^\\[\\]]*?)\\]|(?<table>[^\\[\\]\\.\\s]*?))" +
//                    ")\\s*" +
//                "(WHERE" +
//                    "\\s*(?<where>" +
//                        "(\\s*" +
//                            "(?<param>[^=\\s]*?)" +
//                            "\\s*=\\s*" +
//                            "\"" +
//                                "(?<value>" +
//                                    "[^\"\\\\]*" +
//                                    "(\\\\.[^\"\\\\]*)*" +
//                                ")" +
//                            "\""+
//                            "\\s*(AND)?" +
//                        ")+" +
//                    ")\\s*" +
//                ")?" +
//                "$"
//            ;

//            Console.WriteLine(pattern);

//            Match m = Regex.Match(query.Trim(), pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);

//            for(int i = 0; i < m.Groups["param"].Captures.Count; i++)
//            {
//                string whereParam = m.Groups["param"].Captures[i].Value.Trim();
//                string whereValue = m.Groups["value"].Captures[i].Value.Trim();

//                Console.WriteLine(String.Format("ExtractQuery() : {0}, {1}!", whereParam, whereValue));
//            }

//            if (m.Success)
//            {
//                var select = m.Groups["select"];
//                var field = m.Groups["field"];
//                var from = m.Groups["from"];
//                var db = m.Groups["db"];
//                var table = m.Groups["table"];
//                var where = m.Groups["where"];
//                var param = m.Groups["param"];
//                var value = m.Groups["value"];
//            }

//            var a = 1;
        }
    }
}
