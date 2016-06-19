using System;
using TCode.r2rml4net.Mapping;
using VDS.RDF;

namespace Slp.Evi.Storage.Mapping
{
    /// <summary>
    /// Object map for class
    /// </summary>
    public class ClassObjectMap : IObjectMap
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
        public Uri URI { get; }

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
        public Uri BaseUri { get; }

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