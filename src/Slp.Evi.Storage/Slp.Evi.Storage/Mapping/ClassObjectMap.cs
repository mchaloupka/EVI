using System;
using Slp.Evi.Storage.Mapping.Representation;
using Slp.Evi.Storage.Utils;
using TCode.r2rml4net.Mapping;
using VDS.RDF;

namespace Slp.Evi.Storage.Mapping
{
    /// <summary>
    /// Object map for class
    /// </summary>
    public class ClassObjectMap
        : IObjectMapping
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClassObjectMap"/> class.
        /// </summary>
        /// <param name="classUri">The class URI.</param>
        public ClassObjectMap(Uri classUri)
        {
            // TODO: Implement this class
        }

        /// <inheritdoc />
        public bool IsConstantValued { get; }

        /// <inheritdoc />
        public bool IsColumnValued { get; }

        /// <inheritdoc />
        public bool IsTemplateValued { get; }

        /// <inheritdoc />
        public string ColumnName { get; }

        /// <inheritdoc />
        public string Template { get; }

        /// <inheritdoc />
        public Uri BaseUri { get; }

        /// <inheritdoc />
        public ITriplesMapping TriplesMap { get; }

        /// <inheritdoc />
        public ITermTypeInformation TermType { get; }

        /// <inheritdoc />
        public ParsedLiteralParts Literal { get; }
    }
}