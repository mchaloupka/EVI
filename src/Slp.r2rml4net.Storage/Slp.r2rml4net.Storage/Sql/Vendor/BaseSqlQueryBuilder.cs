using System;
using System.Linq;
using System.Text;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sql.Algebra;
using Slp.r2rml4net.Storage.Sql.Algebra.Condition;
using Slp.r2rml4net.Storage.Sql.Algebra.Expression;
using Slp.r2rml4net.Storage.Sql.Algebra.Operator;
using Slp.r2rml4net.Storage.Sql.Algebra.Source;

namespace Slp.r2rml4net.Storage.Sql.Vendor
{
    /// <summary>
    /// SQL query builder
    /// </summary>
    public class BaseSqlQueryBuilder : ISqlSourceVisitor, IConditionVisitor, IExpressionVisitor
    {
        /// <summary>
        /// Generates the query.
        /// </summary>
        /// <param name="sqlSource">The SQL source.</param>
        /// <param name="context">The context.</param>
        /// <returns>The SQL query.</returns>
        public string GenerateQuery(INotSqlOriginalDbSource sqlSource, QueryContext context)
        {
            var vContext = new VisitorContext(new StringBuilder(), context);
            sqlSource.Accept(this, vContext);
            return vContext.Sb.ToString();
        }

        /// <summary>
        /// The context for visitor
        /// </summary>
        protected class VisitorContext
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="VisitorContext"/> class.
            /// </summary>
            /// <param name="sb">The string builder.</param>
            /// <param name="context">The context.</param>
            public VisitorContext(StringBuilder sb, QueryContext context)
            {
                Sb = sb;
                Context = context;
            }

            /// <summary>
            /// Gets the string builder.
            /// </summary>
            /// <value>The string builder.</value>
            public StringBuilder Sb { get; private set; }

            /// <summary>
            /// Gets the context.
            /// </summary>
            /// <value>The context.</value>
            public QueryContext Context { get; private set; }
        }

        #region ISqlSource
        /// <summary>
        /// Visits the specified no row source.
        /// </summary>
        /// <param name="noRowSource">The no row source.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public object Visit(NoRowSource noRowSource, object data)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Visits the specified single empty row source.
        /// </summary>
        /// <param name="singleEmptyRowSource">The single empty row source.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public object Visit(SingleEmptyRowSource singleEmptyRowSource, object data)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Visits the specified SQL select operator.
        /// </summary>
        /// <param name="sqlSelectOp">The SQL select operator.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        /// <exception cref="System.Exception">To enable limit and offset, it is needed to use also order by clause</exception>
        public object Visit(SqlSelectOp sqlSelectOp, object data)
        {
            var context = (VisitorContext)data;

            context.Sb.Append("SELECT");

            if(sqlSelectOp.IsDistinct)
            {
                context.Sb.Append(" DISTINCT");
            }

            if (sqlSelectOp.Limit.HasValue && !sqlSelectOp.Offset.HasValue)
            {
                context.Sb.Append(" TOP ");
                context.Sb.Append(sqlSelectOp.Limit.Value);
            }

            var cols = sqlSelectOp.Columns.OrderBy(x => x.Name).ToArray();
            if(cols.Length > 0)
            {
                for (int i = 0; i < cols.Length; i++)
                {
                    if (i != 0)
                        context.Sb.Append(",");
                    context.Sb.Append(" ");

                    GenerateSelectColumnQuery(cols[i], context);
                }
            }
            else
            {
                context.Sb.Append(" NULL AS c");
            }
            

            context.Sb.Append(" FROM ");
            GenerateInnerQuery(sqlSelectOp.OriginalSource, context);

            foreach (var join in sqlSelectOp.JoinSources)
            {
                context.Sb.Append(" INNER JOIN ");
                GenerateInnerQuery(join.Source, context);
                context.Sb.Append(" ON ");
                join.Condition.Accept(this, context);
            }

            foreach (var join in sqlSelectOp.LeftOuterJoinSources)
            {
                context.Sb.Append(" LEFT OUTER JOIN ");
                GenerateInnerQuery(join.Source, context);
                context.Sb.Append(" ON ");
                join.Condition.Accept(this, context);
            }

            var conditions = sqlSelectOp.Conditions.ToArray();

            for (int i = 0; i < conditions.Length; i++)
            {
                context.Sb.Append(i == 0 ? " WHERE " : " AND ");

                conditions[i].Accept(this, context);
            }

            var orderings = sqlSelectOp.Orderings.ToArray();

            for (int i = 0; i < orderings.Length; i++)
            {
                context.Sb.Append(i == 0 ? " ORDER BY " : ", ");

                orderings[i].Expression.Accept(this, context);

                context.Sb.Append(orderings[i].Descending ? " DESC" : " ASC");
            }

