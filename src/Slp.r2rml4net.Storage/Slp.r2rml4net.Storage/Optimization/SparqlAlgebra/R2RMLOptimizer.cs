using System;
using System.Linq;
using Slp.r2rml4net.Storage.Mapping.Utils;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sparql.Algebra;
using Slp.r2rml4net.Storage.Sparql.Algebra.Operator;
using Slp.r2rml4net.Storage.Utils;
using TCode.r2rml4net.Mapping;
using VDS.RDF;
using VDS.RDF.Query.Patterns;

namespace Slp.r2rml4net.Storage.Optimization.SparqlAlgebra
{
    /// <summary>
    /// R2RML mapping optimization
    /// </summary>
    public class R2RmlOptimizer : ISparqlAlgebraOptimizer
    {
        /// <summary>
        /// Processes the algebra.
        /// </summary>
        /// <param name="algebra">The algebra.</param>
        /// <param name="context">The query context.</param>
        /// <returns>The processed algebra.</returns>
        public ISparqlQuery ProcessAlgebra(ISparqlQuery algebra, QueryContext context)
        {
            if (algebra is BgpOp)
            {
                return ProcessBgp((BgpOp)algebra, context).FinalizeAfterTransform();
            }
            else
            {
                var innerQueries = algebra.GetInnerQueries().ToList();

                foreach (var query in innerQueries)
                {
                    var processed = ProcessAlgebra(query, context);

                    if (processed != query)
                    {
                        algebra.ReplaceInnerQuery(query, processed);
                    }
                }

                return algebra.FinalizeAfterTransform();
            }
        }

        /// <summary>
        /// Processes the BGP.
        /// </summary>
        /// <param name="bgpOp">The BGP op.</param>
        /// <param name="context">The context.</param>
        /// <returns>The processed algebra.</returns>
        private ISparqlQuery ProcessBgp(BgpOp bgpOp, QueryContext context)
        {
            if (!CanObjectMatch(bgpOp, context))
                return new NoSolutionOp();

            if (!CanPredicateMatch(bgpOp, context))
                return new NoSolutionOp();

            if (!CanSubjectMatch(bgpOp, context))
                return new NoSolutionOp();

            return bgpOp;
        }

        /// <summary>
        /// Can the predicate match.
        /// </summary>
        /// <param name="bgpOp">The BGP operator.</param>
        /// <param name="context">The query context.</param>
        /// <returns><c>true</c> if it can match the specified BGP operator; otherwise, <c>false</c>.</returns>
        private bool CanPredicateMatch(BgpOp bgpOp, QueryContext context)
        {
            var pattern = bgpOp.PredicatePattern;

            if (pattern is NodeMatchPattern)
            {
                var nmp = (NodeMatchPattern)pattern;
                var r2RmlDef = (ITermMap)bgpOp.R2RmlPredicateMap;

                return CanMatch(nmp, r2RmlDef);
            }

            return true;
        }

        /// <summary>
        /// Can the subject match.
        /// </summary>
        /// <param name="bgpOp">The BGP operator.</param>
        /// <param name="context">The query context.</param>
        /// <returns><c>true</c> if it can match the specified BGP operator; otherwise, <c>false</c>.</returns>
        private bool CanSubjectMatch(BgpOp bgpOp, QueryContext context)
        {
            var pattern = bgpOp.SubjectPattern;

            if(pattern is NodeMatchPattern)
            {
                var nmp = (NodeMatchPattern)pattern;
                var r2RmlDef = (ITermMap)bgpOp.R2RmlSubjectMap;

                return CanMatch(nmp, r2RmlDef);
            }

            return true;
        }

        /// <summary>
        /// Can the object match.
        /// </summary>
        /// <param name="bgpOp">The BGP operator.</param>
        /// <param name="context">The query context.</param>
        /// <returns><c>true</c> if it can match the specified BGP operator; otherwise, <c>false</c>.</returns>
        /// <exception cref="System.Exception">R2RMLObjectMap or R2RMLRefObjectMap must be assigned</exception>
        private bool CanObjectMatch(BgpOp bgpOp, QueryContext context)
        {
            var pattern = bgpOp.ObjectPattern;
            ITermMap r2RmlDef;

            if(bgpOp.R2RmlObjectMap != null)
            {
                r2RmlDef = bgpOp.R2RmlObjectMap;
            }
            else if(bgpOp.R2RmlRefObjectMap != null)
            {
                var parentTriples = bgpOp.R2RmlRefObjectMap.GetParentTriplesMap(context.Mapping.Mapping);
                r2RmlDef = parentTriples.SubjectMap;
            }
            else
            {
                throw new Exception("R2RMLObjectMap or R2RMLRefObjectMap must be assigned");
            }
            
            if(pattern is NodeMatchPattern)
            {
                return CanMatch((NodeMatchPattern)pattern, r2RmlDef);
            }

            return true;
        }

        /// <summary>
        /// Determines whether the pattern can match the mapping.
        /// </summary>
        /// <param name="nmp">The pattern.</param>
        /// <param name="r2RmlTerm">The R2RML mapping.</param>
        /// <returns><c>true</c> if the pattern can match the mapping; otherwise, <c>false</c>.</returns>
        /// <exception cref="System.Exception">IObjectMap must have URI or Literal assigned.</exception>
        private bool CanMatch(NodeMatchPattern nmp, ITermMap r2RmlTerm)
        {
            var node = nmp.Node;
            
            if (r2RmlTerm.IsConstantValued)
            {
                if (r2RmlTerm is IUriValuedTermMap)
                {
                    var uriValued = (IUriValuedTermMap)r2RmlTerm;
                    
                    if (node is UriNode)
                    {
                        var uNode = (UriNode)node;

                        return uriValued.URI.UriEquals(uNode.Uri);
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (r2RmlTerm is IObjectMap)
                {
                    var objectMap = (IObjectMap)r2RmlTerm;

                    if (objectMap.URI != null)
                    {
                        if(node is UriNode)
                        {
                            var uNode = (UriNode)node;

                            return objectMap.URI.UriEquals(uNode.Uri);
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else if (objectMap.Literal != null)
                    {
                        if (nmp.Node.NodeType == NodeType.Uri)
                            return false;
                        else if(nmp.Node.NodeType == NodeType.Literal)
                        {
                            // NOTE: Better comparison
                            var ln = (LiteralNode)nmp.Node;
                            var literal = objectMap.Literal;

                            return ln.ToString() == literal;
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
