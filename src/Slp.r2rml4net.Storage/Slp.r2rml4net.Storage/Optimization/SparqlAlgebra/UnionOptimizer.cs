using System.Collections.Generic;
using System.Linq;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sparql.Algebra;
using Slp.r2rml4net.Storage.Sparql.Algebra.Operator;
using TCode.r2rml4net.Mapping;

namespace Slp.r2rml4net.Storage.Optimization.SparqlAlgebra
{
    /// <summary>
    /// The union optimizer
    /// </summary>
    public class UnionOptimizer : ISparqlAlgebraOptimizer, ISparqlQueryVisitor
    {
        /// <summary>
        /// The join optimizer
        /// </summary>
        private readonly JoinOptimizer _joinOptimizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnionOptimizer"/> class.
        /// </summary>
        public UnionOptimizer()
        {
            _joinOptimizer = new JoinOptimizer();
        }

        /// <summary>
        /// Processes the algebra.
        /// </summary>
        /// <param name="algebra">The algebra.</param>
        /// <param name="context">The query context.</param>
        /// <returns>The processed algebra.</returns>
        public ISparqlQuery ProcessAlgebra(ISparqlQuery algebra, QueryContext context)
        {
            return (ISparqlQuery)algebra.Accept(this, new VisitData(context));
        }

        /// <summary>
        /// Visits the specified BGP operator.
        /// </summary>
        /// <param name="bgpOp">The BGP operator.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value.</returns>
        public object Visit(BgpOp bgpOp, object data)
        {
            return bgpOp.FinalizeAfterTransform();
        }

        /// <summary>
        /// Visits the specified join operator.
        /// </summary>
        /// <param name="joinOp">The join operator.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value.</returns>
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

        /// <summary>
        /// Creates the cartesian product.
        /// </summary>
        /// <param name="subQueries">The sub queries.</param>
        /// <param name="subUnions">The sub unions.</param>
        /// <param name="context">The query context.</param>
        /// <returns>The cartesian product.</returns>
        private IEnumerable<IEnumerable<ISparqlQuery>> CreateCartesians(List<ISparqlQuery> subQueries, List<UnionOp> subUnions, QueryContext context)
        {
            var leftCartesian = new CartesianResult();
            bool leftOk = true;

            foreach (var query in subQueries)
            {
                if(query is BgpOp)
                {
                    var bgp = (BgpOp)query;

                    _joinOptimizer.GetBgpInfo(bgp, leftCartesian.Variables, context);

                    if(!_joinOptimizer.ProcessBgp(bgp, leftCartesian.Variables, context))
                    {
                        leftOk = false;
                        break;
                    }
                }

                leftCartesian.Queries.Add(query);
            }

            var currentCartesians = new List<CartesianResult>();

            if (leftOk)
            {
                currentCartesians.Add(leftCartesian);

                var right = subUnions.Select(x => x.GetInnerQueries());

                foreach (var union in right)
                {
                    currentCartesians = ProcessCartesian(currentCartesians, union, context);
                }
            }

            return currentCartesians.Select(x => x.Queries);
        }

        /// <summary>
        /// Processes the current cartesian product.
        /// </summary>
        /// <param name="currentCartesians">The current cartesian product.</param>
        /// <param name="union">The union.</param>
        /// <param name="context">The query context.</param>
        /// <returns>The cartesian product.</returns>
        private List<CartesianResult> ProcessCartesian(List<CartesianResult> currentCartesians, IEnumerable<ISparqlQuery> union, QueryContext context)
        {
            List<CartesianResult> result = new List<CartesianResult>();

            foreach (var cartesian in currentCartesians)
            {
                foreach (var query in union)
                {
                    var bgp = query as BgpOp;

                    if (bgp != null)
                    {
                        if (!_joinOptimizer.ProcessBgp(bgp, cartesian.Variables, context))
                            continue;
                    }

                    var cart = cartesian.Clone();

                    if(bgp != null)
                    {
                        _joinOptimizer.GetBgpInfo(bgp, cart.Variables, context);
                    }

                    cart.Queries.Add(query);
                    result.Add(cart);
                }
            }

            return result;
        }

        /// <summary>
        /// Cartesian result
        /// </summary>
        private class CartesianResult
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="CartesianResult"/> class.
            /// </summary>
            public CartesianResult()
            {
                Variables = new Dictionary<string, List<ITermMap>>();
                Queries = new List<ISparqlQuery>();
            }

