using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sql.Algebra;
using Slp.r2rml4net.Storage.Sql.Algebra.Utils;
using VDS.RDF;

namespace Slp.r2rml4net.Storage.Sql.Binders
{
    /// <summary>
    /// CASE value binder.
    /// </summary>
    public class CaseValueBinder : IBaseValueBinder
    {
        /// <summary>
        /// The statements
        /// </summary>
        private List<CaseStatementBinder> statements;

        /// <summary>
        /// Initializes a new instance of the <see cref="CaseValueBinder"/> class.
        /// </summary>
        /// <param name="variableName">Name of the variable.</param>
        public CaseValueBinder(string variableName)
        {
            statements = new List<CaseStatementBinder>();
            this.VariableName = variableName;
        }

        /// <summary>
        /// Adds the value binder.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="valueBinder">The value binder.</param>
        /// <exception cref="System.Exception">Cannot add value binder to case value binder with different variable name</exception>
        public void AddValueBinder(ICondition condition, IBaseValueBinder valueBinder)
        {
            if (valueBinder.VariableName != this.VariableName)
                throw new Exception("Cannot add value binder to case value binder with different variable name");

            statements.Add(new CaseStatementBinder(condition, valueBinder));
        }

        /// <summary>
        /// Loads the node value.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="row">The db row.</param>
        /// <param name="context">The query context.</param>
        /// <returns>The node.</returns>
        /// <exception cref="System.Exception">Every row should be in some case</exception>
        public INode LoadNode(INodeFactory factory, IQueryResultRow row, QueryContext context)
        {
            var matchingStatement = statements.Where(x => x.Condition.StaticEvaluation(row)).FirstOrDefault();

            if(matchingStatement != null)
            {
                return matchingStatement.ValueBinder.LoadNode(factory, row, context);
            }
            else
            {
                throw new Exception("Every row should be in some case");
            }
        }

        /// <summary>
        /// Gets the name of the variable.
        /// </summary>
        /// <value>The name of the variable.</value>
        public string VariableName { get; private set; }

        /// <summary>
        /// Gets the assigned columns.
        /// </summary>
        /// <value>The assigned columns.</value>
        public IEnumerable<ISqlColumn> AssignedColumns
        {
            get
            {
                return this.statements.SelectMany(x => x.ValueBinder.AssignedColumns.Union(x.Condition.GetAllReferencedColumns())).Distinct();
            }
        }

        /// <summary>
        /// Gets the statements.
        /// </summary>
        /// <value>The statements.</value>
        public IEnumerable<CaseStatementBinder> Statements { get { return statements; } }

        /// <summary>
        /// Replaces the assigned column.
        /// </summary>
        /// <param name="oldColumn">The old column.</param>
        /// <param name="newColumn">The new column.</param>
        public void ReplaceAssignedColumn(ISqlColumn oldColumn, ISqlColumn newColumn)
        {
            foreach (var binder in statements)
            {
                binder.ValueBinder.ReplaceAssignedColumn(oldColumn, newColumn);
                binder.Condition.ReplaceColumnReference(oldColumn, newColumn);
            }
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public object Clone()
        {
            var newBinder = new CaseValueBinder(this.VariableName);

            foreach (var binder in this.statements)
            {
                newBinder.statements.Add(new CaseStatementBinder((ICondition)binder.Condition.Clone(), (IBaseValueBinder)binder.ValueBinder.Clone()));
            }

            return newBinder;
        }

        /// <summary>
        /// Accepts the specified visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value from visitor.</returns>
        [DebuggerStepThrough]
        public object Accept(IValueBinderVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }

        /// <summary>
        /// Removes the statement.
        /// </summary>
        /// <param name="caseStatement">The case statement.</param>
        public void RemoveStatement(CaseStatementBinder caseStatement)
        {
            this.statements.Remove(caseStatement);
        }
    }

    /// <summary>
    /// CASE statement in value binder
    /// </summary>
    public class CaseStatementBinder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CaseStatementBinder"/> class.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="valueBinder">The value binder.</param>
        public CaseStatementBinder(ICondition condition, IBaseValueBinder valueBinder)
        {
            this.Condition = condition;
            this.ValueBinder = valueBinder;
        }

        /// <summary>
        /// Gets or sets the condition.
        /// </summary>
        /// <value>The condition.</value>
        public ICondition Condition { get; set; }

        /// <summary>
        /// Gets or sets the value binder.
        /// </summary>
        /// <value>The value binder.</value>
        public IBaseValueBinder ValueBinder { get; set; }
    }
}
