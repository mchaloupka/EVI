using System.Collections.Concurrent;
using Slp.Evi.Storage.Database;
using Slp.Evi.Storage.MsSql;
using VDS.RDF.Storage;

namespace Slp.Evi.Test.System.Sparql
{
    public abstract class SparqlFixture
    {
        private readonly ConcurrentDictionary<string, MsSqlEviStorage> _storageCache = new ConcurrentDictionary<string, MsSqlEviStorage>();

        public IQueryableStorage GetStorage(string storageName)
        {
            return _storageCache.GetOrAdd(storageName, CreateStorage);
        }

        private MsSqlEviStorage CreateStorage(string storageName)
        {
            return SparqlTestHelpers.InitializeDataset(storageName, GetSqlDb());
        }

        protected abstract ISqlDatabase GetSqlDb();
    }
}