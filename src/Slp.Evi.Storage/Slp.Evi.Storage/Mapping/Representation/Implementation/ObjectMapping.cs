using System;
using Slp.Evi.Storage.Utils;
using TCode.r2rml4net.Mapping;

namespace Slp.Evi.Storage.Mapping.Representation.Implementation
{
    public class ObjectMapping
        : TermMapping, IObjectMapping
    {
        protected ObjectMapping() { }

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