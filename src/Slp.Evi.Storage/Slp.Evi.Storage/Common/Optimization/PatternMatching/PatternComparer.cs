using System;
using System.Collections.Generic;
using System.Text;
using Slp.Evi.Storage.Sparql.Algebra;
using TCode.r2rml4net;

namespace Slp.Evi.Storage.Common.Optimization.PatternMatching
{
    /// <summary>
    /// Class PatternComparer.
    /// </summary>
    public class PatternComparer
    {
        /// <summary>
        /// Compares the specified pattern <paramref name="left"/> to the pattern <paramref name="right"/>
        /// </summary>
        public CompareResult Compare(Pattern left, Pattern right)
        {
            var isIriEscaped = left.IsIriEscaped && right.IsIriEscaped;

            int leftStart = 0;
            int rightStart = 0;
            StringBuilder leftPrefix = new StringBuilder();
            StringBuilder rightPrefix = new StringBuilder();

            int leftEnd = left.PatternItems.Length - 1;
            int rightEnd = right.PatternItems.Length - 1;
            StringBuilder leftSuffix = new StringBuilder();
            StringBuilder rightSuffix = new StringBuilder();

            var leftPatternItems = left.PatternItems;
            var rightPatternItems = right.PatternItems;

            bool performedAction;
            List<MatchCondition> conditions = new List<MatchCondition>();

            do
            {
                performedAction = false;

                GetPrefix(leftPatternItems, leftPrefix, ref leftStart, leftEnd, leftSuffix);
                GetPrefix(rightPatternItems, rightPrefix, ref rightStart, rightEnd, rightSuffix);

                GetSuffix(leftPatternItems, leftStart, ref leftEnd, leftSuffix);
                GetSuffix(rightPatternItems, rightStart, ref rightEnd, rightSuffix);

                if (ProcessPrefixes(leftPrefix, rightPrefix, conditions, ref performedAction))
                {
                    break;
                }

                if (leftStart > leftEnd)
                {
                    leftSuffix = leftPrefix;
                }

                if (rightStart > rightEnd)
                {
                    rightSuffix = rightPrefix;
                }

                if (ProcessSuffixes(leftSuffix, rightSuffix, conditions, ref performedAction))
                {
                    break;
                }

                if (leftStart > leftEnd)
                {
                    leftSuffix = new StringBuilder();
                }

                if (rightStart > rightEnd)
                {
                    rightSuffix = new StringBuilder();
                }

                if (!performedAction && isIriEscaped)
                {
                    var leftIriEscapedPrefix = GetIriEscapedPrefix(leftPatternItems, ref leftStart, leftEnd, leftPrefix, leftSuffix);
                    var rightIriEscapedPrefix = GetIriEscapedPrefix(rightPatternItems, ref rightStart, rightEnd, rightPrefix, rightSuffix);

                    if (leftIriEscapedPrefix.Count > 0 || rightIriEscapedPrefix.Count > 0)
                    {
                        MatchCondition createdCondition;
                        if (CreateConditionFromIriEscapedPrefixes(leftIriEscapedPrefix, rightIriEscapedPrefix,
                            out createdCondition))
                        {
                            if (createdCondition != null)
                            {
                                conditions.Add(createdCondition);
                            }
                        }
                        else
                        {
                            conditions.Clear();
                            conditions.Add(MatchCondition.CreateAlwaysFalseCondition());
                            break;
                        }

                        performedAction = true;
                    }
                }

            } while (performedAction);

            var remainingLeftPattern = GetRemainingPattern(left.IsIriEscaped, leftPatternItems, leftPrefix, leftStart, leftEnd, leftSuffix);
            var remainingRightPattern = GetRemainingPattern(right.IsIriEscaped, rightPatternItems, rightPrefix, rightStart, rightEnd, rightSuffix);

            if (remainingLeftPattern.PatternItems.Length > 0 || remainingRightPattern.PatternItems.Length > 0)
            {
                conditions.Add(MatchCondition.CreateCondition(remainingLeftPattern, remainingRightPattern));
            }

            return new CompareResult(conditions);
        }

