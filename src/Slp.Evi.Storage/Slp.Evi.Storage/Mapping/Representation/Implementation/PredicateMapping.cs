using TCode.r2rml4net.Mapping;

namespace Slp.Evi.Storage.Mapping.Representation.Implementation
{
    public class PredicateMapping
        : TermMapping, IPredicateMapping
    {
        protected PredicateMapping() { }

        public static IPredicateMapping Create(IPredicateMap predicateMap, TriplesMapping tr, RepresentationCreationContext creationContext)
        {
            var res = new PredicateMapping();
            Fill(res, predicateMap, tr, creationContext);
            return res;
        }
    }
}