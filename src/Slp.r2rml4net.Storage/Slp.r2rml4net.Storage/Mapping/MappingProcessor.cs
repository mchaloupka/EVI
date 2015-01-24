using System;
using System.Collections.Generic;
using System.Linq;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sparql.Algebra;
using Slp.r2rml4net.Storage.Sparql.Algebra.Operator;
using TCode.r2rml4net;
using TCode.r2rml4net.Mapping;
using VDS.RDF;

namespace Slp.r2rml4net.Storage.Mapping
{
    /// <summary>
    /// Processor for R2RML mapping
    /// </summary>
    public class MappingProcessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MappingProcessor"/> class.
        /// </summary>
        /// <param name="mapping">The R2RML mapping.</param>
        public MappingProcessor(IR2RML mapping)
        {
            Mapping = mapping;
            Cache = new R2RmlCache();
        }

        /// <summary>
        /// Gets the R2RML mapping.
        /// </summary>
        /// <value>The R2RML mapping.</value>
        public IR2RML Mapping { get; private set; }

        /// <summary>
        /// Gets the R2RML cache.
        /// </summary>
        /// <value>The R2RML cache.</value>
        public R2RmlCache Cache { get; private set; }

        /// <summary>
        /// Processes the SPARQL algebra.
        /// </summary>
        /// <param name="algebra">The SPARQL algebra.</param>
        /// <param name="context">The query context.</param>
        /// <returns>The processed SPARQL algebra.</returns>
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

