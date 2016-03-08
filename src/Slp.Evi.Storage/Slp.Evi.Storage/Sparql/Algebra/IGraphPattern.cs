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
    }
}
