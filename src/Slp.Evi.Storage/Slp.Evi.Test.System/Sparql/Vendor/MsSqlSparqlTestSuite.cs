using Microsoft.Extensions.Configuration;
using Slp.Evi.Storage.Database;
using Slp.Evi.Storage.Database.Vendor.MsSql;
using Xunit;

namespace Slp.Evi.Test.System.SPARQL.SPARQL_TestSuite.Db
{
    public sealed class MsSqlDbFixture
        : SparqlFixture
    {
        protected override ISqlDatabase GetSqlDb()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("database.json")
                .AddEnvironmentVariables();

            var config = builder.Build();
            var connectionString = config.GetConnectionString("mssql");

            return (new MsSqlDbFactory()).CreateSqlDb(connectionString);
        }
    }

    public class MsSqlDb
        : SparqlTestSuite, IClassFixture<MsSqlDbFixture>
    {
        public MsSqlDb(MsSqlDbFixture fixture)
            : base(fixture)
        {
        }
    }
}
