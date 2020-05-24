using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using Slp.Evi.Benchmark.Sparql.Vendor;

namespace Slp.Evi.Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
#if DEBUG
            BenchmarkRunner.Run<MsSqlSparqlBenchmark>(new DebugBuildConfig());
#else
             BenchmarkRunner.Run<MsSqlSparqlBenchmark>();
#endif

        }
    }
}
