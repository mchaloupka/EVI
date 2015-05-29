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

namespace Slp.r2rml4net.Test.System.SPARQL.SPARQL_TestSuite
{
    public abstract class TestSuite
        : BaseSPARQLTestSuite
    {
        [TestMethod]
        public void entailment_rdf01()
        {
            var testName = MethodBase.GetCurrentMethod().Name;
            var dataFile = @"Data\entailment\rdf01.ttl";
            var queryFile = @"Data\entailment\rdf01.rq";
            var resultFile = @"Data\entailment\rdf01.srx";

            var sqlDb = GetSqlDb();
            CreateTable(sqlDb, testName);
            var mapping = LoadDataFile(dataFile, sqlDb, testName);

            var storage = new R2RmlStorage(sqlDb, mapping, GetStorageFactory());
            var query = GetQuery(queryFile);

            var result = storage.Query(query);

            var expected = GetExpected(resultFile);

            AssertBagEqual(expected, result);
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
                    "CREATE TABLE {0} (subject nvarchar(max) NULL, predicate nvarchar(max) NULL, object nvarchar(max) NULL)"
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
            triplesMap.SubjectMap.IsTemplateValued("{subject}");
            var poMap = triplesMap.CreatePropertyObjectMap();
            poMap.CreatePredicateMap().IsTemplateValued("{predicate}");
            poMap.CreateObjectMap().IsColumnValued("object");
            return mapping;
        }

        private static void InsertTripleToDb(ISqlDb sqlDb, string tableName, Triple triple)
        {
            sqlDb.ExecuteQuery(string.Format(
                "INSERT INTO {0} VALUES (\'{1}\', \'{2}\', \'{3}\')",
                tableName, triple.Subject, triple.Predicate.ToString(), triple.Object.ToString()
                ));
        }

        private static string GetPath(string dataFile)
        {
            var path = string.Format(".\\SPARQL\\SPARQL_TestSuite\\{0}", dataFile);
            return path;
        }
    }
}
