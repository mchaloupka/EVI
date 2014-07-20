using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sparql.Algebra;
using Slp.r2rml4net.Storage.Sql;
using Slp.r2rml4net.Storage.Sql.Algebra;
using Slp.r2rml4net.Storage.Sql.Algebra.Condition;
using Slp.r2rml4net.Storage.Sql.Algebra.Expression;
using Slp.r2rml4net.Storage.Sql.Algebra.Operator;

namespace Slp.r2rml4net.Storage.Optimization.SqlAlgebra
{
    /// <summary>
    /// Comparison of constants
    /// </summary>
    public class ConstantExprEqualityOptimizer : BaseConditionOptimizer
    {
        /// <summary>
        /// Processes the equals condition.
        /// </summary>
        /// <param name="equalsCondition">The equals condition.</param>
        /// <param name="context">The context.</param>
        protected override ICondition ProcessEqualsCondition(EqualsCondition equalsCondition, QueryContext context)
        {
            var leftOp = equalsCondition.LeftOperand;
            var rightOp = equalsCondition.RightOperand;

            if (ExpressionsAlwaysEqual(leftOp, rightOp))
            {
                return new AlwaysTrueCondition();
            }
            else if (!ExpressionsCanBeEqual(leftOp, rightOp))
            {
                return new AlwaysFalseCondition();
            }
            else
            {
                return equalsCondition;
            }
        }

        /// <summary>
        /// Are expressions always equal?
        /// </summary>
        /// <param name="first">The first.</param>
        /// <param name="second">The second.</param>
        /// <returns><c>true</c> if they are always equal, <c>false</c> otherwise.</returns>
        /// <exception cref="System.Exception">Unknown expression type</exception>
        private bool ExpressionsAlwaysEqual(IExpression first, IExpression second)
        {
            if (first is ConcatenationExpr || second is ConcatenationExpr)
            {
                return false;
            }
            else if (first is ColumnExpr || second is ColumnExpr)
            {
                return false;
            }
            else if (first is ConstantExpr && second is ConstantExpr)
            {
                return ConstantExprAreEqual((ConstantExpr)first, (ConstantExpr)second);
            }

            throw new Exception("Unknown expression type");
        }

        /// <summary>
        /// Can be the expressions equal?
        /// </summary>
        /// <param name="first">The first.</param>
        /// <param name="second">The second.</param>
        /// <returns><c>true</c> if they can be equal, <c>false</c> otherwise.</returns>
        /// <exception cref="System.Exception">Unknown expression type</exception>
        private bool ExpressionsCanBeEqual(IExpression first, IExpression second)
        {
            if (first is ConcatenationExpr || second is ConcatenationExpr)
            {
                return true;
            }
            else if (first is ColumnExpr || second is ColumnExpr)
            {
                return true;
            }
            else if (first is ConstantExpr && second is ConstantExpr)
            {
                return ConstantExprAreEqual((ConstantExpr)first, (ConstantExpr)second);
            }

            throw new Exception("Unknown expression type");
        }

        /// <summary>
        /// Are the constant expressions equal?
        /// </summary>
        /// <param name="constantExpr1">The constant expr1.</param>
        /// <param name="constantExpr2">The constant expr2.</param>
        /// <returns><c>true</c> if the constant expressions are equal, <c>false</c> otherwise.</returns>
        private bool ConstantExprAreEqual(ConstantExpr constantExpr1, ConstantExpr constantExpr2)
        {
            return constantExpr1.SqlString == constantExpr2.SqlString;
        }
    }
}
