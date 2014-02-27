using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sparql;
using Slp.r2rml4net.Storage.Sql.Algebra;
using Slp.r2rml4net.Storage.Sql.Algebra.Operator;
using Slp.r2rml4net.Storage.Sql.Algebra.Source;

namespace Slp.r2rml4net.Storage.Sql.Vendor
{
    public abstract class BaseSqlDb : ISqlDb
    {
        public abstract IQueryResultReader ExecuteQuery(string query, QueryContext context);

        public string GenerateQuery(ISqlQuery sqlAlgebra, QueryContext context)
        {
            GenerateNames(sqlAlgebra.SqlSource, context);

            StringBuilder sb = new StringBuilder();
            GenerateQuery(sb, sqlAlgebra.SqlSource, context);
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
            else if(sqlSource is SqlStatement)
            {
                GenerateStatementQuery(sb, (SqlStatement)sqlSource, context);
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

        private void GenerateSelectOpQuery(StringBuilder sb, SqlSelectOp sqlSelectOp, QueryContext context)
        {
            sb.Append("SELECT");
            var cols = sqlSelectOp.Columns.ToArray();
            for (int i = 0; i < cols.Length; i++)
            {
                if (i != 0)
                    sb.Append(",");
                sb.Append(" ");
                GenerateColumnQuery(sb, cols[i], context);
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

        private void GenerateConditionQuery(StringBuilder sb, ICondition condition, QueryContext context)
        {
            throw new NotImplementedException();
        }

        private void GenerateColumnQuery(StringBuilder sb, ISqlColumn col, QueryContext context)
        {
            if (col is SqlTableColumn)
            {
                var tCol = (SqlTableColumn)col;
                sb.Append(col.Source.Name);
                sb.Append(".");
                sb.Append(tCol.OriginalName);
            }
            else if (col is SqlSelectColumn)
            {
                var sCol = (SqlSelectColumn)col;
                sb.Append(sCol.OriginalColumn.Source.Name);
                sb.Append(".");
                sb.Append(sCol.OriginalColumn.Name);
            }
            else throw new Exception("Unknown column type");

            if (!string.IsNullOrEmpty(col.Name))
            {
                sb.Append(" AS ");
                sb.Append(col.Name);
            }
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
            else
            {
                throw new NotImplementedException();
            }
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
            GenerateColumnNames(sqlSelectOp, context);
            GenerateSourceNames(sqlSelectOp, context);
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

            while (context.IsAlreadyUsedSqlSourceName(curName))
            {
                curName = string.Format("{0}{1}", prefix, counter++);
            }

            context.RegisterUsedSqlSourceName(curName);
            source.Name = curName;
        }

        private void GenerateColumnNames(SqlSelectOp sqlSelectOp, QueryContext context)
        {
            List<string> colNames = new List<string>();

            foreach (var col in sqlSelectOp.Columns)
            {
                var orName = GetOriginalColumnName(col);
                var newName = orName;
                int counter = 2;

                while (colNames.Contains(newName))
                {
                    newName = string.Format("{0}{1}", orName, counter++);
                }

                colNames.Add(newName);
                col.Name = newName;
            }
        }

        private string GetOriginalColumnName(ISqlColumn col)
        {
            if (col is IOriginalSqlColumn)
            {
                return ((IOriginalSqlColumn)col).OriginalName;
            }
            else if (col is INotOriginalSqlColumn)
            {
                return GetOriginalColumnName(((INotOriginalSqlColumn)col).OriginalColumn);
            }
            else
                throw new Exception("Column must be original or not original");
        }
    }
}
