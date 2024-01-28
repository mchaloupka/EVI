using System;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Slp.Evi.Storage.MsSql;
using Slp.Evi.Storage.MsSql.Database;
using TCode.r2rml4net;
using Xunit;
using NotImplementedException = System.NotImplementedException;

namespace Slp.Evi.Test.System.Sparql.Vendor
{
    public class MsSqlSparqlTestSuite(MsSqlSparqlFixture fixture)
        : SparqlTestSuite<MsSqlEviStorage, MsSqlDatabase>(fixture), IClassFixture<MsSqlSparqlFixture>;

    public sealed class MsSqlSparqlFixture()
        : SparqlFixture<MsSqlEviStorage, MsSqlDatabase>(new MsSqlSparqlTestHelpers())
    {
        protected override MsSqlDatabase GetSqlDb() => new(GetConnectionString("mssql"), 30);
    }

    public class MsSqlSparqlTestHelpers
        : SparqlTestHelpers<MsSqlEviStorage, MsSqlDatabase>
    {
        /// <inheritdoc />
        protected override void CreateTable(MsSqlDatabase sqlDb, XElement table)
        {
            var tableName = table.Attribute("name").Value;
            sqlDb.ExecuteQuery(new MsSqlQuery($"IF OBJECT_ID(\'{tableName}\', 'U') IS NOT NULL DROP TABLE {tableName}"));

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

            RemovePrimaryKey(sqlDb, tableName, primaryKeyName);

            sqlDb.ExecuteQuery(new MsSqlQuery(sb.ToString()));

            AddPrimaryKey(sqlDb, tableName, primaryKeyName, table);
        }

        private void RemovePrimaryKey(MsSqlDatabase sqlDb, string tableName, string primaryKeyName)
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

            sqlDb.ExecuteQuery(new MsSqlQuery(sb.ToString()));
        }

        private void AddPrimaryKey(MsSqlDatabase sqlDb, string tableName, string primaryKeyName, XElement table)
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
                sqlDb.ExecuteQuery(new MsSqlQuery(sb.ToString()));
            }
        }

        /// <inheritdoc />
        protected override void ExecuteQuery(MsSqlDatabase sqlDb, XElement query)
        {
            sqlDb.ExecuteQuery(new MsSqlQuery(query.Value));
        }

        /// <inheritdoc />
        protected override MsSqlEviStorage CreateStorage(IR2RML mapping, MsSqlDatabase database) =>
            new(mapping, database);
    }
}
