using System;
using System.Data.SqlClient;
using Slp.Evi.Common.Database;
using Slp.Evi.Database;

namespace Slp.Evi.Storage.MsSql.Database
{
    public sealed class MsSqlDatabase
        : ISqlDatabase<MsSqlQuery, string>
    {
        private readonly MsSqlDatabaseSchema _databaseSchema;
        private readonly string _connectionString;

        public MsSqlDatabase(string connectionString)
        {
            _connectionString = connectionString;

            using (var connection = GetRawConnection())
            {
                _databaseSchema = MsSqlDatabaseSchema.CreateFromDatabase(connection);
            }
        }

        /// <inheritdoc />
        public ISqlResultReader<string> ExecuteQuery(MsSqlQuery query)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public ISqlDatabaseSchema DatabaseSchema => _databaseSchema;

        private SqlConnection GetRawConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
