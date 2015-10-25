using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Mapping.Utils;
using Slp.r2rml4net.Storage.Sparql.Algebra;
using Slp.r2rml4net.Storage.Sparql.Algebra.Patterns;
using Slp.r2rml4net.Storage.Sparql.Utils.CodeGeneration;
using Slp.r2rml4net.Storage.Utils;
using TCode.r2rml4net.Extensions;
using TCode.r2rml4net.Mapping;
using VDS.RDF;
using VDS.RDF.Query.Patterns;

namespace Slp.r2rml4net.Storage.Sparql.Optimization.Optimizers
{
    /// <summary>
    /// The triple pattern optimization
    /// </summary>
    public class TriplePatternOptimizer
        : BaseSparqlOptimizer<object>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TriplePatternOptimizer"/> class.
        /// </summary>
        public TriplePatternOptimizer() 
            : base(new TriplePatternOptimizerImplementation())
        { }

        /// <summary>
        /// The implementation class for <see cref="TriplePatternOptimizer"/>
        /// </summary>
        public class TriplePatternOptimizerImplementation
            : BaseSparqlOptimizerImplementation<object>
        {
            /// <summary>
            /// Process the <see cref="RestrictedTriplePattern"/>
            /// </summary>
            /// <param name="toTransform">The instance to process</param>
            /// <param name="data">The passed data</param>
            /// <returns>The transformation result</returns>
            protected override IGraphPattern Transform(RestrictedTriplePattern toTransform, OptimizationContext data)
            {
                if (!CanObjectMatch(toTransform, data)
                    || !CanPredicateMatch(toTransform, data)
                    || !CanSubjectMatch(toTransform, data))
                {
                    return new NotMatchingPattern();
                }
                else
                {
                    return toTransform;
                }
            }

            /// <summary>
            /// Can the subject match the pattern?.
            /// </summary>
            /// <param name="toTransform">To transform.</param>
            /// <param name="data">The data.</param>
            private bool CanSubjectMatch(RestrictedTriplePattern toTransform, OptimizationContext data)
            {
                var pattern = toTransform.SubjectPattern;

                if (pattern is NodeMatchPattern)
                {
                    var nodeMatchPattern = (NodeMatchPattern) pattern;

                    return CanMatch(nodeMatchPattern.Node, toTransform.SubjectMap);
                }
                else
                {
                    return true;
                }
            }

            /// <summary>
            /// Can the predicate match the pattern?.
            /// </summary>
            /// <param name="toTransform">To transform.</param>
            /// <param name="data">The data.</param>
            private bool CanPredicateMatch(RestrictedTriplePattern toTransform, OptimizationContext data)
            {
                var pattern = toTransform.PredicatePattern;

                if (pattern is NodeMatchPattern)
                {
                    var nodeMatchPattern = (NodeMatchPattern)pattern;

                    return CanMatch(nodeMatchPattern.Node, toTransform.PredicateMap);
                }
                else
                {
                    return true;
                }
            }

            /// <summary>
            /// Can the object match the pattern?.
            /// </summary>
            /// <param name="toTransform">To transform.</param>
            /// <param name="data">The data.</param>
            private bool CanObjectMatch(RestrictedTriplePattern toTransform, OptimizationContext data)
            {
                var pattern = toTransform.ObjectPattern;
                ITermMap r2RmlDef;

                if (toTransform.ObjectMap != null)
                {
                    r2RmlDef = toTransform.ObjectMap;
                }
                else if (toTransform.RefObjectMap != null)
                {
                    var parentTriples = toTransform.RefObjectMap.ParentTriplesMap;
                    r2RmlDef = parentTriples.SubjectMap;
                }
                else
                {
                    throw new Exception("R2RMLObjectMap or R2RMLRefObjectMap must be assigned");
                }

                if (pattern is NodeMatchPattern)
                {
                    return CanMatch(((NodeMatchPattern)pattern).Node, r2RmlDef);
                }

                return true;
            }

            /// <summary>
            /// Determines whether the pattern can match the mapping.
            /// </summary>
            /// <param name="node">The match pattern node.</param>
            /// <param name="termMap">The mapping.</param>
            public bool CanMatch(INode node, ITermMap termMap)
            {
                if (termMap.IsConstantValued)
                {
                    if (termMap is IUriValuedTermMap)
                    {
                        var uriValued = (IUriValuedTermMap)termMap;

                        if (node is IUriNode)
                        {
                            var uNode = (IUriNode)node;

                            return uriValued.URI.UriEquals(uNode.Uri);
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else if (termMap is IObjectMap)
                    {
                        var objectMap = (IObjectMap)termMap;

                        if (objectMap.URI != null)
                        {
                            if (node is IUriNode)
                            {
                                var uNode = (IUriNode)node;

                                return objectMap.URI.UriEquals(uNode.Uri);
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else if (objectMap.Literal != null)
                        {
                            if (node.NodeType == NodeType.Uri)
                                return false;
                            else if (node.NodeType == NodeType.Literal)
                            {
                                // NOTE: Better comparison
                                var ln = (ILiteralNode)node;

                                return ln.Value == objectMap.Literal;
                            }
                        }
                        else
                            throw new Exception("IObjectMap must have URI or Literal assigned.");
                    }
                }

                return true;
            }
        }
    }
}
