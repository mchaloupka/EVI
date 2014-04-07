using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sparql.Algebra;
using Slp.r2rml4net.Storage.Sparql.Algebra.Operator;
using TCode.r2rml4net;
using TCode.r2rml4net.Mapping;
using VDS.RDF;

namespace Slp.r2rml4net.Storage.Mapping
{
    public class MappingProcessor
    {
        private IR2RML mapping;

        public MappingProcessor(IR2RML mapping)
        {
            this.mapping = mapping;
        }

        public IR2RML Mapping
        {
            get { return mapping; }
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

                foreach (var classUri in subjectMap.Classes)
                {
                    ConstrainBgp(bgpOp, union, tripleDef, subjectMap, classUri, graphMaps);
                }
            }

            return union;
        }

        private void ConstrainBgp(BgpOp bgpOp, UnionOp union, ITriplesMap tripleDef, ISubjectMap subjectMap, Uri classUri, IEnumerable<IGraphMap> graphMaps)
        {
            if (graphMaps.Any())
            {
                foreach (var graphMap in graphMaps)
                {
                    var clone = bgpOp.Clone();
                    clone.R2RMLTripleDef = tripleDef;
                    clone.R2RMLSubjectMap = subjectMap;
                    clone.R2RMLPredicateMap = new ClassPredicateMap(tripleDef.BaseUri);
                    clone.R2RMLObjectMap = new ClassObjectMap(tripleDef.BaseUri, classUri);
                    clone.R2RMLGraphMap = graphMap;
                    union.AddToUnion(clone);
                }
            }
            else
            {
                var clone = bgpOp.Clone();
                clone.R2RMLTripleDef = tripleDef;
                clone.R2RMLSubjectMap = subjectMap;
                clone.R2RMLPredicateMap = new ClassPredicateMap(tripleDef.BaseUri);
                clone.R2RMLObjectMap = new ClassObjectMap(tripleDef.BaseUri, classUri);
                union.AddToUnion(clone);
            }
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

        private class ClassPredicateMap : IPredicateMap
        {
            public ClassPredicateMap(Uri baseUri)
            {
                this.BaseUri = baseUri;
                this.URI = UriFactory.Create("http://www.w3.org/1999/02/22-rdf-syntax-ns#type");
            }

            public Uri URI { get; private set; }

            public string ColumnName
            {
                get { throw new NotSupportedException(); }
            }

            public string InverseExpression
            {
                get { throw new NotSupportedException(); }
            }

            public bool IsColumnValued
            {
                get { return false; }
            }

            public bool IsConstantValued
            {
                get { return true; }
            }

            public bool IsTemplateValued
            {
                get { return false; }
            }

            public string Template
            {
                get { return null; }
            }

            public ITermType TermType
            {
                get { return new ClassPredicateMapTermType(); }
            }

            public Uri TermTypeURI
            {
                get { return UriFactory.Create("http://www.w3.org/ns/r2rml#IRI"); }
            }

            public Uri BaseUri { get; private set; }

            public VDS.RDF.INode Node
            {
                get { return null; }
            }

            private class ClassPredicateMapTermType : ITermType
            {
                public bool IsBlankNode
                {
                    get { return false; }
                }

                public bool IsLiteral
                {
                    get { return false; }
                }

                public bool IsURI
                {
                    get { return true; }
                }
            }

        }

        private class ClassObjectMap : IObjectMap
        {
            public ClassObjectMap(Uri baseUri, Uri classUri)
            {
                this.BaseUri = baseUri;
                this.URI = classUri;
            }

            public Uri URI { get; private set; }

            public Uri DataTypeURI
            {
                get { return null; }
            }

            public string Language
            {
                get { return null; }
            }

            public string Literal
            {
                get { return null; }
            }

            public string ColumnName
            {
                get { throw new NotSupportedException(); }
            }

            public string InverseExpression
            {
                get { throw new NotSupportedException(); }
            }

            public bool IsColumnValued
            {
                get { return false; }
            }

            public bool IsConstantValued
            {
                get { return true; }
            }

            public bool IsTemplateValued
            {
                get { return false; }
            }

            public string Template
            {
                get { throw new NotSupportedException(); }
            }

            public ITermType TermType
            {
                get { return new ClassObjectMapTermType(); }
            }

            public Uri TermTypeURI
            {
                get { return UriFactory.Create("http://www.w3.org/ns/r2rml#IRI"); }
            }

            public Uri BaseUri { get; private set; }

            public INode Node
            {
                get { return null; }
            }

            private class ClassObjectMapTermType : ITermType
            {
                public bool IsBlankNode
                {
                    get { return false; }
                }

                public bool IsLiteral
                {
                    get { return false; }
                }

                public bool IsURI
                {
                    get { return true; }
                }
            }
        }


    }
}
