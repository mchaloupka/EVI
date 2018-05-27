using System.Linq;
using Microsoft.Extensions.Configuration;
using Slp.Evi.Storage.Database;
using Slp.Evi.Storage.Database.Vendor.MsSql;
using Xunit;

namespace Slp.Evi.Test.System.Sparql.Vendor
{
    public sealed class MsSqlSparqlFixture
        : SparqlFixture
    {
        public MsSqlSparqlFixture()
        {
            // Prepare all databases beforehand
            var bootUp = SparqlTestSuite.TestData.Select(x => x[0]).Cast<string>().Distinct()
                .Select(x => base.GetStorage(x));
        }

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

    public class MsSqlSparqlTestSuite
        : SparqlTestSuite, IClassFixture<MsSqlSparqlFixture>
    {
        public MsSqlSparqlTestSuite(MsSqlSparqlFixture fixture)
            : base(fixture)
        {
        }
    }
}
