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
            // TODO: Optimize joins
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
            // TODO: Implement this
            return true;
        }

        private bool CanObjectMatch(BgpOp bgpOp, QueryContext context)
        {
            // TODO: Implement this
            return true;
        }

        private bool CanMatch(NodeMatchPattern nmp, ITermMap r2rmlTerm)
        {
            var node = nmp.Node;
            // TODO: Has node uri?
            // http://dotnetrdf.org/API/dotNetRDF~VDS.RDF.INode.html

            if (r2rmlTerm.IsConstantValued)
            {
                if (r2rmlTerm is IUriValuedTermMap)
                {
                    var uriValued = (IUriValuedTermMap)r2rmlTerm;
                    
                    if (node is UriNode)
                    {
                        var uNode = (UriNode)node;

                        return UriEquals(uriValued.URI, uNode.Uri);
                    }
                }
                else if (r2rmlTerm is IObjectMap)
                {
                    // TODO: Compare literal terms or uri
                }
            }
            // TODO: Template compare

            return true;
        }

        private static bool UriEquals(Uri first, Uri second)
        {
            return first.Equals(second) && string.Equals(first.Fragment, second.Fragment);
        }
    }
}
