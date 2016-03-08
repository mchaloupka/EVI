using System.Collections.Generic;
using System.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Slp.Evi.Storage;
using Slp.Evi.Storage.Bootstrap;
using Slp.Evi.Storage.Database;
using Slp.Evi.Storage.Database.Vendor.MsSql;

namespace Slp.Evi.Test.System.SPARQL.SPARQL_TestSuite.Db
{
    [TestClass]
    public class MSSQLDb : TestSuite
    {
        private static Dictionary<string, EviQueryableStorage> _storages;

        [ClassInitialize]
        public static void TestSuiteInitialization(TestContext context)
        {
            _storages = new Dictionary<string, EviQueryableStorage>();

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

        private static IEviQueryableStorageFactory GetStorageFactory()
        {
            return new DefaultEviQueryableStorageFactory();
        }

        protected override EviQueryableStorage GetStorage(string storageName)
        {
            return _storages[storageName];
        }
    }
}
