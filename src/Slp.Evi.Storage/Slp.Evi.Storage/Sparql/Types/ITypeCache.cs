using TCode.r2rml4net.Mapping;

namespace Slp.Evi.Storage.Sparql.Types
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
        IValueType GetValueType(IMapBase termMap);
    }
}