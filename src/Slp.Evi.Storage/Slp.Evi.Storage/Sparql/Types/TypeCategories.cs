namespace Slp.Evi.Storage.Sparql.Types
{
    /// <summary>
    /// Type categories for <see cref="IValueType"/>
    /// </summary>
    public enum TypeCategories
    {
        /// <summary>
        /// Represents blank nodes
        /// </summary>
        BlankNode = 0,
        /// <summary>
        /// Represent IRI nodes
        /// </summary>
        IRI = 1,
        /// <summary>
        /// Represent simple literals (without types)
        /// </summary>
        SimpleLiteral = 2,
        /// <summary>
        /// Represent numeric literals
        /// </summary>
        NumericLiteral = 3,
        /// <summary>
        /// Represent string literals
        /// </summary>
        StringLiteral = 4,
        /// <summary>
        /// Represent boolean literal
        /// </summary>
        BooleanLiteral = 5,
        /// <summary>
        /// Represent date-time literal
        /// </summary>
        DateTimeLiteral = 6,
        /// <summary>
        /// Represent other literals
        /// </summary>
        OtherLiterals = 7
    }
}