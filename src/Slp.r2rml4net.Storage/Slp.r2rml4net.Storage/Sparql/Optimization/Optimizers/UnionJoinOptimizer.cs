using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Sparql.Algebra;
using Slp.r2rml4net.Storage.Sparql.Algebra.Patterns;
using Slp.r2rml4net.Storage.Sparql.Utils.CodeGeneration;
using TCode.r2rml4net.Mapping;

namespace Slp.r2rml4net.Storage.Sparql.Optimization.Optimizers
{
    /// <summary>
    /// The union / join optimization
    /// </summary>
    public class UnionJoinOptimizer
        : BaseSparqlOptimizer<object>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnionJoinOptimizer"/> class.
        /// </summary>
        public UnionJoinOptimizer() 
            : base(new UnionJoinOptimizerImplementation())
        { }

        /// <summary>
        /// The implementation class for <see cref="UnionJoinOptimizer"/>
        /// </summary>
        public class UnionJoinOptimizerImplementation
            : BaseSparqlOptimizerImplementation<object>
        {
            /// <summary>
            /// Process the <see cref="JoinPattern"/>
            /// </summary>
            /// <param name="toTransform">The instance to process</param>
            /// <param name="data">The passed data</param>
            /// <returns>The transformation result</returns>
            protected override IGraphPattern Transform(JoinPattern toTransform, OptimizationContext data)
            {
                List<IGraphPattern> childPatterns = new List<IGraphPattern>();
                List<UnionPattern> childUnionPatterns = new List<UnionPattern>();


                foreach (var pattern in toTransform.JoinedGraphPatterns)
                { 
                    ProcessJoinChild(childPatterns, childUnionPatterns, pattern);
                }

                var cartesianProducts = CreateCartesian(childPatterns, childUnionPatterns,
                    data);

                List<JoinPattern> resultJoinPatterns = cartesianProducts.Select(cartesianProduct => new JoinPattern(cartesianProduct.ToList())).ToList();

                if (resultJoinPatterns.Count == 0)
                {
                    return new NotMatchingPattern();
                }
                else if (resultJoinPatterns.Count == 1)
                {
                    return resultJoinPatterns[0];
                }
                else
                {
                    return new UnionPattern(resultJoinPatterns);
                }
            }

            /// <summary>
            /// Processes children of the <see cref="JoinPattern"/>
            /// </summary>
            /// <param name="childPatterns">All child patterns of a type different from <see cref="UnionPattern"/> and <see cref="JoinPattern"/></param>
            /// <param name="childUnionPatterns">All child patterns of a type <see cref="UnionPattern"/></param>
            /// <param name="joinedGraphPattern">The child to be processed</param>
            private void ProcessJoinChild(List<IGraphPattern> childPatterns, List<UnionPattern> childUnionPatterns, IGraphPattern joinedGraphPattern)
            {
                if (joinedGraphPattern is UnionPattern)
                {
                    childUnionPatterns.Add((UnionPattern)joinedGraphPattern);
                }
                else if (joinedGraphPattern is JoinPattern)
                {
                    foreach (var innerJoinedGraphPattern in ((JoinPattern)joinedGraphPattern).JoinedGraphPatterns)
                    {
                        ProcessJoinChild(childPatterns, childUnionPatterns, innerJoinedGraphPattern);
                    }
                }
                else
                {
                    childPatterns.Add(joinedGraphPattern);
                }
            }

            /// <summary>
            /// Creates the Cartesian product
            /// </summary>
            /// <param name="childPatterns">All child patterns of a type different from <see cref="UnionPattern"/> and <see cref="JoinPattern"/></param>
            /// <param name="childUnionPatterns">All child patterns of a type <see cref="UnionPattern"/></param>
            /// <param name="data">The context</param>
            /// <returns></returns>
            private IEnumerable<IEnumerable<IGraphPattern>> CreateCartesian(List<IGraphPattern> childPatterns, List<UnionPattern> childUnionPatterns, OptimizationContext data)
            {
                var leftCartesian = new CartesianResult();
                bool leftOk = true;

                foreach (var childPattern in childPatterns)
                {
                    if (childPattern is TriplePattern)
                    {
                        var triplePattern = (TriplePattern) childPattern;

                        if (!leftCartesian.VerifyTriplePattern(triplePattern, data))
                        {
                            leftOk = false;
                            break;
                        }

                        leftCartesian.AddTriplePatternInfo(triplePattern, data);
                    }

                    leftCartesian.Queries.Add(childPattern);
                }

                var currentCartesians = new List<CartesianResult>();

                if (leftOk)
                {
                    currentCartesians.Add(leftCartesian);

                    currentCartesians = childUnionPatterns
                        .Aggregate(currentCartesians, 
                        (current, childUnionPattern) => ProcessCartesian(current, childUnionPattern, data));
                }

                return currentCartesians.Select(x => x.Queries);
            }

            /// <summary>
            /// Processes the current Cartesian product
            /// </summary>
            /// <param name="currentCartesians">Current Cartesian products</param>
            /// <param name="childUnionPattern">The <see cref="UnionPattern"/> to process</param>
            /// <param name="data">The context</param>
            /// <returns></returns>
            private List<CartesianResult> ProcessCartesian(List<CartesianResult> currentCartesians, UnionPattern childUnionPattern, OptimizationContext data)
            {
                List<CartesianResult> result = new List<CartesianResult>();

                foreach (var currentCartesian in currentCartesians)
                {
                    foreach (var unionedGraphPattern in childUnionPattern.UnionedGraphPatterns)
                    {
                        var triplePattern = unionedGraphPattern as TriplePattern;
                        if (triplePattern != null)
                        {
                            if (!currentCartesian.VerifyTriplePattern(triplePattern, data))
                            {
                                continue;
                            }
                        }

                        var newCartesian = currentCartesian.Clone();

                        if (triplePattern != null)
                        {
                            newCartesian.AddTriplePatternInfo(triplePattern, data);
                        }

                        newCartesian.Queries.Add(unionedGraphPattern);
                        result.Add(newCartesian);
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
                    Queries = new List<IGraphPattern>();
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
                private Dictionary<string, List<ITermMap>> Variables { get; set; }

                /// <summary>
                /// Gets the queries.
                /// </summary>
                /// <value>The queries.</value>
                public List<IGraphPattern> Queries { get; private set; }

                /// <summary>
                /// Adds the information from the passed <see cref="TriplePattern"/> to the <see cref="CartesianResult"/>
                /// </summary>
                /// <param name="triplePattern">The passed <see cref="TriplePattern"/> to process</param>
                /// <param name="data">The context</param>
                public void AddTriplePatternInfo(TriplePattern triplePattern, OptimizationContext data)
                {
                    // TODO: Missing implementation
                }

                /// <summary>
                /// Verifies whether the <see cref="TriplePattern"/> can be added to the <see cref="CartesianResult"/>
                /// </summary>
                /// <param name="triplePattern">The passed <see cref="TriplePattern"/> to process</param>
                /// <param name="data">The context</param>
                /// <returns>Returns <c>true</c> if the <paramref name="triplePattern"/> can be added to the <see cref="CartesianResult"/>; <c>false</c> otherwise</returns>
                public bool VerifyTriplePattern(TriplePattern triplePattern, OptimizationContext data)
                {
                    // TODO: Missing implementation
                    return true;
                }
            }
        }
    }
}
