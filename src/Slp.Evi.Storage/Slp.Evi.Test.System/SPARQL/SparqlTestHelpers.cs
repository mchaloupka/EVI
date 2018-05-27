using System;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Slp.Evi.Storage;
using Slp.Evi.Storage.Bootstrap;
using Slp.Evi.Storage.Database;
using TCode.r2rml4net.Mapping.Fluent;

namespace Slp.Evi.Test.System.Sparql
{
    public static class SparqlTestHelpers
    {
        public static string GetPath(string dataFile)
        {
            var path = $".\\SPARQL\\SPARQL_TestSuite\\{dataFile}";
            return path;
        }

        public static EviQueryableStorage InitializeDataset(string dataset, ISqlDatabase sqlDb, IEviQueryableStorageFactory storageFactory)
        {
            var datasetFile = GetPath(@"Data\Datasets\" + dataset);

            var doc = XDocument.Load(datasetFile);
            var sqlCommands = doc.Root
                .Elements()
                .Where(x => x.Name == "sql")
                .Single()
                .Elements();

            foreach (var command in sqlCommands)
            {
                if (command.Name == "table")
                    CreateTable(sqlDb, command);
                else if (command.Name == "query")
                    ExecuteQuery(sqlDb, command);
                else
                    throw new Exception(String.Format("Unknown sql command {1} when creating dataset {0}", dataset, command.Name));
            }

            var mappingString = doc.Root.Elements().Where(x => x.Name == "mapping").Single().Value;
            var mapping = R2RMLLoader.Load(mappingString);
            return new EviQueryableStorage(sqlDb, mapping, storageFactory);
        }

        private static void ExecuteQuery(ISqlDatabase sqlDb, XElement query)
        {
            sqlDb.ExecuteQuery(query.Value);
        }

        private static void CreateTable(ISqlDatabase sqlDb, XElement table)
        {
            var tableName = table.Attribute("name").Value;
            sqlDb.ExecuteQuery(String.Format("IF OBJECT_ID(\'{0}\', 'U') IS NOT NULL DROP TABLE {0}", tableName));

            StringBuilder sb = new StringBuilder();
            sb.Append("CREATE TABLE ");
            sb.Append(tableName);
            sb.Append(" (");

            bool first = true;

            foreach (var tablePart in table.Elements())
            {
                if (!first)
                {
                    sb.Append(", ");
                }
                else
                {
                    first = false;
                }

                if (tablePart.Name == "column")
                {
                    sb.Append(tablePart.Attribute("name").Value);
                    sb.Append(' ');
                    sb.Append(tablePart.Attribute("type").Value);
                    sb.Append(' ');

                    if (tablePart.Attribute("nullable") != null && tablePart.Attribute("nullable").Value == "true")
                    {
                        sb.Append("NULL");
                    }
                    else
                    {
                        sb.Append("NOT NULL");
                    }
                }
                else
                    throw new Exception(String.Format("Unknown table part {1} when creating table {0}", tableName, tablePart.Name));
            }

            sb.Append(")");

            string primaryKeyName = "pk_" + tableName;

            RemovePrimaryKey(sqlDb, tableName, primaryKeyName, table);

            sqlDb.ExecuteQuery(sb.ToString());

            AddPrimaryKey(sqlDb, tableName, primaryKeyName, table);
        }

        private static void RemovePrimaryKey(ISqlDatabase sqlDb, string tableName, string primaryKeyName, XElement table)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'");
            sb.Append(primaryKeyName);
            sb.Append("') AND type in (N'U'))");
            sb.AppendLine();
            sb.Append("ALTER TABLE ");
            sb.Append(tableName);
            sb.Append(" DROP CONSTRAINT ");
            sb.Append(primaryKeyName);

            sqlDb.ExecuteQuery(sb.ToString());
        }

        private static void AddPrimaryKey(ISqlDatabase sqlDb, string tableName, string primaryKeyName, XElement table)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("ALTER TABLE ");
            sb.Append(tableName);
            sb.Append(" ADD CONSTRAINT ");
            sb.Append(primaryKeyName);
            sb.Append(" PRIMARY KEY (");

            bool first = true;
            foreach (var tablePart in table.Elements())
            {
                if (tablePart.Name == "column"
                    && tablePart.Attribute("primary-key") != null
                    && tablePart.Attribute("primary-key").Value == "true")
                {
                    if (!first)
                    {
                        sb.Append(", ");
                    }
                    else
                    {
                        first = false;
                    }

                    sb.Append(tablePart.Attribute("name").Value);
                }
            }

            if (!first)
            {
                sb.Append(")");
                sqlDb.ExecuteQuery(sb.ToString());
            }
        }
    }
}