using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// Initializes a new instance of the <see cref="SqlColumn"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public SqlColumn(string name)
        {
            Name = name;
        }
    }
}
