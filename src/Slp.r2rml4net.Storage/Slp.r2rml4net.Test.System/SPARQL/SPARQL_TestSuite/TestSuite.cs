using System.IO;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Slp.r2rml4net.Storage;

namespace Slp.r2rml4net.Test.System.SPARQL.SPARQL_TestSuite
{
    public abstract class TestSuite
        : BaseSPARQLTestSuite
    {
        protected static readonly string[] StorageNames = { "simple.xml", "students.xml" };

        [TestMethod]
        public void simple_single()
        {
            var storage = GetStorage("simple.xml");
            var queryFile = @"Data\Simple\single.rq";
            var resultFile = @"Data\Simple\single.srx";
            var query = GetQuery(queryFile);
            var result = storage.Query(query);
            var expected = GetExpected(resultFile);
            AssertBagEqual(expected, result);
        }

        [TestMethod]
        public void simple_join()
        {
            var storage = GetStorage("simple.xml");
            var queryFile = @"Data\Simple\join.rq";
            var resultFile = @"Data\Simple\join.srx";
            var query = GetQuery(queryFile);
            var result = storage.Query(query);
            var expected = GetExpected(resultFile);
            AssertBagEqual(expected, result);
        }

        [TestMethod]
        public void simple_union()
        {
            var storage = GetStorage("simple.xml");
            var queryFile = @"Data\Simple\union.rq";
            var resultFile = @"Data\Simple\union.srx";
            var query = GetQuery(queryFile);
            var result = storage.Query(query);
            var expected = GetExpected(resultFile);
            AssertBagEqual(expected, result);
        }

        [TestMethod]
        public void simple_empty()
        {
            var storage = GetStorage("simple.xml");
            var queryFile = @"Data\Simple\empty.rq";
            var resultFile = @"Data\Simple\empty.srx";
            var query = GetQuery(queryFile);
            var result = storage.Query(query);
            var expected = GetExpected(resultFile);
            AssertBagEqual(expected, result);
        }

        [TestMethod]
        public void simple_null()
        {
            var storage = GetStorage("simple.xml");
            var queryFile = @"Data\Simple\null.rq";
            var resultFile = @"Data\Simple\null.srx";
            var query = GetQuery(queryFile);
            var result = storage.Query(query);
            var expected = GetExpected(resultFile);
            AssertBagEqual(expected, result);
        }

        [TestMethod]
        public void students_no_result()
        {
            var storage = GetStorage("students.xml");
            var queryFile = @"Data\Simple\no_result.rq";
            var resultFile = @"Data\Simple\no_result.srx";
            var query = GetQuery(queryFile);
            var result = storage.Query(query);
            var expected = GetExpected(resultFile);
            AssertBagEqual(expected, result);
        }

        [TestMethod]
        public void students_student_names()
        {
            var storage = GetStorage("students.xml");
            var queryFile = @"Data\Students\student_names.rq";
            var resultFile = @"Data\Students\student_names.srx";
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
