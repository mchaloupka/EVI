using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TCode.r2rml4net;
using TCode.r2rml4net.Mapping;
using VDS.RDF;

namespace Slp.r2rml4net.Storage.Mapping.Utils
{
    public static class FixesExtensions
    {
        private const string rrPrefix = "http://www.w3.org/ns/r2rml#";
        private const string rrJoinCondition = rrPrefix + "joinCondition";
        private const string rrChild = rrPrefix + "child";
        private const string rrParent = rrPrefix + "parent";

        public static IEnumerable<JoinCondition> GetJoinConditions(this IRefObjectMap refObjectPattern)
        {
            var mapping = (IGraph)refObjectPattern.GetType().GetProperty("R2RMLMappings", BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic).GetValue(refObjectPattern);
            var node = refObjectPattern.Node;

            var joinConditions = mapping.GetTriplesWithSubject(node).WithPredicate(mapping.CreateUriNode(new Uri(rrJoinCondition))).Select(x => x.Object);

            foreach (var joinCondition in joinConditions)
            {
                var inner = mapping.GetTriplesWithSubject(joinCondition);
                var child = inner.WithPredicate(mapping.CreateUriNode(new Uri(rrChild))).Select(x => x.Object).OfType<ILiteralNode>().Select(x => x.Value).First();
                var parent = inner.WithPredicate(mapping.CreateUriNode(new Uri(rrParent))).Select(x => x.Object).OfType<ILiteralNode>().Select(x => x.Value).First();

                yield return new JoinCondition(child, parent);
            }


            //return refObjectPattern.JoinConditions;
        }

        public static ITriplesMap GetParentTriplesMap(this IRefObjectMap refObjectPattern, IR2RML mapping)
        {
            // TODO: Remove this method as soon as the reference will be public

            var subjectMap = refObjectPattern.SubjectMap;

            foreach (var tripleMap in mapping.TriplesMaps)
            {
                if (tripleMap.SubjectMap == subjectMap)
                    return tripleMap;
            }

            throw new Exception("Parent triples map not found");
        }
    }
}
