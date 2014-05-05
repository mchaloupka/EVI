using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sparql.Algebra;
using Slp.r2rml4net.Storage.Sparql.Algebra.Operator;
using TCode.r2rml4net.Mapping;

namespace Slp.r2rml4net.Storage.Optimization.SparqlAlgebra
{
    public class UnionOptimizer : ISparqlAlgebraOptimizer, ISparqlQueryVisitor
    {
        private JoinOptimizer _joinOptimizer;

        public UnionOptimizer()
        {
            this._joinOptimizer = new JoinOptimizer();
        }

        public ISparqlQuery ProcessAlgebra(ISparqlQuery algebra, QueryContext context)
        {
            return (ISparqlQuery)algebra.Accept(this, new VisitData(context));
        }

        public object Visit(BgpOp bgpOp, object data)
        {
            return bgpOp.FinalizeAfterTransform();
        }

        public object Visit(JoinOp joinOp, object data)
        {
            if (((VisitData)data).IsVisited(joinOp))
                return joinOp;

            List<ISparqlQuery> subQueries = new List<ISparqlQuery>();
            List<UnionOp> subUnions = new List<UnionOp>();

            bool changed = false;

            foreach (var oldInner in joinOp.GetInnerQueries())
            {
                var inner = (ISparqlQuery)oldInner.Accept(this, data);

                changed = ProcessJoinChild(subQueries, subUnions, inner, oldInner) || changed;
            }

            if (!changed)
            {
                ((VisitData)data).Visit(joinOp);

                return joinOp.FinalizeAfterTransform();
            }

            IEnumerable<IEnumerable<ISparqlQuery>> cartesian = CreateCartesians(subQueries, subUnions, ((VisitData)data).Context);

            List<JoinOp> resultJoins = new List<JoinOp>();
            foreach (var cartesItem in cartesian)
            {
                var join = new JoinOp();
                foreach (var subItem in cartesItem)
                {
                    join.AddToJoin(subItem);
                }
                resultJoins.Add(join);
            }

            if (resultJoins.Count == 0)
            {
                return new NoSolutionOp();
            }
            else if (resultJoins.Count == 1)
            {
                return resultJoins[0].Accept(this, data);
            }
            else
            {
                var union = new UnionOp();

                foreach (var resJoin in resultJoins)
                {
                    union.AddToUnion(resJoin);
                }

                return union.Accept(this, data);
            }
        }

        private IEnumerable<IEnumerable<ISparqlQuery>> CreateCartesians(List<ISparqlQuery> subQueries, List<UnionOp> subUnions, QueryContext context)
        {
            var leftCartesian = new List<List<ISparqlQuery>>() { subQueries };

            if (subUnions.Count == 0)
                return leftCartesian;

            var right = subUnions.Select(x => x.GetInnerQueries().ToList()).ToList();
            var rightCartesian = CreateCartesians(right, context);

            return ProcessCartesian(leftCartesian, rightCartesian, context);
        }

        private List<List<ISparqlQuery>> CreateCartesians(IEnumerable<IEnumerable<ISparqlQuery>> unions, QueryContext context)
        {
            var count = unions.Count();

            if (count == 1)
            {
                var union = unions.First();
                return new List<List<ISparqlQuery>>(union.Select(x => new List<ISparqlQuery>() { x }));
            }

            var splitCount = count / 2;

            var left = unions.Take(splitCount);
            var right = unions.Skip(splitCount);

            var leftCartesian = CreateCartesians(left, context);
            var rightCartesian = CreateCartesians(right, context);

            return ProcessCartesian(leftCartesian, rightCartesian, context);
        }

        private List<List<ISparqlQuery>> ProcessCartesian(List<List<ISparqlQuery>> left, List<List<ISparqlQuery>> right, QueryContext context)
        {
            List<List<ISparqlQuery>> result = new List<List<ISparqlQuery>>();

            foreach (var li in left)
            {
                foreach (var ri in right)
                {
                    var item = ProcessCartesian(li, ri, context);

                    if (item != null)
                        result.Add(item);
                }
            }

            return result;
        }

        private List<ISparqlQuery> ProcessCartesian(List<ISparqlQuery> li, List<ISparqlQuery> ri, QueryContext context)
        {
            Dictionary<string, List<ITermMap>> variables = new Dictionary<string, List<ITermMap>>();

            List<ISparqlQuery> result = new List<ISparqlQuery>();

            var subItems = li.Union(ri);

            foreach (var item in subItems)
            {
                if(item is BgpOp)
                {
                    var bgp = (BgpOp)item;

                    this._joinOptimizer.GetBgpInfo(bgp, variables, context);
                    if (!this._joinOptimizer.ProcessBgp(bgp, variables, context))
                        return null;
                }

                result.Add(item);
            }

            return result;
        }

