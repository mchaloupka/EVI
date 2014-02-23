using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDS.RDF.Query.Patterns;

namespace Slp.r2rml4net.Storage.Sparql.Algebra
{
    [DebuggerDisplay("BGP")]
    public class BgpOp : ISparqlQueryPart
    {
        private PatternItem predicatePattern;
        private PatternItem objectPattern;
        private PatternItem subjectPattern;

        public BgpOp(PatternItem objectPattern, PatternItem predicatePattern, PatternItem subjectPattern)
        {
            this.objectPattern = objectPattern;
            this.predicatePattern = predicatePattern;
            this.subjectPattern = subjectPattern;
        }

        public IEnumerable<ISparqlQuery> GetInnerQueries()
        {
            yield break;
        }

        public void ReplaceInnerQuery(ISparqlQuery q, ISparqlQuery processed)
        {
            throw new Exception("Should not be called, BgpOp has no subqueries");
        }

        public BgpOp Clone()
        {
            return new BgpOp(objectPattern, predicatePattern, subjectPattern);
        }

        public TCode.r2rml4net.Mapping.IGraphMap R2RMLGraphMap { get; set; }
        public TCode.r2rml4net.Mapping.IObjectMap R2RMLObjectMap { get; set; }
        public TCode.r2rml4net.Mapping.IPredicateMap R2RMLSubjectMap { get; set; }
        public TCode.r2rml4net.Mapping.ITriplesMap R2RMLTripleDef { get; set; }
        public TCode.r2rml4net.Mapping.IRefObjectMap R2RMLRefObjectMap { get; set; }

        public override string ToString()
        {
            return "BGP";
        }
    }
}
