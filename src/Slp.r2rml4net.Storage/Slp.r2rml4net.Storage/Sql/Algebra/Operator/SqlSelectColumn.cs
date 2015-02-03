using DatabaseSchemaReader.DataSchema;

namespace Slp.r2rml4net.Storage.Sql.Algebra.Operator
{
    /// <summary>
    /// SELECT column
    /// </summary>
    public class SqlSelectColumn : ISqlColumn
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlSelectColumn"/> class.
        /// </summary>
        /// <param name="originalColumn">The original column.</param>
        /// <param name="source">The source.</param>
        public SqlSelectColumn(ISqlColumn originalColumn, ISqlSource source)
        {
            OriginalColumn = originalColumn;
            Source = source;
            Name = null;
        }

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
        /// <value>The type of the SQL column.</value>
        public DataType SqlColumnType
        {
            get { return OriginalColumn.SqlColumnType; }
        }

        /// <summary>
        /// Gets the original column.
        /// </summary>
        /// <value>The original column.</value>
        public ISqlColumn OriginalColumn { get; private set; }
    }
}
