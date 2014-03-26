using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Sql.Algebra.Expression
{
    public class CaseExpr : IExpression
    {
        private List<CaseStatementExpression> statements;

        public CaseExpr()
        {
            statements = new List<CaseStatementExpression>();
        }

        public object Clone()
        {
            var cas = new CaseExpr();

            foreach (var item in statements)
            {
                cas.statements.Add(new CaseStatementExpression((ICondition)item.Condition.Clone(), (IExpression)item.Expression.Clone()));
            }

            return cas;
        }

        public IEnumerable<CaseStatementExpression> Statements { get { return statements; } }

        public object Accept(IExpressionVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }

        public void AddStatement(ICondition condition, IExpression expression)
        {
            this.statements.Add(new CaseStatementExpression(condition, expression));
        }

        public void RemoveStatement(CaseStatementExpression statement)
        {
            var index = this.statements.IndexOf(statement);

            if (index > -1)
                this.statements.RemoveAt(index);
        }
    }

    public class CaseStatementExpression
    {
        public ICondition Condition { get; set; }
        public IExpression Expression { get; set; }

        public CaseStatementExpression(ICondition condition, IExpression expression)
        {
            this.Condition = condition;
            this.Expression = expression;
        }

    }
}
