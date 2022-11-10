using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JsonToSQL
{
    public class JsonConvert
    {

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string DatabaseName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string DefaultTableName { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <value></value>
        public string SchemaName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public bool HasDropTableStatement { get; set; }
        public bool HasCreateDbStatement { get; set; }

        public string CreateDbStatement =>
            "USE master" + Environment.NewLine +
            "GO" + Environment.NewLine + Environment.NewLine +
            "IF NOT EXISTS(SELECT name FROM sys.databases" + Environment.NewLine +
           $"WHERE name = '{DatabaseName}')" + Environment.NewLine +
           $"CREATE DATABASE {DatabaseName}" + Environment.NewLine +
            "GO" + Environment.NewLine;

        // should only be a method if multiple table names are specified (which aren't though)
        public string DropTableStatement(string tableName) =>
           $"USE {DatabaseName}" + Environment.NewLine +
            "GO" + Environment.NewLine + Environment.NewLine +
           $"IF OBJECT_ID('{SchemaName}.{tableName}', 'U') IS NOT NULL" + Environment.NewLine +
           $"DROP TABLE {SchemaName}.{tableName}" + Environment.NewLine +
            "GO" + Environment.NewLine + Environment.NewLine;


        /// <summary>
        /// Creates a JsonConvert object for generation of DDL Code
        /// </summary>
        /// <param name="databaseName">name of the database</param>
        /// <param name="defaultTableName">if json parser wont find multiple tables, this defualt will be used</param>
        /// <param name="schemaName">name of used schema (default dbo)</param>
        /// <param name="hasDropTableStatement">is drop table if exists required? (default true)</param>
        /// <param name="HasCreateDbStatement">is craete db if not exists required? (default false)</param>
        public JsonConvert(string databaseName
            , string defaultTableName
            , string schemaName = "dbo"
            , bool hasDropTableStatement = true
            , bool hasCreateDbStatement = false
        )
        {
            DatabaseName = databaseName;
            DefaultTableName = defaultTableName;
            SchemaName = schemaName;
            HasDropTableStatement = hasDropTableStatement;
            HasCreateDbStatement = hasCreateDbStatement;
        }


        // ? Id will always be auto generated, right?

        private DataSet ds = new DataSet();
        private List<TableRelation> relations = new List<TableRelation>();

        private int index = 0;

        public string ToSQL(Stream jsonStream)
        {
            using (StreamReader sr = new StreamReader(jsonStream))
            {
                string json = sr.ReadToEnd();

                return ToSQL(json);
            }
        }

        public string ToSQL(string json)
        {
            ds.DataSetName = this.DatabaseName;

            var jToken = JToken.Parse(json);

            if (jToken.Type == JTokenType.Object) //single json object 
            {
                ParseJObject(jToken.ToObject<JObject>(), this.DefaultTableName, string.Empty, 1, 1);
            }
            else //multiple json objects in array 
            {
                var counter = 1;
                // HACK there are no derived table names here
                foreach (var jObject in jToken.Children<JObject>())
                {
                    ParseJObject(jObject, this.DefaultTableName, string.Empty, counter, counter);
                    counter++;
                }
            }

            foreach (TableRelation rel in relations.OrderByDescending(i => i.Order))
            {
                if (string.IsNullOrEmpty(rel.Source)) continue;

                DataTable source = ds.Tables[rel.Source];
                DataTable target = ds.Tables[rel.Target];

                if (source == null)
                {
                    target.Columns.Remove(rel.Source + "ID");
                    continue;
                }

                source.PrimaryKey = new DataColumn[] { source.Columns[0] };

                ForeignKeyConstraint fk = new ForeignKeyConstraint("ForeignKey", source.Columns[0], target.Columns[1]);
                target.Constraints.Add(fk);
            }

            string createScript = GenerateDbSchema(ds, relations, HasDropTableStatement, HasCreateDbStatement);


            string script = createScript + SqlScript.GenerateInsertQueries(ds, relations);

            return script;
        }


        private void ParseJObject(JObject jObject, string tableName, string parentTableName, int pkValue, int fkValue)
        {
            if (jObject.Count > 0)
            {
                DataTable dt = new DataTable(tableName);

                Dictionary<string, string> dic = new Dictionary<string, string>();
                List<SqlColumn> listColumns = new List<SqlColumn>();
                //SqlColumn sqlColumn = new SqlColumn();
                //listColumns.Add(sqlColumn);
                //sqlColumn
                listColumns.Add(new SqlColumn { Name = tableName + "ID", Value = pkValue.ToString(), Type = "System.Int32" });
                dic[tableName + "ID"] = pkValue.ToString(); //primary key

                if (!string.IsNullOrEmpty(parentTableName))
                {
                    listColumns.Add(new SqlColumn { Name = parentTableName + "ID", Value = fkValue.ToString(), Type = "System.Int32" });
                    dic[parentTableName + "ID"] = fkValue.ToString(); //foreign key
                }

                foreach (JProperty property in jObject.Properties())
                {
                    string key = property.Name;
                    JToken jToken = property.Value;

                    if (jToken.Type == JTokenType.Object)
                    {
                        var jO = jToken.ToObject<JObject>();
                        ParseJObject(jO, tableName + "_" + key, tableName, pkValue, pkValue);
                        //pkValue = pkValue + 1;
                    }
                    else if (jToken.Type == JTokenType.Array)
                    {
                        var arrs = jToken.ToObject<JArray>();
                        var objects = arrs.Children<JObject>();
                        if (objects.Count() > 0)
                        {
                            //index = 0;
                            foreach (var arr in objects)
                            {
                                index = index + 1;
                                var jo = arr.ToObject<JObject>();
                                ParseJObject(jo, tableName + "_" + key, tableName, index, pkValue);
                            }
                        }
                        else
                        {
                            listColumns.Add(new SqlColumn { Name = key, Value = string.Join(",", arrs.ToObject<string[]>()), Type = "System." + jToken.Type.ToString() });
                            dic[key] = string.Join(",", arrs.ToObject<string[]>());
                        }
                    }
                    else
                    {
                        listColumns.Add(new SqlColumn { Name = key, Value = jToken.ToString(), Type = "System." + jToken.Type.ToString() });
                        dic[key] = jToken.ToString();
                    }
                }

                pkValue = pkValue + 1;

                if (ds.Tables.Contains(dt.TableName)) //for array items
                {
                    //ds.Tables[dt.TableName].Rows.Add(dic.Values.ToArray());
                    foreach (string key in dic.Keys)
                    {
                        if (!ds.Tables[dt.TableName].Columns.Contains(key))
                        {
                            ds.Tables[dt.TableName].Columns.Add(AddColumn(key, "System.String", false));
                        }
                    }

                    DataRow dr = ds.Tables[dt.TableName].NewRow();
                    foreach (string key in dic.Keys)
                    {
                        dr[key] = dic[key];
                    }

                    ds.Tables[dt.TableName].Rows.Add(dr);
                }
                else if (dic.Keys.Count > 1)
                {
                    for (int i = 0; i < dic.Keys.Count; i++)
                    {
                        string type = i == 0 ? "System.Int32" : "System.String";

                        if (!string.IsNullOrEmpty(parentTableName) && i == 1)
                        {
                            type = "System.Int32"; //foreign key
                        }

                        dt.Columns.Add(AddColumn(dic.Keys.ToArray()[i], type, i == 0 ? true : false));
                    }

                    dt.Rows.Add(dic.Values.ToArray());
                    ds.Tables.Add(dt);

                    relations.Add(new TableRelation()
                    {
                        Source = parentTableName,
                        Target = tableName,
                        Order = ds.Tables.Count
                    });
                }

            }
        }

        private DataColumn AddColumn(string name, string type, bool isPrimaryKey)
        {
            return new DataColumn()
            {
                ColumnName = name,
                DataType = System.Type.GetType(type),
                AutoIncrement = isPrimaryKey ? true : false,
                AutoIncrementSeed = 1,
                AutoIncrementStep = 1,
                AllowDBNull = true
            };
        }

        public string GenerateDbSchema(DataSet ds, List<TableRelation> relations, bool hasDropTableStatement, bool HasCreateDbStatement)
        {
            StringBuilder sb = new StringBuilder();

            if (HasCreateDbStatement)
            {
                sb.AppendLine(CreateDbStatement);
            }


            foreach (TableRelation rel in relations.OrderByDescending(i => i.Order))
            {
                DataTable table = ds.Tables[rel.Target];

                if (HasDropTableStatement)
                {
                    sb.AppendLine(DropTableStatement(table.TableName));
                }

                sb.AppendLine(SqlScript.CreateTABLE(table));
                sb.AppendLine("GO");
                sb.AppendLine(string.Empty);
            }

            return sb.ToString();
        }

    }
}
