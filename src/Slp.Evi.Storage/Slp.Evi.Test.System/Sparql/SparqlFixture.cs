using System.Collections.Concurrent;
using Slp.Evi.Storage.Database;
using Slp.Evi.Storage.MsSql;
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