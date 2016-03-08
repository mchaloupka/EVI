using Slp.Evi.Storage.Sparql.Algebra.Modifiers;
using Slp.Evi.Storage.Utils;

namespace Slp.Evi.Storage.Sparql.Algebra
{
    /// <summary>
    /// Base interface for SPARQL result modifiers
    /// </summary>
    public interface IModifier
        : ISparqlQuery, IVisitable<IModifierVisitor>
    {
    }
}
