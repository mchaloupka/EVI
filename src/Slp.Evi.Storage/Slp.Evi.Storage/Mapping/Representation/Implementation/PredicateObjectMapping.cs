using System.Collections.Generic;
using System.Linq;
using TCode.r2rml4net.Mapping;

namespace Slp.Evi.Storage.Mapping.Representation.Implementation
{
    public class PredicateObjectMapping
        : IPredicateObjectMapping
    {
        private PredicateObjectMapping() { }

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