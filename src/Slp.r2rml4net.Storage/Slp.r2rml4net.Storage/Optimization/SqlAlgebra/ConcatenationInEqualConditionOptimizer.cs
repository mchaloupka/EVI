using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sql.Algebra;
using Slp.r2rml4net.Storage.Sql.Algebra.Condition;
using Slp.r2rml4net.Storage.Sql.Algebra.Expression;

namespace Slp.r2rml4net.Storage.Optimization.SqlAlgebra
{
    /// <summary>
    /// CONCAT in equal optimization
    /// </summary>
    public class ConcatenationInEqualConditionOptimizer : BaseConditionOptimizer
    {
        /// <summary>
        /// Processes the equals condition.
        /// </summary>
        /// <param name="equalsCondition">The equals condition.</param>
        /// <param name="context">The context.</param>
        /// <returns>ICondition.</returns>
        protected override ICondition ProcessEqualsCondition(EqualsCondition equalsCondition, QueryContext context)
        {
            var leftOperand = equalsCondition.LeftOperand;
            var rightOperand = equalsCondition.RightOperand;

            if (leftOperand is ConcatenationExpr)
            {
                return ExpandEquals((ConcatenationExpr)leftOperand, rightOperand, context) ?? equalsCondition;
            }
            else if (rightOperand is ConcatenationExpr)
            {
                return ExpandEquals((ConcatenationExpr)rightOperand, leftOperand, context) ?? equalsCondition;
            }
            else
            {
                return equalsCondition;
            }
        }

