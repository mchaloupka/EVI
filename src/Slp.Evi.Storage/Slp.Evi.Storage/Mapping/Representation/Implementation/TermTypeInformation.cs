using TCode.r2rml4net.Mapping;

namespace Slp.Evi.Storage.Mapping.Representation.Implementation
{
    /// <summary>
    /// Representation of term type
    /// </summary>
    /// <seealso cref="Slp.Evi.Storage.Mapping.Representation.ITermTypeInformation" />
    public class TermTypeInformation
        : ITermTypeInformation
    {
        /// <summary>
        /// Prevents a default instance of the <see cref="TermTypeInformation"/> class from being created.
        /// </summary>
        private TermTypeInformation() { }

        /// <summary>
        /// Creates the specified term type information.
        /// </summary>
        /// <param name="triplesMapSubjectMap">The term mapping.</param>
        /// <param name="creationContext">The creation context.</param>
        public static ITermTypeInformation Create(ITermMap triplesMapSubjectMap, RepresentationCreationContext creationContext)
        {
            var res = new TermTypeInformation();
            res.IsBlankNode = triplesMapSubjectMap.TermType.IsBlankNode;
            res.IsIri = triplesMapSubjectMap.TermType.IsURI;
            res.IsLiteral = triplesMapSubjectMap.TermType.IsLiteral;
            return res;
        }

        /// <inheritdoc />
        public bool IsBlankNode { get; private set; }

        /// <inheritdoc />
        public bool IsIri { get; private set; }

        /// <inheritdoc />
        public bool IsLiteral { get; private set; }

        public static ITermTypeInformation CreateIriTermType()
        {
            var res = new TermTypeInformation
            {
                IsBlankNode = false,
                IsIri = true,
                IsLiteral = false
            };
            return res;
        }
    }
}