        /// <summary>
        /// Processes the basic graph pattern.
        /// </summary>
        /// <param name="bgpOp">The basic graph pattern.</param>
        /// <param name="context">The query context.</param>
        /// <returns>The processed SPARQL algebra.</returns>
        private ISparqlQuery ProcessBgp(BgpOp bgpOp, QueryContext context)
        {
            var union = new UnionOp();

            foreach (var tripleDef in Mapping.TriplesMaps)
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

        /// <summary>
        /// Constrains the BGP using the class in R2RML mapping.
        /// </summary>
        /// <param name="bgpOp">The BGP operator.</param>
        /// <param name="union">The union operator to contain the result.</param>
        /// <param name="tripleDef">The triple definition.</param>
        /// <param name="subjectMap">The subject map.</param>
        /// <param name="classUri">The class URI.</param>
        /// <param name="graphMaps">The graph maps.</param>
        private void ConstrainBgp(BgpOp bgpOp, UnionOp union, ITriplesMap tripleDef, ISubjectMap subjectMap, Uri classUri, IEnumerable<IGraphMap> graphMaps)
        {
            if (graphMaps.Any())
            {
                foreach (var graphMap in graphMaps)
                {
                    var clone = bgpOp.Clone();
                    clone.R2RmlTripleDef = tripleDef;
                    clone.R2RmlSubjectMap = subjectMap;
                    clone.R2RmlPredicateMap = new ClassPredicateMap(tripleDef.BaseUri);
                    clone.R2RmlObjectMap = new ClassObjectMap(tripleDef.BaseUri, classUri);
                    clone.R2RmlGraphMap = graphMap;
                    union.AddToUnion(clone);
                }
            }
            else
            {
                var clone = bgpOp.Clone();
                clone.R2RmlTripleDef = tripleDef;
                clone.R2RmlSubjectMap = subjectMap;
                clone.R2RmlPredicateMap = new ClassPredicateMap(tripleDef.BaseUri);
                clone.R2RmlObjectMap = new ClassObjectMap(tripleDef.BaseUri, classUri);
                union.AddToUnion(clone);
            }
        }

        /// <summary>
        /// Constrains the BGP using the reference object map.
        /// </summary>
        /// <param name="bgpOp">The BGP operator.</param>
        /// <param name="union">The union operator to contain the result.</param>
        /// <param name="tripleDef">The triple definition.</param>
        /// <param name="subjectMap">The subject map.</param>
        /// <param name="predicateMap">The predicate map.</param>
        /// <param name="refObjectMap">The reference object map.</param>
        /// <param name="graphMaps">The graph maps.</param>
        private void ConstrainBgp(BgpOp bgpOp, UnionOp union, ITriplesMap tripleDef, ISubjectMap subjectMap, IPredicateMap predicateMap, IRefObjectMap refObjectMap, IEnumerable<IGraphMap> graphMaps)
        {
            if (graphMaps.Any())
            {
                foreach (var graphMap in graphMaps)
                {
                    var clone = bgpOp.Clone();
                    clone.R2RmlTripleDef = tripleDef;
                    clone.R2RmlSubjectMap = subjectMap;
                    clone.R2RmlRefObjectMap = refObjectMap;
                    clone.R2RmlGraphMap = graphMap;
                    clone.R2RmlPredicateMap = predicateMap;
                    union.AddToUnion(clone);
                }
            }
            else
            {
                var clone = bgpOp.Clone();
                clone.R2RmlTripleDef = tripleDef;
                clone.R2RmlSubjectMap = subjectMap;
                clone.R2RmlRefObjectMap = refObjectMap;
                clone.R2RmlPredicateMap = predicateMap;
                union.AddToUnion(clone);
            }
        }

        /// <summary>
        /// Constrains the BGP using the object map.
        /// </summary>
        /// <param name="bgpOp">The BGP operator.</param>
        /// <param name="union">The union operator to contain the result.</param>
        /// <param name="tripleDef">The triple definition.</param>
        /// <param name="subjectMap">The subject map.</param>
        /// <param name="predicateMap">The predicate map.</param>
        /// <param name="objectMap">The object map.</param>
        /// <param name="graphMaps">The graph maps.</param>
        private void ConstrainBgp(BgpOp bgpOp, UnionOp union, ITriplesMap tripleDef, ISubjectMap subjectMap, IPredicateMap predicateMap, IObjectMap objectMap, IEnumerable<IGraphMap> graphMaps)
        {
            if (graphMaps.Any())
            {
                foreach (var graphMap in graphMaps)
                {
                    var clone = bgpOp.Clone();
                    clone.R2RmlTripleDef = tripleDef;
                    clone.R2RmlSubjectMap = subjectMap;
                    clone.R2RmlObjectMap = objectMap;
                    clone.R2RmlGraphMap = graphMap;
                    clone.R2RmlPredicateMap = predicateMap;
                    union.AddToUnion(clone);
                }
            }
            else
            {
                var clone = bgpOp.Clone();
                clone.R2RmlTripleDef = tripleDef;
                clone.R2RmlSubjectMap = subjectMap;
                clone.R2RmlObjectMap = objectMap;
                clone.R2RmlPredicateMap = predicateMap;
                union.AddToUnion(clone);
            }
        }

        /// <summary>
        /// Predicate map for class
        /// </summary>
        private class ClassPredicateMap : IPredicateMap
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ClassPredicateMap"/> class.
            /// </summary>
            /// <param name="baseUri">The base URI.</param>
            public ClassPredicateMap(Uri baseUri)
            {
                BaseUri = baseUri;
                URI = UriFactory.Create("http://www.w3.org/1999/02/22-rdf-syntax-ns#type");
            }

            /// <summary>
            /// Get the URI
            /// </summary>
            /// <value>The URI.</value>
            public Uri URI { get; private set; }

            /// <summary>
            /// Gets column or null if not set
            /// </summary>
            /// <value>The name of the column.</value>
            /// <exception cref="System.NotSupportedException"></exception>
            public string ColumnName
            {
                get { throw new NotSupportedException(); }
            }

            /// <summary>
            /// Gets the inverse expression associated with this <see cref="T:TCode.r2rml4net.Mapping.ITermMap" /> or null if not set
            /// </summary>
            /// <value>The inverse expression.</value>
            /// <exception cref="System.NotSupportedException"></exception>
            /// <remarks>See http://www.w3.org/TR/r2rml/#inverse</remarks>
            public string InverseExpression
            {
                get { throw new NotSupportedException(); }
            }

            /// <summary>
            /// Gets value indicating whether <a href="http://www.w3.org/TR/r2rml/#term-map">term map</a> is <a href="http://www.w3.org/TR/r2rml/#from-column">column valued</a>
            /// </summary>
            /// <value><c>true</c> if this instance is column valued; otherwise, <c>false</c>.</value>
            public bool IsColumnValued
            {
                get { return false; }
            }

            /// <summary>
            /// Gets value indicating whether <a href="http://www.w3.org/TR/r2rml/#term-map">term map</a> is <a href="http://www.w3.org/TR/r2rml/#constant">constant valued</a>
            /// </summary>
            /// <value><c>true</c> if this instance is constant valued; otherwise, <c>false</c>.</value>
            public bool IsConstantValued
            {
                get { return true; }
            }

            /// <summary>
            /// Gets value indicating whether <a href="http://www.w3.org/TR/r2rml/#term-map">term map</a> is <a href="http://www.w3.org/TR/r2rml/#from-template">template valued</a>
            /// </summary>
            /// <value><c>true</c> if this instance is template valued; otherwise, <c>false</c>.</value>
            public bool IsTemplateValued
            {
                get { return false; }
            }

            /// <summary>
            /// Gets template or null if absent
            /// </summary>
            /// <value>The template.</value>
            public string Template
            {
                get { return null; }
            }

            /// <summary>
            /// Gets <a href="http://www.w3.org/TR/r2rml/#term-map">term map's</a><a href="http://www.w3.org/TR/r2rml/#termtype">term type</a>
            /// </summary>
            /// <value>The type of the term.</value>
            public ITermType TermType
            {
                get { return new ClassPredicateMapTermType(); }
            }

            /// <summary>
            /// Returns term type set with configuration
            /// or a default value
            /// </summary>
            /// <value>The term type URI.</value>
            /// <remarks>Default value is described on http://www.w3.org/TR/r2rml/#dfn-term-type</remarks>
            public Uri TermTypeURI
            {
                get { return UriFactory.Create("http://www.w3.org/ns/r2rml#IRI"); }
            }

            /// <summary>
            /// Base mapping URI. It will be used to resolve relative values when generating terms
            /// </summary>
            /// <value>The base URI.</value>
            public Uri BaseUri { get; private set; }

            /// <summary>
            /// The node representing this <see cref="T:TCode.r2rml4net.Mapping.IMapBase" />
            /// </summary>
            /// <value>The node.</value>
            public INode Node
            {
                get { return null; }
            }

            /// <summary>
            /// Term type for class predicate term
            /// </summary>
            private class ClassPredicateMapTermType : ITermType
            {
                /// <summary>
                /// Gets value indicating whether the term map's term type is rr:BlankNode
                /// </summary>
                /// <value><c>true</c> if this instance is blank node; otherwise, <c>false</c>.</value>
                public bool IsBlankNode
                {
                    get { return false; }
                }

                /// <summary>
                /// Gets value indicating whether the term map's term type is rr:Literal
                /// </summary>
                /// <value><c>true</c> if this instance is literal; otherwise, <c>false</c>.</value>
                public bool IsLiteral
                {
                    get { return false; }
                }

                /// <summary>
                /// Gets value indicating whether the term map's term type is rr:IRI
                /// </summary>
                /// <value><c>true</c> if this instance is URI; otherwise, <c>false</c>.</value>
                public bool IsURI
                {
                    get { return true; }
                }
            }

        }

