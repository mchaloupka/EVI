using System.Collections.Generic;
using System.Linq;
using TCode.r2rml4net.Mapping;

namespace Slp.Evi.Storage.Mapping.Representation.Implementation
{
    public class RefObjectMapping
        : IRefObjectMapping
    {
        private RefObjectMapping() { }

        public static IRefObjectMapping Create(IRefObjectMap objectMap, TriplesMapping tr, RepresentationCreationContext creationContext)
        {
            var res = new RefObjectMapping();
            res.TriplesMap = tr;
            res.JoinConditions = objectMap.JoinConditions.Select(JoinCondition.Create).ToArray();
            creationContext.GetSubjectMap(objectMap.SubjectMap, (subjectMap) => res.TargetSubjectMap = subjectMap);
            return res;
        }

        /// <inheritdoc />
        public ITriplesMapping TriplesMap { get; private set; }

        /// <inheritdoc />
        public ITermTypeInformation TermType => TargetSubjectMap.TermType;

        /// <inheritdoc />
        public IEnumerable<IJoinCondition> JoinConditions { get; private set; }

        /// <inheritdoc />
        public ISubjectMapping TargetSubjectMap { get; private set; }
    }
}