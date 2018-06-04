using System.Collections.Generic;

namespace Slp.Evi.Storage.Mapping.Representation
{
    /// <summary>
    /// Represents a predicate object mapping
    /// </summary>
    public interface IPredicateObjectMapping
    {
        /// <summary>
        /// The graph mappings
        /// </summary>
        IEnumerable<IGraphMapping> GraphMaps { get; }

        /// <summary>
        /// The predicate mappings
        /// </summary>
        IEnumerable<IPredicateMapping> PredicateMaps { get; }

        /// <summary>
        /// The object mappings
        /// </summary>
        IEnumerable<IObjectMapping> ObjectMaps { get; }

        /// <summary>
        /// The ref-object mappings
        /// </summary>
        IEnumerable<IRefObjectMapping> RefObjectMaps { get; }
    }
}