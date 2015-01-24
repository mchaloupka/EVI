using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sql.Binders;

namespace Slp.r2rml4net.Storage.Sql.Algebra.Operator
{
    /// <summary>
    /// SELECT operator.
    /// </summary>
    public class SqlSelectOp : INotSqlOriginalDbSource
    {
        /// <summary>
        /// Gets the original source (FROM).
        /// </summary>
        /// <value>The original source.</value>
        public ISqlSource OriginalSource { get { return _originalSource; } }

        /// <summary>
        /// Gets the join sources (INNER JOIN).
        /// </summary>
        /// <value>The join sources.</value>
        public IEnumerable<ConditionedSource> JoinSources { get { return _joinSources; } }

        /// <summary>
        /// Gets the left outer join sources (LEFT OUTER JOIN).
        /// </summary>
        /// <value>The left outer join sources.</value>
        public IEnumerable<ConditionedSource> LeftOuterJoinSources { get { return _leftOuterJoinSources; } }

        /// <summary>
        /// Gets the conditions.
        /// </summary>
        /// <value>The conditions.</value>
        public IEnumerable<ICondition> Conditions { get { return _conditions; } }

        /// <summary>
        /// Gets the orderings.
        /// </summary>
        /// <value>The orderings.</value>
        public IEnumerable<SqlOrderByComparator> Orderings { get { return _orderings; } }

        /// <summary>
        /// Gets or sets the offset.
        /// </summary>
        /// <value>The offset.</value>
        public int? Offset { get; set; }

        /// <summary>
        /// Gets or sets the limit.
        /// </summary>
        /// <value>The limit.</value>
        public int? Limit { get; set; }

        /// <summary>
        /// The columns
        /// </summary>
        private readonly List<ISqlColumn> _columns;

        /// <summary>
        /// The join sources
        /// </summary>
        private readonly List<ConditionedSource> _joinSources;

        /// <summary>
        /// The left outer join sources
        /// </summary>
        private readonly List<ConditionedSource> _leftOuterJoinSources;

        /// <summary>
        /// The conditions
        /// </summary>
        private readonly List<ICondition> _conditions;

        /// <summary>
        /// The orderings
        /// </summary>
        private readonly List<SqlOrderByComparator> _orderings;

        /// <summary>
        /// The original source
        /// </summary>
        private readonly ISqlSource _originalSource;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlSelectOp"/> class.
        /// </summary>
        /// <param name="originalSource">The original source.</param>
        public SqlSelectOp(ISqlSource originalSource)
        {
            _originalSource = originalSource;
            _columns = new List<ISqlColumn>();
            _joinSources = new List<ConditionedSource>();
            _leftOuterJoinSources = new List<ConditionedSource>();
            _conditions = new List<ICondition>();
            _valueBinders = new List<IBaseValueBinder>();
            _orderings = new List<SqlOrderByComparator>();
        }

        /// <summary>
        /// Gets the select column.
        /// </summary>
        /// <param name="sourceColumn">The source column.</param>
        /// <returns>The column.</returns>
        public ISqlColumn GetSelectColumn(ISqlColumn sourceColumn)
        {
            var col = _columns.OfType<SqlSelectColumn>()
                .FirstOrDefault(x => x.OriginalColumn == sourceColumn);

            if (col == null)
            {
                col = new SqlSelectColumn(sourceColumn, this);
                _columns.Add(col);
            }

            return col;
        }

        /// <summary>
        /// Gets the expression column.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>The column.</returns>
        public ISqlColumn GetExpressionColumn(IExpression expression)
        {
            var col = new SqlExpressionColumn(expression, this);
            _columns.Add(col);
            return col;
        }

        /// <summary>
        /// Adds the condition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        public void AddCondition(ICondition condition)
        {
            _conditions.Add(condition);
        }

        /// <summary>
        /// Removes the column.
        /// </summary>
        /// <param name="col">The col.</param>
        public void RemoveColumn(ISqlColumn col)
        {
            if (_columns.Contains(col))
                _columns.Remove(col);
        }

