using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using Slp.Evi.Benchmark.Sparql;

namespace Slp.Evi.Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
#if DEBUG
            BenchmarkRunner.Run<SparqlBenchmark>(new DebugBuildConfig());
#else
            BenchmarkRunner.Run<SparqlBenchmark>();
#endif

        }
    }
}
