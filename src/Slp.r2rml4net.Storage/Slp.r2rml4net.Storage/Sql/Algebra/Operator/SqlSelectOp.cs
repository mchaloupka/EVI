using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Sql.Algebra.Condition;
using Slp.r2rml4net.Storage.Sql.Binders;

namespace Slp.r2rml4net.Storage.Sql.Algebra.Operator
{
    public class SqlSelectOp : INotSqlOriginalDbSource
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
            this.valueBinders = new List<IBaseValueBinder>();
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

        private List<IBaseValueBinder> valueBinders;

        public void AddValueBinder(IBaseValueBinder valueBinder)
        {
            this.valueBinders.Add(valueBinder);
        }

        public IEnumerable<IBaseValueBinder> ValueBinders
        {
            get { return valueBinders; }
        }

        public bool HaveOnlyOriginalSourceAndConditions { get { return joinSources.Count == 0 && leftOuterJoinSources.Count == 0; } }

        public void AddJoinedSource(ISqlSource sqlSource, ICondition condition, Query.QueryContext context)
        {
            this.joinSources.Add(new ConditionedSource(condition, sqlSource));
        }

        public void ReplaceValueBinder(IBaseValueBinder fBinder, IBaseValueBinder sBinder)
        {
            var index = this.valueBinders.IndexOf(fBinder);

            if (index > -1)
                this.valueBinders[index] = sBinder;
        }

        public void RemoveValueBinder(IBaseValueBinder valueBinder)
        {
            var index = this.valueBinders.IndexOf(valueBinder);

            if (index > -1)
                this.valueBinders.RemoveAt(index);
        }
    }
}
