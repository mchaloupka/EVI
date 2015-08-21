using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseSchemaReader.DataSchema;

namespace Slp.r2rml4net.Storage.Relational.Query.Conditions.Assignment
{
    /// <summary>
    /// The variable from assignment
    /// </summary>
    public class AssignedVariable
        : ICalculusVariable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssignedVariable"/> class.
        /// </summary>
        /// <param name="sqlType">SQL type of the variable.</param>
        public AssignedVariable(DataType sqlType)
        {
            SqlType = sqlType;
        }

        /// <summary>
        /// The SQL type of the expression.
        /// </summary>
        public DataType SqlType { get; }
    }
}
