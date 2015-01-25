using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DatabaseSchemaReader.DataSchema;
using Slp.r2rml4net.Storage.Sql.Algebra.Operator;

namespace Slp.r2rml4net.Storage.Sql.Algebra.Source
{
    /// <summary>
    /// SQL table
    /// </summary>
    public class SqlTable : ISqlOriginalDbSource
    {
        /// <summary>
        /// The database schema info of the table
        /// </summary>
        private readonly DatabaseTable _tableInfo;

        /// <summary>
        /// Gets the name of the table.
        /// </summary>
        /// <value>The name of the table.</value>
        public string TableName { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlTable"/> class.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="tableInfo">Database schema info of the table</param>
        public SqlTable(string tableName, DatabaseTable tableInfo)
        {
            _tableInfo = tableInfo;
            TableName = tableName;
            _columns = new List<SqlTableColumn>();
        }

        /// <summary>
        /// The columns
        /// </summary>
        private readonly List<SqlTableColumn> _columns;

        /// <summary>
        /// Gets the column.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        /// <returns>ISqlColumn.</returns>
        public ISqlColumn GetColumn(string columnName)
        {
            var col = _columns.FirstOrDefault(x => x.Name == columnName);

            if (col != null) 
                return col;

            col = new SqlTableColumn(columnName, this, GetSqlColumnType(columnName));
            _columns.Add(col);

            return col;
        }

        /// <summary>
        /// Gets the type of the SQL column.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        /// <returns>DataType.</returns>
        /// <exception cref="System.Exception">Column not present in the database</exception>
        private DataType GetSqlColumnType(string columnName)
        {
            var columnSchema = _tableInfo.Columns.FirstOrDefault(x => x.Name == columnName);

            if(columnSchema == null)
                throw new Exception("Column not present in the database");

            return columnSchema.DataType;
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }


        /// <summary>
        /// Gets the columns.
        /// </summary>
        /// <value>The columns.</value>
        public IEnumerable<ISqlColumn> Columns
        {
            get { return _columns.AsEnumerable(); }
        }

        /// <summary>
        /// Accepts the specified visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value from visitor.</returns>
        [DebuggerStepThrough]
        public object Accept(ISqlSourceVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }

        /// <summary>
        /// Removes the column.
        /// </summary>
        /// <param name="col">The col.</param>
        public void RemoveColumn(ISqlColumn col)
        {
            if (col is SqlTableColumn)
            {
                _columns.Remove((SqlTableColumn)col);
            }
        }
    }
}
