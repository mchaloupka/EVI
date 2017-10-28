namespace Slp.Evi.Storage.Types
{
    /// <summary>
    /// Represents IRI value type
    /// </summary>
    /// <seealso cref="IValueType" />
    public class IRIValueType
        : IValueType
    {
        /// <inheritdoc />
        public TypeCategories Category => TypeCategories.IRI;
    }
}
