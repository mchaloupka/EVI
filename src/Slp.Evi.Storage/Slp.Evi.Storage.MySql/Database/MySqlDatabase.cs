using System.Data;
using MySqlConnector;
using Slp.Evi.Storage.Core.Common.Database;
using Slp.Evi.Storage.Core.Database;
using Slp.Evi.Storage.MySql.QueryWriter;
using Slp.Evi.Storage.MySql.Reader;

namespace Slp.Evi.Storage.MySql.Database
{
    public sealed class MySqlDatabase
        : ISqlDatabase<MySqlQuery>
    {
        private readonly MySqlDatabaseSchema _databaseSchema;
        private readonly string _connectionString;
        private readonly int _queryTimeout;

        public MySqlDatabase(string connectionString, int queryTimeout)
        {
            _connectionString = connectionString;
            _queryTimeout = queryTimeout;

            using var connection = GetRawConnection();
            _databaseSchema = MySqlDatabaseSchema.CreateFromDatabase(connection);
        }

        /// <inheritdoc />
        public ISqlResultReader ExecuteQuery(MySqlQuery query)
        {
            var sqlConnection = GetRawConnection();
            var command = new MySqlCommand();
            command.CommandText = query.QueryString;
            command.CommandType = CommandType.Text;
            command.Connection = sqlConnection;
            command.CommandTimeout = _queryTimeout;

            sqlConnection.Open();

            try
            {
                var reader = command.ExecuteReader();
                return new MySqlReader(reader, sqlConnection);
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
        public ISqlDatabaseWriter<MySqlQuery> Writer => new MySqlQueryWriter(_databaseSchema);

        private MySqlConnection GetRawConnection()
        {
            return new MySqlConnection(_connectionString);
        }
    }
}
