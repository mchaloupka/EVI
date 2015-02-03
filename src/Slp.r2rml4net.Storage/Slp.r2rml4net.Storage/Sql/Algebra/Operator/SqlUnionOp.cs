using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DatabaseSchemaReader.DataSchema;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sql.Algebra.Utils;
using Slp.r2rml4net.Storage.Sql.Binders;

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
        private readonly List<SqlUnionColumn> _columns;

        /// <summary>
        /// The sources
        /// </summary>
        private readonly List<SqlSelectOp> _sources;

        /// <summary>
        /// The value binders
        /// </summary>
        private readonly List<IBaseValueBinder> _valueBinders;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlUnionOp"/> class.
        /// </summary>
        /// <param name="context">Qury context</param>
        public SqlUnionOp(QueryContext context)
        {
            CaseColumn = new SqlUnionColumn(this, context.Db.SqlTypeForDecider);
            _columns = new List<SqlUnionColumn>();
            _sources = new List<SqlSelectOp>();
            _valueBinders = new List<IBaseValueBinder>();
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
                yield return CaseColumn;

                foreach (var col in _columns)
                    yield return col;
            }
        }

        /// <summary>
        /// Adds the source.
        /// </summary>
        /// <param name="select">The select.</param>
        public void AddSource(SqlSelectOp select)
        {
            _sources.Add(select);
        }

        /// <summary>
        /// Adds the value binder.
        /// </summary>
        /// <param name="valueBinder">The value binder.</param>
        public void AddValueBinder(IBaseValueBinder valueBinder)
        {
            _valueBinders.Add(valueBinder);
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
        /// Gets the value binders.
        /// </summary>
        /// <value>The value binders.</value>
        public IEnumerable<IBaseValueBinder> ValueBinders
        {
            get { return _valueBinders; }
        }

        /// <summary>
        /// Gets the unioned column.
        /// </summary>
        /// <param name="sqlDataType">SQL data type of the column</param>
        public SqlUnionColumn GetUnionedColumn(DataType sqlDataType)
        {
            var col = new SqlUnionColumn(this, sqlDataType);
            _columns.Add(col);
            return col;
        }

        /// <summary>
        /// Gets the sources.
        /// </summary>
        /// <value>The sources.</value>
        public IEnumerable<SqlSelectOp> Sources { get { return _sources; } }

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
                _columns.Remove((SqlUnionColumn)col);
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
                _sources.Remove((SqlSelectOp)source);

                foreach (var col in _columns)
                {
                    var containedColumns = col.OriginalColumns.Where(x => x.Source == source).ToList();

                    foreach (var ccol in containedColumns)
                    {
                        col.RemoveColumn(ccol);
                    }
                }
                
                foreach (var valBinder in _valueBinders.Cast<CaseValueBinder>())
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
