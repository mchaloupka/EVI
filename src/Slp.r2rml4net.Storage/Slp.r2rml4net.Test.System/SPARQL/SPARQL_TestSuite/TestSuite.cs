using System.IO;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Slp.r2rml4net.Storage;

namespace Slp.r2rml4net.Test.System.SPARQL.SPARQL_TestSuite
{
    public abstract class TestSuite
        : BaseSPARQLTestSuite
    {
        protected static readonly string[] StorageNames = new string[] { "simple.xml" };

        [TestMethod]
        public void simple_rdf01()
        {
            var storage = GetStorage("simple.xml");
            var queryFile = @"Data\Simple\rdf01.rq";
            var resultFile = @"Data\Simple\rdf01.srx";
            var query = GetQuery(queryFile);
            var result = storage.Query(query);
            var expected = GetExpected(resultFile);
            AssertBagEqual(expected, result);
        }

        protected abstract R2RMLStorage GetStorage(string storageName);

        private XDocument GetExpected(string resultFile)
        {
            var doc = XDocument.Load(GetPath(resultFile));
            return doc;
        }

        private static string GetQuery(string queryFile)
        {
            var query = string.Empty;

            using (var fsr = new FileStream(GetPath(queryFile), FileMode.Open, FileAccess.Read))
            using (var sr = new StreamReader(fsr))
            {
                query = sr.ReadToEnd();
            }

            return query;
        }
    }
}
