using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseSchemaReader.DataSchema;

namespace Slp.r2rml4net.Storage.Relational.Query
{
    /// <summary>
    /// Variable representation
    /// </summary>
    public interface ICalculusVariable
    {
        /// <summary>
        /// The SQL type of the expression.
        /// </summary>
        DataType SqlType { get; }
    }
}
