using System.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Slp.r2rml4net.Storage.Bootstrap;
using Slp.r2rml4net.Storage.Sql;

namespace Slp.r2rml4net.Test.System.SPARQL.SPARQL_TestSuite.Db
{
    [TestClass]
    public class MSSQLDb : TestSuite
    {
        protected override IR2RMLStorageFactory GetStorageFactory()
        {
            return new DefaultIr2RmlStorageFactory();
        }

        protected override ISqlDb GetSqlDb()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["mssql_connection"].ConnectionString;
            return (new DefaultSqlDbFactory()).CreateSqlDb(connectionString);
        }
    }
}
