using System;
using Slp.Evi.Storage.Utils;

namespace Slp.Evi.Storage.Mapping.Representation
{
    /// <summary>
    /// Represents the object mapping
    /// </summary>
    public interface IObjectMapping
        : ITermMapping
    {
        /// <summary>
        /// Gets the literal (in case that the mapping <see cref="ITermMapping.IsConstantValued"/> is <c>true</c>).
        /// </summary>
        /// <remarks>
        /// The literal is available if it is a literal constant, otherwise check <see cref="ITermMapping.Iri"/>.
        /// </remarks>
        ParsedLiteralParts Literal { get; }

        /// <summary>
        /// Gets the datatype URI of the RDF term generated from this term map
        /// </summary>
        Uri DataTypeIRI { get; }

        /// <summary>
        /// Gets the language tag of the RDF term generated from this term map
        /// </summary>
        string Language { get; }
    }
}