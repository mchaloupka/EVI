using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseSchemaReader.DataSchema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Slp.r2rml4net.Storage.Bootstrap;
using Slp.r2rml4net.Storage.Database;
using Slp.r2rml4net.Storage.Database.Vendor.MsSql;
using Slp.r2rml4net.Storage.DBSchema;
using Slp.r2rml4net.Storage.Mapping;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Relational.Optimization;
using Slp.r2rml4net.Storage.Relational.Optimization.Optimizers;
using Slp.r2rml4net.Storage.Relational.Query;
using Slp.r2rml4net.Storage.Relational.Query.Sources;
using Slp.r2rml4net.Test.Unit.DbSchema;
using Slp.r2rml4net.Test.Unit.Relational.Utilities;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace Slp.r2rml4net.Test.Unit.Relational.Optimization
{
    public abstract class BaseOptimizerTest<T>
    {
        protected void AssertFilterConditionsEqual(IFilterCondition expected, IFilterCondition actual)
        {
            var equalityAssert = new SqlAlgebraEqualityChecker();
            var checkResult = expected.Accept(equalityAssert, actual);
            Assert.IsTrue((bool)checkResult, "The conditions are not equal");
        }

        protected virtual ISqlDatabase GetDb()
        {
            return (new MsSqlDbFactory()).CreateSqlDb("");
        }

        protected virtual SparqlQuery GenerateSparqlQuery()
        {
            SparqlQueryParser parser = new SparqlQueryParser();
            return parser.ParseFromString("SELECT * WHERE { }");
        }

        protected const string DummyTableName = "dummy";

        protected virtual IDbSchemaProvider GetSchemaProvider()
        {
            var schemaProvider = new DbSchemaProviderMock();

            var tableInfo = new DatabaseTable();
            tableInfo.AddColumn("col1", DbType.String);
            tableInfo.AddColumn("col2", DbType.String);

            schemaProvider.AddDatabaseTableInfo(DummyTableName, tableInfo);
            return schemaProvider;
        }

        protected virtual QueryContext GenerateQueryContext()
        {
            var factory = new R2RMLDefaultStorageFactory();
            return factory.CreateQueryContext(GenerateSparqlQuery(), null, GetDb(), GetSchemaProvider(), null);
        }

        protected virtual SqlTable GetDummyTable(QueryContext queryContext)
        {
            var dummyTable = new SqlTable(queryContext.SchemaProvider.GetTableInfo(DummyTableName));
            return dummyTable;
        }

        protected BaseRelationalOptimizer<T>.OptimizationContext GetContext(QueryContext queryContext)
        {
            return new BaseRelationalOptimizer<T>.OptimizationContext()
            {
                Context = queryContext,
                Data = default(T)
            };
        }
    }
}
