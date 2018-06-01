using System;
using VDS.RDF;

namespace Slp.Evi.Storage.Mapping.Representation.Implementation
{
    /// <summary>
    /// Predicate map for class
    /// </summary>
    public class ClassPredicateMap
        : PredicateMapping
    {
        private const string ClassIri = "http://www.w3.org/1999/02/22-rdf-syntax-ns#type";

        /// <summary>
        /// Initializes a new instance of the <see cref="ClassPredicateMap"/> class.
        /// </summary>
        public ClassPredicateMap(Uri baseUri, ITriplesMapping parentTriplesMapping)
        {
            TriplesMap = parentTriplesMapping;
            TermType = TermTypeInformation.CreateIriTermType();
            IsConstantValued = true;
            Iri = UriFactory.Create(ClassIri);
            BaseIri = baseUri;
        }
    }
}