using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Sql.Algebra.Condition;
using Slp.r2rml4net.Storage.Sql.Binders;

namespace Slp.r2rml4net.Storage.Sql.Algebra.Operator
{
    public class SqlSelectOp : INotSqlOriginalDbSource
    {
        public ISqlSource OriginalSource { get { return originalSource; } }

        public IEnumerable<ConditionedSource> JoinSources { get { return joinSources; } }

        public IEnumerable<ConditionedSource> LeftOuterJoinSources { get { return leftOuterJoinSources; } }

        public IEnumerable<ICondition> Conditions { get { return conditions; } }

        public IEnumerable<SqlOrderByComparator> Orderings { get { return orderings; } }

        public int? Offset { get; set; }

        public int? Limit { get; set; }

        private List<ISqlColumn> columns;
        private List<ConditionedSource> joinSources;
        private List<ConditionedSource> leftOuterJoinSources;
        private List<ICondition> conditions;
        private List<SqlOrderByComparator> orderings;
        private ISqlSource originalSource;

        public SqlSelectOp(ISqlSource originalSource)
        {
            this.originalSource = originalSource;
            this.columns = new List<ISqlColumn>();
            this.joinSources = new List<ConditionedSource>();
            this.leftOuterJoinSources = new List<ConditionedSource>();
            this.conditions = new List<ICondition>();
            this.valueBinders = new List<IBaseValueBinder>();
            this.orderings = new List<SqlOrderByComparator>();
        }

        public ISqlColumn GetSelectColumn(ISqlColumn sourceColumn)
        {
            var col = this.columns.OfType<SqlSelectColumn>().Where(x => x.OriginalColumn == sourceColumn).FirstOrDefault();

            if (col == null)
            {
                col = new SqlSelectColumn(sourceColumn, this);
                this.columns.Add(col);
            }

            return col;
        }

        public ISqlColumn GetExpressionColumn(IExpression expression)
        {
            var col = new SqlExpressionColumn(expression, this);
            this.columns.Add(col);
            return col;
        }

        public void AddCondition(ICondition condition)
        {
            this.conditions.Add(condition);
        }

        public void RemoveColumn(ISqlColumn col)
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


        public IEnumerable<ISqlColumn> Columns
        {
            get { return this.columns; }
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

        public void AddJoinedSource(ISqlSource sqlSource, ICondition condition, Query.QueryContext context)
        {
            this.joinSources.Add(new ConditionedSource(condition, sqlSource));
        }

        public void AddLeftOuterJoinedSource(ISqlSource sqlSource, ICondition condition, Query.QueryContext context)
        {
            this.leftOuterJoinSources.Add(new ConditionedSource(condition, sqlSource));
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

        [DebuggerStepThrough]
        public object Accept(ISqlSourceVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }

        public void InsertOrdering(IExpression expression, bool descending)
        {
            this.orderings.Insert(0, new SqlOrderByComparator(expression, descending));
        }

        public bool IsDistinct { get; set; }

        public bool IsReduced { get; set; }
    }
}
