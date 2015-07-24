using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Slp.r2rml4net.Storage;
using Slp.r2rml4net.Storage.Bootstrap;
using Slp.r2rml4net.Storage.Database;
using Slp.r2rml4net.Storage.Database.Vendor.MsSql;
using TCode.r2rml4net.Mapping.Fluent;

namespace Slp.r2rml4net.Test.System.SPARQL.SPARQL_TestSuite.Db
{
    [TestClass]
    public class MSSQLDb : TestSuite
    {
        private static Dictionary<string, R2RMLStorage> _storages;

        [ClassInitialize]
        public static void TestSuiteInitialization(TestContext context)
        {
            _storages = new Dictionary<string, R2RMLStorage>();

            foreach (var dataset in StorageNames)
            {
                var storage = InitializeDataset(dataset, GetSqlDb(), GetStorageFactory());
                _storages.Add(dataset, storage);
            }
        }

        private static ISqlDatabase GetSqlDb()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["mssql_connection"].ConnectionString;
            return (new MsSqlDbFactory()).CreateSqlDb(connectionString);
        }

        private static IR2RMLStorageFactory GetStorageFactory()
        {
            return new DefaultR2RMLlStorageFactory();
        }

        protected override R2RMLStorage GetStorage(string storageName)
        {
            return _storages[storageName];
        }
    }
}
