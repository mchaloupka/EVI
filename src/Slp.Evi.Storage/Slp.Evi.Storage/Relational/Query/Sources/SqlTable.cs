using System;
using System.Collections.Generic;
using System.Diagnostics;
using DatabaseSchemaReader.DataSchema;

namespace Slp.Evi.Storage.Relational.Query.Sources
{
    /// <summary>
    /// SQL table representation
    /// </summary>
    public class SqlTable 
        : ISqlCalculusSource
    {
        /// <summary>
        /// Gets the provided variables.
        /// </summary>
        /// <value>The variables.</value>
        public IEnumerable<ICalculusVariable> Variables { get { return _variables.Values; } }

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
            _variables = new Dictionary<string, ICalculusVariable>();

            foreach (var dbColumn in _tableInfo.Columns)
            {
                _variables.Add(dbColumn.Name, new SqlColumn(dbColumn.Name, this, dbColumn.DataType));
            }
        }

        /// <summary>
        /// The variables
        /// </summary>
        private readonly Dictionary<string, ICalculusVariable> _variables;

        /// <summary>
        /// Gets the variable.
        /// </summary>
        /// <param name="name">The SQL name.</param>
        /// <returns>ICalculusVariable.</returns>
        public ICalculusVariable GetVariable(string name)
        {
            if (!_variables.ContainsKey(name))
            {
                throw new ArgumentException("Unknown column name", nameof(name));
            }

            return _variables[name];
        }

        /// <summary>
        /// Accepts the specified visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value from visitor.</returns>
        [DebuggerStepThrough]
        public object Accept(ICalculusSourceVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }
    }
}
