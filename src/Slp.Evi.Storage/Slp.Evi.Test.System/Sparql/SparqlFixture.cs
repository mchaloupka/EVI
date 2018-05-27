using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Slp.Evi.Storage;
using Slp.Evi.Storage.Bootstrap;
using Slp.Evi.Storage.Database;

namespace Slp.Evi.Test.System.Sparql
{
    public abstract class SparqlFixture
    {
        private readonly ConcurrentDictionary<string, EviQueryableStorage> _storages = new ConcurrentDictionary<string, EviQueryableStorage>();

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
            return _storages.GetOrAdd(storageName, CreateStorage);
        }

        private EviQueryableStorage CreateStorage(string storageName)
        {
            return SparqlTestHelpers.InitializeDataset(storageName, GetSqlDb(), GetStorageFactory());
        }

        protected abstract ISqlDatabase GetSqlDb();
    }
}