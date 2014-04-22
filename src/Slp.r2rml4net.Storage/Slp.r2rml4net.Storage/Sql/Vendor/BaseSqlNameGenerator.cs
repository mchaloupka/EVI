using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sql.Algebra;
using Slp.r2rml4net.Storage.Sql.Algebra.Expression;
using Slp.r2rml4net.Storage.Sql.Algebra.Operator;
using Slp.r2rml4net.Storage.Sql.Algebra.Source;

namespace Slp.r2rml4net.Storage.Sql.Vendor
{
    public class BaseSqlNameGenerator : ISqlSourceVisitor
    {
        public void GenerateNames(INotSqlOriginalDbSource source, QueryContext context)
        {
            source.Accept(this, context);
        }

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
            var context = (QueryContext)data;
            GenerateColumnNames(sqlSelectOp.Columns, context);

            GenerateSourceName(sqlSelectOp.OriginalSource, context);
            sqlSelectOp.OriginalSource.Accept(this, context);

            foreach (var join in sqlSelectOp.JoinSources)
            {
                GenerateSourceName(join.Source, context);
                join.Source.Accept(this, context);
            }

            foreach (var join in sqlSelectOp.LeftOuterJoinSources)
            {
                GenerateSourceName(join.Source, context);
                join.Source.Accept(this, context);
            }

            return null;
        }

        public object Visit(SqlUnionOp sqlUnionOp, object data)
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
            GenerateColumnNames(sqlUnionOp.Columns, (QueryContext)data);

            foreach (var unColumn in sqlUnionOp.Columns.Cast<SqlUnionColumn>())
            {
                foreach (var select in sqlUnionOp.Sources)
                {
                    var column = unColumn.OriginalColumns.First(x => x.Source == select);
                    column.Name = unColumn.Name;
                }
            }

            foreach (var select in sqlUnionOp.Sources)
            {
                // In union the sources are not named
                select.Accept(this, data);
            }

            return null;
        }

        public object Visit(Algebra.Source.SqlStatement sqlStatement, object data)
        {
            foreach (var col in sqlStatement.Columns.OfType<IOriginalSqlColumn>())
            {
                col.Name = col.OriginalName;
            }

            return null;
        }

        public object Visit(Algebra.Source.SqlTable sqlTable, object data)
        {
            foreach (var col in sqlTable.Columns.OfType<IOriginalSqlColumn>())
            {
                col.Name = col.OriginalName;
            }

            return null;
        }

        private void GenerateColumnNames(IEnumerable<ISqlColumn> columns, QueryContext queryContext)
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

                while (colNames.Contains(DataReaderWrapper.GetColumnNameUnquoted(curName)))
                {
                    curName = string.Format("{0}{1}{2}", orName, counter++, appendString);
                }

                colNames.Add(DataReaderWrapper.GetColumnNameUnquoted((curName)));
                col.Name = curName;
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
            else if (source is SqlUnionOp)
                prefix = "un";
            else
                throw new NotImplementedException();

            int counter = 2;
            var curName = prefix;
            string appendString = string.Empty;

            if (prefix.EndsWith("]"))
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
            else if (col is SqlUnionColumn)
            {
                var unCol = (SqlUnionColumn)col;

                ISqlColumn innerCol = null;

                foreach (var subCol in unCol.OriginalColumns)
                {
                    if (subCol is IOriginalSqlColumn)
                    {
                        return GetOriginalColumnName(subCol);
                    }
                    else if (!(innerCol is SqlSelectColumn) && subCol is SqlSelectColumn)
                    {
                        innerCol = subCol;
                    }
                    else if (!(innerCol is SqlSelectColumn) && !(innerCol is SqlExpressionColumn) && subCol is SqlExpressionColumn)
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
    }
}
