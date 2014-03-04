using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Sql
{
    public interface IQueryResultReader : IDisposable
    {

        bool HasNextRow { get; }

        IQueryResultRow Read();
    }

    public interface IQueryResultRow
    {

        IEnumerable<IQueryResultColumn> Columns { get; }

        IQueryResultColumn GetColumn(string p);
    }

    public interface IQueryResultColumn
    {
        string Name { get; }

        object Value { get; }

        bool GetBooleanValue();
    }
}