        /// <summary>
        /// Object map for class
        /// </summary>
        private class ClassObjectMap : IObjectMap
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ClassObjectMap"/> class.
            /// </summary>
            /// <param name="baseUri">The base URI.</param>
            /// <param name="classUri">The class URI.</param>
            public ClassObjectMap(Uri baseUri, Uri classUri)
            {
                BaseUri = baseUri;
                URI = classUri;
            }

            /// <summary>
            /// Gets constant object URI or null if absent
            /// </summary>
            /// <value>The URI.</value>
            public Uri URI { get; private set; }

            /// <summary>
            /// Gets the datatype URI of the RDF term generated from this term map
            /// </summary>
            /// <value>The data type URI.</value>
            public Uri DataTypeURI
            {
                get { return null; }
            }

            /// <summary>
            /// Gets the language tag of the RDF term generated from this term map
            /// </summary>
            /// <value>The language.</value>
            public string Language
            {
                get { return null; }
            }

            /// <summary>
            /// Gets the literal value of the RDF term generated from this term map
            /// </summary>
            /// <value>The literal.</value>
            public string Literal
            {
                get { return null; }
            }

            /// <summary>
            /// Gets column or null if not set
            /// </summary>
            /// <value>The name of the column.</value>
            /// <exception cref="System.NotSupportedException"></exception>
            public string ColumnName
            {
                get { throw new NotSupportedException(); }
            }

