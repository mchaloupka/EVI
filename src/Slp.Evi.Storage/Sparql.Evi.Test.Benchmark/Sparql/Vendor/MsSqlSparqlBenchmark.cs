using System;
using System.Collections.Generic;
using System.Text;
using Slp.Evi.Test.System.Sparql.Vendor;

namespace Sparql.Evi.Test.Benchmark.Sparql.Vendor
{
    public class MsSqlSparqlBenchmark
        : SparqlBenchmark
    {
        public MsSqlSparqlBenchmark()
            : base(new MsSqlSparqlFixture())
        { }
    }
}
