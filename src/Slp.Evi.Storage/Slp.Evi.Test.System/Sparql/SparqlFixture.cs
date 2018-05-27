using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Slp.Evi.Storage;
using Slp.Evi.Storage.Bootstrap;
using Slp.Evi.Storage.Database;

namespace Slp.Evi.Test.System.SPARQL.SPARQL_TestSuite
{
    public abstract class SparqlFixture
    {
        private readonly Dictionary<string, EviQueryableStorage> _storages = new Dictionary<string, EviQueryableStorage>();

        protected SparqlFixture()
        {
            var storageNames = SparqlTestSuite.TestData.Select(x => x[0]).Cast<string>().Distinct();

            foreach (var dataset in storageNames)
            {
                var storage = SparqlTestHelpers.InitializeDataset(dataset, GetSqlDb(), GetStorageFactory());
                _storages.Add(dataset, storage);
            }
        }

        private IEviQueryableStorageFactory GetStorageFactory()
        {
            var loggerFactory = new LoggerFactory();

            if (Environment.GetEnvironmentVariable("APPVEYOR") != "True")
            {
                loggerFactory.AddConsole(LogLevel.Trace);
            }

            return new DefaultEviQueryableStorageFactory(loggerFactory);
        }

        public EviQueryableStorage GetStorage(string storageName)
        {
            return _storages[storageName];
        }

        protected abstract ISqlDatabase GetSqlDb();
    }
}