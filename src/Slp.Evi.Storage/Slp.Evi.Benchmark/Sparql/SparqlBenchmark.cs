using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using Slp.Evi.Test.System.Sparql;
using VDS.RDF.Storage;
using Slp.Evi.Test.System.Sparql.Vendor;

namespace Slp.Evi.Benchmark.Sparql
{
    public interface ISparqlBenchmarkArgument
    {
        void RunTest();
    }

    public class SparqlBenchmarkArguments<TStorage, TDatabase>
        : ISparqlBenchmarkArgument where TStorage : IQueryableStorage
    {
        private readonly string _dataset;
        private readonly string _queryFile;
        private readonly IQueryableStorage _storage;
        private readonly string _query;
        private readonly string _databaseName;

        public SparqlBenchmarkArguments(string dataset, string queryFile, string databaseName, SparqlFixture<TStorage, TDatabase> fixture)
        {
            _dataset = dataset;
            _queryFile = queryFile;
            _databaseName = databaseName;
            _storage = fixture.GetStorage(dataset);
            _query = SparqlTestData.GetQuery($"Data\\{_dataset}\\{_queryFile}.rq");
        }

        public void RunTest()
        {
            _storage.Query(_query);
        }

        public override string ToString()
        {
            // The argument is trimmed to 20 characters, let's make it fit
            var trimmedDataset = _dataset.Substring(0, 3);
            var totalLength = _databaseName.Length + trimmedDataset.Length + _queryFile.Length + 2;
            var toTrim = totalLength - 20;

            if (toTrim > 0)
            {
                return $"{_databaseName}-{trimmedDataset}.{_queryFile.Substring(toTrim)}";
            }
            else
            {
                return $"{_databaseName}-{trimmedDataset}.{_queryFile}";
            }
        }
    }

    public class SparqlBenchmark
    {
        public SparqlBenchmark()
        {
            var arguments = new List<ISparqlBenchmarkArgument>();
            AddDatabaseToArguments("mssql", new MsSqlSparqlFixture(), arguments);
            AddDatabaseToArguments("mysql", new MySqlSparqlFixture(), arguments);
            TestParamSource = arguments;
        }

        private static void AddDatabaseToArguments<TStorage, TDatabase> (
            string databaseName,
            SparqlFixture<TStorage, TDatabase> fixture,
            ICollection<ISparqlBenchmarkArgument> arguments
        ) where TStorage : IQueryableStorage
        {
            foreach (var testData in SparqlTestData.TestData)
            {
                arguments
                    .Add(new SparqlBenchmarkArguments<TStorage, TDatabase>(
                        testData[0] as string,
                        testData[1] as string,
                        databaseName,
                        fixture
                    )
                );
            }
        }

        public IEnumerable<ISparqlBenchmarkArgument> TestParamSource { get; }

        [ParamsSource(nameof(TestParamSource))]
        public ISparqlBenchmarkArgument Argument { get; set; }

        [Benchmark]
        public void RunTest()
        {
            Argument.RunTest();
        }
    }
}
