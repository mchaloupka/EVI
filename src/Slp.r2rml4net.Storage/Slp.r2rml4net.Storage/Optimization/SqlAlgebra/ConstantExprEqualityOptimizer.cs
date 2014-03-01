using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sparql;
using Slp.r2rml4net.Storage.Sql;
using Slp.r2rml4net.Storage.Sql.Algebra;
using Slp.r2rml4net.Storage.Sql.Algebra.Condition;
using Slp.r2rml4net.Storage.Sql.Algebra.Expression;
using Slp.r2rml4net.Storage.Sql.Algebra.Operator;

namespace Slp.r2rml4net.Storage.Optimization.SqlAlgebra
{
    public class ConstantExprEqualityOptimizer : BaseConditionOptimizer
    {
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

        private bool ConstantExprAreEqual(ConstantExpr constantExpr1, ConstantExpr constantExpr2)
        {
            return constantExpr1.SqlString == constantExpr2.SqlString;
        }
    }
}
