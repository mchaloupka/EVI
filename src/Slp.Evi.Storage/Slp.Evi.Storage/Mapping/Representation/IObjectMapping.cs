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
    }
}