using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Utils;

namespace Slp.r2rml4net.Storage.Sparql.Algebra.Operator
{
    public interface ISparqlQueryVisitor : IVisitor
    {
        object Visit(BgpOp bgpOp, object data);

        object Visit(JoinOp joinOp, object data);

        object Visit(OneEmptySolutionOp oneEmptySolutionOp, object data);

        object Visit(UnionOp unionOp, object data);

        object Visit(NoSolutionOp noSolutionOp, object data);

        object Visit(SelectOp selectOp, object data);

        object Visit(SliceOp sliceOp, object data);

        object Visit(OrderByOp orderByOp, object data);

        object Visit(DistinctOp distinctOp, object data);
    }
}
