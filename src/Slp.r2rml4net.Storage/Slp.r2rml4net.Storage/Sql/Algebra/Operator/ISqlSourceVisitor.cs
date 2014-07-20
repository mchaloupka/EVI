using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Utils;

namespace Slp.r2rml4net.Storage.Sql.Algebra.Operator
{
    /// <summary>
    /// Visitor for SQL source
    /// </summary>
    public interface ISqlSourceVisitor : IVisitor
    {
        /// <summary>
        /// Visits the specified no row source.
        /// </summary>
        /// <param name="noRowSource">The no row source.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        object Visit(NoRowSource noRowSource, object data);

        /// <summary>
        /// Visits the specified single empty row source.
        /// </summary>
        /// <param name="singleEmptyRowSource">The single empty row source.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        object Visit(SingleEmptyRowSource singleEmptyRowSource, object data);

        /// <summary>
        /// Visits the specified SQL select operator.
        /// </summary>
        /// <param name="sqlSelectOp">The SQL select operator.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        object Visit(SqlSelectOp sqlSelectOp, object data);

        /// <summary>
        /// Visits the specified SQL union operator.
        /// </summary>
        /// <param name="sqlUnionOp">The SQL union operator.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        object Visit(SqlUnionOp sqlUnionOp, object data);

        /// <summary>
        /// Visits the specified SQL statement.
        /// </summary>
        /// <param name="sqlStatement">The SQL statement.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        object Visit(Source.SqlStatement sqlStatement, object data);

        /// <summary>
        /// Visits the specified SQL table.
        /// </summary>
        /// <param name="sqlTable">The SQL table.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        object Visit(Source.SqlTable sqlTable, object data);
    }
}
