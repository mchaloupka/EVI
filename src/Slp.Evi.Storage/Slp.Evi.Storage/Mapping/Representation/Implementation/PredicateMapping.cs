using TCode.r2rml4net.Mapping;

namespace Slp.Evi.Storage.Mapping.Representation.Implementation
{
    /// <summary>
    /// Implementation of <see cref="IPredicateMapping"/>.
    /// </summary>
    public class PredicateMapping
        : TermMapping, IPredicateMapping
    {
        /// <summary>
        /// Creates an instance of <see cref="PredicateMapping"/>.
        /// </summary>
        protected PredicateMapping() { }

        /// <summary>
        /// Creates an instance of <see cref="IPredicateMapping"/> from <see cref="IPredicateMap"/>.
        /// </summary>
        /// <param name="predicateMap"></param>
        /// <param name="tr"></param>
        /// <param name="creationContext"></param>
        /// <returns></returns>
        public static IPredicateMapping Create(IPredicateMap predicateMap, TriplesMapping tr, RepresentationCreationContext creationContext)
        {
            var res = new PredicateMapping();
            Fill(res, predicateMap, tr, creationContext);
            return res;
        }
    }
}