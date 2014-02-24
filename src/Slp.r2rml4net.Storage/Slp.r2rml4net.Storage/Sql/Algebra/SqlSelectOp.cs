using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Sql.Algebra
{
    public class SqlSelectOp : ISqlSource
    {
        public IEnumerable<SqlSelectColumn> Columns { get { return columns; } }

        public ISqlSource OriginalSource { get { return originalSource; } }

        public IEnumerable<Tuple<ICondition, ISqlSource>> JoinSources { get { return joinSources; } }

        public IEnumerable<ISqlSource> LeftOuterJoinSources { get { return leftOuterJoinSources; } }

        public IEnumerable<ICondition> Conditions { get { return conditions; } }

        private List<SqlSelectColumn> columns;
        private List<Tuple<ICondition, ISqlSource>> joinSources;
        private List<ISqlSource> leftOuterJoinSources;
        private List<ICondition> conditions;
        private ISqlSource originalSource;

        public SqlSelectOp(ISqlSource originalSource)
        {
            this.originalSource = originalSource;
            this.columns = new List<SqlSelectColumn>();
            this.joinSources = new List<Tuple<ICondition, ISqlSource>>();
            this.leftOuterJoinSources = new List<ISqlSource>();
            this.conditions = new List<ICondition>();
        }

        public ISqlColumn GetSelectColumn(ISqlColumn sourceColumn)
        {
            var col = this.columns.Where(x => x.OriginalColumn == sourceColumn).FirstOrDefault();

            if (col == null)
            {
                col = new SqlSelectColumn(sourceColumn, this);
                this.columns.Add(col);
            }

            return col;
        }
    }
}
