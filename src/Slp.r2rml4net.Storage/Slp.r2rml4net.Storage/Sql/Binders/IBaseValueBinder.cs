using System;
using System.Collections.Generic;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sql.Algebra;
using VDS.RDF;
namespace Slp.r2rml4net.Storage.Sql.Binders
{
    public interface IBaseValueBinder
    {
        INode LoadNode(INodeFactory factory, IQueryResultRow row, QueryContext context);

        string VariableName { get; }

        IEnumerable<ISqlColumn> AssignedColumns { get; }
    }
}
