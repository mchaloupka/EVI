
using System;
using System.Collections.Generic;
using System.Text;

namespace Slp.r2rml4net.Storage.Common.Optimization.PatternMatching
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
            int leftStart = 0;
            int rightStart = 0;
            StringBuilder leftPrefix = new StringBuilder();
            StringBuilder rightPrefix = new StringBuilder();

            int leftEnd = left.PatternItems.Length - 1;
            int rightEnd = right.PatternItems.Length - 1;
            StringBuilder leftSuffix = new StringBuilder();
            StringBuilder rightSuffix = new StringBuilder();

            bool performedAction;
            List<MatchCondition> conditions = new List<MatchCondition>();

            do
            {
                performedAction = false;

                GetPrefix(left.PatternItems, leftPrefix, ref leftStart, leftEnd, leftSuffix);
                GetPrefix(right.PatternItems, rightPrefix, ref rightStart, rightEnd, rightSuffix);

                GetSuffix(left.PatternItems, leftStart, ref leftEnd, leftSuffix);
                GetSuffix(right.PatternItems, rightStart, ref rightEnd, rightSuffix);

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
                        break;
                    }
                }

                if (leftStart > leftEnd)
                {
                    leftSuffix = leftPrefix;
                }

                if (rightStart > rightEnd)
                {
                    rightSuffix = rightPrefix;
                }

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
                        break;
                    }
                }

                if (leftStart > leftEnd)
                {
                    leftSuffix = new StringBuilder();
                }

                if (rightStart > rightEnd)
                {
                    rightSuffix = new StringBuilder();
                }
            } while (performedAction);

            var remainingLeftPattern = GetRemainingPattern(left, leftPrefix, leftStart, leftEnd, leftSuffix);
            var remainingRightPattern = GetRemainingPattern(right, rightPrefix, rightStart, rightEnd, rightSuffix);

            if (remainingLeftPattern.PatternItems.Length > 0 || remainingRightPattern.PatternItems.Length > 0)
            {
                conditions.Add(MatchCondition.CreateCondition(remainingLeftPattern, remainingRightPattern));
            }

            return new CompareResult(conditions);
        }

        /// <summary>
        /// Gets the remaining pattern.
        /// </summary>
        /// <param name="pattern">The pattern.</param>
        /// <param name="prefix">The prefix.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="suffix">The suffix.</param>
        private Pattern GetRemainingPattern(Pattern pattern, StringBuilder prefix, int start, int end, StringBuilder suffix)
        {
            return new Pattern(pattern.IsIriEscaped, GetRemainingPatternItems(pattern, prefix, start, end, suffix));
        }

        /// <summary>
        /// Gets the remaining pattern items.
        /// </summary>
        /// <param name="pattern">The pattern.</param>
        /// <param name="prefix">The prefix.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="suffix">The suffix.</param>
        private IEnumerable<PatternItem> GetRemainingPatternItems(Pattern pattern, StringBuilder prefix, int start, int end, StringBuilder suffix)
        {
            if (prefix.Length > 0)
            {
                yield return new PatternItem(prefix.ToString());
            }

            for (int i = start; i <= end; i++)
            {
                yield return pattern.PatternItems[i];
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
