using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Sql.Algebra.Condition;

namespace Slp.r2rml4net.Storage.Sql.Algebra.Operator
{
    public class SqlSelectOp : ISqlSource
    {
        public IEnumerable<SqlSelectColumn> Columns { get { return columns; } }

        public ISqlSource OriginalSource { get { return originalSource; } }

        public IEnumerable<ConditionedSource> JoinSources { get { return joinSources; } }

        public IEnumerable<ConditionedSource> LeftOuterJoinSources { get { return leftOuterJoinSources; } }

        public IEnumerable<ICondition> Conditions { get { return conditions; } }

        private List<SqlSelectColumn> columns;
        private List<ConditionedSource> joinSources;
        private List<ConditionedSource> leftOuterJoinSources;
        private List<ICondition> conditions;
        private ISqlSource originalSource;

        public SqlSelectOp(ISqlSource originalSource)
        {
            this.originalSource = originalSource;
            this.columns = new List<SqlSelectColumn>();
            this.joinSources = new List<ConditionedSource>();
            this.leftOuterJoinSources = new List<ConditionedSource>();
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

        public void AddCondition(ICondition condition)
        {
            this.conditions.Add(condition);
        }

        public void RemoveColumn(SqlSelectColumn col)
        {
            if (this.columns.Contains(col))
                this.columns.Remove(col);
        }

        public void ReplaceCondition(ICondition cond, ICondition processedCondition)
        {
            var index = conditions.IndexOf(cond);

            if (index > -1)
                conditions[index] = processedCondition;
        }

        public void RemoveCondition(ICondition cond)
        {
            var index = conditions.IndexOf(cond);

            if (index > -1)
                conditions.RemoveAt(index);
        }

        public void ClearConditions()
        {
            this.conditions.Clear();
        }

        public string Name { get; set; }


        IEnumerable<ISqlColumn> ISqlSource.Columns
        {
            get { return this.Columns.Cast<ISqlColumn>(); }
        }
    }
}
