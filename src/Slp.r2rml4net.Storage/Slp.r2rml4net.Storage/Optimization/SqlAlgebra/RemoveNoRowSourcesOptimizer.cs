using System.Collections.Generic;
using System.Linq;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sql.Algebra;
using Slp.r2rml4net.Storage.Sql.Algebra.Condition;
using Slp.r2rml4net.Storage.Sql.Algebra.Operator;
using Slp.r2rml4net.Storage.Sql.Algebra.Source;
using Slp.r2rml4net.Storage.Sql.Binders;

namespace Slp.r2rml4net.Storage.Optimization.SqlAlgebra
{
    /// <summary>
    /// Remove no row sources optimizer
    /// </summary>
    public class RemoveNoRowSourcesOptimizer : ISqlAlgebraOptimizer, ISqlSourceVisitor
    {
        /// <summary>
        /// Processes the algebra.
        /// </summary>
        /// <param name="algebra">The algebra.</param>
        /// <param name="context">The query context.</param>
        /// <returns>The processed algebra.</returns>
        public INotSqlOriginalDbSource ProcessAlgebra(INotSqlOriginalDbSource algebra, QueryContext context)
        {
            return (INotSqlOriginalDbSource)algebra.Accept(this, context);
        }

        /// <summary>
        /// Visits the specified no row source.
        /// </summary>
        /// <param name="noRowSource">The no row source.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        public object Visit(NoRowSource noRowSource, object data)
        {
            return noRowSource;
        }

        /// <summary>
        /// Visits the specified single empty row source.
        /// </summary>
        /// <param name="singleEmptyRowSource">The single empty row source.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        public object Visit(SingleEmptyRowSource singleEmptyRowSource, object data)
        {
            return singleEmptyRowSource;
        }

        /// <summary>
        /// Visits the specified SQL select operator.
        /// </summary>
        /// <param name="sqlSelectOp">The SQL select operator.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        public object Visit(SqlSelectOp sqlSelectOp, object data)
        {
            if (sqlSelectOp.Conditions.OfType<AlwaysFalseCondition>().Any())
                return CreateNoRowSource(sqlSelectOp);

            if (sqlSelectOp.JoinSources.Select(x => x.Condition).OfType<AlwaysFalseCondition>().Any())
                return CreateNoRowSource(sqlSelectOp);

            var originalSource = (ISqlSource)sqlSelectOp.OriginalSource.Accept(this, data);

            if (originalSource is NoRowSource)
                return CreateNoRowSource(sqlSelectOp);

            if (sqlSelectOp.JoinSources.Select(x => x.Source).Select(x => x.Accept(this, data)).OfType<NoRowSource>().Any())
                return CreateNoRowSource(sqlSelectOp);

            return sqlSelectOp;
        }

        /// <summary>
        /// Visits the specified SQL union operator.
        /// </summary>
        /// <param name="sqlUnionOp">The SQL union operator.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        public object Visit(SqlUnionOp sqlUnionOp, object data)
        {
            List<ISqlSource> toRemove = new List<ISqlSource>();

            foreach (var source in sqlUnionOp.Sources)
            {
                var accepted = (ISqlSource)source.Accept(this, data);

                if (accepted is NoRowSource)
                    toRemove.Add(source);
            }

            if (toRemove.Count == sqlUnionOp.Sources.Count())
                return CreateNoRowSource(sqlUnionOp);

            foreach (var source in toRemove)
            {
                sqlUnionOp.RemoveSource(source);
            }

            return sqlUnionOp;
        }

        /// <summary>
        /// Visits the specified SQL statement.
        /// </summary>
        /// <param name="sqlStatement">The SQL statement.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        public object Visit(SqlStatement sqlStatement, object data)
        {
            return sqlStatement;
        }

        /// <summary>
        /// Visits the specified SQL table.
        /// </summary>
        /// <param name="sqlTable">The SQL table.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        public object Visit(SqlTable sqlTable, object data)
        {
            return sqlTable;
        }

        /// <summary>
        /// Creates the no row source.
        /// </summary>
        /// <param name="sqlSelectOp">The SQL select op.</param>
        /// <returns>NoRowSource.</returns>
        private NoRowSource CreateNoRowSource(INotSqlOriginalDbSource sqlSelectOp)
        {
            var noRowSource = new NoRowSource();

            foreach (var valBinder in sqlSelectOp.ValueBinders)
            {
                noRowSource.AddValueBinder(new BlankValueBinder(valBinder.VariableName));
            }

            return noRowSource;
        }
    }
}
