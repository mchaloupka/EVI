using System.Collections.Generic;
using System.Linq;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sql.Algebra;
using Slp.r2rml4net.Storage.Sql.Algebra.Operator;
using Slp.r2rml4net.Storage.Sql.Algebra.Source;
using Slp.r2rml4net.Storage.Sql.Algebra.Utils;

namespace Slp.r2rml4net.Storage.Optimization.SqlAlgebra
{
    /// <summary>
    /// Removes unused columns
    /// </summary>
    public class RemoveUnusedColumnsOptimization : ISqlAlgebraOptimizer, ISqlSourceVisitor
    {
        /// <summary>
        /// Visit data for the visitor
        /// </summary>
        private class VisitorData
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="VisitorData"/> class.
            /// </summary>
            /// <param name="neededColumns">The needed columns.</param>
            public VisitorData(IEnumerable<ISqlColumn> neededColumns)
            {
                NeededColumns = neededColumns.ToList();
            }

            /// <summary>
            /// Gets or sets the needed columns.
            /// </summary>
            /// <value>The needed columns.</value>
            public List<ISqlColumn> NeededColumns { get; private set; }
        }

        /// <summary>
        /// Processes the algebra.
        /// </summary>
        /// <param name="algebra">The algebra.</param>
        /// <param name="context">The query context.</param>
        /// <returns>The processed algebra.</returns>
        public INotSqlOriginalDbSource ProcessAlgebra(INotSqlOriginalDbSource algebra, QueryContext context)
        {
            algebra.Accept(this, new VisitorData(new List<ISqlColumn>()));
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
            var cvd = (VisitorData)data;

            var cols = sqlSelectOp.Columns.ToArray();

            var needed = new List<ISqlColumn>();
            needed.AddRange(cvd.NeededColumns);
            needed.AddRange(GetNeededColumnsOfValueVariables(sqlSelectOp));
            needed.AddRange(GetNeededColumnsOfOrderings(sqlSelectOp));

            foreach (var col in cols)
            {
                if (!needed.Contains(col))
                    sqlSelectOp.RemoveColumn(col);
            }

            var neededOrig = GetReferencedColumnsByColumns(needed);

            foreach (var cond in sqlSelectOp.Conditions)
            {
                neededOrig.AddRange(cond.GetAllReferencedColumns());
            }

            foreach (var join in sqlSelectOp.JoinSources)
            {
                neededOrig.AddRange(join.Condition.GetAllReferencedColumns());
            }

            foreach (var join in sqlSelectOp.LeftOuterJoinSources)
            {
                neededOrig.AddRange(join.Condition.GetAllReferencedColumns());
            }

            var ncvd = new VisitorData(neededOrig);

            sqlSelectOp.OriginalSource.Accept(this, ncvd);

            foreach (var join in sqlSelectOp.JoinSources)
            {
                join.Source.Accept(this, ncvd);
            }

            foreach (var join in sqlSelectOp.LeftOuterJoinSources)
            {
                join.Source.Accept(this, ncvd);
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
            var cvd = (VisitorData)data;

            var cols = sqlUnionOp.Columns.ToArray();

            var needed = new List<ISqlColumn>();
            needed.AddRange(cvd.NeededColumns);
            needed.AddRange(GetNeededColumnsOfValueVariables(sqlUnionOp));

            foreach (var col in cols)
            {
                if (!needed.Contains(col))
                    sqlUnionOp.RemoveColumn(col);
            }

            var neededOrig = GetReferencedColumnsByColumns(needed);

            foreach (var source in sqlUnionOp.Sources)
            {
                source.Accept(this, new VisitorData(neededOrig));
            }

            return null;
        }

        /// <summary>
        /// Gets the referenced columns by columns.
        /// </summary>
        /// <param name="needed">The needed.</param>
        /// <returns>List&lt;ISqlColumn&gt;.</returns>
        private static List<ISqlColumn> GetReferencedColumnsByColumns(IEnumerable<ISqlColumn> needed)
        {
            var neededOrig = new List<ISqlColumn>();
            neededOrig.AddRange(needed.OfType<SqlSelectColumn>().Select(x => x.OriginalColumn));
            neededOrig.AddRange(needed.OfType<SqlUnionColumn>().SelectMany(x => x.OriginalColumns));
            neededOrig.AddRange(needed.OfType<SqlExpressionColumn>().SelectMany(x => x.Expression.GetAllReferencedColumns()));
            return neededOrig;
        }

        /// <summary>
        /// Gets the needed columns of value variables.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>IEnumerable&lt;ISqlColumn&gt;.</returns>
        private IEnumerable<ISqlColumn> GetNeededColumnsOfValueVariables(INotSqlOriginalDbSource source)
        {
            foreach (var binder in source.ValueBinders)
            {
                foreach (var col in binder.AssignedColumns)
                {
                    yield return col;
                }
            }
        }

        /// <summary>
        /// Gets the needed columns of orderings.
        /// </summary>
        /// <param name="sqlSelectOp">The SQL select op.</param>
        /// <returns>IEnumerable&lt;ISqlColumn&gt;.</returns>
        private IEnumerable<ISqlColumn> GetNeededColumnsOfOrderings(SqlSelectOp sqlSelectOp)
        {
            foreach (var ordering in sqlSelectOp.Orderings)
            {
                foreach (var col in ordering.Expression.GetAllReferencedColumns())
                {
                    yield return col;
                }
            }
        }

        /// <summary>
        /// Visits the specified SQL statement.
        /// </summary>
        /// <param name="sqlStatement">The SQL statement.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        public object Visit(SqlStatement sqlStatement, object data)
        {
            var cvd = (VisitorData)data;

            foreach (var col in sqlStatement.Columns.ToArray())
            {
                if (!cvd.NeededColumns.Contains(col))
                    sqlStatement.RemoveColumn(col);
            }

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
            var cvd = (VisitorData)data;

            foreach (var col in sqlTable.Columns.ToArray())
            {
                if (!cvd.NeededColumns.Contains(col))
                    sqlTable.RemoveColumn(col);
            }

            return null;
        }
    }
}