        /// <summary>
        /// Expands the equals operator.
        /// </summary>
        /// <param name="leftOperand">The left operand.</param>
        /// <param name="rightOperand">The right operand.</param>
        /// <param name="context">The context.</param>
        private ICondition ExpandEquals(ConcatenationExpr leftOperand, IExpression rightOperand, QueryContext context)
        {
            if (rightOperand is ConcatenationExpr)
            {
                return ExpandEquals(leftOperand, (ConcatenationExpr)rightOperand, context);
            }
            else if (rightOperand is ConstantExpr)
            {
                return ExpandEquals(leftOperand, (ConstantExpr)rightOperand, context);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Expands the equals operator.
        /// </summary>
        /// <param name="leftOperand">The left operand.</param>
        /// <param name="rightOperand">The right operand.</param>
        /// <param name="context">The context.</param>
        private ICondition ExpandEquals(ConcatenationExpr leftOperand, ConstantExpr rightOperand, QueryContext context)
        {
            var rightConcat = new ConcatenationExpr(new List<IExpression>() { rightOperand });
            return ExpandEquals(leftOperand, rightConcat, context);
        }

        /// <summary>
        /// Expands the equals operator.
        /// </summary>
        /// <param name="leftOperand">The left operand.</param>
        /// <param name="rightOperand">The right operand.</param>
        /// <param name="context">The context.</param>
        private ICondition ExpandEquals(ConcatenationExpr leftOperand, ConcatenationExpr rightOperand, QueryContext context)
        {
            if (!CanOptimize(leftOperand, context) || !CanOptimize(rightOperand, context))
                return null;

            var result = ProcessConcatenationEquals(leftOperand, rightOperand, context);

            var andCond = new AndCondition();

            foreach (var cond in result)
            {
                if (cond is AlwaysTrueCondition)
                    continue;

                if (cond is AlwaysFalseCondition)
                    return new AlwaysFalseCondition();

                andCond.AddToCondition(cond);
            }

            if (andCond.Conditions.Any())
            {
                if (andCond.Conditions.Count() > 1)
                    return andCond;
                else
                    return andCond.Conditions.First();
            }
            else
                return new AlwaysTrueCondition();
        }

        /// <summary>
        /// Determines whether we can optimize the specified operand.
        /// </summary>
        /// <param name="operand">The operand.</param>
        /// <param name="context">The context.</param>
        /// <returns><c>true</c> if we can optimize the specified operand; otherwise, <c>false</c>.</returns>
        private bool CanOptimize(ConcatenationExpr operand, QueryContext context)
        {
            var isAnyNotColumnAndNotConstant = operand.Parts
                .Where(x => !(x is ConstantExpr))
                .Any(x => !(x is ColumnExpr));

            var isAnyNotStringConstants = operand.Parts
                .OfType<ConstantExpr>()
                .Any(x => !(x.Value is string));

            return !isAnyNotStringConstants && !isAnyNotColumnAndNotConstant;
        }

        /// <summary>
        /// Processes the concatenation equals operator.
        /// </summary>
        /// <param name="leftOperand">The left operand.</param>
        /// <param name="rightOperand">The right operand.</param>
        /// <param name="context">The context.</param>
        private IEnumerable<ICondition> ProcessConcatenationEquals(ConcatenationExpr leftOperand, ConcatenationExpr rightOperand, QueryContext context)
        {
            var leftParts = leftOperand.Parts.ToArray();
            var rightParts = rightOperand.Parts.ToArray();

            //if (leftParts.OfType<ColumnExpr>().Where(x => !x.IsIriEscapedValue).Any() || rightParts.OfType<ColumnExpr>().Where(x => !x.IsIriEscapedValue).Any())
                return ProcessNotEscapedConcatenation(leftParts, rightParts, context);
            //else
                //return ProcessEscapedConcatenation(leftParts, rightParts, context);
        }

        /// <summary>
        /// Processes the escaped concatenation.
        /// </summary>
        /// <param name="leftParts">The left parts.</param>
        /// <param name="rightParts">The right parts.</param>
        /// <param name="context">The context.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        private IEnumerable<ICondition> ProcessEscapedConcatenation(IExpression[] leftParts, IExpression[] rightParts, QueryContext context)
        {
            // TODO: Implement this to improve work with multiple columns in template
            throw new NotImplementedException();
        }

        /// <summary>
        /// Processes the not escaped concatenation.
        /// </summary>
        /// <param name="leftParts">The left parts.</param>
        /// <param name="rightParts">The right parts.</param>
        /// <param name="context">The context.</param>
        private IEnumerable<ICondition> ProcessNotEscapedConcatenation(IExpression[] leftParts, IExpression[] rightParts, QueryContext context)
        {
            int leftStartIndex;
            int leftEndIndex;
            string leftPrefix = GetPrefix(leftParts, out leftStartIndex);
            string leftSuffix = GetSuffix(leftParts, leftStartIndex, out leftEndIndex);

            int rightStartIndex;
            int rightEndIndex;
            string rightPrefix = GetPrefix(rightParts, out rightStartIndex);
            string rightSuffix = GetSuffix(rightParts, rightStartIndex, out rightEndIndex);

            yield return PrefixComparison(ref leftPrefix, ref rightPrefix, context);
            yield return SuffixComparison(ref leftSuffix, ref rightSuffix, context);

            yield return new EqualsCondition(
                CreateConcatenationEqual(leftStartIndex, leftEndIndex, leftPrefix, leftSuffix, leftParts),
                CreateConcatenationEqual(rightStartIndex, rightEndIndex, rightPrefix, rightSuffix, rightParts)
            );
        }

        /// <summary>
        /// Creates the concatenation equal.
        /// </summary>
        /// <param name="startIndex">The start index.</param>
        /// <param name="endIndex">The end index.</param>
        /// <param name="prefix">The prefix.</param>
        /// <param name="suffix">The suffix.</param>
        /// <param name="parts">The parts.</param>
        private IExpression CreateConcatenationEqual(int startIndex, int endIndex, string prefix, string suffix, IExpression[] parts)
        {
            var expressions = new List<IExpression>();

            if (!string.IsNullOrEmpty(prefix))
            {
                expressions.Add(new ConstantExpr(prefix));
            }

            for (int i = startIndex; i < endIndex; i++)
            {
                expressions.Add(parts[i]);
            }

            if (!string.IsNullOrEmpty(suffix))
            {
                expressions.Add(new ConstantExpr(suffix));
            }

            if (expressions.Count == 0)
                return new ConstantExpr(string.Empty);
            else if (expressions.Count == 1)
                return expressions[0];
            else
                return new ConcatenationExpr(expressions);
        }

        /// <summary>
        /// Compares the suffixes
        /// </summary>
        /// <param name="leftSuffix">The left suffix.</param>
        /// <param name="rightSuffix">The right suffix.</param>
        /// <param name="context">The context.</param>
        /// <returns>ICondition.</returns>
        private ICondition SuffixComparison(ref string leftSuffix, ref string rightSuffix, QueryContext context)
        {
            int index;
            var ok = true;

            for (index = 0; index < leftSuffix.Length && index < rightSuffix.Length; index++)
            {
                if (leftSuffix[leftSuffix.Length - index - 1] != rightSuffix[rightSuffix.Length - index - 1])
                {
                    ok = false;
                }
            }

            leftSuffix = leftSuffix.Substring(0, leftSuffix.Length - index);
            rightSuffix = rightSuffix.Substring(0, rightSuffix.Length - index);

            if (ok)
                return new AlwaysTrueCondition();
            else
                return new AlwaysFalseCondition();
        }

        /// <summary>
        /// Compares the prefixes
        /// </summary>
        /// <param name="leftPrefix">The left prefix.</param>
        /// <param name="rightPrefix">The right prefix.</param>
        /// <param name="context">The context.</param>
        /// <returns>ICondition.</returns>
        private ICondition PrefixComparison(ref string leftPrefix, ref string rightPrefix, QueryContext context)
        {
            int index;
            var ok = true;

            for (index = 0; index < leftPrefix.Length && index < rightPrefix.Length; index++)
            {
                if (leftPrefix[index] != rightPrefix[index])
                {
                    ok = false;
                }
            }

            leftPrefix = leftPrefix.Substring(index);
            rightPrefix = rightPrefix.Substring(index);

            if (ok)
                return new AlwaysTrueCondition();
            else
                return new AlwaysFalseCondition();
        }

        /// <summary>
        /// Gets the suffix.
        /// </summary>
        /// <param name="parts">The parts.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="endIndex">The end index.</param>
        /// <returns>System.String.</returns>
        /// <exception cref="System.Exception">
        /// Should not happen after passing CanWeOptimize method (value is not string)
        /// or
        /// Should not happen after passing CanWeOptimize method (unsupported expression type)
        /// </exception>
        private string GetSuffix(IExpression[] parts, int startIndex, out int endIndex)
        {
            StringBuilder sb = new StringBuilder();

            for (endIndex = parts.Length; endIndex > startIndex; endIndex--)
            {
                if (parts[endIndex - 1] is ColumnExpr)
                    break;
                else if (parts[endIndex - 1] is ConstantExpr)
                {
                    var value = ((ConstantExpr)parts[endIndex - 1]).Value;

                    if (!(value is string))
                        throw new Exception("Should not happen after passing CanWeOptimize method (value is not string)");

                    var s = (string)value;

                    sb.Insert(0, s);
                }
                else
                    throw new Exception("Should not happen after passing CanWeOptimize method (unsupported expression type)");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Gets the prefix.
        /// </summary>
        /// <param name="parts">The parts.</param>
        /// <param name="startIndex">The start index.</param>
        /// <returns>System.String.</returns>
        /// <exception cref="System.Exception">
        /// Should not happen after passing CanWeOptimize method (value is not string)
        /// or
        /// Should not happen after passing CanWeOptimize method (unsupported expression type)
        /// </exception>
        private string GetPrefix(IExpression[] parts, out int startIndex)
        {
            StringBuilder sb = new StringBuilder();

            for (startIndex = 0; startIndex < parts.Length; startIndex++)
            {
                if (parts[startIndex] is ColumnExpr)
                    break;
                else if (parts[startIndex] is ConstantExpr)
                {
                    var value = ((ConstantExpr)parts[startIndex]).Value;

                    if (!(value is string))
                        throw new Exception("Should not happen after passing CanWeOptimize method (value is not string)");

                    var s = (string)value;

                    sb.Append(s);
                }
                else
                    throw new Exception("Should not happen after passing CanWeOptimize method (unsupported expression type)");
            }

            return sb.ToString();
        }
    }
}
