using Slp.r2rml4net.Storage.Utils;

namespace Slp.r2rml4net.Storage.Sparql.Old.Operator
{
    /// <summary>
    /// Visitor for sparql query
    /// </summary>
    public interface ISparqlQueryVisitor : IVisitor
    {
        /// <summary>
        /// Visits the specified BGP operator.
        /// </summary>
        /// <param name="bgpOp">The BGP operator.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value.</returns>
        object Visit(BgpOp bgpOp, object data);

        /// <summary>
        /// Visits the specified join operator.
        /// </summary>
        /// <param name="joinOp">The join operator.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value.</returns>
        object Visit(JoinOp joinOp, object data);

        /// <summary>
        /// Visits the specified one empty solution operator.
        /// </summary>
        /// <param name="oneEmptySolutionOp">The one empty solution operator.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value.</returns>
        object Visit(OneEmptySolutionOp oneEmptySolutionOp, object data);

        /// <summary>
        /// Visits the specified union operator.
        /// </summary>
        /// <param name="unionOp">The union operator.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value.</returns>
        object Visit(UnionOp unionOp, object data);

        /// <summary>
        /// Visits the specified no solution operator.
        /// </summary>
        /// <param name="noSolutionOp">The no solution operator.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value.</returns>
        object Visit(NoSolutionOp noSolutionOp, object data);

        /// <summary>
        /// Visits the specified select operator.
        /// </summary>
        /// <param name="selectOp">The select operator.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value.</returns>
        object Visit(SelectOp selectOp, object data);

        /// <summary>
        /// Visits the specified slice operator.
        /// </summary>
        /// <param name="sliceOp">The slice operator.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value.</returns>
        object Visit(SliceOp sliceOp, object data);

        /// <summary>
        /// Visits the specified order by operator.
        /// </summary>
        /// <param name="orderByOp">The order by operator.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value.</returns>
        object Visit(OrderByOp orderByOp, object data);

        /// <summary>
        /// Visits the specified distinct operator.
        /// </summary>
        /// <param name="distinctOp">The distinct operator.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value.</returns>
        object Visit(DistinctOp distinctOp, object data);

        /// <summary>
        /// Visits the specified reduced operator.
        /// </summary>
        /// <param name="reducedOp">The reduced operator.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value.</returns>
        object Visit(ReducedOp reducedOp, object data);

        /// <summary>
        /// Visits the specified bind operator.
        /// </summary>
        /// <param name="bindOp">The bind operator.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value.</returns>
        object Visit(BindOp bindOp, object data);
    }
}
