using DatabaseSchemaReader.DataSchema;

namespace Slp.Evi.Storage.Relational.Query.Sources
{
    /// <summary>
    /// The SQL column
    /// </summary>
    public class SqlColumn 
        : ICalculusVariable
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; }

        /// <summary>
        /// Gets the table.
        /// </summary>
        /// <value>The table.</value>
        public SqlTable Table { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlColumn" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="table">The SQL table</param>
        /// <param name="sqlType">The SQL type of the column.</param>
        public SqlColumn(string name, SqlTable table, DataType sqlType)
        {
            Name = name;
            Table = table;
            SqlType = sqlType;
        }

        /// <summary>
        /// The SQL type of the expression.
        /// </summary>
        public DataType SqlType { get; }
    }
}
