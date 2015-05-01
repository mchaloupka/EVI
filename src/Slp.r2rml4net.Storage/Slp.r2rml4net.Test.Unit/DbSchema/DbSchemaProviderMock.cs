using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseSchemaReader.DataSchema;
using Slp.r2rml4net.Storage.DBSchema;
using Slp.r2rml4net.Storage.Sql;

namespace Slp.r2rml4net.Test.Unit.DbSchema
{
    public class DbSchemaProviderMock : IDbSchemaProvider
    {
        /// <summary>
        /// The table info cache
        /// </summary>
        private readonly Dictionary<string, DatabaseTable> _tableCache;

        public DbSchemaProviderMock()
        {
            _tableCache = new Dictionary<string, DatabaseTable>();
        }

        /// <summary>
        /// Gets the table information.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>DatabaseTable.</returns>
        /// <exception cref="System.Exception">Table not found in database schema</exception>
        public DatabaseTable GetTableInfo(string tableName)
        {

            if (_tableCache.ContainsKey(tableName))
                return _tableCache[tableName];

            throw new Exception("Table not found in database schema");
        }

        public void AddDatabaseTableInfo(string tableName, DatabaseTable tableInfo)
        {
            _tableCache.Add(tableName, tableInfo);
        }
    }
}
