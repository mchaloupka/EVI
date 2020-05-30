using TCode.r2rml4net.Mapping;

namespace Slp.Evi.Storage.Mapping.Representation.Implementation
{
    /// <summary>
    /// Implementation of <see cref="IGraphMapping"/>.
    /// </summary>
    public class GraphMapping
        : TermMapping, IGraphMapping
    {
        /// <summary>
        /// Creates an instance of <see cref="GraphMapping"/>.
        /// </summary>
        private GraphMapping() { }

        /// <summary>
        /// Creates an instance of <see cref="IGraphMapping"/> for an <see cref="IGraphMap"/>.
        /// </summary>
        public static IGraphMapping Create(IGraphMap graphMap, TriplesMapping parentTriplesMapping, RepresentationCreationContext creationContext)
        {
            var res = new GraphMapping();
            Fill(res, graphMap, parentTriplesMapping, creationContext);
            return res;
        }
    }
}