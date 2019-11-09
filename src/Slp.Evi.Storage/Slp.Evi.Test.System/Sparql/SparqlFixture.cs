using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Slp.Evi.FuncStorage;
using Slp.Evi.Storage.Database;
using VDS.RDF.Storage;

namespace Slp.Evi.Test.System.Sparql
{
    public abstract class SparqlFixture
    {
        private readonly ConcurrentDictionary<string, EviStorage> _storages = new ConcurrentDictionary<string, EviStorage>();

        public IQueryableStorage GetStorage(string storageName)
        {
            return _storages.GetOrAdd(storageName, CreateStorage);
        }

        private EviStorage CreateStorage(string storageName)
        {
            return SparqlTestHelpers.InitializeDataset(storageName, GetSqlDb());
        }

        protected abstract ISqlDatabase GetSqlDb();
    }
}