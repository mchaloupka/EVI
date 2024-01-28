using Slp.Evi.Storage.MsSql;
using Slp.Evi.Storage.MsSql.Database;
using Slp.Evi.Test.System.Sparql.Vendor;

namespace Slp.Evi.Benchmark.Sparql.Vendor
{
    public class MsSqlSparqlBenchmark()
        : SparqlBenchmark<MsSqlEviStorage, MsSqlDatabase>(new MsSqlSparqlFixture());
}
