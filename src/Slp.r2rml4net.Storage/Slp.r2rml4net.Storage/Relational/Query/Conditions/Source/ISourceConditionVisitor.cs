using Slp.r2rml4net.Storage.Utils;

namespace Slp.r2rml4net.Storage.Relational.Query.Conditions.Source
{
    /// <summary>
    /// Visitor for source conditions
    /// </summary>
    public interface ISourceConditionVisitor
        : IVisitor
    {
        /// <summary>
        /// Visits <see cref="TupleFromSourceCondition"/>
        /// </summary>
        /// <param name="tupleFromSourceCondition">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        object Visit(TupleFromSourceCondition tupleFromSourceCondition, object data);

        /// <summary>
        /// Visits <see cref="UnionedSourcesCondition"/>
        /// </summary>
        /// <param name="unionedSourcesCondition">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        object Visit(UnionedSourcesCondition unionedSourcesCondition, object data);
    }
}
