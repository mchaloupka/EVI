using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseSchemaReader.DataSchema;

namespace Slp.r2rml4net.Storage.Relational.Query.Source
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
        public string Name { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlColumn" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="sqlType">The SQL type of the column.</param>
        public SqlColumn(string name, DataType sqlType)
        {
            Name = name;
            SqlType = sqlType;
        }

        /// <summary>
        /// The SQL type of the expression.
        /// </summary>
        public DataType SqlType { get; private set; }
    }
}
