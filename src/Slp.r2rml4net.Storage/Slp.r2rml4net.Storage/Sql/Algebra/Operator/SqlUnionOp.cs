using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Sql.Binders;
using Slp.r2rml4net.Storage.Sql.Algebra.Utils;

namespace Slp.r2rml4net.Storage.Sql.Algebra.Operator
{
    /// <summary>
    /// UNION operator.
    /// </summary>
    public class SqlUnionOp : INotSqlOriginalDbSource
    {
        /// <summary>
        /// The columns
        /// </summary>
        private List<SqlUnionColumn> columns;

        /// <summary>
        /// The sources
        /// </summary>
        private List<SqlSelectOp> sources;

        /// <summary>
        /// The value binders
        /// </summary>
        private List<IBaseValueBinder> valueBinders;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlUnionOp"/> class.
        /// </summary>
        public SqlUnionOp()
        {
            this.CaseColumn = new SqlUnionColumn(this);
            this.columns = new List<SqlUnionColumn>();
            this.sources = new List<SqlSelectOp>();
            this.valueBinders = new List<IBaseValueBinder>();
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets the case column.
        /// </summary>
        /// <value>The case column.</value>
        public SqlUnionColumn CaseColumn { get; private set; }

        /// <summary>
        /// Gets the columns.
        /// </summary>
        /// <value>The columns.</value>
        public IEnumerable<ISqlColumn> Columns
        {
            get
            {
                yield return this.CaseColumn;

                foreach (var col in this.columns)
                    yield return col;
            }
        }

        /// <summary>
        /// Adds the source.
        /// </summary>
        /// <param name="select">The select.</param>
        public void AddSource(SqlSelectOp select)
        {
            this.sources.Add(select);
        }

        /// <summary>
        /// Adds the value binder.
        /// </summary>
        /// <param name="valueBinder">The value binder.</param>
        public void AddValueBinder(IBaseValueBinder valueBinder)
        {
            this.valueBinders.Add(valueBinder);
        }

        /// <summary>
        /// Replaces the value binder.
        /// </summary>
        /// <param name="oldBinder">The old binder.</param>
        /// <param name="newBinder">The new binder.</param>
        public void ReplaceValueBinder(IBaseValueBinder oldBinder, IBaseValueBinder newBinder)
        {
            var index = this.valueBinders.IndexOf(oldBinder);

            if (index > -1)
                this.valueBinders[index] = newBinder;
        }

        /// <summary>
        /// Removes the value binder.
        /// </summary>
        /// <param name="valueBinder">The value binder.</param>
        public void RemoveValueBinder(IBaseValueBinder valueBinder)
        {
            var index = this.valueBinders.IndexOf(valueBinder);

            if (index > -1)
                this.valueBinders.RemoveAt(index);
        }

        /// <summary>
        /// Gets the value binders.
        /// </summary>
        /// <value>The value binders.</value>
        public IEnumerable<IBaseValueBinder> ValueBinders
        {
            get { return this.valueBinders; }
        }

        /// <summary>
        /// Gets the unioned column.
        /// </summary>
        /// <returns>SqlUnionColumn.</returns>
        public SqlUnionColumn GetUnionedColumn()
        {
            var col = new SqlUnionColumn(this);
            this.columns.Add(col);
            return col;
        }

        /// <summary>
        /// Gets the sources.
        /// </summary>
        /// <value>The sources.</value>
        public IEnumerable<SqlSelectOp> Sources { get { return sources; } }

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
        /// Removes the column.
        /// </summary>
        /// <param name="col">The column.</param>
        public void RemoveColumn(ISqlColumn col)
        {
            if(col is SqlUnionColumn)
            {
                this.columns.Remove((SqlUnionColumn)col);
            }
        }

        /// <summary>
        /// Removes the source.
        /// </summary>
        /// <param name="source">The source.</param>
        public void RemoveSource(ISqlSource source)
        {
            if (source is SqlSelectOp)
            {
                this.sources.Remove((SqlSelectOp)source);

                List<SqlUnionColumn> colToDelete = new List<SqlUnionColumn>();

                foreach (var col in this.columns)
                {
                    var containedColumns = col.OriginalColumns.Where(x => x.Source == source).ToList();

                    foreach (var ccol in containedColumns)
                    {
                        col.RemoveColumn(ccol);
                    }
                }
                
                foreach (var valBinder in this.valueBinders.Cast<CaseValueBinder>())
                {
                    var sourceStatements = valBinder.Statements.Where(x => x.Condition.GetAllReferencedColumns().All(y => y.Source == source)).ToList();

                    foreach (var statement in sourceStatements)
                    {
                        valBinder.RemoveStatement(statement);
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is reduced.
        /// </summary>
        /// <value><c>true</c> if this instance is reduced; otherwise, <c>false</c>.</value>
        public bool IsReduced { get; set; }
    }
}
