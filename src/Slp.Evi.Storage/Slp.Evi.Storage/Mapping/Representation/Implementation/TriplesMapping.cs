using System.Collections.Generic;
using System.Linq;
using TCode.r2rml4net.Mapping;

namespace Slp.Evi.Storage.Mapping.Representation.Implementation
{
    /// <summary>
    /// Implementation of <see cref="ITriplesMapping"/>.
    /// </summary>
    public sealed class TriplesMapping
        : ITriplesMapping
    {
        /// <summary>
        /// Creates an instance of <see cref="TriplesMapping"/>.
        /// </summary>
        private TriplesMapping()
        { }

        /// <summary>
        /// Creates an instance of <see cref= "ITriplesMapping" /> from <see cref="ITriplesMap"/>.
        /// </summary>
        public static ITriplesMapping Create(ITriplesMap triplesMap, RepresentationCreationContext creationContext)
        {
            var tr = new TriplesMapping();
            tr.SubjectMap = SubjectMapping.Create(triplesMap.SubjectMap, tr, creationContext);
            tr.PredicateObjectMaps =
                triplesMap.PredicateObjectMaps.Select(x => PredicateObjectMapping.Create(x, tr, creationContext)).ToArray();
            tr.TableName = triplesMap.TableName;
            tr.SqlStatement = triplesMap.SqlQuery;
            return tr;
        }

        /// <inheritdoc />
        public ISubjectMapping SubjectMap { get; private set; }

        /// <inheritdoc />
        public IEnumerable<IPredicateObjectMapping> PredicateObjectMaps { get; private set; }

        /// <inheritdoc />
        public string TableName { get; private set; }

        /// <inheritdoc />
        public string SqlStatement { get; private set; }
    }
}

