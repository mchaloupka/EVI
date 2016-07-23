using Slp.Evi.Storage.Utils;

namespace Slp.Evi.Storage.Sparql.Algebra.Modifiers
{
    /// <summary>
    /// Visitor interface of SPARQL result modifiers
    /// </summary>
    public interface IModifierVisitor
        : IVisitor
    {
        /// <summary>
        /// Visits <see cref="SelectModifier"/>
        /// </summary>
        /// <param name="selectModifier">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        object Visit(SelectModifier selectModifier, object data);

        /// <summary>
        /// Visits <see cref="OrderByModifier"/>
        /// </summary>
        /// <param name="orderByModifier">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        object Visit(OrderByModifier orderByModifier, object data);

        /// <summary>
        /// Visits <see cref="SliceModifier"/>
        /// </summary>
        /// <param name="sliceModifier">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        object Visit(SliceModifier sliceModifier, object data);
    }
}