        private static bool ProcessJoinChild(List<ISparqlQuery> subQueries, List<UnionOp> subUnions, ISparqlQuery inner, ISparqlQuery oldInner)
        {
            if (inner is UnionOp)
            {
                subUnions.Add((UnionOp)inner);

                return true;
            }
            else if (inner is JoinOp)
            {
                foreach (var subInner in inner.GetInnerQueries())
                {
                    ProcessJoinChild(subQueries, subUnions, subInner, subInner);
                }

                return true;
            }
            else
            {
                subQueries.Add(inner);

                return inner != oldInner;
            }
        }

        public object Visit(OneEmptySolutionOp oneEmptySolutionOp, object data)
        {
            return oneEmptySolutionOp.FinalizeAfterTransform();
        }

        public object Visit(UnionOp unionOp, object data)
        {
            if (((VisitData)data).IsVisited(unionOp))
                return unionOp;

            var newUnion = new UnionOp();
            bool changed = false;

            foreach (var oldInner in unionOp.GetInnerQueries())
            {
                var inner = (ISparqlQuery)oldInner.Accept(this, data);

                changed = ProcessUnionChild(newUnion, inner, oldInner) || (inner != oldInner);
            }

            if (changed)
                return newUnion.Accept(this, data);
            else
            {
                ((VisitData)data).Visit(unionOp);
                return unionOp.FinalizeAfterTransform();
            }
        }

        private bool ProcessUnionChild(UnionOp newUnion, ISparqlQuery inner, ISparqlQuery oldInner)
        {
            if (inner is UnionOp)
            {
                foreach (var subInner in inner.GetInnerQueries())
                {
                    ProcessUnionChild(newUnion, subInner, subInner);
                }

                return true;
            }
            else
            {
                newUnion.AddToUnion(inner);
                return false;
            }
        }

        public object Visit(NoSolutionOp noSolutionOp, object data)
        {
            return noSolutionOp.FinalizeAfterTransform();
        }

        public object Visit(SelectOp selectOp, object data)
        {
            var vd = (VisitData)data;

            if (vd.IsVisited(selectOp))
                return selectOp;

            var inner = (ISparqlQuery)selectOp.InnerQuery.Accept(this, data);

            if (inner != selectOp.InnerQuery)
                selectOp.ReplaceInnerQuery(selectOp.InnerQuery, inner);

            return selectOp.FinalizeAfterTransform();
        }

        public object Visit(SliceOp sliceOp, object data)
        {
            if (((VisitData)data).IsVisited(sliceOp))
                return sliceOp;

            var inner = (ISparqlQuery)sliceOp.InnerQuery.Accept(this, data);

            if (inner != sliceOp.InnerQuery)
                sliceOp.ReplaceInnerQuery(sliceOp.InnerQuery, inner);

            ((VisitData)data).Visit(sliceOp);
            return sliceOp.FinalizeAfterTransform();
        }

        public object Visit(OrderByOp orderByOp, object data)
        {
            if (((VisitData)data).IsVisited(orderByOp))
                return orderByOp;

            var inner = (ISparqlQuery)orderByOp.InnerQuery.Accept(this, data);

            if (inner != orderByOp.InnerQuery)
                orderByOp.ReplaceInnerQuery(orderByOp.InnerQuery, inner);

            ((VisitData)data).Visit(orderByOp);
            return orderByOp.FinalizeAfterTransform();
        }

        public object Visit(DistinctOp distinctOp, object data)
        {
            if (((VisitData)data).IsVisited(distinctOp))
                return distinctOp;

            var inner = (ISparqlQuery)distinctOp.InnerQuery.Accept(this, data);

            if (inner != distinctOp.InnerQuery)
                distinctOp.ReplaceInnerQuery(distinctOp.InnerQuery, inner);

            ((VisitData)data).Visit(distinctOp);
            return distinctOp.FinalizeAfterTransform();
        }

        public object Visit(ReducedOp reducedOp, object data)
        {
            if (((VisitData)data).IsVisited(reducedOp))
                return reducedOp;

            var inner = (ISparqlQuery)reducedOp.InnerQuery.Accept(this, data);

            if (inner != reducedOp.InnerQuery)
                reducedOp.ReplaceInnerQuery(reducedOp.InnerQuery, inner);

            ((VisitData)data).Visit(reducedOp);
            return reducedOp.FinalizeAfterTransform();
        }

        private class VisitData
        {
            private HashSet<ISparqlQuery> visited;

            public VisitData(QueryContext context)
            {
                this.Context = context;
                this.visited = new HashSet<ISparqlQuery>();
            }

            public QueryContext Context { get; private set; }

            public bool IsVisited(ISparqlQuery query)
            {
                return visited.Contains(query);
            }

            public void Visit(ISparqlQuery query)
            {
                visited.Add(query);
            }
        }
    }
}
