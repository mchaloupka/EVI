using System;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Slp.Evi.Storage.MySql;
using Slp.Evi.Storage.MySql.Database;
using TCode.r2rml4net;
using Xunit;

namespace Slp.Evi.Test.System.Sparql.Vendor
{
    public class MySqlSparqlTestSuite(MySqlSparqlFixture fixture)
        : SparqlTestSuite<MySqlEviStorage, MySqlDatabase>(fixture), IClassFixture<MySqlSparqlFixture>;

    public sealed class MySqlSparqlFixture()
        : SparqlFixture<MySqlEviStorage, MySqlDatabase>(new MySqlSparqlTestHelpers())
    {
        protected override MySqlDatabase GetSqlDb() => new(GetConnectionString("MySql"), 30);
    }

    public class MySqlSparqlTestHelpers
        : SparqlTestHelpers<MySqlEviStorage, MySqlDatabase>
    {
        /// <inheritdoc />
        protected override void CreateTable(MySqlDatabase sqlDb, XElement table)
        {
            var tableName = table.Attribute("name").Value;
            sqlDb.ExecuteQuery(new MySqlQuery($"DROP TABLE IF EXISTS {tableName}"));

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
                    sb.Append(tablePart.Attribute("mysqltype").Value);
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

            var primaryKeys = table.Elements()
                .Where(x => x.Name == "column" && x.Attribute("primary-key")?.Value == "true")
                .Select(x => x.Attribute("name").Value);

            if (primaryKeys.Any())
            {
                sb.Append(", PRIMARY KEY(");
                sb.Append(String.Join(", ", primaryKeys));
                sb.Append(")");
            }

            sb.Append(")");

            sqlDb.ExecuteQuery(new MySqlQuery(sb.ToString()));
        }

        /// <inheritdoc />
        protected override void ExecuteQuery(MySqlDatabase sqlDb, XElement query)
        {
            sqlDb.ExecuteQuery(new MySqlQuery(query.Value));
        }

        /// <inheritdoc />
        protected override MySqlEviStorage CreateStorage(IR2RML mapping, MySqlDatabase database) =>
            new(mapping, database);
    }
}
