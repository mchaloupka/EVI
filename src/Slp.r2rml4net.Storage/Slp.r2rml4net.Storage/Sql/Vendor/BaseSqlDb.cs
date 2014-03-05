using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sparql;
using Slp.r2rml4net.Storage.Sql.Algebra;
using Slp.r2rml4net.Storage.Sql.Algebra.Condition;
using Slp.r2rml4net.Storage.Sql.Algebra.Expression;
using Slp.r2rml4net.Storage.Sql.Algebra.Operator;
using Slp.r2rml4net.Storage.Sql.Algebra.Source;

namespace Slp.r2rml4net.Storage.Sql.Vendor
{
    public abstract class BaseSqlDb : ISqlDb
    {
        public abstract IQueryResultReader ExecuteQuery(string query, QueryContext context);

        public string GenerateQuery(INotSqlOriginalDbSource sqlAlgebra, QueryContext context)
        {
            GenerateNames(sqlAlgebra, context);

            StringBuilder sb = new StringBuilder();
            GenerateQuery(sb, sqlAlgebra, context);
            return sb.ToString();
        }

        private void GenerateQuery(StringBuilder sb, ISqlSource sqlSource, QueryContext context)
        {
            if (sqlSource is SqlSelectOp)
            {
                GenerateSelectOpQuery(sb, (SqlSelectOp)sqlSource, context);
            }
            else if (sqlSource is SqlTable)
            {
                GenerateTableQuery(sb, (SqlTable)sqlSource, context);
            }
            else if (sqlSource is SqlStatement)
            {
                GenerateStatementQuery(sb, (SqlStatement)sqlSource, context);
            }
            else if(sqlSource is SqlUnionOp)
            {
                GenerateUnionOpQuery(sb, (SqlUnionOp)sqlSource, context);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private void GenerateInnerQuery(StringBuilder sb, ISqlSource sqlSource, QueryContext context)
        {
            if (IsDelimiterNeeded(sqlSource))
            {
                sb.Append("(");
                GenerateQuery(sb, sqlSource, context);
                sb.Append(")");
            }
            else
            {
                GenerateQuery(sb, sqlSource, context);
            }

            if (!string.IsNullOrEmpty(sqlSource.Name))
            {
                sb.Append(" AS ");
                sb.Append(sqlSource.Name);
            }
        }

        private bool IsDelimiterNeeded(ISqlSource sqlSource)
        {
            return !(sqlSource is SqlTable);
        }

        private void GenerateUnionOpQuery(StringBuilder sb, SqlUnionOp sqlUnionOp, QueryContext context)
        {
            bool first = true;

            foreach (var select in sqlUnionOp.Sources)
            {
                if (first)
                    first = false;
                else
                    sb.Append(" UNION ");

                GenerateSelectOpQuery(sb, select, context);
            }
        }

        private void GenerateSelectOpQuery(StringBuilder sb, SqlSelectOp sqlSelectOp, QueryContext context)
        {
            sb.Append("SELECT");
            var cols = sqlSelectOp.Columns.OrderBy(x => x.Name).ToArray();
            for (int i = 0; i < cols.Length; i++)
            {
                if (i != 0)
                    sb.Append(",");
                sb.Append(" ");
                GenerateSelectColumnQuery(sb, cols[i], context);
            }

            sb.Append(" FROM ");
            GenerateInnerQuery(sb, sqlSelectOp.OriginalSource, context);

            foreach (var join in sqlSelectOp.JoinSources)
            {
                sb.Append(" INNER JOIN ");
                GenerateInnerQuery(sb, join.Source, context);
                sb.Append(" ON ");
                GenerateConditionQuery(sb, join.Condition, context);
            }

            foreach (var join in sqlSelectOp.LeftOuterJoinSources)
            {
                sb.Append(" LEFT OUTER JOIN ");
                GenerateInnerQuery(sb, join.Source, context);
                sb.Append(" ON ");
                GenerateConditionQuery(sb, join.Condition, context);
            }

            var conditions = sqlSelectOp.Conditions.ToArray();

            for (int i = 0; i < conditions.Length; i++)
            {
                if (i == 0)
                {
                    sb.Append(" WHERE ");
                }
                else
                {
                    sb.Append(" AND ");
                }

                GenerateConditionQuery(sb, conditions[i], context);
            }
        }

        

        private void GenerateSelectColumnQuery(StringBuilder sb, ISqlColumn column, QueryContext context)
        {
            if (column is SqlSelectColumn)
            {
                var sqlSelectColumn = (SqlSelectColumn)column;

                GenerateColumnQuery(sb, sqlSelectColumn.OriginalColumn, context);
            }
            else if(column is SqlExpressionColumn)
            {
                var sqlExpressionColumn = (SqlExpressionColumn)column;

                GenerateExpressionQuery(sb, sqlExpressionColumn.Expression, context);
            }
            else
            {
                throw new NotImplementedException();
            }

            if (!string.IsNullOrEmpty(column.Name))
            {
                sb.Append(" AS ");
                sb.Append(column.Name);
            }
            else
            {
                throw new Exception("All names should be set");
            }
        }

        private void GenerateColumnQuery(StringBuilder sb, ISqlColumn col, QueryContext context)
        {
            sb.Append(col.Source.Name);
            sb.Append(".");
            sb.Append(col.Name);
        }

        private void GenerateTableQuery(StringBuilder sb, SqlTable sqlTable, QueryContext context)
        {
            sb.Append(sqlTable.TableName);
        }

        private void GenerateStatementQuery(StringBuilder sb, SqlStatement sqlStatement, QueryContext context)
        {
            sb.Append(sqlStatement.SqlQuery);
        }

        private void GenerateNames(ISqlSource sqlSource, QueryContext context)
        {
            if (sqlSource is SqlSelectOp)
            {
                GenerateSelectOpNames((SqlSelectOp)sqlSource, context);
            }
            else if (sqlSource is SqlTable)
            {
                GenerateTableNames((SqlTable)sqlSource, context);
            }
            else if (sqlSource is SqlStatement)
            {
                GenerateStatementNames((SqlStatement)sqlSource, context);
            }
            else if(sqlSource is SqlUnionOp)
            {
                GenerateUnionOpNames((SqlUnionOp)sqlSource, context);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private void GenerateUnionOpNames(SqlUnionOp sqlUnionOp, QueryContext context)
        {
            foreach (var unColumn in sqlUnionOp.Columns.Cast<SqlUnionColumn>())
            {
                foreach (var select in sqlUnionOp.Sources)
                {
                    var column = unColumn.OriginalColumns.FirstOrDefault(x => x.Source == select);

                    if (column == null)
                    {
                        column = select.GetExpressionColumn(new NullExpr());
                        unColumn.AddColumn(column);
                    }
                }
            }

            sqlUnionOp.CaseColumn.Name = "uncase";
            GenerateColumnNames(sqlUnionOp.Columns, context);

            foreach (var unColumn in sqlUnionOp.Columns.Cast<SqlUnionColumn>())
            {
                foreach (var select in sqlUnionOp.Sources)
                {
                    var column = unColumn.OriginalColumns.First(x => x.Source == select);
                    column.Name = unColumn.Name;
                }
            }

            GenerateSourceNames(sqlUnionOp, context);
        }

        private void GenerateStatementNames(SqlStatement sqlStatement, QueryContext context)
        {
            foreach (var col in sqlStatement.Columns.OfType<IOriginalSqlColumn>())
            {
                col.Name = col.OriginalName;
            }
        }

        private void GenerateTableNames(SqlTable sqlTable, QueryContext context)
        {
            foreach (var col in sqlTable.Columns.OfType<IOriginalSqlColumn>())
            {
                col.Name = col.OriginalName;
            }
        }

        private void GenerateSelectOpNames(SqlSelectOp sqlSelectOp, QueryContext context)
        {
            GenerateColumnNames(sqlSelectOp.Columns, context);
            GenerateSourceNames(sqlSelectOp, context);
        }

        private void GenerateSourceNames(SqlUnionOp sqlUnionOp, QueryContext context)
        {
            foreach (var select in sqlUnionOp.Sources)
            {
                // In union the sources are not named
                GenerateNames(select, context);
            }
        }

        private void GenerateSourceNames(SqlSelectOp sqlSelectOp, QueryContext context)
        {
            GenerateSourceName(sqlSelectOp.OriginalSource, context);
            GenerateNames(sqlSelectOp.OriginalSource, context);

            foreach (var join in sqlSelectOp.JoinSources)
            {
                GenerateSourceName(join.Source, context);
                GenerateNames(join.Source, context);
            }

            foreach (var join in sqlSelectOp.LeftOuterJoinSources)
            {
                GenerateSourceName(join.Source, context);
                GenerateNames(join.Source, context);
            }
        }

        private void GenerateSourceName(ISqlSource source, QueryContext context)
        {
            string prefix = string.Empty;

            if (source is SqlSelectOp)
                prefix = "sel";
            else if (source is SqlTable)
                prefix = "tab";
            else if (source is SqlStatement)
                prefix = "sta";
            else
                throw new NotImplementedException();

            int counter = 2;
            var curName = prefix;
            string appendString = string.Empty;

            if(prefix.EndsWith("]"))
            {
                prefix = prefix.Substring(0, prefix.Length - 1);
                appendString = "]";
            }

            while (context.IsAlreadyUsedSqlSourceName(curName))
            {
                curName = string.Format("{0}{1}{2}", prefix, counter++, appendString);
            }

            context.RegisterUsedSqlSourceName(curName);
            source.Name = curName;
        }

        private void GenerateColumnNames(IEnumerable<ISqlColumn> columns, QueryContext context)
        {
            List<string> colNames = new List<string>();

            foreach (var col in columns.Where(x => x.Name != null))
            {
                colNames.Add(col.Name);
            }

            foreach (var col in columns)
            {
                if (col.Name != null)
                    continue;

                var orName = GetOriginalColumnName(col);
                int counter = 2;
                var curName = orName;
                string appendString = string.Empty;

                if (orName.EndsWith("]"))
                {
                    orName = orName.Substring(0, orName.Length - 1);
                    appendString = "]";
                }

                while (colNames.Contains(curName))
                {
                    curName = string.Format("{0}{1}{2}", orName, counter++, appendString);
                }

                colNames.Add(curName);
                col.Name = curName;
            }
        }

        private string GetOriginalColumnName(ISqlColumn col)
        {
            if (col is IOriginalSqlColumn)
            {
                return ((IOriginalSqlColumn)col).OriginalName;
            }
            else if (col is SqlSelectColumn)
            {
                return GetOriginalColumnName(((SqlSelectColumn)col).OriginalColumn);
            }
            else if (col is SqlExpressionColumn)
            {
                return "expr";
            }
            else if(col is SqlUnionColumn)
            {
                var unCol = (SqlUnionColumn)col;

                ISqlColumn innerCol = null;

                foreach (var subCol in unCol.OriginalColumns)
                {
                    if(subCol is IOriginalSqlColumn)
                    {
                        return GetOriginalColumnName(subCol);
                    }
                    else if(!(innerCol is SqlSelectColumn) && subCol is SqlSelectColumn)
                    {
                        innerCol = subCol;
                    }
                    else if(!(innerCol is SqlSelectColumn) && !(innerCol is SqlExpressionColumn) && subCol is SqlExpressionColumn)
                    {
                        innerCol = subCol;
                    }
                    else if (!(innerCol is SqlSelectColumn) && !(innerCol is SqlExpressionColumn) && !(innerCol is SqlUnionColumn) && subCol is SqlUnionColumn)
                    {
                        innerCol = subCol;
                    }
                }

                if (innerCol == null)
                    return "un";
                else
                    return GetOriginalColumnName(innerCol);
            }
            else
                throw new NotImplementedException();
        }

        #region Conditions
        private void GenerateConditionQuery(StringBuilder sb, ICondition condition, QueryContext context)
        {
            if (condition is AlwaysFalseCondition)
            {
                sb.Append("1=0");
            }
            else if (condition is AlwaysTrueCondition)
            {
                sb.Append("1=1");
            }
            else if (condition is AndCondition)
            {
                GenerateAndConditionQuery(sb, (AndCondition)condition, context);
            }
            else if (condition is OrCondition)
            {
                GenerateOrConditionQuery(sb, (OrCondition)condition, context);
            }
            else if (condition is EqualsCondition)
            {
                GenerateEqualsConditionQuery(sb, (EqualsCondition)condition, context);
            }
            else if(condition is NotCondition)
            {
                GenerateNotConditionQuery(sb, (NotCondition)condition, context);
            }
            else if (condition is IsNullCondition)
            {
                GenerateIsNullConditionQuery(sb, (IsNullCondition)condition, context);
            }
            else
                throw new Exception("Unknown condition type");
        }

        private void GenerateIsNullConditionQuery(StringBuilder sb, IsNullCondition isNullCondition, QueryContext context)
        {
            GenerateColumnQuery(sb, isNullCondition.Column, context);
            sb.Append(" IS NULL");
        }

        private void GenerateNotConditionQuery(StringBuilder sb, NotCondition notCondition, QueryContext context)
        {
            sb.Append("NOT ");
            GenerateConditionQuery(sb, notCondition.InnerCondition, context);
        }

        private void GenerateEqualsConditionQuery(StringBuilder sb, EqualsCondition equalsCondition, QueryContext context)
        {
            var leftExpr = equalsCondition.LeftOperand;
            var rightExpr = equalsCondition.RightOperand;

            GenerateExpressionQuery(sb, leftExpr, context);

            sb.Append("=");
            GenerateExpressionQuery(sb, rightExpr, context);
        }

        private void GenerateOrConditionQuery(StringBuilder sb, OrCondition orCondition, QueryContext context)
        {
            var conditions = orCondition.Conditions.ToArray();

            if (conditions.Length == 0)
            {
                throw new Exception("Cannot generate query for empty OR condition");
            }
            else if (conditions.Length == 1)
            {
                GenerateConditionQuery(sb, conditions[0], context);
            }
            else
            {
                for (int i = 0; i < conditions.Length; i++)
                {
                    if (i > 0)
                        sb.Append(" OR ");

                    sb.Append("(");
                    GenerateConditionQuery(sb, conditions[i], context);
                    sb.Append(")");
                }
            }
        }

        private void GenerateAndConditionQuery(StringBuilder sb, AndCondition andCondition, QueryContext context)
        {
            var conditions = andCondition.Conditions.ToArray();

            if (conditions.Length == 0)
            {
                throw new Exception("Cannot generate query for empty AND condition");
            }
            else if (conditions.Length == 1)
            {
                GenerateConditionQuery(sb, conditions[0], context);
            }
            else
            {
                for (int i = 0; i < conditions.Length; i++)
                {
                    if (i > 0)
                        sb.Append(" AND ");

                    sb.Append("(");
                    GenerateConditionQuery(sb, conditions[i], context);
                    sb.Append(")");
                }
            }
        } 
        #endregion

        #region Expressions
        private void GenerateExpressionQuery(StringBuilder sb, IExpression leftExpr, QueryContext context)
        {
            if (leftExpr is ColumnExpr)
            {
                GenerateColumnExpressionQuery(sb, (ColumnExpr)leftExpr, context);
            }
            else if(leftExpr is ConcatenationExpr)
            {
                GenerateConcatenationExpressionQuery(sb, (ConcatenationExpr)leftExpr, context);
            }
            else if(leftExpr is ConstantExpr)
            {
                GenerateConstantExpressionQuery(sb, (ConstantExpr)leftExpr, context);
            }
            else if(leftExpr is NullExpr)
            {
                GenerateNullExpressionQuery(sb, (NullExpr)leftExpr, context);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private void GenerateNullExpressionQuery(StringBuilder sb, NullExpr nullExpr, QueryContext context)
        {
            sb.Append("NULL");
        }

        private void GenerateConstantExpressionQuery(StringBuilder sb, ConstantExpr constantExpr, QueryContext context)
        {
            sb.Append(constantExpr.SqlString);
        }

        private void GenerateConcatenationExpressionQuery(StringBuilder sb, ConcatenationExpr concatenationExpr, QueryContext context)
        {
            sb.Append("CONCAT(");

            var parts = concatenationExpr.Parts.ToArray();

            for (int i = 0; i < parts.Length; i++)
            {
                if (i > 0)
                    sb.Append(", ");

                sb.Append("CAST(");
                GenerateExpressionQuery(sb, parts[i], context);
                sb.Append(" AS nvarchar(MAX))");
            }

            sb.Append(")");
        }

        private void GenerateColumnExpressionQuery(StringBuilder sb, ColumnExpr columnExpr, QueryContext context)
        {
            GenerateColumnQuery(sb, columnExpr.Column, context);
        }
        #endregion
    }
}
