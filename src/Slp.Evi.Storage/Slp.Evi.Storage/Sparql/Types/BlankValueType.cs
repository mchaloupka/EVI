using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.Evi.Storage.Sparql.Types
{
    /// <summary>
    /// Represents blank value type
    /// </summary>
    /// <seealso cref="Slp.Evi.Storage.Sparql.Types.IValueType" />
    public class BlankValueType
        : IValueType
    {
        /// <summary>
        /// Gets a value indicating whether this instance is IRI.
        /// </summary>
        /// <value><c>true</c> if this instance is IRI; otherwise, <c>false</c>.</value>
        public bool IsIRI => false;

        /// <summary>
        /// Gets a value indicating whether this instance is blank.
        /// </summary>
        /// <value><c>true</c> if this instance is blank; otherwise, <c>false</c>.</value>
        public bool IsBlank => true;

        /// <summary>
        /// Gets a value indicating whether this instance is literal.
        /// </summary>
        /// <value><c>true</c> if this instance is literal; otherwise, <c>false</c>.</value>
        public bool IsLiteral => false;
    }
}
