using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sparql.Algebra;
using Slp.r2rml4net.Storage.Sparql.Algebra.Operator;
using TCode.r2rml4net.Mapping;
using VDS.RDF;
using VDS.RDF.Query.Patterns;
using Slp.r2rml4net.Storage.Utils;
using Slp.r2rml4net.Storage.Mapping.Utils;

namespace Slp.r2rml4net.Storage.Optimization.SparqlAlgebra
{
    /// <summary>
    /// R2RML mapping optimization
    /// </summary>
    public class R2RMLOptimizer : ISparqlAlgebraOptimizer
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
                var r2rmlDef = (ITermMap)bgpOp.R2RMLPredicateMap;

                return CanMatch(nmp, r2rmlDef);
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
                var r2rmlDef = (ITermMap)bgpOp.R2RMLSubjectMap;

                return CanMatch(nmp, r2rmlDef);
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
            ITermMap r2rmlDef = null;

            if(bgpOp.R2RMLObjectMap != null)
            {
                r2rmlDef = bgpOp.R2RMLObjectMap;
            }
            else if(bgpOp.R2RMLRefObjectMap != null)
            {
                var parentTriples = bgpOp.R2RMLRefObjectMap.GetParentTriplesMap(context.Mapping.Mapping);
                r2rmlDef = parentTriples.SubjectMap;
            }
            else
            {
                throw new Exception("R2RMLObjectMap or R2RMLRefObjectMap must be assigned");
            }
            
            if(pattern is NodeMatchPattern)
            {
                return CanMatch((NodeMatchPattern)pattern, r2rmlDef);
            }

            return true;
        }

        /// <summary>
        /// Determines whether the pattern can match the mapping.
        /// </summary>
        /// <param name="nmp">The pattern.</param>
        /// <param name="r2rmlTerm">The R2RML mapping.</param>
        /// <returns><c>true</c> if the pattern can match the mapping; otherwise, <c>false</c>.</returns>
        /// <exception cref="System.Exception">IObjectMap must have URI or Literal assigned.</exception>
        private bool CanMatch(NodeMatchPattern nmp, ITermMap r2rmlTerm)
        {
            var node = nmp.Node;
            
            if (r2rmlTerm.IsConstantValued)
            {
                if (r2rmlTerm is IUriValuedTermMap)
                {
                    var uriValued = (IUriValuedTermMap)r2rmlTerm;
                    
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
                else if (r2rmlTerm is IObjectMap)
                {
                    var objectMap = (IObjectMap)r2rmlTerm;

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
