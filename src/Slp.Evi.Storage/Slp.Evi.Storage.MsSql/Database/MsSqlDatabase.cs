using System;
using System.Data.SqlClient;
using Slp.Evi.Common.Database;

namespace Slp.Evi.Storage.MsSql.Database
{
    public sealed class MsSqlDatabase
        : ISqlDatabase
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
        public void ExecuteQuery(string query)
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
