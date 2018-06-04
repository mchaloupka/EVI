using System.Collections.Generic;

namespace Slp.Evi.Storage.Mapping.Representation
{
    /// <summary>
    /// Represents a ref-object mapping
    /// </summary>
    public interface IRefObjectMapping
        : IBaseMapping
    {
        /// <summary>
        /// The join conditions with the target
        /// </summary>
        IEnumerable<IJoinCondition> JoinConditions { get; }

        /// <summary>
        /// The target subject map
        /// </summary>
        ISubjectMapping TargetSubjectMap { get; }
    }
}