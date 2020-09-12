using System;
using System.Data.SqlClient;
using Slp.Evi.Common.Database;
using Slp.Evi.Database;
using Slp.Evi.Storage.MsSql.QueryWriter;

namespace Slp.Evi.Storage.MsSql.Database
{
    public sealed class MsSqlDatabase
        : ISqlDatabase<MsSqlQuery>
    {
        private readonly MsSqlDatabaseSchema _databaseSchema;
        private readonly string _connectionString;

        public MsSqlDatabase(string connectionString)
        {
            _connectionString = connectionString;

            using var connection = GetRawConnection();
            _databaseSchema = MsSqlDatabaseSchema.CreateFromDatabase(connection);
        }

        /// <inheritdoc />
        public ISqlResultReader ExecuteQuery(MsSqlQuery query)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public ISqlDatabaseSchema DatabaseSchema => _databaseSchema;

        /// <inheritdoc />
        public ISqlDatabaseWriter<MsSqlQuery> Writer => new MsSqlQueryWriter();

        private SqlConnection GetRawConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
