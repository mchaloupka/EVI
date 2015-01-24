using System.Linq;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sql.Algebra;
using Slp.r2rml4net.Storage.Sql.Algebra.Operator;
using Slp.r2rml4net.Storage.Sql.Algebra.Source;
using Slp.r2rml4net.Storage.Sql.Algebra.Utils;

namespace Slp.r2rml4net.Storage.Optimization.SqlAlgebra
{
    /// <summary>
    /// REDUCED optimization
    /// </summary>
    public class ReducedSelectOptimization : ISqlAlgebraOptimizer, ISqlSourceVisitor
    {
        /// <summary>
        /// Processes the algebra.
        /// </summary>
        /// <param name="algebra">The algebra.</param>
        /// <param name="context">The query context.</param>
        /// <returns>The processed algebra.</returns>
        public INotSqlOriginalDbSource ProcessAlgebra(INotSqlOriginalDbSource algebra, QueryContext context)
        {
            var vData = new VisitData(context, false);
            algebra.Accept(this, vData);
            return algebra;
        }

        /// <summary>
        /// Visits the specified no row source.
        /// </summary>
        /// <param name="noRowSource">The no row source.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        public object Visit(NoRowSource noRowSource, object data)
        {
            return null;
        }

        /// <summary>
        /// Visits the specified single empty row source.
        /// </summary>
        /// <param name="singleEmptyRowSource">The single empty row source.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        public object Visit(SingleEmptyRowSource singleEmptyRowSource, object data)
        {
            return null;
        }

        /// <summary>
        /// Visits the specified SQL select operator.
        /// </summary>
        /// <param name="sqlSelectOp">The SQL select operator.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        public object Visit(SqlSelectOp sqlSelectOp, object data)
        {
            var vData = (VisitData)data;
            bool isReduced = false;

            if(sqlSelectOp.IsReduced || sqlSelectOp.IsDistinct)
            {
                vData = vData.SetReduced(true);
                isReduced = true;
            }
            else if (vData.IsParentReduced)
            {
                sqlSelectOp.IsReduced = true;
                isReduced = true;
            }

            sqlSelectOp.OriginalSource.Accept(this, vData);

            foreach (var source in sqlSelectOp.JoinSources.Select(x => x.Source))
            {
                source.Accept(this, data);
            }

            foreach (var source in sqlSelectOp.LeftOuterJoinSources.Select(x => x.Source))
            {
                source.Accept(this, data);
            }

            if (isReduced)
            {
                var constantResult = !sqlSelectOp.Columns.OfType<SqlSelectColumn>().Any();

                if (sqlSelectOp.Columns.OfType<SqlExpressionColumn>().SelectMany(x => x.Expression.GetAllReferencedColumns()).Any())
                    constantResult = false;

                if (constantResult)
                {
                    if (!sqlSelectOp.Limit.HasValue || sqlSelectOp.Limit.Value > 1)
                    { 
                        sqlSelectOp.Limit = 1;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Visits the specified SQL union operator.
        /// </summary>
        /// <param name="sqlUnionOp">The SQL union operator.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        public object Visit(SqlUnionOp sqlUnionOp, object data)
        {
            var vData = (VisitData)data;

            if (sqlUnionOp.IsReduced)
            {
                vData.SetReduced(true);
            }
            else if (vData.IsParentReduced)
            {
                sqlUnionOp.IsReduced = true;
            }

            foreach (var source in sqlUnionOp.Sources)
            {
                source.Accept(this, data);
            }

            return null;
        }

        /// <summary>
        /// Visits the specified SQL statement.
        /// </summary>
        /// <param name="sqlStatement">The SQL statement.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        public object Visit(SqlStatement sqlStatement, object data)
        {
            return null;
        }

        /// <summary>
        /// Visits the specified SQL table.
        /// </summary>
        /// <param name="sqlTable">The SQL table.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        public object Visit(SqlTable sqlTable, object data)
        {
            return null;
        }

        /// <summary>
        /// Visit data for the visitor
        /// </summary>
        private class VisitData
        {
            /// <summary>
            /// Gets or sets the context.
            /// </summary>
            /// <value>The context.</value>
            public QueryContext Context { get; private set; }

            /// <summary>
            /// Gets or sets a value indicating whether this instance is parent reduced.
            /// </summary>
            /// <value><c>true</c> if this instance is parent reduced; otherwise, <c>false</c>.</value>
            public bool IsParentReduced { get; private set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="VisitData"/> class.
            /// </summary>
            /// <param name="context">The context.</param>
            /// <param name="isParentReduced">if set to <c>true</c> [is parent reduced].</param>
            public VisitData(QueryContext context, bool isParentReduced)
            {
                Context = context;
                IsParentReduced = isParentReduced;
            }

            /// <summary>
            /// Clones this instance.
            /// </summary>
            /// <returns>VisitData.</returns>
            public VisitData Clone()
            {
                var vd = new VisitData(Context, IsParentReduced);

                return vd;
            }

            /// <summary>
            /// Sets the reduced.
            /// </summary>
            /// <param name="isReduced">if set to <c>true</c> [is reduced].</param>
            /// <returns>VisitData.</returns>
            public VisitData SetReduced(bool isReduced)
            {
                var vd = Clone();
                vd.IsParentReduced = isReduced;
                return vd;
            }
        }
    }
}
