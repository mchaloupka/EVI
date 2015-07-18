using System.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Slp.r2rml4net.Storage.Bootstrap;
using Slp.r2rml4net.Storage.Database;
using Slp.r2rml4net.Storage.Database.Vendor.MsSql;

namespace Slp.r2rml4net.Test.System.SPARQL.SPARQL_TestSuite.Db
{
    [TestClass]
    public class MSSQLDb : TestSuite
    {
        protected override IR2RmlStorageFactory GetStorageFactory()
        {
            return new DefaultIr2RmlStorageFactory();
        }

        protected override ISqlDatabase GetSqlDb()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["mssql_connection"].ConnectionString;
            return (new MsSqlDbFactory()).CreateSqlDb(connectionString);
        }
    }
}
