using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Slp.r2rml4net.Storage.Bootstrap;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sql;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace Slp.r2rml4net.Test.System.SPARQL.SPARQL_TestSuite
{
    public abstract class TestSuite
    {
        [TestMethod]
        public void entailment_rdf01()
        {
            string testName = MethodBase.GetCurrentMethod().Name;
            string dataFile = @"Data\entailment\rdf01.ttl";
            //string queryFile = @"Data\entailment\rdf01.rq";
            //string resultFile = @"Data\entailment\rdf01.srx";

            var sqlDb = GetSqlDb();
            CreateTable(sqlDb, testName);
            LoadDataFile(dataFile, sqlDb, testName);
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

        private static void LoadDataFile(string dataFile, ISqlDb sqlDb, string testName)
        {
            var filePath = GetPath(dataFile);

            TurtleParser ttlparser = new TurtleParser();
            IGraph g = new Graph();

            ttlparser.Load(g, filePath);

            foreach (var triple in g.Triples)
            {
                InsertTripleToDb(sqlDb, testName, triple);
            }

            // TODO: Change result type to R2RML and generate the mapping
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
