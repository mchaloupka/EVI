using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sparql.Algebra;
using Slp.r2rml4net.Storage.Sparql.Algebra.Operator;

namespace Slp.r2rml4net.Storage.Optimization.SparqlAlgebra
{
    public class UnionOptimizer : ISparqlAlgebraOptimizer, ISparqlQueryVisitor
    {
        public ISparqlQuery ProcessAlgebra(ISparqlQuery algebra, QueryContext context)
        {
            return (ISparqlQuery)algebra.Accept(this, new VisitData(context));
        }

        public object Visit(BgpOp bgpOp, object data)
        {
            return bgpOp;
        }

        public object Visit(JoinOp joinOp, object data)
        {
            if (((VisitData)data).IsVisited(joinOp))
                return joinOp;

            List<ISparqlQuery> subQueries = new List<ISparqlQuery>();
            List<ISparqlQuery> subUnions = new List<ISparqlQuery>();

            bool changed = false;

            foreach (var oldInner in joinOp.GetInnerQueries())
            {
                var inner = (ISparqlQuery)oldInner.Accept(this, data);

                changed = ProcessJoinChild(subQueries, subUnions, inner, oldInner) || changed;
            }

            if (!changed)
            {
                ((VisitData)data).Visit(joinOp);

                return joinOp;
            }

            IEnumerable<IEnumerable<ISparqlQuery>> cartesian = new List<List<ISparqlQuery>>() { subQueries };

            foreach (var subUnion in subUnions)
            {
                cartesian = ProcessJoinCartesian(cartesian, (UnionOp)subUnion);
            }

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

            if (resultJoins.Count == 1)
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

        private IEnumerable<IEnumerable<ISparqlQuery>> ProcessJoinCartesian(IEnumerable<IEnumerable<ISparqlQuery>> cartesian, UnionOp unionOp)
        {
            foreach (var subCar in cartesian)
            {
                foreach (var subUnion in unionOp.GetInnerQueries())
                {
                    yield return ProcessJoinCartesian(subCar, subUnion);
                }
            }
        }

        private IEnumerable<ISparqlQuery> ProcessJoinCartesian(IEnumerable<ISparqlQuery> subCar, ISparqlQuery subUnion)
        {
            foreach (var item in subCar)
            {
                yield return item;
            }

            yield return subUnion;
        }

        private static bool ProcessJoinChild(List<ISparqlQuery> subQueries, List<ISparqlQuery> subUnions, ISparqlQuery inner, ISparqlQuery oldInner)
        {
            if (inner is UnionOp)
            {
                subUnions.Add(inner);

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
            return oneEmptySolutionOp;
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

                changed = ProcessUnionChild(newUnion, inner, oldInner) || changed;
            }

            if (changed)
                return newUnion.Accept(this, data);
            else
            {
                ((VisitData)data).Visit(unionOp);
                return unionOp;
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

                return inner != oldInner;
            }
        }

        public object Visit(NoSolutionOp noSolutionOp, object data)
        {
            return noSolutionOp;
        }

        public object Visit(SelectOp selectOp, object data)
        {
            var vd = (VisitData)data;

            if (vd.IsVisited(selectOp))
                return selectOp;

            var inner = (ISparqlQuery)selectOp.InnerQuery.Accept(this, data);

            if (inner != selectOp.InnerQuery)
                selectOp.ReplaceInnerQuery(selectOp.InnerQuery, inner);

            if(inner is UnionOp && IsProjectionOnly(selectOp))
            {
                UnionOp union = new UnionOp();

                foreach (var source in ((UnionOp)inner).GetInnerQueries())
                {
                    var projectedSource = CreateProjection(selectOp, source, vd);
                    union.AddToUnion(projectedSource);
                }

                vd.Visit(union);
                return union;
            }
            else
            {
                vd.Visit(selectOp);
                return selectOp;
            }
        }

        private SelectOp CreateProjection(SelectOp selectOp, ISparqlQuery source, VisitData vd)
        {
            if (selectOp.IsSelectAll)
                return new SelectOp(source);
            else
                return new SelectOp(source, selectOp.Variables);
        }

        private bool IsProjectionOnly(SelectOp selectOp)
        {
            if (selectOp.Variables.Where(x => x.IsAggregate).Any())
                return false;
            else
                return true;
        }

        public object Visit(SliceOp sliceOp, object data)
        {
            if (((VisitData)data).IsVisited(sliceOp))
                return sliceOp;

            var inner = (ISparqlQuery)sliceOp.InnerQuery.Accept(this, data);

            if (inner != sliceOp.InnerQuery)
                sliceOp.ReplaceInnerQuery(sliceOp.InnerQuery, inner);

            ((VisitData)data).Visit(sliceOp);
            return sliceOp;
        }

        public object Visit(OrderByOp orderByOp, object data)
        {
            if (((VisitData)data).IsVisited(orderByOp))
                return orderByOp;

            var inner = (ISparqlQuery)orderByOp.InnerQuery.Accept(this, data);

            if (inner != orderByOp.InnerQuery)
                orderByOp.ReplaceInnerQuery(orderByOp.InnerQuery, inner);

            ((VisitData)data).Visit(orderByOp);
            return orderByOp;
        }

        public object Visit(DistinctOp distinctOp, object data)
        {
            if (((VisitData)data).IsVisited(distinctOp))
                return distinctOp;

            var inner = (ISparqlQuery)distinctOp.InnerQuery.Accept(this, data);

            if (inner != distinctOp.InnerQuery)
                distinctOp.ReplaceInnerQuery(distinctOp.InnerQuery, inner);

            ((VisitData)data).Visit(distinctOp);
            return distinctOp;
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
