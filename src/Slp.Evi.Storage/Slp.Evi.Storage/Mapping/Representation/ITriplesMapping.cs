using System;
using System.Collections.Generic;

namespace Slp.Evi.Storage.Mapping.Representation
{
    /// <summary>
    /// Represents a mapping of a set of triples with shared <see cref="ISubjectMapping"/> and data source.
    /// </summary>
    public interface ITriplesMapping
    {
        /// <summary>
        /// The subject mapping
        /// </summary>
        ISubjectMapping SubjectMap { get; }

        /// <summary>
        /// The set of predicate object mappings
        /// </summary>
        IEnumerable<IPredicateObjectMapping> PredicateObjectMaps { get; }

        /// <summary>
        /// The table name
        /// </summary>
        /// <remarks>
        /// Either <see cref="TableName"/> or <see cref="SqlStatement"/> should be set.
        /// </remarks>
        string TableName { get; }

        /// <summary>
        /// The sql statement to retrieve data
        /// </summary>
        /// <remarks>
        /// Either <see cref="TableName"/> or <see cref="SqlStatement"/> should be set.
        /// </remarks>
        string SqlStatement { get; }

        /// <summary>
        /// The base IRI for this triple map
        /// </summary>
        Uri BaseIri { get; }
    }
}
