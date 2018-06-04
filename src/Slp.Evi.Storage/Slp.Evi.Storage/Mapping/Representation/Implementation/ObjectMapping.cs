using System;
using Slp.Evi.Storage.Utils;
using TCode.r2rml4net.Mapping;

namespace Slp.Evi.Storage.Mapping.Representation.Implementation
{
    /// <summary>
    /// Implementation of <see cref="IObjectMapping"/>.
    /// </summary>
    public class ObjectMapping
        : TermMapping, IObjectMapping
    {
        /// <summary>
        /// Creates an instance of <see cref="ObjectMapping"/>.
        /// </summary>
        protected ObjectMapping() { }

        /// <summary>
        /// Creates an instance of <see cref="IObjectMapping"/> from <see cref="IObjectMap"/>.
        /// </summary>
        public static IObjectMapping Create(IObjectMap objectMap, TriplesMapping tr, RepresentationCreationContext creationContext)
        {
            var res = new ObjectMapping();
            Fill(res, objectMap, tr, creationContext);

            if (objectMap.IsConstantValued && res.Iri == null)
            {
                res.Literal = objectMap.Parsed();
            }

            res.DataTypeIri = objectMap.DataTypeURI;
            res.Language = objectMap.Language;
            return res;
        }

        /// <inheritdoc />
        public ParsedLiteralParts Literal { get; private set; }

        /// <inheritdoc />
        public Uri DataTypeIri { get; private set; }

        /// <inheritdoc />
        public string Language { get; private set; }
    }
}