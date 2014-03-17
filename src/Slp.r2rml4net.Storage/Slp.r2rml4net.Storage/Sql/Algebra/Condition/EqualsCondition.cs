using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Sql.Algebra.Condition
{
    public class EqualsCondition : ICondition
    {
        public EqualsCondition(IExpression leftOperand, IExpression rightOperand)
        {
            this.LeftOperand = leftOperand;
            this.RightOperand = rightOperand;
        }

        public IExpression RightOperand { get; private set; }

        public IExpression LeftOperand { get; private set; }

        [DebuggerStepThrough]
        public object Accept(IConditionVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }

        public object Clone()
        {
            return new EqualsCondition((IExpression)this.LeftOperand.Clone(), (IExpression)this.RightOperand.Clone());
        }
    }
}
