using System;

namespace Slp.Evi.Storage.Mapping.Representation
{
    /// <summary>
    /// Base interface for all direct term mappings (all except <see cref="IRefObjectMapping"/>).
    /// </summary>
    public interface ITermMapping
        : IBaseMapping
    {
        /// <summary>
        /// Determines whether the mapped value is a constant
        /// </summary>
        bool IsConstantValued { get; }

        /// <summary>
        /// Determines whether the value is retrieved from a column
        /// </summary>
        bool IsColumnValued { get; }

        /// <summary>
        /// Determines whether the mapped value is templated
        /// </summary>
        bool IsTemplateValued { get; }

        /// <summary>
        /// Gets the column name (in case that the mapping <see cref="IsColumnValued"/> is <c>true</c>).
        /// </summary>
        string ColumnName { get; }

        /// <summary>
        /// Gets the template (in case that the mapping <see cref="IsTemplateValued"/> is <c>true</c>).
        /// </summary>
        string Template { get; }

        /// <summary>
        /// Gets the base IRI for value creation
        /// </summary>
        Uri BaseIri { get; }

        /// <summary>
        /// Gets the IRI (in case that the mapping <see cref="IsConstantValued"/> is <c>true</c>).
        /// </summary>
        /// <remarks>
        /// The IRI is available on this level as all mappings may contain an IRI. If it is a literal
        /// constant, it has to be a part of <see cref="IObjectMapping"/>.
        /// </remarks>
        Uri Iri { get; }
    }
}