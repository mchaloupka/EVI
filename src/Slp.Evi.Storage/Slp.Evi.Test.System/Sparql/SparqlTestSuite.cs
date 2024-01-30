using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using VDS.RDF.Storage;
using Xunit;

namespace Slp.Evi.Test.System.Sparql
{
    public abstract class SparqlTestSuite<TStorage, TDatabase>(SparqlFixture<TStorage, TDatabase> fixture) : BaseTestSuite
        where TStorage : IQueryableStorage
    {
        [Theory]
        [MemberData(nameof(TestData))]
        public void TestQuery(string dataset, string testName, QueryVerificationType verificationType)
        {
            var storage = fixture.GetStorage(dataset);
            var path = $"Data\\{dataset}\\{testName}";
            var query = SparqlTestData.GetQuery($"{path}.rq");
            var expected = GetExpected($"{path}.srx");
            var result = storage.Query(query);

            switch (verificationType)
            {
                case QueryVerificationType.BagEqual:
                    AssertBagEqual(expected, result);
                    break;
                case QueryVerificationType.Equal:
                    AssertEqual(expected, result);
                    break;
                case QueryVerificationType.None:
                    Assert.NotNull(result);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(verificationType), verificationType, null);
            }

        }

        private static XDocument GetExpected(string resultFile)
        {
            if (resultFile == null)
                return null;

            var doc = XDocument.Load(SparqlTestData.GetPath(resultFile));
            return doc;
        }

        public static IEnumerable<object[]> TestData => SparqlTestData.TestData;
    }

    public enum QueryVerificationType
    {
        BagEqual,
        Equal,
        None
    }
}
