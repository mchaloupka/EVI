using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseSchemaReader.DataSchema;

namespace Slp.r2rml4net.Storage.Relational.Query.Source
{
    /// <summary>
    /// SQL table representation
    /// </summary>
    public class SqlTable : ICalculusSource
    {
        /// <summary>
        /// Gets the provided variables.
        /// </summary>
        /// <value>The variables.</value>
        public IEnumerable<ICalculusVariable> Variables { get; private set; }

        /// <summary>
        /// Gets the name of the table.
        /// </summary>
        /// <value>The name of the table.</value>
        public string TableName { get; private set; }

        /// <summary>
        /// The database schema information
        /// </summary>
        private readonly DatabaseTable _tableInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlTable"/> class.
        /// </summary>
        /// <param name="tableInfo">The table information.</param>
        public SqlTable(DatabaseTable tableInfo)
        {
            _tableInfo = tableInfo;
            TableName = tableInfo.Name;
        }
    }
}
