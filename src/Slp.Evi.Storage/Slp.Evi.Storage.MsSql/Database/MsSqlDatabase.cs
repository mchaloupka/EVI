using System.Data;
using System.Data.SqlClient;
using Slp.Evi.Common.Database;
using Slp.Evi.Database;
using Slp.Evi.Storage.MsSql.QueryWriter;
using Slp.Evi.Storage.MsSql.Reader;

namespace Slp.Evi.Storage.MsSql.Database
{
    public sealed class MsSqlDatabase
        : ISqlDatabase<MsSqlQuery>
    {
        private readonly MsSqlDatabaseSchema _databaseSchema;
        private readonly string _connectionString;
        private readonly int _queryTimeout;

        public MsSqlDatabase(string connectionString, int queryTimeout)
        {
            _connectionString = connectionString;
            _queryTimeout = queryTimeout;

            using var connection = GetRawConnection();
            _databaseSchema = MsSqlDatabaseSchema.CreateFromDatabase(connection);
        }

        /// <inheritdoc />
        public ISqlResultReader ExecuteQuery(MsSqlQuery query)
        {
            var sqlConnection = GetRawConnection();
            var command = new SqlCommand()
            {
                CommandText = query.QueryString,
                CommandType = CommandType.Text,
                Connection = sqlConnection,
                CommandTimeout = _queryTimeout
            };

            sqlConnection.Open();

            try
            {
                var reader = command.ExecuteReader();
                return new MsSqlReader(reader, sqlConnection);
            }
            catch
            {
                try
                {
                    sqlConnection.Close();
                    sqlConnection.Dispose();
                }
                catch
                {
                    // Exception ignored (the previously thrown exception is more important)
                }

                throw;
            }
        }

        /// <inheritdoc />
        public ISqlDatabaseSchema DatabaseSchema => _databaseSchema;

        /// <inheritdoc />
        public ISqlDatabaseWriter<MsSqlQuery> Writer => new MsSqlQueryWriter(_databaseSchema);

        private SqlConnection GetRawConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
