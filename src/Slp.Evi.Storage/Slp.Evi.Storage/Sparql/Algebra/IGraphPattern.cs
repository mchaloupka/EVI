using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Slp.Evi.Storage.Sparql.Algebra.Patterns;
using Slp.Evi.Storage.Utils;

namespace Slp.Evi.Storage.Sparql.Algebra
{
    /// <summary>
    /// Base interface for all patterns
    /// </summary>
    public interface IGraphPattern
        : ISparqlQuery, IVisitable<IGraphPatternVisitor>
    {
        /// <summary>
        /// Gets the set of always bound variables.
        /// </summary>
        IEnumerable<string> AlwaysBoundVariables { get; }
    }
}
