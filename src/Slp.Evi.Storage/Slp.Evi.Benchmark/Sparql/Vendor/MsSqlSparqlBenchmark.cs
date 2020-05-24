using Slp.Evi.Test.System.Sparql.Vendor;

namespace Slp.Evi.Benchmark.Sparql.Vendor
{
    public class MsSqlSparqlBenchmark
        : SparqlBenchmark
    {
        public MsSqlSparqlBenchmark()
            : base(new MsSqlSparqlFixture())
        { }
    }
}
