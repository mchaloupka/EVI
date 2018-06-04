namespace Slp.Evi.Storage.Mapping.Representation
{
    /// <summary>
    /// Base abstraction for a mapping
    /// </summary>
    public interface IBaseMapping
    {
        /// <summary>
        /// A triples mapping which contains this mapping
        /// </summary>
        ITriplesMapping TriplesMap { get; }

        /// <summary>
        /// The term type for this mapping
        /// </summary>
        ITermTypeInformation TermType { get; }
    }
}