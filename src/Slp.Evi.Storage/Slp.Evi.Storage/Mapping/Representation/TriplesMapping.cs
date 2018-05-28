using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TCode.r2rml4net.Mapping;
using VDS.RDF;

namespace Slp.Evi.Storage.Mapping.Representation
{
    public class TriplesMapping
    {
        private TriplesMapping()
        { }

        public static ITriplesMapping Create(ITriplesMap triplesMap, RepresentationCreationContext creationContext)
        {
            throw new NotImplementedException();
        }
    }

    public class SubjectMapping
    {
        private SubjectMapping() { }

        public static ISubjectMapping Create(ISubjectMap subjectMap, TriplesMapping context,
            RepresentationCreationContext creationContext)
        {
            throw new NotImplementedException();
        }
    }

    public class GraphMapping
    {
        private GraphMapping() { }

        public static IGraphMapping Create(IGraphMap graphMap, TriplesMapping context, TriplesMapping triplesMapping)
        {
            throw new NotImplementedException();
        }
    }

    public class PredicateObjectMapping
    {
        private PredicateObjectMapping() { }

        public static IPredicateObjectMapping Create(IPredicateObjectMap predicateObjectMap, TriplesMapping context,
            RepresentationCreationContext creationContext)
        {
            throw new NotImplementedException();
        }
    }
}

