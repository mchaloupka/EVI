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
    }
}