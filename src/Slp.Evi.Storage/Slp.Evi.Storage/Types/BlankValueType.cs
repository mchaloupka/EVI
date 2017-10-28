namespace Slp.Evi.Storage.Types
{
    /// <summary>
    /// Represents blank value type
    /// </summary>
    /// <seealso cref="IValueType" />
    public class BlankValueType
        : IValueType
    {
        /// <inheritdoc />
        public TypeCategories Category => TypeCategories.BlankNode;
    }
}