            /// <summary>
            /// Clones this instance.
            /// </summary>
            /// <returns>The cloned instance.</returns>
            public CartesianResult Clone()
            {
                var cr = new CartesianResult();

                foreach (var q in Queries)
                {
                    cr.Queries.Add(q);
                }

                foreach (var variable in Variables.Keys)
                {
                    cr.Variables[variable] = new List<ITermMap>();

                    foreach (var termMap in Variables[variable])
                    {
                        cr.Variables[variable].Add(termMap);
                    }
                }

                return cr;
            }

            /// <summary>
            /// Gets the variables mappings.
            /// </summary>
            /// <value>The variables.</value>
            public Dictionary<string, List<ITermMap>> Variables { get; private set; }

            /// <summary>
            /// Gets the queries.
            /// </summary>
            /// <value>The queries.</value>
            public List<ISparqlQuery> Queries { get; private set; }
        }

        /// <summary>
        /// Processes the join child.
        /// </summary>
        /// <param name="subQueries">The sub queries.</param>
        /// <param name="subUnions">The sub unions.</param>
        /// <param name="inner">The inner query.</param>
        /// <param name="oldInner">The old inner query.</param>
        /// <returns><c>true</c> if modified, <c>false</c> otherwise.</returns>
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

        /// <summary>
        /// Visits the specified one empty solution operator.
        /// </summary>
        /// <param name="oneEmptySolutionOp">The one empty solution operator.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value.</returns>
        public object Visit(OneEmptySolutionOp oneEmptySolutionOp, object data)
        {
            return oneEmptySolutionOp.FinalizeAfterTransform();
        }

        /// <summary>
        /// Visits the specified union operator.
        /// </summary>
        /// <param name="unionOp">The union operator.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value.</returns>
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

        /// <summary>
        /// Processes the union child.
        /// </summary>
        /// <param name="newUnion">The new union.</param>
        /// <param name="inner">The inner query.</param>
        /// <param name="oldInner">The old inner query.</param>
        /// <returns><c>true</c> if modified, <c>false</c> otherwise.</returns>
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

        /// <summary>
        /// Visits the specified no solution operator.
        /// </summary>
        /// <param name="noSolutionOp">The no solution operator.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value.</returns>
        public object Visit(NoSolutionOp noSolutionOp, object data)
        {
            return noSolutionOp.FinalizeAfterTransform();
        }

        /// <summary>
        /// Visits the specified select operator.
        /// </summary>
        /// <param name="selectOp">The select operator.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value.</returns>
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

        /// <summary>
        /// Visits the specified slice operator.
        /// </summary>
        /// <param name="sliceOp">The slice operator.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value.</returns>
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

        /// <summary>
        /// Visits the specified order by operator.
        /// </summary>
        /// <param name="orderByOp">The order by operator.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value.</returns>
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

        /// <summary>
        /// Visits the specified distinct operator.
        /// </summary>
        /// <param name="distinctOp">The distinct operator.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value.</returns>
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

        /// <summary>
        /// Visits the specified reduced operator.
        /// </summary>
        /// <param name="reducedOp">The reduced operator.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value.</returns>
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

        /// <summary>
        /// Visits the specified bind operator.
        /// </summary>
        /// <param name="bindOp">The bind operator.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value.</returns>
        public object Visit(BindOp bindOp, object data)
        {
            if (((VisitData)data).IsVisited(bindOp))
                return bindOp;

            var inner = (ISparqlQuery)bindOp.InnerQuery.Accept(this, data);

            if (inner != bindOp.InnerQuery)
                bindOp.ReplaceInnerQuery(bindOp.InnerQuery, inner);

            ((VisitData)data).Visit(bindOp);
            return bindOp.FinalizeAfterTransform();
        }

        /// <summary>
        /// The visit data.
        /// </summary>
        private class VisitData
        {
            /// <summary>
            /// The visited operators
            /// </summary>
            private readonly HashSet<ISparqlQuery> _visited;

            /// <summary>
            /// Initializes a new instance of the <see cref="VisitData"/> class.
            /// </summary>
            /// <param name="context">The query context.</param>
            public VisitData(QueryContext context)
            {
                Context = context;
                _visited = new HashSet<ISparqlQuery>();
            }

            /// <summary>
            /// Gets or sets the query context.
            /// </summary>
            /// <value>The query context.</value>
            public QueryContext Context { get; private set; }

            /// <summary>
            /// Determines whether the specified query is visited.
            /// </summary>
            /// <param name="query">The query.</param>
            /// <returns><c>true</c> if the specified query is visited; otherwise, <c>false</c>.</returns>
            public bool IsVisited(ISparqlQuery query)
            {
                return _visited.Contains(query);
            }

            /// <summary>
            /// Visits the specified query.
            /// </summary>
            /// <param name="query">The query context.</param>
            public void Visit(ISparqlQuery query)
            {
                _visited.Add(query);
            }
        }
    }
}
