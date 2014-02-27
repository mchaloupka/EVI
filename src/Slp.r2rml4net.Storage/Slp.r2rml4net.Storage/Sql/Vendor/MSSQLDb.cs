using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Sql.Vendor
{
    public class MSSQLDb : BaseSqlDb
    {
        private string connectionString;

        public MSSQLDb(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public override IQueryResultReader ExecuteQuery(string query, Query.QueryContext context)
        {
            SqlConnection sqlConnection = new SqlConnection(this.connectionString);
            SqlCommand command = new SqlCommand();
            command.CommandText = query;
            command.CommandType = System.Data.CommandType.Text;
            command.Connection = sqlConnection;

            sqlConnection.Open();

            var reader = command.ExecuteReader();
            return new DataReaderWrapper(reader, () => sqlConnection.State == System.Data.ConnectionState.Open, () => sqlConnection.Close());
        }
    }
}
