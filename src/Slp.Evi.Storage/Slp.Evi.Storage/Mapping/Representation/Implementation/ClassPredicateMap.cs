using System;

namespace Slp.Evi.Storage.Mapping.Representation.Implementation
{
    /// <summary>
    /// Predicate map for class
    /// </summary>
    public class ClassPredicateMap
        : IPredicateMapping
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClassPredicateMap"/> class.
        /// </summary>
        public ClassPredicateMap()
        {
            //URI = UriFactory.Create("http://www.w3.org/1999/02/22-rdf-syntax-ns#type");
            // TODO: Implement this class
        }

        /// <inheritdoc />
        public bool IsConstantValued { get; }

        /// <inheritdoc />
        public bool IsColumnValued { get; }

        /// <inheritdoc />
        public bool IsTemplateValued { get; }

        /// <inheritdoc />
        public string ColumnName { get; }

        /// <inheritdoc />
        public string Template { get; }

        /// <inheritdoc />
        public Uri BaseIri { get; }

        /// <inheritdoc />
        public Uri Iri { get; }

        /// <inheritdoc />
        public ITriplesMapping TriplesMap { get; }

        /// <inheritdoc />
        public ITermTypeInformation TermType { get; }
    }
}