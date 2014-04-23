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

namespace Slp.r2rml4net.Storage.Optimization.SparqlAlgebra
{
    public class R2RMLOptimizer : ISparqlAlgebraOptimizer
    {
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
                var parentTriples = GetParentTriplesMap(context, bgpOp.R2RMLRefObjectMap);
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

        private ITriplesMap GetParentTriplesMap(QueryContext context, IRefObjectMap refObjectPatern)
        {
            // TODO: Remove this method as soon as the reference will be public

            var subjectMap = refObjectPatern.SubjectMap;

            foreach (var tripleMap in context.Mapping.Mapping.TriplesMaps)
            {
                if (tripleMap.SubjectMap == subjectMap)
                    return tripleMap;
            }

            throw new Exception("Triples map not found");
        }

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
                        // TODO: Static literal comparison
                    }
                    else
                        throw new Exception("IObjectMap must have URI or Literal assigned.");
                }
            }

            return true;
        }
    }
}
