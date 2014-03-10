using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sql.Algebra;
using Slp.r2rml4net.Storage.Sql.Algebra.Condition;
using Slp.r2rml4net.Storage.Sql.Algebra.Expression;
using Slp.r2rml4net.Storage.Sql.Algebra.Operator;
using Slp.r2rml4net.Storage.Sql.Algebra.Source;

namespace Slp.r2rml4net.Storage.Sql.Vendor
{
    public class BaseSqlQueryBuilder : ISqlSourceVisitor, IConditionVisitor, IExpressionVisitor
    {
        public string GenerateQuery(INotSqlOriginalDbSource sqlSource, QueryContext context)
        {
            var vContext = new VisitorContext(new StringBuilder(), context);
            sqlSource.Accept(this, vContext);
            return vContext.SB.ToString();
        }

        protected class VisitorContext
        {
            public VisitorContext(StringBuilder sb, QueryContext context)
            {
                this.SB = sb;
                this.Context = context;
            }

            public StringBuilder SB { get; private set; }

            public QueryContext Context { get; private set; }
        }

        #region ISqlSource
        public object Visit(NoRowSource noRowSource, object data)
        {
            throw new NotImplementedException();
        }

        public object Visit(SingleEmptyRowSource singleEmptyRowSource, object data)
        {
            throw new NotImplementedException();
        }

        public object Visit(SqlSelectOp sqlSelectOp, object data)
        {
            var context = (VisitorContext)data;

            context.SB.Append("SELECT");
            var cols = sqlSelectOp.Columns.OrderBy(x => x.Name).ToArray();
            for (int i = 0; i < cols.Length; i++)
            {
                if (i != 0)
                    context.SB.Append(",");
                context.SB.Append(" ");

                GenerateSelectColumnQuery(cols[i], context);
            }

            context.SB.Append(" FROM ");
            GenerateInnerQuery(sqlSelectOp.OriginalSource, context);

            foreach (var join in sqlSelectOp.JoinSources)
            {
                context.SB.Append(" INNER JOIN ");
                GenerateInnerQuery(join.Source, context);
                context.SB.Append(" ON ");
                join.Condition.Accept(this, context);
            }

            foreach (var join in sqlSelectOp.LeftOuterJoinSources)
            {
                context.SB.Append(" LEFT OUTER JOIN ");
                GenerateInnerQuery(join.Source, context);
                context.SB.Append(" ON ");
                join.Condition.Accept(this, context);
            }

            var conditions = sqlSelectOp.Conditions.ToArray();

            for (int i = 0; i < conditions.Length; i++)
            {
                if (i == 0)
                {
                    context.SB.Append(" WHERE ");
                }
                else
                {
                    context.SB.Append(" AND ");
                }

                conditions[i].Accept(this, context);
            }

            return null;
        }

        public object Visit(SqlUnionOp sqlUnionOp, object data)
        {
            var context = (VisitorContext)data;
            bool first = true;

            foreach (var select in sqlUnionOp.Sources)
            {
                if (first)
                    first = false;
                else
                    context.SB.Append(" UNION ");

                select.Accept(this, context);
            }

            return null;
        }

        public object Visit(Algebra.Source.SqlStatement sqlStatement, object data)
        {
            var context = (VisitorContext)data;
            context.SB.Append(sqlStatement.SqlQuery);
            return null;
        }

        public object Visit(Algebra.Source.SqlTable sqlTable, object data)
        {
            var context = (VisitorContext)data;
            context.SB.Append(sqlTable.TableName);
            return null;
        }

        private void GenerateInnerQuery(ISqlSource sqlSource, VisitorContext context)
        {
            if (IsDelimiterNeeded(sqlSource))
            {
                context.SB.Append("(");
                sqlSource.Accept(this, context);
                context.SB.Append(")");
            }
            else
            {
                sqlSource.Accept(this, context);
            }

            if (!string.IsNullOrEmpty(sqlSource.Name))
            {
                context.SB.Append(" AS ");
                context.SB.Append(sqlSource.Name);
            }
        }

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
                context.SB.Append(" AS ");
                context.SB.Append(column.Name);
            }
            else
            {
                throw new Exception("All names should be set");
            }
        }

        private void GenerateColumnQuery(ISqlColumn col, VisitorContext context)
        {
            context.SB.Append(col.Source.Name);
            context.SB.Append(".");
            context.SB.Append(col.Name);
        }

        private bool IsDelimiterNeeded(ISqlSource sqlSource)
        {
            return !(sqlSource is SqlTable);
        } 
        #endregion

        #region ICondition
        public object Visit(AlwaysFalseCondition condition, object data)
        {
            var context = (VisitorContext)data;
            context.SB.Append("1=0");
            return null;
        }

        public object Visit(AlwaysTrueCondition condition, object data)
        {
            var context = (VisitorContext)data;
            context.SB.Append("1=1");
            return null;
        }

        public object Visit(EqualsCondition condition, object data)
        {
            var context = (VisitorContext)data;
            var leftExpr = condition.LeftOperand;
            var rightExpr = condition.RightOperand;

            // TODO: Decide whether cast needed
            context.SB.Append("CAST(");
            leftExpr.Accept(this, context);
            context.SB.Append(" AS nvarchar(MAX))");

            context.SB.Append("=");

            context.SB.Append("CAST(");
            rightExpr.Accept(this, context);
            context.SB.Append(" AS nvarchar(MAX))");

            return null;
        }

        public object Visit(IsNullCondition condition, object data)
        {
            var context = (VisitorContext)data;
            GenerateColumnQuery(condition.Column, context);
            context.SB.Append(" IS NULL");
            return null;
        }

        public object Visit(NotCondition condition, object data)
        {
            var context = (VisitorContext)data;
            context.SB.Append("NOT ");
            condition.InnerCondition.Accept(this, context);
            return null;
        }

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
                        context.SB.Append(" AND ");

                    context.SB.Append("(");
                    conditions[i].Accept(this, context);
                    context.SB.Append(")");
                }
            }

            return null;
        }

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
                        context.SB.Append(" OR ");

                    context.SB.Append("(");
                    conditions[i].Accept(this, context);
                    context.SB.Append(")");
                }
            }

            return null;
        } 
        #endregion

        #region IExpression
        public object Visit(ColumnExpr expression, object data)
        {
            var context = (VisitorContext)data;
            GenerateColumnQuery(expression.Column, context);
            return null;
        }

        public object Visit(ConstantExpr expression, object data)
        {
            var context = (VisitorContext)data;
            context.SB.Append(expression.SqlString);
            return null;
        }

        public object Visit(ConcatenationExpr expression, object data)
        {
            var context = (VisitorContext)data;

            context.SB.Append("CONCAT(");

            var parts = expression.Parts.ToArray();

            for (int i = 0; i < parts.Length; i++)
            {
                if (i > 0)
                    context.SB.Append(", ");

                context.SB.Append("CAST(");
                parts[i].Accept(this, context);
                context.SB.Append(" AS nvarchar(MAX))");
            }

            context.SB.Append(")");

            return null;
        }

        public object Visit(NullExpr nullExpr, object data)
        {
            var context = (VisitorContext)data;
            context.SB.Append("NULL");
            return null;
        } 
        #endregion
    }
}
