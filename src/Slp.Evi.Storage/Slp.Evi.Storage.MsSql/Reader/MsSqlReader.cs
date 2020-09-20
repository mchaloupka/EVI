using System.Data.SqlClient;
using Slp.Evi.Database;

namespace Slp.Evi.Storage.MsSql.Reader
{
    public sealed class MsSqlReader
        : ISqlResultReader
    {
        private readonly SqlDataReader _reader;
        private readonly SqlConnection _sqlConnection;
        private MsSqlReaderRow _currentRow;

        public MsSqlReader(SqlDataReader reader, SqlConnection sqlConnection)
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
            _currentRow = _reader.Read() ? MsSqlReaderRow.Create(_reader) : null;
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
