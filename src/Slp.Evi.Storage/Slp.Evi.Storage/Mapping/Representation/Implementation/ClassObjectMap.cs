using System;

namespace Slp.Evi.Storage.Mapping.Representation.Implementation
{
    /// <summary>
    /// Object map for class
    /// </summary>
    public class ClassObjectMap
        : ObjectMapping
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClassObjectMap"/> class.
        /// </summary>
        /// <param name="classUri">The class URI.</param>
        /// <param name="baseUri"></param>
        /// <param name="parentTriplesMapping"></param>
        public ClassObjectMap(Uri classUri, Uri baseUri, ITriplesMapping parentTriplesMapping)
        {
            TriplesMap = parentTriplesMapping;
            TermType = TermTypeInformation.CreateIriTermType();
            IsConstantValued = true;
            Iri = classUri;
            BaseIri = baseUri;
        }
    }
}