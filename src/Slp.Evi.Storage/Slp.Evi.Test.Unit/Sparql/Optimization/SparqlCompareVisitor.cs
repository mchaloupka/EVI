using System;
using System.Collections.Generic;
using System.Linq;
using Slp.Evi.Storage.Sparql.Algebra.Modifiers;
using Slp.Evi.Storage.Sparql.Algebra.Patterns;
using Slp.Evi.Storage.Sparql.Utils.CodeGeneration;

namespace Slp.Evi.Test.Unit.Sparql.Optimization
{
    public class SparqlCompareVisitor
        : BaseGraphPatternTransformerG<object, bool, bool>
    {
        protected override bool Transform(SelectModifier toTransform, object data)
        {
            throw new NotImplementedException();
        }

        protected override bool Transform(EmptyPattern toTransform, object data)
        {
            throw new NotImplementedException();
        }

        protected override bool Transform(FilterPattern toTransform, object data)
        {
            throw new NotImplementedException();
        }

        protected override bool Transform(NotMatchingPattern toTransform, object data)
        {
            throw new NotImplementedException();
        }

        protected override bool Transform(GraphPattern toTransform, object data)
        {
            throw new NotImplementedException();
        }

        protected override bool Transform(JoinPattern toTransform, object data)
        {
            var other = data as JoinPattern;

            if (other == null)
            {
                return false;
            }

            if (!BagCollectionEqual(toTransform.Variables, other.Variables, (x,y) => x == y))
            {
                return false;
            }

            if (!BagCollectionEqual(toTransform.JoinedGraphPatterns, other.JoinedGraphPatterns, TransformGraphPattern))
            {
                return false;
            }

            return true;
        }

        protected override bool Transform(LeftJoinPattern toTransform, object data)
        {
            throw new NotImplementedException();
        }

        protected override bool Transform(MinusPattern toTransform, object data)
        {
            throw new NotImplementedException();
        }

        protected override bool Transform(TriplePattern toTransform, object data)
        {
            throw new NotImplementedException();
        }

        protected override bool Transform(UnionPattern toTransform, object data)
        {
            var other = data as UnionPattern;

            if (other == null)
            {
                return false;
            }

            if (!BagCollectionEqual(toTransform.Variables, other.Variables, (x, y) => x == y))
            {
                return false;
            }

            if (!BagCollectionEqual(toTransform.UnionedGraphPatterns, other.UnionedGraphPatterns, TransformGraphPattern))
            {
                return false;
            }

            return true;
        }

        protected override bool Transform(RestrictedTriplePattern toTransform, object data)
        {
            var other = data as RestrictedTriplePattern;

            if (other == null)
            {
                return false;
            }

            if (!BagCollectionEqual(toTransform.Variables, other.Variables, (x, y) => x == y))
            {
                return false;
            }

            if (!object.Equals(toTransform.GraphMap, other.GraphMap))
            {
                return false;
            }

            if (!object.Equals(toTransform.PredicateMap, other.PredicateMap))
            {
                return false;
            }

            if (!object.Equals(toTransform.PredicatePattern, other.PredicatePattern))
            {
                return false;
            }

            if (!object.Equals(toTransform.RefObjectMap, other.RefObjectMap))
            {
                return false;
            }

            if (!object.Equals(toTransform.ObjectMap, other.ObjectMap))
            {
                return false;
            }

            if (!object.Equals(toTransform.ObjectPattern, other.ObjectPattern))
            {
                return false;
            }

            if (!object.Equals(toTransform.SubjectMap, other.SubjectMap))
            {
                return false;
            }

            if (!object.Equals(toTransform.SubjectPattern, other.SubjectPattern))
            {
                return false;
            }

            if (!object.Equals(toTransform.TripleMap, other.TripleMap))
            {
                return false;
            }

            return true;
        }

        private bool BagCollectionEqual<T>(IEnumerable<T> leftItemsCollection, IEnumerable<T> rightItemsCollection, Func<T, T, bool> compareFunc)
        {
            var leftItems = leftItemsCollection.ToList();
            var rightItems = rightItemsCollection.ToList();

            foreach (var leftItem in leftItems)
            {
                int foundIndex = -1;

                for (int i = 0; i < rightItems.Count; i++)
                {
                    if (compareFunc(leftItem, rightItems[i]))
                    {
                        foundIndex = i;
                    }
                }

                if (foundIndex >= 0)
                {
                    rightItems.RemoveAt(foundIndex);
                }
                else
                {
                    return false;
                }
            }

            return rightItems.Count == 0;
        }
    }
}
