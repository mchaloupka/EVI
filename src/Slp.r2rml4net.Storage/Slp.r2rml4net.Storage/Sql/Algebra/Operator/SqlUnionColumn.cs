using System.Collections.Generic;
using DatabaseSchemaReader.DataSchema;

namespace Slp.r2rml4net.Storage.Sql.Algebra.Operator
{
    /// <summary>
    /// UNION column.
    /// </summary>
    public class SqlUnionColumn : ISqlColumn
    {
        /// <summary>
        /// The original columns
        /// </summary>
        private readonly List<ISqlColumn> _originalColumns;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlUnionColumn"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="sqlDataType">The sql type of the column</param>
        public SqlUnionColumn(ISqlSource source, DataType sqlDataType)
        {
            Source = source;
            _originalColumns = new List<ISqlColumn>();
            SqlColumnType = sqlDataType;
        }

        /// <summary>
        /// Adds the column.
        /// </summary>
        /// <param name="column">The column.</param>
        public void AddColumn(ISqlColumn column)
        {
            _originalColumns.Add(column);
        }

        /// <summary>
        /// Gets the original columns.
        /// </summary>
        /// <value>The original columns.</value>
        public IEnumerable<ISqlColumn> OriginalColumns { get { return _originalColumns; } }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets the source.
        /// </summary>
        /// <value>The source.</value>
        public ISqlSource Source { get; private set; }

        /// <summary>
        /// Gets the SQL type of the column.
        /// </summary>
        public DataType SqlColumnType { get; private set; }

        /// <summary>
        /// Removes the column.
        /// </summary>
        /// <param name="ccol">The column.</param>
        public void RemoveColumn(ISqlColumn ccol)
        {
            _originalColumns.Remove(ccol);
        }
    }
}