            if (sqlSelectOp.Offset.HasValue)
            {
                if (orderings.Length == 0)
                    throw new Exception("To enable limit and offset, it is needed to use also order by clause");

                context.Sb.Append(" OFFSET ");
                context.Sb.Append(sqlSelectOp.Offset ?? 0);
                context.Sb.Append(" ROWS");

                if (sqlSelectOp.Limit.HasValue)
                {
                    context.Sb.Append(" FETCH NEXT ");
                    context.Sb.Append(sqlSelectOp.Limit.Value);
                    context.Sb.Append(" ROWS ONLY");
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
            var context = (VisitorContext)data;
            bool first = true;

            foreach (var select in sqlUnionOp.Sources)
            {
                if (first)
                    first = false;
                else if(sqlUnionOp.IsReduced)
                    context.Sb.Append(" UNION ");
                else
                    context.Sb.Append(" UNION ALL ");

                select.Accept(this, context);
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
            var context = (VisitorContext)data;
            context.Sb.Append(sqlStatement.SqlQuery);
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
            var context = (VisitorContext)data;
            context.Sb.Append(sqlTable.TableName);
            return null;
        }

        /// <summary>
        /// Generates the inner query.
        /// </summary>
        /// <param name="sqlSource">The SQL source.</param>
        /// <param name="context">The context.</param>
        private void GenerateInnerQuery(ISqlSource sqlSource, VisitorContext context)
        {
            if (IsDelimiterNeeded(sqlSource))
            {
                context.Sb.Append("(");
                sqlSource.Accept(this, context);
                context.Sb.Append(")");
            }
            else
            {
                sqlSource.Accept(this, context);
            }

            if (!string.IsNullOrEmpty(sqlSource.Name))
            {
                context.Sb.Append(" AS ");
                context.Sb.Append(sqlSource.Name);
            }
        }

        /// <summary>
        /// Generates the select column query.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="context">The context.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        /// <exception cref="System.Exception">All names should be set</exception>
        private void GenerateSelectColumnQuery(ISqlColumn column, VisitorContext context)
        {
            if (column is SqlSelectColumn)
            {
                var sqlSelectColumn = (SqlSelectColumn)column;

                GenerateColumnQuery(sqlSelectColumn.OriginalColumn, context);
            }
            else if (column is SqlExpressionColumn)
            {
                var sqlExpressionColumn = (SqlExpressionColumn)column;

                sqlExpressionColumn.Expression.Accept(this, context);
            }
            else
            {
                throw new NotImplementedException();
            }

            if (!string.IsNullOrEmpty(column.Name))
            {
                context.Sb.Append(" AS ");
                context.Sb.Append(column.Name);
            }
            else
            {
                throw new Exception("All names should be set");
            }
        }

        /// <summary>
        /// Generates the column query.
        /// </summary>
        /// <param name="col">The column.</param>
        /// <param name="context">The context.</param>
        private void GenerateColumnQuery(ISqlColumn col, VisitorContext context)
        {
            if (!string.IsNullOrEmpty(col.Source.Name)) // root is not named
            {
                context.Sb.Append(col.Source.Name);
                context.Sb.Append(".");
            }
            context.Sb.Append(col.Name);
        }

        /// <summary>
        /// Determines whether the delimiter is needed for the specified SQL source.
        /// </summary>
        /// <param name="sqlSource">The SQL source.</param>
        /// <returns><c>true</c> if the delimiter is needed for the specified SQL source; otherwise, <c>false</c>.</returns>
        private bool IsDelimiterNeeded(ISqlSource sqlSource)
        {
            return !(sqlSource is SqlTable);
        }
        #endregion

        #region ICondition
        /// <summary>
        /// Visits the specified condition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        public object Visit(AlwaysFalseCondition condition, object data)
        {
            var context = (VisitorContext)data;
            context.Sb.Append("1=0");
            return null;
        }

        /// <summary>
        /// Visits the specified condition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        public object Visit(AlwaysTrueCondition condition, object data)
        {
            var context = (VisitorContext)data;
            context.Sb.Append("1=1");
            return null;
        }

        /// <summary>
        /// Visits the specified condition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        public object Visit(EqualsCondition condition, object data)
        {
            var context = (VisitorContext)data;
            var leftExpr = condition.LeftOperand;
            var rightExpr = condition.RightOperand;

            // TODO: Decide whether cast needed
            context.Sb.Append("CAST(");
            leftExpr.Accept(this, context);
            context.Sb.Append(" AS nvarchar(MAX))");

            context.Sb.Append("=");

            context.Sb.Append("CAST(");
            rightExpr.Accept(this, context);
            context.Sb.Append(" AS nvarchar(MAX))");

            return null;
        }

        /// <summary>
        /// Visits the specified condition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        public object Visit(IsNullCondition condition, object data)
        {
            var context = (VisitorContext)data;
            GenerateColumnQuery(condition.Column, context);
            context.Sb.Append(" IS NULL");
            return null;
        }

        /// <summary>
        /// Visits the specified condition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        public object Visit(NotCondition condition, object data)
        {
            var context = (VisitorContext)data;
            context.Sb.Append("NOT ");
            condition.InnerCondition.Accept(this, context);
            return null;
        }

        /// <summary>
        /// Visits the specified condition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        /// <exception cref="System.Exception">Cannot generate query for empty AND condition</exception>
        public object Visit(AndCondition condition, object data)
        {
            var context = (VisitorContext)data;
            var conditions = condition.Conditions.ToArray();

            if (conditions.Length == 0)
            {
                throw new Exception("Cannot generate query for empty AND condition");
            }
            else if (conditions.Length == 1)
            {
                conditions[0].Accept(this, context);
            }
            else
            {
                for (int i = 0; i < conditions.Length; i++)
                {
                    if (i > 0)
                        context.Sb.Append(" AND ");

                    context.Sb.Append("(");
                    conditions[i].Accept(this, context);
                    context.Sb.Append(")");
                }
            }

            return null;
        }

        /// <summary>
        /// Visits the specified condition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        /// <exception cref="System.Exception">Cannot generate query for empty OR condition</exception>
        public object Visit(OrCondition condition, object data)
        {
            var context = (VisitorContext)data;
            var conditions = condition.Conditions.ToArray();

            if (conditions.Length == 0)
            {
                throw new Exception("Cannot generate query for empty OR condition");
            }
            else if (conditions.Length == 1)
            {
                conditions[0].Accept(this, context);
            }
            else
            {
                for (int i = 0; i < conditions.Length; i++)
                {
                    if (i > 0)
                        context.Sb.Append(" OR ");

                    context.Sb.Append("(");
                    conditions[i].Accept(this, context);
                    context.Sb.Append(")");
                }
            }

            return null;
        }
        #endregion

        #region IExpression
        /// <summary>
        /// Visits the specified coalesceExpr.
        /// </summary>
        /// <param name="expression">The coalesceExpr.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        public object Visit(ColumnExpr expression, object data)
        {
            var context = (VisitorContext)data;
            GenerateColumnQuery(expression.Column, context);
            return null;
        }

        /// <summary>
        /// Visits the specified coalesceExpr.
        /// </summary>
        /// <param name="expression">The coalesceExpr.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        public object Visit(ConstantExpr expression, object data)
        {
            var context = (VisitorContext)data;
            context.Sb.Append(expression.SqlString);
            return null;
        }

        /// <summary>
        /// Visits the specified coalesceExpr.
        /// </summary>
        /// <param name="expression">The coalesceExpr.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        public object Visit(ConcatenationExpr expression, object data)
        {
            var context = (VisitorContext)data;

            context.Sb.Append("CONCAT(");

            var parts = expression.Parts.ToArray();

            for (int i = 0; i < parts.Length; i++)
            {
                if (i > 0)
                    context.Sb.Append(", ");

                context.Sb.Append("CAST(");
                parts[i].Accept(this, context);
                context.Sb.Append(" AS nvarchar(MAX))");
            }

            context.Sb.Append(")");

            return null;
        }

        /// <summary>
        /// Visits the specified coalesceExpr.
        /// </summary>
        /// <param name="nullExpr">The coalesceExpr.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        public object Visit(NullExpr nullExpr, object data)
        {
            var context = (VisitorContext)data;
            context.Sb.Append("NULL");
            return null;
        }

        /// <summary>
        /// Visits the specified coalesceExpr.
        /// </summary>
        /// <param name="coalesceExpr">The coalesceExpr.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        public object Visit(CoalesceExpr coalesceExpr, object data)
        {
            var context = (VisitorContext)data;

            context.Sb.Append("COALESCE(");

            var inners = coalesceExpr.Expressions.ToArray();

            for (int i = 0; i < inners.Length; i++)
            {
                if (i > 0)
                    context.Sb.Append(", ");

                inners[i].Accept(this, data);
            }

            context.Sb.Append(")");

            return null;
        }

        /// <summary>
        /// Visits the specified coalesceExpr.
        /// </summary>
        /// <param name="caseExpr">The coalesceExpr.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        public object Visit(CaseExpr caseExpr, object data)
        {
            var context = (VisitorContext)data;

            context.Sb.Append("CASE");

            foreach (var statement in caseExpr.Statements)
            {
                context.Sb.Append(" WHEN ");
                statement.Condition.Accept(this, data);
                context.Sb.Append(" THEN ");
                statement.Expression.Accept(this, data);
            }

            context.Sb.Append(" END");

            return null;
        }
        #endregion
    }
}
