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
        private BaseSqlQueryBuilder queryBuilder;
        private BaseSqlNameGenerator nameGenerator;

        public BaseSqlDb()
        {
            this.queryBuilder = new BaseSqlQueryBuilder();
            this.nameGenerator = new BaseSqlNameGenerator();
        }

        public abstract IQueryResultReader ExecuteQuery(string query, QueryContext context);

        public string GenerateQuery(INotSqlOriginalDbSource sqlAlgebra, QueryContext context)
        {
            this.nameGenerator.GenerateNames(sqlAlgebra, context);

            return this.queryBuilder.GenerateQuery(sqlAlgebra, context);
        }

        public bool CanBeUnioned(ISqlColumn column, ISqlColumn other)
        {
            if(column is SqlSelectColumn)
            {
                return CanBeUnioned(((SqlSelectColumn)column).OriginalColumn, other);
            }
            else if(column is SqlUnionColumn)
            {
                var unColumn = (SqlUnionColumn)column;
                var firstCol = unColumn.OriginalColumns.FirstOrDefault();

                if (firstCol != null)
                    return CanBeUnioned(firstCol, other);
                else
                    return false;
            }
            else if(column is SqlExpressionColumn)
            {
                // TODO: Determine sql type
                return false;
            }
            else if(column is SqlTableColumn && other is SqlTableColumn)
            {
                // TODO: Make it accordingly to type
                if(column.Source is SqlTable && other.Source is SqlTable)
                {
                    var columnTableName = ((SqlTable)column.Source).TableName;
                    var otherTableName = ((SqlTable)other.Source).TableName;

                    return columnTableName == otherTableName && (((SqlTableColumn)column).OriginalName == ((SqlTableColumn)other).OriginalName);
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return CanBeUnioned(other, column);
            }
        }
    }
}
