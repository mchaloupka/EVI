using Slp.Evi.Storage.Utils;

namespace Slp.Evi.Storage.Relational.Query.Sources
{
    /// <summary>
    /// Visitor for <see cref="ICalculusSource"/>
    /// </summary>
    public interface ICalculusSourceVisitor : IVisitor
    {
        /// <summary>
        /// Visits <see cref="CalculusModel"/>
        /// </summary>
        /// <param name="calculusModel">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        object Visit(CalculusModel calculusModel, object data);

        /// <summary>
        /// Visits <see cref="SqlTable"/>
        /// </summary>
        /// <param name="sqlTable">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        object Visit(SqlTable sqlTable, object data);

        /// <summary>
        /// Visits <see cref="ModifiedCalculusModel"/>
        /// </summary>
        /// <param name="calculusModel">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        object Visit(ModifiedCalculusModel calculusModel, object data);
    }
}
