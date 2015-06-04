using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Slp.r2rml4net.Storage.Bootstrap;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sql;
using VDS.RDF;
using VDS.RDF.Parsing;
using TCode.r2rml4net;
using Slp.r2rml4net.Storage;
using TCode.r2rml4net.Extensions;

namespace Slp.r2rml4net.Test.System.SPARQL.SPARQL_TestSuite
{
    public abstract class TestSuite
        : BaseSPARQLTestSuite
    {
        private const string TestDataNs = "http://example.org/ns#";

        public TestContext TestContext { get; set; }

        [TestMethod]
        public void simple_rdf01()
        {
            var testName = MethodBase.GetCurrentMethod().Name;
            var dataFile = @"Data\Simple\rdf01.ttl";
            var queryFile = @"Data\Simple\rdf01.rq";
            var resultFile = @"Data\Simple\rdf01.srx";

            TestContext.WriteLine("Start: {0}", DateTime.Now.ToLongTimeString());

            var sqlDb = GetSqlDb();

            TestContext.WriteLine("Create table: {0}", DateTime.Now.ToLongTimeString());

            CreateTable(sqlDb, testName);

            TestContext.WriteLine("Load data file: {0}", DateTime.Now.ToLongTimeString());

            var mapping = LoadDataFile(dataFile, sqlDb, testName);

            TestContext.WriteLine("Create storage: {0}", DateTime.Now.ToLongTimeString());

            var storage = new R2RmlStorage(sqlDb, mapping, GetStorageFactory());

            TestContext.WriteLine("Create query: {0}", DateTime.Now.ToLongTimeString());

            var query = GetQuery(queryFile);

            TestContext.WriteLine("Execute query: {0}", DateTime.Now.ToLongTimeString());

            var result = storage.Query(query);

            TestContext.WriteLine("Get expected result: {0}", DateTime.Now.ToLongTimeString());

            var expected = GetExpected(resultFile);

            TestContext.WriteLine("Compare result: {0}", DateTime.Now.ToLongTimeString());

            AssertBagEqual(expected, result);

            TestContext.WriteLine("End: {0}", DateTime.Now.ToLongTimeString());
        }

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

        protected abstract IR2RmlStorageFactory GetStorageFactory();

        protected abstract ISqlDb GetSqlDb();

        private static void CreateTable(ISqlDb sqlDb, string testName)
        {
            sqlDb.ExecuteQuery(string.Format("IF OBJECT_ID(\'{0}\', 'U') IS NOT NULL DROP TABLE {0}", testName));
            sqlDb.ExecuteQuery(
                string.Format(
                    "CREATE TABLE {0} (subject nvarchar(max) NULL, predicate nvarchar(max) NULL, uri_object nvarchar(max) NULL)"
                    , testName));
        }

        private static IR2RML LoadDataFile(string dataFile, ISqlDb sqlDb, string testName)
        {
            var filePath = GetPath(dataFile);

            TurtleParser ttlparser = new TurtleParser();
            IGraph g = new Graph();

            ttlparser.Load(g, filePath);

            foreach (var triple in g.Triples)
            {
                InsertTripleToDb(sqlDb, testName, triple);
            }

            var mapping = new FluentR2RML();
            var triplesMap = mapping.CreateTriplesMapFromTable(testName);
            triplesMap.SubjectMap.IsTemplateValued("http://example.org/ns#{subject}");
            var poMap = triplesMap.CreatePropertyObjectMap();
            poMap.CreatePredicateMap().IsTemplateValued("http://example.org/ns#{predicate}");
            poMap.CreateObjectMap().IsTemplateValued("http://example.org/ns#{uri_object}");
            return mapping;
        }

        private static void InsertTripleToDb(ISqlDb sqlDb, string tableName, Triple triple)
        {
            var subjectData = triple.Subject.ToString().Replace(TestDataNs, "");
            var predicateData = triple.Predicate.ToString().Replace(TestDataNs, "");

            if (triple.Object is IUriNode)
            {
                var objectData = triple.Object.ToString().Replace(TestDataNs, "");

                sqlDb.ExecuteQuery(string.Format(
                    "INSERT INTO {0} VALUES (\'{1}\', \'{2}\', \'{3}\')",
                    tableName, subjectData, predicateData, objectData
                    ));
            }
            else
            {
                throw  new NotImplementedException();
            }
        }

        private static string GetPath(string dataFile)
        {
            var path = string.Format(".\\SPARQL\\SPARQL_TestSuite\\{0}", dataFile);
            return path;
        }
    }
}
