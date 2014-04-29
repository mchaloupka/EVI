using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sql.Algebra;
using Slp.r2rml4net.Storage.Sql.Algebra.Condition;
using Slp.r2rml4net.Storage.Sql.Algebra.Expression;

namespace Slp.r2rml4net.Storage.Optimization.SqlAlgebra
{
    public class ConcatenationInEqualConditionOptimizer : BaseConditionOptimizer
    {
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

        private ICondition ExpandEquals(IExpression leftOperand, IExpression rightOperand, QueryContext context)
        {
            if (leftOperand is ConcatenationExpr)
            {
                return ExpandEquals(leftOperand, (ConcatenationExpr)leftOperand, context);
            }
            else if (rightOperand is ConcatenationExpr)
            {
                return ExpandEquals(leftOperand, (ConcatenationExpr)rightOperand, context);
            }
            else
            {
                return null;
            }
        }

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

        private ICondition ExpandEquals(ConcatenationExpr leftOperand, ConstantExpr rightOperand, QueryContext context)
        {
            var rightConcat = new ConcatenationExpr(new List<IExpression>() { rightOperand });
            return ExpandEquals(leftOperand, rightConcat, context);
        }

        private ICondition ExpandEquals(ConcatenationExpr leftOperand, ConcatenationExpr rightOperand, QueryContext context)
        {
            if (!CanWeOptimize(leftOperand, context) || !CanWeOptimize(rightOperand, context))
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

        private bool CanWeOptimize(ConcatenationExpr operand, QueryContext context)
        {
            var isAnyNotColumnAndNotConstant = operand.Parts
                .Where(x => !(x is ConstantExpr))
                .Where(x => !(x is ColumnExpr))
                .Any();

            var isAnyNotStringConstants = operand.Parts
                .OfType<ConstantExpr>()
                .Where(x => !(x.Value is string))
                .Any();

            return !isAnyNotStringConstants && !isAnyNotColumnAndNotConstant;
        }

        private IEnumerable<ICondition> ProcessConcatenationEquals(ConcatenationExpr leftOperand, ConcatenationExpr rightOperand, QueryContext context)
        {
            var leftParts = leftOperand.Parts.ToArray();
            var rightParts = rightOperand.Parts.ToArray();

            //if (leftParts.OfType<ColumnExpr>().Where(x => !x.IsIriEscapedValue).Any() || rightParts.OfType<ColumnExpr>().Where(x => !x.IsIriEscapedValue).Any())
                return ProcessNotEscapedConcatenation(leftParts, rightParts, context);
            //else
                //return ProcessEscapedConcatenation(leftParts, rightParts, context);
        }

        private IEnumerable<ICondition> ProcessEscapedConcatenation(IExpression[] leftParts, IExpression[] rightParts, QueryContext context)
        {
            // NOTE: Implement this to improve work with multiple columns in template
            throw new NotImplementedException();
        }

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

        private ICondition SuffixComparison(ref string leftSuffix, ref string rightSuffix, QueryContext context)
        {
            var index = 0;
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

        private ICondition PrefixComparison(ref string leftPrefix, ref string rightPrefix, QueryContext context)
        {
            var index = 0;
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

        //private ICondition ExpandEquals(ConcatenationExpr leftOperand, ConcatenationExpr rightOperand, QueryContext context)
        //{
        //    var leftParts = leftOperand.Parts.ToList();
        //    var rightParts = rightOperand.Parts.ToList();

        //    List<ICondition> conditions = new List<ICondition>();

        //    while (true)
        //    {
        //        bool needsRepeat = false;

        //        var conds = ExpandEqualsTextPartsFromStart(context, leftParts, rightParts);

        //        if (conds.Count > 0)
        //        {
        //            if (conds.OfType<AlwaysFalseCondition>().Any())
        //                return new AlwaysFalseCondition();

        //            needsRepeat = true;
        //            conditions.AddRange(conds);
        //        }

        //        if (!needsRepeat)
        //            break;
        //    }

        //    if (leftParts.Count != 0 || rightParts.Count != 0)
        //    {
        //        if (leftParts.Count == 0)
        //        {
        //            conditions.AddRange(ExpandEqualsToNoParts(rightParts, context));
        //        }
        //        else if (rightParts.Count == 0)
        //        {
        //            conditions.AddRange(ExpandEqualsToNoParts(leftParts, context));
        //        }
        //        else
        //        {
        //            IExpression leftExpr = null;
        //            IExpression rightExpr = null;

        //            if (leftParts.Count == 1)
        //            {
        //                leftExpr = leftParts[0];
        //            }
        //            else
        //            {
        //                leftExpr = new ConcatenationExpr(leftParts);
        //            }

        //            if (rightParts.Count == 1)
        //            {
        //                rightExpr = rightParts[0];
        //            }
        //            else
        //            {
        //                rightExpr = new ConcatenationExpr(rightParts);
        //            }

        //            conditions.Add(new EqualsCondition(leftExpr, rightExpr));
        //        }
        //    }

        //    if (conditions.Count == 0)
        //    {
        //        return new AlwaysTrueCondition();
        //    }
        //    else if (conditions.Count == 1)
        //    {
        //        return conditions[0];
        //    }
        //    else
        //    {
        //        var andCondition = new AndCondition();

        //        foreach (var cond in conditions)
        //        {
        //            andCondition.AddToCondition(cond);
        //        }

        //        return andCondition;
        //    }
        //}

        //// TODO: Implement it for other directions
        //private List<ICondition> ExpandEqualsTextPartsFromStart(QueryContext context, List<IExpression> leftParts, List<IExpression> rightParts)
        //{
        //    if (leftParts.Count == 0 && rightParts.Count == 0)
        //    {
        //        return new List<ICondition>() { new AlwaysTrueCondition() };
        //    }
        //    else if (leftParts.Count == 0)
        //    {
        //        return ExpandEqualsToNoParts(rightParts, context);
        //    }
        //    else if (rightParts.Count == 0)
        //    {
        //        return ExpandEqualsToNoParts(leftParts, context);
        //    }
        //    else if (leftParts.Count == 1 && rightParts.Count == 1)
        //    {
        //        return new List<ICondition>() { new EqualsCondition(leftParts[0], rightParts[0]) };
        //    }
        //    else
        //    {
        //        List<ICondition> conditions = new List<ICondition>();

        //        var leftStartText = string.Empty;
        //        var rightStartText = string.Empty;

        //        int leftToRemove = 0;
        //        int rightToRemove = 0;

        //        for (int i = 0; i < leftParts.Count; i++)
        //        {
        //            if (leftParts[i] is ColumnExpr)
        //            {
        //                break;
        //            }
        //            else if (leftParts[i] is ConstantExpr)
        //            {
        //                leftStartText += ((ConstantExpr)leftParts[i]).Value.ToString();
        //                leftToRemove++;
        //            }
        //        }

        //        for (int i = 0; i < leftToRemove; i++)
        //        {
        //            leftParts.RemoveAt(0);
        //        }

        //        for (int i = 0; i < rightParts.Count; i++)
        //        {
        //            if (rightParts[i] is ColumnExpr)
        //            {
        //                break;
        //            }
        //            else if (rightParts[i] is ConstantExpr)
        //            {
        //                rightStartText += ((ConstantExpr)rightParts[i]).Value.ToString();
        //                rightToRemove++;
        //            }
        //        }

        //        for (int i = 0; i < rightToRemove; i++)
        //        {
        //            rightParts.RemoveAt(0);
        //        }

        //        int skipChars = 0;

        //        for (; skipChars < leftStartText.Length && skipChars < rightStartText.Length; skipChars++)
        //        {
        //            if (leftStartText[skipChars] != rightStartText[skipChars])
        //            {
        //                return new List<ICondition>() { new AlwaysFalseCondition() };
        //            }
        //        }

        //        leftStartText = leftStartText.Substring(skipChars);
        //        rightStartText = rightStartText.Substring(skipChars);

        //        if (!string.IsNullOrEmpty(leftStartText))
        //        {
        //            leftParts.Insert(0, new ConstantExpr(leftStartText));
        //        }
        //        else if (!string.IsNullOrEmpty(rightStartText))
        //        {
        //            rightParts.Insert(0, new ConstantExpr(rightStartText));
        //        }

        //        return conditions;
        //    }
        //}

        //private List<ICondition> ExpandEqualsToNoParts(List<IExpression> leftParts, QueryContext context)
        //{
        //    List<ICondition> conditions = new List<ICondition>();

        //    foreach (var part in leftParts)
        //    {
        //        if (part is ConstantExpr)
        //        {
        //            if (((ConstantExpr)part).Value.ToString().Length > 0)
        //                return new List<ICondition>() { new AlwaysFalseCondition() };
        //        }
        //        else if (part is ColumnExpr)
        //        {
        //            conditions.Add(new EqualsCondition(part, new ConstantExpr(string.Empty)));
        //        }
        //        else
        //        {
        //            throw new Exception("Concatenation should contain only constant or column expressions");
        //        }
        //    }

        //    return conditions;
        //}


    }
}
