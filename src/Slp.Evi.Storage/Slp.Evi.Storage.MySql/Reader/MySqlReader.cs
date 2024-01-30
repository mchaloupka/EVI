using MySqlConnector;
using Slp.Evi.Storage.Core.Database;

namespace Slp.Evi.Storage.MySql.Reader
{
    public sealed class MySqlReader
        : ISqlResultReader
    {
        private readonly MySqlDataReader _reader;
        private readonly MySqlConnection _sqlConnection;
        private MySqlReaderRow _currentRow;

        public MySqlReader(MySqlDataReader reader, MySqlConnection sqlConnection)
        {
            _reader = reader;
            _sqlConnection = sqlConnection;

            if (reader.HasRows)
            {
                FetchRow();
            }
            else
            {
                _currentRow = null;
            }
        }

        private void FetchRow()
        {
            _currentRow = _reader.Read() ? MySqlReaderRow.Create(_reader) : null;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _sqlConnection.Close();
            _sqlConnection.Dispose();
        }

        /// <inheritdoc />
        public ISqlResultRow ReadRow()
        {
            var row = _currentRow;
            FetchRow();
            return row;
        }

        /// <inheritdoc />
        public bool HasNextRow => _currentRow != null;
    }
}
