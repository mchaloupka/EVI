using System.Data;
using DatabaseSchemaReader.DataSchema;
using Microsoft.Extensions.Logging;
using Moq;
using Slp.Evi.Storage.Bootstrap;
using Slp.Evi.Storage.Database;
using Slp.Evi.Storage.Database.Vendor.MsSql;
using Slp.Evi.Storage.DBSchema;
using Slp.Evi.Storage.Mapping;
using Slp.Evi.Storage.Query;
using Slp.Evi.Storage.Relational.PostProcess.Optimizers;
using Slp.Evi.Storage.Relational.Query;
using Slp.Evi.Storage.Relational.Query.Sources;
using Slp.Evi.Test.Unit.Mocks;
using Slp.Evi.Test.Unit.Relational.Utilities;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using Xunit;

namespace Slp.Evi.Test.Unit.Relational.PostProcess
{
    public abstract class BaseOptimizerTest<T>
    {
        protected void AssertFilterConditionsEqual(IFilterCondition expected, IFilterCondition actual)
        {
            var equalityAssert = new SqlAlgebraEqualityChecker();
            var checkResult = expected.Accept(equalityAssert, actual);
            Assert.True((bool)checkResult, "The conditions are not equal");
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
            tableInfo.AddColumn("col3", DbType.String);
            tableInfo.AddColumn("col4", DbType.String);
            tableInfo.AddColumn("col5", DbType.String);

            schemaProvider.AddDatabaseTableInfo(DummyTableName, tableInfo);
            return schemaProvider;
        }

        protected virtual IQueryContext GenerateQueryContext()
        {
            var mappingMock = new Mock<IMappingProcessor>();
            mappingMock.Setup(x => x.GetMappingTransformer()).Returns(() => null);

            var factory = new DefaultEviQueryableStorageFactory(new LoggerFactory());
            return factory.CreateQueryContext(GenerateSparqlQuery(), mappingMock.Object, GetDb(), GetSchemaProvider(), null, null);
        }

        protected virtual SqlTable GetDummyTable(IQueryContext queryContext)
        {
            var dummyTable = new SqlTable(queryContext.SchemaProvider.GetTableInfo(DummyTableName));
            return dummyTable;
        }

        protected BaseRelationalOptimizer<T>.OptimizationContext GetContext(IQueryContext queryContext)
        {
            return new BaseRelationalOptimizer<T>.OptimizationContext()
            {
                Context = queryContext,
                Data = default(T)
            };
        }
    }
}
