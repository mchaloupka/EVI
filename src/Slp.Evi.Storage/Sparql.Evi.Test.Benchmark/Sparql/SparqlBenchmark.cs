using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using Slp.Evi.Storage;
using Slp.Evi.Test.System.Sparql;

namespace Sparql.Evi.Test.Benchmark.Sparql
{
    public class SparqlBenchmarkArguments
    {
        private string _dataset;
        private string _queryFile;
        private EviQueryableStorage _storage;
        private string _query;

        public SparqlBenchmarkArguments(string dataset, string queryFile, SparqlFixture fixture)
        {
            _dataset = dataset;
            _queryFile = queryFile;
            _storage = fixture.GetStorage(dataset);
            _query = SparqlTestSuite.GetQuery($"Data\\{_dataset}\\{_queryFile}.rq");
        }

        public void RunTest()
        {
            _storage.Query(_query);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{_dataset}\\{_queryFile}.rq";
        }
    }

    public abstract class SparqlBenchmark
    {
        private readonly SparqlFixture _fixture;

        protected SparqlBenchmark(SparqlFixture fixture)
        {
            _fixture = fixture;
        }

        public IEnumerable<SparqlBenchmarkArguments> TestParamSource =>
            SparqlTestSuite.TestData
                .Select(x => new SparqlBenchmarkArguments(x[0] as string, x[1] as string, _fixture));

        [ParamsSource(nameof(TestParamSource))]
        public SparqlBenchmarkArguments Argument { get; set; }

        [Benchmark]
        public void RunTest()
        {
            Argument.RunTest();
        }
    }
}