            /// <summary>
            /// Gets the inverse expression associated with this <see cref="T:TCode.r2rml4net.Mapping.ITermMap" /> or null if not set
            /// </summary>
            /// <value>The inverse expression.</value>
            /// <exception cref="System.NotSupportedException"></exception>
            /// <remarks>See http://www.w3.org/TR/r2rml/#inverse</remarks>
            public string InverseExpression
            {
                get { throw new NotSupportedException(); }
            }

            /// <summary>
            /// Gets value indicating whether <a href="http://www.w3.org/TR/r2rml/#term-map">term map</a> is <a href="http://www.w3.org/TR/r2rml/#from-column">column valued</a>
            /// </summary>
            /// <value><c>true</c> if this instance is column valued; otherwise, <c>false</c>.</value>
            public bool IsColumnValued
            {
                get { return false; }
            }

            /// <summary>
            /// Gets value indicating whether <a href="http://www.w3.org/TR/r2rml/#term-map">term map</a> is <a href="http://www.w3.org/TR/r2rml/#constant">constant valued</a>
            /// </summary>
            /// <value><c>true</c> if this instance is constant valued; otherwise, <c>false</c>.</value>
            public bool IsConstantValued
            {
                get { return true; }
            }

            /// <summary>
            /// Gets value indicating whether <a href="http://www.w3.org/TR/r2rml/#term-map">term map</a> is <a href="http://www.w3.org/TR/r2rml/#from-template">template valued</a>
            /// </summary>
            /// <value><c>true</c> if this instance is template valued; otherwise, <c>false</c>.</value>
            public bool IsTemplateValued
            {
                get { return false; }
            }

            /// <summary>
            /// Gets template or null if absent
            /// </summary>
            /// <value>The template.</value>
            /// <exception cref="System.NotSupportedException"></exception>
            public string Template
            {
                get { throw new NotSupportedException(); }
            }

            /// <summary>
            /// Gets <a href="http://www.w3.org/TR/r2rml/#term-map">term map's</a><a href="http://www.w3.org/TR/r2rml/#termtype">term type</a>
            /// </summary>
            /// <value>The type of the term.</value>
            public ITermType TermType
            {
                get { return new ClassObjectMapTermType(); }
            }

            /// <summary>
            /// Returns term type set with configuration
            /// or a default value
            /// </summary>
            /// <value>The term type URI.</value>
            /// <remarks>Default value is described on http://www.w3.org/TR/r2rml/#dfn-term-type</remarks>
            public Uri TermTypeURI
            {
                get { return UriFactory.Create("http://www.w3.org/ns/r2rml#IRI"); }
            }

            /// <summary>
            /// Base mapping URI. It will be used to resolve relative values when generating terms
            /// </summary>
            /// <value>The base URI.</value>
            public Uri BaseUri { get; private set; }

            /// <summary>
            /// The node representing this <see cref="T:TCode.r2rml4net.Mapping.IMapBase" />
            /// </summary>
            /// <value>The node.</value>
            public INode Node
            {
                get { return null; }
            }

            /// <summary>
            /// Term type for class object map
            /// </summary>
            private class ClassObjectMapTermType : ITermType
            {
                /// <summary>
                /// Gets value indicating whether the term map's term type is rr:BlankNode
                /// </summary>
                /// <value><c>true</c> if this instance is blank node; otherwise, <c>false</c>.</value>
                public bool IsBlankNode
                {
                    get { return false; }
                }

                /// <summary>
                /// Gets value indicating whether the term map's term type is rr:Literal
                /// </summary>
                /// <value><c>true</c> if this instance is literal; otherwise, <c>false</c>.</value>
                public bool IsLiteral
                {
                    get { return false; }
                }

                /// <summary>
                /// Gets value indicating whether the term map's term type is rr:IRI
                /// </summary>
                /// <value><c>true</c> if this instance is URI; otherwise, <c>false</c>.</value>
                public bool IsURI
                {
                    get { return true; }
                }
            }
        }
    }
}
