using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Slp.r2rml4net.Storage.Sql.Algebra.Source
{
    public class SqlTable : ISqlOriginalDbSource
    {
        public string TableName { get; private set; }

        public SqlTable(string tableName)
        {
            this.TableName = tableName;
            this.columns = new List<SqlTableColumn>();
        }

        private List<SqlTableColumn> columns;

        public ISqlColumn GetColumn(string columnName)
        {
            var col = columns.Where(x => x.Name == columnName).FirstOrDefault();

            if (col == null)
            {
                col = new SqlTableColumn(columnName, this);
                columns.Add(col);
            }

            return col;
        }

        public string Name { get; set; }


        public IEnumerable<ISqlColumn> Columns
        {
            get { return columns.AsEnumerable(); }
        }

        [DebuggerStepThrough]
        public object Accept(Operator.ISqlSourceVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }

        public void RemoveColumn(ISqlColumn col)
        {
            if (col is SqlTableColumn)
            {
                this.columns.Remove((SqlTableColumn)col);
            }
        }
    }
}
