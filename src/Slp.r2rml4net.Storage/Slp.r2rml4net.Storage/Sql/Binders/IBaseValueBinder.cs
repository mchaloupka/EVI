﻿using System;
using System.Collections.Generic;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sql.Algebra;
using Slp.r2rml4net.Storage.Utils;
using VDS.RDF;
namespace Slp.r2rml4net.Storage.Sql.Binders
{
    public interface IBaseValueBinder : ICloneable, IVisitable<IValueBinderVisitor>
    {
        INode LoadNode(INodeFactory factory, IQueryResultRow row, QueryContext context);

        string VariableName { get; }

        IEnumerable<ISqlColumn> AssignedColumns { get; }

        void ReplaceAssignedColumn(ISqlColumn oldColumn, ISqlColumn newColumn);
    }
}