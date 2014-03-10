using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Utils;

namespace Slp.r2rml4net.Storage.Sql.Algebra.Operator
{
    public interface ISqlSourceVisitor : IVisitor
    {
        object Visit(NoRowSource noRowSource, object data);

        object Visit(SingleEmptyRowSource singleEmptyRowSource, object data);

        object Visit(SqlSelectOp sqlSelectOp, object data);

        object Visit(SqlUnionOp sqlUnionOp, object data);

        object Visit(Source.SqlStatement sqlStatement, object data);

        object Visit(Source.SqlTable sqlTable, object data);
    }
}