        /// <summary>
        /// Replaces the condition.
        /// </summary>
        /// <param name="cond">The cond.</param>
        /// <param name="processedCondition">The processed condition.</param>
        public void ReplaceCondition(ICondition cond, ICondition processedCondition)
        {
            var index = _conditions.IndexOf(cond);

            if (index > -1)
                _conditions[index] = processedCondition;
        }

        /// <summary>
        /// Removes the condition.
        /// </summary>
        /// <param name="cond">The cond.</param>
        public void RemoveCondition(ICondition cond)
        {
            var index = _conditions.IndexOf(cond);

            if (index > -1)
                _conditions.RemoveAt(index);
        }

        /// <summary>
        /// Clears the conditions.
        /// </summary>
        public void ClearConditions()
        {
            _conditions.Clear();
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }


        /// <summary>
        /// Gets the columns.
        /// </summary>
        /// <value>The columns.</value>
        public IEnumerable<ISqlColumn> Columns
        {
            get { return _columns; }
        }

        /// <summary>
        /// The value binders
        /// </summary>
        private readonly List<IBaseValueBinder> _valueBinders;

        /// <summary>
        /// Adds the value binder.
        /// </summary>
        /// <param name="valueBinder">The value binder.</param>
        public void AddValueBinder(IBaseValueBinder valueBinder)
        {
            _valueBinders.Add(valueBinder);
        }

        /// <summary>
        /// Gets the value binders.
        /// </summary>
        /// <value>The value binders.</value>
        public IEnumerable<IBaseValueBinder> ValueBinders
        {
            get { return _valueBinders; }
        }

        /// <summary>
        /// Adds the joined source.
        /// </summary>
        /// <param name="sqlSource">The SQL source.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="context">The context.</param>
        public void AddJoinedSource(ISqlSource sqlSource, ICondition condition, QueryContext context)
        {
            _joinSources.Add(new ConditionedSource(condition, sqlSource));
        }

        /// <summary>
        /// Adds the left outer joined source.
        /// </summary>
        /// <param name="sqlSource">The SQL source.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="context">The context.</param>
        public void AddLeftOuterJoinedSource(ISqlSource sqlSource, ICondition condition, QueryContext context)
        {
            _leftOuterJoinSources.Add(new ConditionedSource(condition, sqlSource));
        }

        /// <summary>
        /// Replaces the value binder.
        /// </summary>
        /// <param name="oldBinder">The old binder.</param>
        /// <param name="newBinder">The new binder.</param>
        public void ReplaceValueBinder(IBaseValueBinder oldBinder, IBaseValueBinder newBinder)
        {
            var index = _valueBinders.IndexOf(oldBinder);

            if (index > -1)
                _valueBinders[index] = newBinder;
        }

        /// <summary>
        /// Removes the value binder.
        /// </summary>
        /// <param name="valueBinder">The value binder.</param>
        public void RemoveValueBinder(IBaseValueBinder valueBinder)
        {
            var index = _valueBinders.IndexOf(valueBinder);

            if (index > -1)
                _valueBinders.RemoveAt(index);
        }

        /// <summary>
        /// Accepts the specified visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value from visitor.</returns>
        [DebuggerStepThrough]
        public object Accept(ISqlSourceVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }

        /// <summary>
        /// Inserts the ordering.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="descending">if set to <c>true</c> [descending].</param>
        public void InsertOrdering(IExpression expression, bool descending)
        {
            _orderings.Insert(0, new SqlOrderByComparator(expression, descending));
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is distinct.
        /// </summary>
        /// <value><c>true</c> if this instance is distinct; otherwise, <c>false</c>.</value>
        public bool IsDistinct { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is reduced.
        /// </summary>
        /// <value><c>true</c> if this instance is reduced; otherwise, <c>false</c>.</value>
        public bool IsReduced { get; set; }
    }
}
