using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseSchemaReader.DataSchema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Slp.r2rml4net.Storage.Bootstrap;
using Slp.r2rml4net.Storage.DBSchema;
using Slp.r2rml4net.Storage.Optimization;
using Slp.r2rml4net.Storage.Optimization.SqlAlgebra;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sql;
using Slp.r2rml4net.Storage.Sql.Algebra;
using Slp.r2rml4net.Storage.Sql.Algebra.Source;
using Slp.r2rml4net.Test.Unit.DbSchema;
using Slp.r2rml4net.Test.Unit.Optimization.SqlAlgebra.Utils;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace Slp.r2rml4net.Test.Unit.Optimization.SqlAlgebra
{
    public class BaseConditionTest
    {
        protected void AssertConditionsEqual(ICondition expected, ICondition actual)
        {
            var equalityAssert = new SqlAlgebraEqualityChecker();
            var checkResult = expected.Accept(equalityAssert, actual);
            Assert.IsTrue((bool)checkResult, "The conditions are not equal");
        }

        protected BaseConditionOptimizer.VisitData GenerateInitialVisitData(QueryContext queryContext)
        {
            return new BaseConditionOptimizer.VisitData { IsOnTheFly = true, SecondRun = false, Context = queryContext };
        }

        protected ISqlDb GetDb()
        {
            return (new DefaultSqlDbFactory()).CreateSqlDb("");
        }

        protected SparqlQuery GenerateSparqlQuery()
        {
            SparqlQueryParser parser = new SparqlQueryParser();
            return parser.ParseFromString("SELECT * WHERE { }");
        }

        protected const string DummyTableName = "dummy";
        
        protected IDbSchemaProvider GetSchemaProvider()
        {
            var schemaProvider = new DbSchemaProviderMock();
            
            var tableInfo = new DatabaseTable();
            tableInfo.AddColumn("col1", DbType.String);
            tableInfo.AddColumn("col2", DbType.String);

            schemaProvider.AddDatabaseTableInfo(DummyTableName, tableInfo);
            return schemaProvider;
        }

        protected QueryContext GenerateQueryContext()
        {
            return new QueryContext(GenerateSparqlQuery(), null, GetDb(), GetSchemaProvider(), null, new ISparqlAlgebraOptimizerOnTheFly[0], new ISqlAlgebraOptimizerOnTheFly[0]);
        }

        protected SqlTable GetDummyTable(QueryContext queryContext)
        {
            var dummyTable = new SqlTable(DummyTableName, queryContext.SchemaProvider.GetTableInfo(DummyTableName), queryContext.Db);
            return dummyTable;
        }
    }
}
