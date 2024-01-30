using System.Collections.Concurrent;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Slp.Evi.Storage.MsSql;
using Slp.Evi.Storage.MsSql.Database;
using Slp.Evi.Test.System.Sparql.Vendor;
using VDS.RDF.Storage;

namespace Slp.Evi.Test.System.Sparql
{
    public abstract class SparqlFixture<TStorage, TDatabase>
        where TStorage : IQueryableStorage
    {
        private readonly ConcurrentDictionary<string, TStorage> _storageCache = new();

        protected SparqlFixture(SparqlTestHelpers<TStorage, TDatabase> testHelpers)
        {
            TestHelpers = testHelpers;

            // Prepare all databases beforehand
            var datasetsToBootup =
                SparqlTestData.TestData
                    .Select(x => x[0])
                    .Cast<string>()
                    .Distinct();

            foreach (var dataset in datasetsToBootup)
            {
                GetStorage(dataset);
            }
        }

        public SparqlTestHelpers<TStorage, TDatabase> TestHelpers { get; }

        public IQueryableStorage GetStorage(string storageName)
        {
            return _storageCache.GetOrAdd(storageName, CreateStorage);
        }

        protected abstract TDatabase GetSqlDb();

        protected TStorage CreateStorage(string storageName) =>
            TestHelpers.InitializeDataset(storageName, GetSqlDb);

        protected string GetConnectionString(string connectionName)
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("database.json")
                .AddEnvironmentVariables();

            var config = builder.Build();
            return config.GetConnectionString(connectionName);
        }
    }
}