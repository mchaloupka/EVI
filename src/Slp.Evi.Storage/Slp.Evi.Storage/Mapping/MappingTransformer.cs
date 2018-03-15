using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Slp.Evi.Storage.Query;
using Slp.Evi.Storage.Sparql.Algebra;
using Slp.Evi.Storage.Sparql.Algebra.Patterns;
using Slp.Evi.Storage.Sparql.PostProcess;
using Slp.Evi.Storage.Sparql.Utils;
using TCode.r2rml4net.Mapping;

namespace Slp.Evi.Storage.Mapping
{
    /// <summary>
    /// Mapping transformer
    /// </summary>
    public class MappingTransformer
        : BaseSparqlTransformer<IQueryContext>, ISparqlPostProcess
    {
        /// <summary>
        /// Processes the specified query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="context">The context.</param>
        public ISparqlQuery Process(ISparqlQuery query, IQueryContext context)
        {
            return TransformSparqlQuery(query, context);
        }

        /// <summary>
        /// The mapping processor
        /// </summary>
        private readonly IMappingProcessor _mappingProcessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="MappingTransformer"/> class.
        /// </summary>
        /// <param name="mappingProcessor">The mapping processor.</param>
        public MappingTransformer(IMappingProcessor mappingProcessor, ILogger<MappingTransformer> logger)
            : base(logger)
        {
            _mappingProcessor = mappingProcessor;
        }

        /// <summary>
        /// Process the <see cref="TriplePattern"/>
        /// </summary>
        /// <param name="triplePattern">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IGraphPattern Transform(TriplePattern triplePattern, IQueryContext data)
        {
            List<RestrictedTriplePattern> patterns = new List<RestrictedTriplePattern>();

            foreach (var tripleMap in _mappingProcessor.Mapping.TriplesMaps)
            {
                var subjectMap = tripleMap.SubjectMap;
                var graphMaps = subjectMap.GraphMaps.ToList();

                foreach (var predicateObjectMap in tripleMap.PredicateObjectMaps)
                {
                    var graphList = new List<IGraphMap>(graphMaps);
                    graphList.AddRange(predicateObjectMap.GraphMaps);

                    foreach (var predicateMap in predicateObjectMap.PredicateMaps)
                    {
                        foreach (var objectMap in predicateObjectMap.ObjectMaps)
                        {
                            ConstrainTriplePattern(triplePattern, patterns, tripleMap, subjectMap, predicateMap, objectMap,
                                graphList);
                        }
                        foreach (var refObjectMap in predicateObjectMap.RefObjectMaps)
                        {
                            ConstrainTriplePattern(triplePattern, patterns, tripleMap, subjectMap, predicateMap, refObjectMap,
                                graphList);
                        }
                    }
                }

                foreach (var classUri in subjectMap.Classes)
                {
                    ConstrainTriplePattern(triplePattern, patterns, tripleMap, subjectMap, classUri, graphMaps);
                }
            }

            return Transform(new UnionPattern(patterns), data);
        }

        /// <summary>
        /// Constrains the triple pattern.
        /// </summary>
        /// <param name="triplePattern">The triple pattern.</param>
        /// <param name="patterns">The patterns.</param>
        /// <param name="tripleMap">The triple map.</param>
        /// <param name="subjectMap">The subject map.</param>
        /// <param name="classUri">The class URI.</param>
        /// <param name="graphMaps">The graph maps.</param>
        private void ConstrainTriplePattern(TriplePattern triplePattern,
            List<RestrictedTriplePattern> patterns, ITriplesMap tripleMap, ISubjectMap subjectMap,
            System.Uri classUri, List<IGraphMap> graphMaps)
        {
            if (graphMaps.Any())
            {
                patterns.AddRange(graphMaps.Select(graphMap =>
                    new RestrictedTriplePattern(triplePattern.SubjectPattern,
                        triplePattern.PredicatePattern, triplePattern.ObjectPattern, tripleMap,
                        subjectMap, new ClassPredicateMap(tripleMap.BaseUri),
                        new ClassObjectMap(tripleMap.BaseUri, classUri), null, graphMap)));
            }
            else
            {
                patterns.Add(
                    new RestrictedTriplePattern(triplePattern.SubjectPattern,
                    triplePattern.PredicatePattern, triplePattern.ObjectPattern, tripleMap,
                    subjectMap, new ClassPredicateMap(tripleMap.BaseUri),
                    new ClassObjectMap(tripleMap.BaseUri, classUri),
                    null, null));
            }
        }

        /// <summary>
        /// Constrains the triple pattern.
        /// </summary>
        /// <param name="triplePattern">The triple pattern.</param>
        /// <param name="patterns">The patterns.</param>
        /// <param name="tripleMap">The triple map.</param>
        /// <param name="subjectMap">The subject map.</param>
        /// <param name="predicateMap">The predicate map.</param>
        /// <param name="refObjectMap">The reference object map.</param>
        /// <param name="graphMaps">The graph maps.</param>
        private void ConstrainTriplePattern(TriplePattern triplePattern,
            List<RestrictedTriplePattern> patterns, ITriplesMap tripleMap, ISubjectMap subjectMap,
            IPredicateMap predicateMap, IRefObjectMap refObjectMap, List<IGraphMap> graphMaps)
        {
            if (graphMaps.Any())
            {
                patterns.AddRange(graphMaps.Select(graphMap =>
                    new RestrictedTriplePattern(triplePattern.SubjectPattern,
                        triplePattern.PredicatePattern, triplePattern.ObjectPattern, tripleMap,
                        subjectMap, predicateMap, null, refObjectMap, graphMap)));
            }
            else
            {
                patterns.Add(
                    new RestrictedTriplePattern(triplePattern.SubjectPattern,
                    triplePattern.PredicatePattern, triplePattern.ObjectPattern, tripleMap,
                    subjectMap, predicateMap, null, refObjectMap, null));
            }
        }

        /// <summary>
        /// Constrains the triple pattern.
        /// </summary>
        /// <param name="triplePattern">The triple pattern.</param>
        /// <param name="patterns">The patterns.</param>
        /// <param name="tripleMap">The triple map.</param>
        /// <param name="subjectMap">The subject map.</param>
        /// <param name="predicateMap">The predicate map.</param>
        /// <param name="objectMap">The object map.</param>
        /// <param name="graphMaps">The graph maps.</param>
        private void ConstrainTriplePattern(TriplePattern triplePattern,
            List<RestrictedTriplePattern> patterns, ITriplesMap tripleMap, ISubjectMap subjectMap,
            IPredicateMap predicateMap, IObjectMap objectMap, List<IGraphMap> graphMaps)
        {
            if (graphMaps.Any())
            {
                patterns.AddRange(graphMaps.Select(graphMap =>
                    new RestrictedTriplePattern(triplePattern.SubjectPattern,
                        triplePattern.PredicatePattern, triplePattern.ObjectPattern, tripleMap,
                        subjectMap, predicateMap, objectMap, null, graphMap)));
            }
            else
            {
                patterns.Add(
                    new RestrictedTriplePattern(triplePattern.SubjectPattern,
                    triplePattern.PredicatePattern, triplePattern.ObjectPattern, tripleMap,
                    subjectMap, predicateMap, objectMap, null, null));
            }
        }
    }
}