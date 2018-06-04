using System;
using Slp.Evi.Storage.Mapping.Representation;

namespace Slp.Evi.Storage.Types
{
    /// <summary>
    /// The type cache.
    /// </summary>
    public interface ITypeCache
    {
        /// <summary>
        /// Gets the index.
        /// </summary>
        int GetIndex(IValueType valueType);

        /// <summary>
        /// Gets the <see cref="IValueType"/> with <paramref name="index"/>.
        /// </summary>
        IValueType GetValueType(int index);

        /// <summary>
        /// Gets the <see cref="IValueType"/> for <paramref name="termMap"/>
        /// </summary>
        IValueType GetValueType(IBaseMapping termMap);

        /// <summary>
        /// Gets the <see cref="IValueType"/> for IRI
        /// </summary>
        IValueType IRIValueType { get; }

        /// <summary>
        /// Gets the <see cref="IValueType"/> for simple literal
        /// </summary>
        IValueType SimpleLiteralValueType { get; }

        /// <summary>
        /// Gets the <see cref="IValueType"/> for literal with language
        /// </summary>
        /// <param name="language">The language.</param>
        IValueType GetValueTypeForLanguage(string language);

        /// <summary>
        /// Gets the <see cref="IValueType"/> for literal with datatype.
        /// </summary>
        /// <param name="dataTypeUri">The data type absolute URI.</param>
        /// <returns>IValueType.</returns>
        IValueType GetValueTypeForDataType(Uri dataTypeUri);
    }
}