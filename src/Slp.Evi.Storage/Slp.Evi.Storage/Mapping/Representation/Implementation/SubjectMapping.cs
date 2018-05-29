using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TCode.r2rml4net.Mapping;

namespace Slp.Evi.Storage.Mapping.Representation.Implementation
{
    /// <summary>
    /// Implementation of <see cref="ISubjectMapping"/>.
    /// </summary>
    public class SubjectMapping
        : TermMapping, ISubjectMapping
    {
        /// <summary>
        /// Creates an instance of <see cref="SubjectMapping"/>.
        /// </summary>
        private SubjectMapping() { }

        /// <summary>
        /// Creates an instance of <see cref= "ISubjectMapping" /> from <see cref="ISubjectMap"/>.
        /// </summary>
        public static ISubjectMapping Create(ISubjectMap triplesMapSubjectMap, TriplesMapping parentTriplesMapping, RepresentationCreationContext creationContext)
        {
            var sm = new SubjectMapping();
            Fill(sm, triplesMapSubjectMap, parentTriplesMapping, creationContext);
            sm.GraphMaps =
                triplesMapSubjectMap.GraphMaps.Select(
                    x => GraphMapping.Create(x, parentTriplesMapping, creationContext)).ToArray();
            sm.Classes = triplesMapSubjectMap.Classes;
            return sm;
        }

        /// <inheritdoc />
        public IEnumerable<IGraphMapping> GraphMaps { get; private set; }

        /// <inheritdoc />
        public IEnumerable<Uri> Classes { get; private set; }
    }
}