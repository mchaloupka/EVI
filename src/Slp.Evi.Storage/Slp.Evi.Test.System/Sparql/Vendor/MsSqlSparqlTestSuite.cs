using Microsoft.Extensions.Configuration;
using Slp.Evi.Storage.Database;
using Slp.Evi.Storage.Database.Vendor.MsSql;
using Xunit;

namespace Slp.Evi.Test.System.Sparql.Vendor
{
    public sealed class MsSqlSparqlTestSuite
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
        : SparqlTestSuite, IClassFixture<MsSqlSparqlTestSuite>
    {
        public MsSqlDb(MsSqlSparqlTestSuite fixture)
            : base(fixture)
        {
        }
    }
}
