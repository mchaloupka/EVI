using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Sql.Binders;

namespace Slp.r2rml4net.Storage.Sql.Algebra
{
    public interface ISqlSource
    {
        string Name { get; set; }

        IEnumerable<ISqlColumn> Columns { get; }
    }

    public interface INotSqlOriginalDbSource : ISqlSource
    {
        void AddValueBinder(IBaseValueBinder valueBinder);

        void ReplaceValueBinder(IBaseValueBinder oldBinder, IBaseValueBinder newBinder);

        void RemoveValueBinder(IBaseValueBinder valueBinder);

        IEnumerable<IBaseValueBinder> ValueBinders { get; }
    }

    public interface ISqlOriginalDbSource : ISqlSource
    {
        ISqlColumn GetColumn(string column);
    }
}
