using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.Extensions.Logging;
using Slp.Evi.Storage;
using Slp.Evi.Storage.Bootstrap;
using Slp.Evi.Storage.Database;
using Slp.Evi.Storage.Database.Vendor.MsSql;
using Xunit;

namespace Slp.Evi.Test.System.SPARQL.SPARQL_TestSuite.Db
{
    public sealed class MsSqlDbFixture
        : BaseSparqlFixture
    {
        protected override ISqlDatabase GetSqlDb()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["mssql_connection"].ConnectionString;
            return (new MsSqlDbFactory()).CreateSqlDb(connectionString);
        }
    }

    public class MsSqlDb
        : TestSuite, IClassFixture<MsSqlDbFixture>
    {
        public MsSqlDb(MsSqlDbFixture fixture)
            : base(fixture)
        {
        }
    }
}
