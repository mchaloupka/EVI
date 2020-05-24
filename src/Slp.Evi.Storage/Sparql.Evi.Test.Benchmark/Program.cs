using BenchmarkDotNet.Running;
using Sparql.Evi.Test.Benchmark.Sparql.Vendor;

namespace Sparql.Evi.Test.Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<MsSqlSparqlBenchmark>();
        }
    }
}
