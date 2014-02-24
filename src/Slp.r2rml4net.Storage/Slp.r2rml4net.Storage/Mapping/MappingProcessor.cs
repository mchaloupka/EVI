using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Sparql;
using Slp.r2rml4net.Storage.Sparql.Algebra;
using TCode.r2rml4net;
using TCode.r2rml4net.Mapping;

namespace Slp.r2rml4net.Storage.Mapping
{
    public class MappingProcessor
    {
        private IR2RML mapping;

        public MappingProcessor(IR2RML mapping)
        {
            this.mapping = mapping;
        }

        public ISparqlQuery ProcessAlgebra(ISparqlQuery algebra, QueryContext context)
        {
            if (algebra is BgpOp)
            {
                return ProcessBgp((BgpOp)algebra, context);
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

                return algebra;
            }
        }

        private ISparqlQuery ProcessBgp(BgpOp bgpOp, QueryContext context)
        {
            var union = new UnionOp();

            foreach (var tripleDef in this.mapping.TriplesMaps)
            {
                var subjectMap = tripleDef.SubjectMap;
                var graphMaps = subjectMap.GraphMaps;

                foreach (var predicateDef in tripleDef.PredicateObjectMaps)
                {
                    foreach (var predicateMap in predicateDef.PredicateMaps)
                    {
                        foreach (var objectMap in predicateDef.ObjectMaps)
                        {
                            ConstrainBgp(bgpOp, union, tripleDef, subjectMap, predicateMap, objectMap, graphMaps.Union(predicateDef.GraphMaps));
                        }
                        foreach (var refObjectMap in predicateDef.RefObjectMaps)
                        {
                            ConstrainBgp(bgpOp, union, tripleDef, subjectMap, predicateMap, refObjectMap, graphMaps.Union(predicateDef.GraphMaps));
                        }
                    }
                }
            }

            return union;
        }

        private void ConstrainBgp(BgpOp bgpOp, UnionOp union, ITriplesMap tripleDef, ISubjectMap subjectMap, IPredicateMap predicateMap, IRefObjectMap refObjectMap, IEnumerable<IGraphMap> graphMaps)
        {
            if (graphMaps.Any())
            {
                foreach (var graphMap in graphMaps)
                {
                    var clone = bgpOp.Clone();
                    clone.R2RMLTripleDef = tripleDef;
                    clone.R2RMLSubjectMap = subjectMap;
                    clone.R2RMLRefObjectMap = refObjectMap;
                    clone.R2RMLGraphMap = graphMap;
                    clone.R2RMLPredicateMap = predicateMap;
                    union.AddToUnion(clone);
                }
            }
            else
            {
                var clone = bgpOp.Clone();
                clone.R2RMLTripleDef = tripleDef;
                clone.R2RMLSubjectMap = subjectMap;
                clone.R2RMLRefObjectMap = refObjectMap;
                clone.R2RMLPredicateMap = predicateMap;
                union.AddToUnion(clone);
            }
        }

        private void ConstrainBgp(BgpOp bgpOp, UnionOp union, ITriplesMap tripleDef, ISubjectMap subjectMap, IPredicateMap predicateMap, IObjectMap objectMap, IEnumerable<IGraphMap> graphMaps)
        {
            if (graphMaps.Any())
            {
                foreach (var graphMap in graphMaps)
                {
                    var clone = bgpOp.Clone();
                    clone.R2RMLTripleDef = tripleDef;
                    clone.R2RMLSubjectMap = subjectMap;
                    clone.R2RMLObjectMap = objectMap;
                    clone.R2RMLGraphMap = graphMap;
                    clone.R2RMLPredicateMap = predicateMap;
                    union.AddToUnion(clone);
                }
            }
            else
            {
                var clone = bgpOp.Clone();
                clone.R2RMLTripleDef = tripleDef;
                clone.R2RMLSubjectMap = subjectMap;
                clone.R2RMLObjectMap = objectMap;
                clone.R2RMLPredicateMap = predicateMap;
                union.AddToUnion(clone);
            }
        }
    }
}
