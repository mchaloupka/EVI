using System.Collections.Generic;
using System.Linq;
using TCode.r2rml4net.Mapping;

namespace Slp.Evi.Storage.Mapping.Representation.Implementation
{
    /// <summary>
    /// Implementation of <see cref="IPredicateObjectMapping"/>
    /// </summary>
    public class PredicateObjectMapping
        : IPredicateObjectMapping
    {
        /// <summary>
        /// Creates an instance of <see cref="PredicateObjectMapping"/>.
        /// </summary>
        private PredicateObjectMapping() { }

        /// <summary>
        /// Creates an instance of <see cref="IPredicateObjectMapping"/> from <see cref="IPredicateObjectMap"/>.
        /// </summary>
        /// <param name="predicateObjectMap"></param>
        /// <param name="tr"></param>
        /// <param name="creationContext"></param>
        /// <returns></returns>
        public static IPredicateObjectMapping Create(IPredicateObjectMap predicateObjectMap, TriplesMapping tr, RepresentationCreationContext creationContext)
        {
            var res = new PredicateObjectMapping();
            res.GraphMaps = predicateObjectMap.GraphMaps
                .Select(x => GraphMapping.Create(x, tr, creationContext)).ToArray();
            res.PredicateMaps = predicateObjectMap.PredicateMaps
                .Select(x => PredicateMapping.Create(x, tr, creationContext)).ToArray();
            res.ObjectMaps = predicateObjectMap.ObjectMaps
                .Select(x => ObjectMapping.Create(x, tr, creationContext)).ToArray();
            res.RefObjectMaps = predicateObjectMap.RefObjectMaps
                .Select(x => RefObjectMapping.Create(x, tr, creationContext)).ToArray();
            return res;
        }

        /// <inheritdoc />
        public IEnumerable<IGraphMapping> GraphMaps { get; private set; }

        /// <inheritdoc />
        public IEnumerable<IPredicateMapping> PredicateMaps { get; private set; }

        /// <inheritdoc />
        public IEnumerable<IObjectMapping> ObjectMaps { get; private set; }

        /// <inheritdoc />
        public IEnumerable<IRefObjectMapping> RefObjectMaps { get; private set; }
    }
}