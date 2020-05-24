using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using Slp.Evi.Storage;
using Slp.Evi.Test.System.Sparql;

namespace Slp.Evi.Benchmark.Sparql
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

        public override string ToString()
        {
            // The argument is trimmed to 20 characters, let's make it fit
            var totalLength = _dataset.Length + _queryFile.Length + 1;
            var toTrim = totalLength - 20;

            if (toTrim > 0)
            {
                var trimmedDataset = _dataset.Substring(0, 3);
                toTrim -= (_dataset.Length - 3);

                if (toTrim > 0)
                {
                    return $"{trimmedDataset}.{_queryFile.Substring(toTrim)}";
                }
                else
                {
                    return $"{trimmedDataset}.{_queryFile}";
                }
            }
            else
            {
                return $"{_dataset}-{_queryFile}";
            }
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
