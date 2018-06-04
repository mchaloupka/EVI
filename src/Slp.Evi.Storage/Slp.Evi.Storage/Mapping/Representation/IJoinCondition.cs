namespace Slp.Evi.Storage.Mapping.Representation
{
    /// <summary>
    /// Represents a join condition for <see cref="IRefObjectMapping"/>.
    /// </summary>
    public interface IJoinCondition
    {
        /// <summary>
        /// The child column
        /// </summary>
        string ChildColumn { get; }

        /// <summary>
        /// The target (the one in joined source) column
        /// </summary>
        string TargetColumn { get; }
    }
}