        private bool CreateConditionFromIriEscapedPrefixes(List<PatternItem> leftIriEscapedPrefix, List<PatternItem> rightIriEscapedPrefix, out MatchCondition createdCondition)
        {
            if (leftIriEscapedPrefix.Count == 0)
            {
                leftIriEscapedPrefix.Add(new PatternItem(string.Empty));
            }

            if (rightIriEscapedPrefix.Count == 0)
            {
                rightIriEscapedPrefix.Add(new PatternItem(string.Empty));
            }

            var leftPattern = new Pattern(true, leftIriEscapedPrefix);
            var rightPattern = new Pattern(true, rightIriEscapedPrefix);

            if (leftPattern.PatternItems.Length == 1 &&
                rightPattern.PatternItems.Length == 1 &&
                leftPattern.PatternItems[0].IsConstant &&
                rightPattern.PatternItems[0].IsConstant)
            {
                if (leftPattern.PatternItems[0].Text == rightPattern.PatternItems[0].Text)
                {
                    createdCondition = null;
                    return true;
                }
                else
                {
                    createdCondition = null;
                    return false;
                }
            }
            else
            {
                createdCondition = MatchCondition.CreateCondition(leftPattern, rightPattern);
                return true;
            }
        }

        /// <summary>
        /// Gets the IRI escaped prefix.
        /// </summary>
        /// <param name="patternItems">The pattern items.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="prefix">The prefix.</param>
        /// <param name="suffix">The suffix.</param>
        private List<PatternItem> GetIriEscapedPrefix(PatternItem[] patternItems, ref int start, int end, StringBuilder prefix, StringBuilder suffix)
        {
            List<PatternItem> toReturn = new List<PatternItem>();

            var foundUnallowed = false;
            var foundString = new StringBuilder();

            for (int i = 0; i < prefix.Length; i++)
            {
                if (MappingHelper.IsIUnreserved(prefix[i]))
                {
                    foundString.Append(prefix[i]);
                }
                else
                {
                    foundUnallowed = true;
                    break;
                }
            }

            if (foundString.Length > 0)
            {
                prefix.Remove(0, foundString.Length);
            }

            if (!foundUnallowed)
            {
                for (; start <= end; start++)
                {
                    var part = patternItems[start];

                    if (part.IsConstant)
                    {
                        for (int i = 0; i < part.Text.Length; i++)
                        {
                            if (MappingHelper.IsIUnreserved(part.Text[i]))
                            {
                                foundString.Append(part.Text[i]);
                            }
                            else
                            {
                                foundUnallowed = true;
                                prefix.Append(part.Text.Substring(i));
                                start++;
                                break;
                            }
                        }

                        if (foundUnallowed)
                        {
                            break;
                        }
                    }
                    else
                    {
                        if (foundString.Length > 0)
                        {
                            toReturn.Add(new PatternItem(foundString.ToString()));
                            foundString.Clear();
                        }

                        toReturn.Add(part);
                    }
                }
            }

            if (!foundUnallowed)
            {
                for (int i = 0; i < prefix.Length; i++)
                {
                    if (MappingHelper.IsIUnreserved(prefix[i]))
                    {
                        foundString.Append(prefix[i]);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (foundString.Length > 0)
            {
                toReturn.Add(new PatternItem(foundString.ToString()));
                foundString.Clear();
            }

            return toReturn;
        }

        /// <summary>
        /// Processes the suffixes.
        /// </summary>
        /// <param name="leftSuffix">The left suffix.</param>
        /// <param name="rightSuffix">The right suffix.</param>
        /// <param name="conditions">The conditions.</param>
        /// <param name="performedAction">if set to <c>true</c> the method performed some action.</param>
        /// <returns><c>true</c> if there was a "never match" condition found.</returns>
        private bool ProcessSuffixes(StringBuilder leftSuffix, StringBuilder rightSuffix, List<MatchCondition> conditions,
            ref bool performedAction)
        {
            var minSharedSuffix = Math.Min(leftSuffix.Length, rightSuffix.Length);

            if (minSharedSuffix > 0)
            {
                var leftSubSuffix = GetAndCutSuffixEnd(leftSuffix, minSharedSuffix);
                var rightSubSuffix = GetAndCutSuffixEnd(rightSuffix, minSharedSuffix);
                performedAction = true;

                if (leftSubSuffix != rightSubSuffix)
                {
                    conditions.Clear();
                    conditions.Add(MatchCondition.CreateAlwaysFalseCondition());
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Processes the prefixes.
        /// </summary>
        /// <param name="leftPrefix">The left prefix.</param>
        /// <param name="rightPrefix">The right prefix.</param>
        /// <param name="conditions">The conditions.</param>
        /// <param name="performedAction">if set to <c>true</c> the method performed some action.</param>
        /// <returns><c>true</c> if there was a "never match" condition found.</returns>
        private static bool ProcessPrefixes(StringBuilder leftPrefix, StringBuilder rightPrefix, List<MatchCondition> conditions,
            ref bool performedAction)
        {
            var minSharedPrefix = Math.Min(leftPrefix.Length, rightPrefix.Length);

            if (minSharedPrefix > 0)
            {
                var leftSubPrefix = GetAndCutPrefixStart(leftPrefix, minSharedPrefix);
                var rightSubPrefix = GetAndCutPrefixStart(rightPrefix, minSharedPrefix);
                performedAction = true;

                if (leftSubPrefix != rightSubPrefix)
                {
                    conditions.Clear();
                    conditions.Add(MatchCondition.CreateAlwaysFalseCondition());
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Gets the remaining pattern.
        /// </summary>
        /// <param name="isIriEscaped">Is the pattern IRI escaped.</param>
        /// <param name="patternItems">The pattern items.</param>
        /// <param name="prefix">The prefix.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="suffix">The suffix.</param>
        private Pattern GetRemainingPattern(bool isIriEscaped, PatternItem[] patternItems, StringBuilder prefix, int start, int end, StringBuilder suffix)
        {
            return new Pattern(isIriEscaped, GetRemainingPatternItems(patternItems, prefix, start, end, suffix));
        }

        /// <summary>
        /// Gets the remaining pattern items.
        /// </summary>
        /// <param name="patternItems">The pattern items.</param>
        /// <param name="prefix">The prefix.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="suffix">The suffix.</param>
        private IEnumerable<PatternItem> GetRemainingPatternItems(PatternItem[] patternItems, StringBuilder prefix, int start, int end, StringBuilder suffix)
        {
            if (prefix.Length > 0)
            {
                yield return new PatternItem(prefix.ToString());
            }

            for (int i = start; i <= end; i++)
            {
                yield return patternItems[i];
            }

            if (suffix.Length > 0)
            {
                yield return new PatternItem(suffix.ToString());
            }
        }

        /// <summary>
        /// Gets and cut the suffix end.
        /// </summary>
        /// <param name="suffix">The suffix.</param>
        /// <param name="minSharedSuffix">The shared suffix to get and cut.</param>
        private string GetAndCutSuffixEnd(StringBuilder suffix, int minSharedSuffix)
        {
            var subSuffix = suffix.ToString().Substring(suffix.Length - minSharedSuffix);
            suffix.Remove(suffix.Length - minSharedSuffix, minSharedSuffix);
            return subSuffix;
        }

        /// <summary>
        /// Gets and cut the prefix start.
        /// </summary>
        /// <param name="prefix">The prefix.</param>
        /// <param name="minSharedPrefix">The shared prefix to get and cut.</param>
        private static string GetAndCutPrefixStart(StringBuilder prefix, int minSharedPrefix)
        {
            var subPrefix = prefix.ToString().Substring(0, minSharedPrefix);
            prefix.Remove(0, minSharedPrefix);
            return subPrefix;
        }

        /// <summary>
        /// Gets the suffix.
        /// </summary>
        /// <param name="patternItems">The pattern items.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="suffix">The suffix.</param>
        private static void GetSuffix(PatternItem[] patternItems, int start, ref int end, StringBuilder suffix)
        {
            for (; end > start; end--)
            {
                var part = patternItems[end];

                if (part.IsConstant)
                {
                    suffix.Insert(0, part.Text);
                }
                else
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Gets the prefix.
        /// </summary>
        /// <param name="patternItems">The pattern items.</param>
        /// <param name="prefix">The prefix.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="suffix">The suffix.</param>
        private static void GetPrefix(PatternItem[] patternItems, StringBuilder prefix, ref int start, int end, StringBuilder suffix)
        {
            for (; start <= end; start++)
            {
                var part = patternItems[start];

                if (part.IsConstant)
                {
                    prefix.Append(part.Text);

                    if (start == end)
                    {
                        suffix.Clear();
                    }
                }
                else
                {
                    break;
                }
            }
        }
    }
}
