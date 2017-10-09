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
        /// <inheritdoc />
        public TypeCategories Category => TypeCategories.BlankNode;
    }
}
