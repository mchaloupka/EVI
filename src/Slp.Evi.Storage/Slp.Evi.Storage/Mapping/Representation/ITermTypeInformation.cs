using System;

namespace Slp.Evi.Storage.Mapping.Representation
{
    /// <summary>
    /// An information about type
    /// </summary>
    public interface ITermTypeInformation
    {
        /// <summary>
        /// Gets whether the term is a blank node
        /// </summary>
        bool IsBlankNode { get; }

        /// <summary>
        /// Gets whether the term is an IRI
        /// </summary>
        bool IsIri { get; }

        /// <summary>
        /// Gets whether the term is a literal
        /// </summary>
        bool IsLiteral { get; }

        /// <summary>
        /// Gets the data-type IRI (if it is a literal)
        /// </summary>
        Uri DataTypeIri { get; }

        /// <summary>
        /// Gets the language (it it is a literal)
        /// </summary>
        string Language { get; }
    }
}