using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Sql
{
    /// <summary>
    /// The SQL result reader
    /// </summary>
    public interface IQueryResultReader : IDisposable
    {

        /// <summary>
        /// Gets a value indicating whether this instance has next row.
        /// </summary>
        /// <value><c>true</c> if this instance has next row; otherwise, <c>false</c>.</value>
        bool HasNextRow { get; }

        /// <summary>
        /// Reads the current row and moves to next one.
        /// </summary>
        /// <returns>Readed row, <c>null</c> if there is no row</returns>
        IQueryResultRow Read();
    }

    /// <summary>
    /// Single row in SQL result
    /// </summary>
    public interface IQueryResultRow
    {

        /// <summary>
        /// Gets the columns.
        /// </summary>
        /// <value>The columns.</value>
        IEnumerable<IQueryResultColumn> Columns { get; }

        /// <summary>
        /// Gets the column.
        /// </summary>
        /// <param name="name">The column name.</param>
        /// <returns>The column.</returns>
        IQueryResultColumn GetColumn(string name);
    }

    /// <summary>
    /// Single cell in SQL result
    /// </summary>
    public interface IQueryResultColumn
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>The value.</value>
        object Value { get; }

        /// <summary>
        /// Gets the boolean value.
        /// </summary>
        /// <returns><c>true</c> if the value is true, <c>false</c> otherwise.</returns>
        bool GetBooleanValue();
    }
}
