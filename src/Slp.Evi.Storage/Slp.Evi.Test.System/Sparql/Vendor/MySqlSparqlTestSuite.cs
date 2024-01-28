using System.Linq;
using Slp.Evi.Storage.MsSql.Database;
using Slp.Evi.Storage.MySql;
using Xunit;

namespace Slp.Evi.Test.System.Sparql.Vendor
{
    //public sealed class MySqlSparqlFixture
    //    : SparqlFixture<MySqlEviStorage>
    //{
    //    public MySqlSparqlFixture()
    //    {
    //        // Prepare all databases beforehand
    //        var datasetsToBootup =
    //            SparqlTestSuite<MySqlEviStorage>.TestData
    //                .Select(x => x[0])
    //                .Cast<string>()
    //                .Distinct();

    //        foreach (var dataset in datasetsToBootup)
    //        {
    //            GetStorage(dataset);
    //        }
    //    }

    //    private MsSqlDatabase GetSqlDb() =>
    //        new MsSqlDatabase(GetConnectionString("mssql"), 30);

    //    /// <inheritdoc />
    //    protected override MySqlEviStorage CreateStorage(string storageName) =>
    //        SparqlTestHelpers.InitializeDataset(storageName, GetSqlDb);
    //}

    //public class MySqlSparqlTestSuite(MySqlSparqlFixture fixture)
    //    : SparqlTestSuite<MySqlEviStorage>(fixture), IClassFixture<MySqlSparqlFixture>;
}
