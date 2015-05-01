using System.Collections.Generic;
using System.Linq;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sql.Algebra;
using Slp.r2rml4net.Storage.Sql.Algebra.Condition;
using Slp.r2rml4net.Storage.Sql.Algebra.Expression;
using Slp.r2rml4net.Storage.Sql.Algebra.Operator;
using Slp.r2rml4net.Storage.Sql.Algebra.Source;
using Slp.r2rml4net.Storage.Sql.Binders;

namespace Slp.r2rml4net.Storage.Optimization.SqlAlgebra
{
    /// <summary>
    /// Base condition optimizer
    /// </summary>
    public abstract class BaseConditionOptimizer : ISqlAlgebraOptimizer, ISqlAlgebraOptimizerOnTheFly, ISqlSourceVisitor, IConditionVisitor, IExpressionVisitor
    {
        /// <summary>
        /// Processes the algebra.
        /// </summary>
        /// <param name="algebra">The algebra.</param>
        /// <param name="context">The query context.</param>
        /// <returns>The processed algebra.</returns>
        public INotSqlOriginalDbSource ProcessAlgebra(INotSqlOriginalDbSource algebra, QueryContext context)
        {
            return (INotSqlOriginalDbSource)algebra.Accept(this, new VisitData() { Context = context, SecondRun = false, IsOnTheFly = false });
        }

        /// <summary>
        /// Processes the algebra.
        /// </summary>
        /// <param name="algebra">The algebra.</param>
        /// <param name="context">The query context.</param>
        /// <returns>The processed algebra.</returns>
        public INotSqlOriginalDbSource ProcessAlgebraOnTheFly(INotSqlOriginalDbSource algebra, QueryContext context)
        {
            return (INotSqlOriginalDbSource)algebra.Accept(this, new VisitData() { Context = context, SecondRun = false, IsOnTheFly = true });
        }

        /// <summary>
        /// Visits the specified no row source.
        /// </summary>
        /// <param name="noRowSource">The no row source.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        public object Visit(NoRowSource noRowSource, object data)
        {
            return noRowSource;
        }

        /// <summary>
        /// Visits the specified single empty row source.
        /// </summary>
        /// <param name="singleEmptyRowSource">The single empty row source.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        public object Visit(SingleEmptyRowSource singleEmptyRowSource, object data)
        {
            return singleEmptyRowSource;
        }

        /// <summary>
        /// Visits the specified SQL select operator.
        /// </summary>
        /// <param name="sqlSelectOp">The SQL select operator.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        public object Visit(SqlSelectOp sqlSelectOp, object data)
        {
            var vd = (VisitData)data;

            if (!vd.IsOnTheFly)
            {
                sqlSelectOp.OriginalSource.Accept(this, data);
            }

            foreach (var column in sqlSelectOp.Columns.OfType<SqlExpressionColumn>())
            {
                var newExpr = (IExpression)column.Expression.Accept(this, data);

                if (newExpr != column.Expression)
                    column.Expression = newExpr;
            }

            foreach (var ordering in sqlSelectOp.Orderings)
            {
                ordering.Expression = (IExpression)ordering.Expression.Accept(this, data);
            }

            foreach (var join in sqlSelectOp.JoinSources.Union(sqlSelectOp.LeftOuterJoinSources))
            {
                if (!vd.IsOnTheFly)
                {
                    join.Source.Accept(this, data);
                }

                var simplified = (ICondition)join.Condition.Accept(this, data);

                if (simplified != join.Condition)
                    join.ReplaceCondition(simplified);
            }

            var conditions = sqlSelectOp.Conditions.ToArray();

            foreach (var cond in conditions)
            {
                var simplified = (ICondition)cond.Accept(this, data);

                if (simplified != cond)
                    sqlSelectOp.ReplaceCondition(cond, simplified);
            }

            var alwaysTrue = sqlSelectOp.Conditions.OfType<AlwaysTrueCondition>().ToArray();

            foreach (var cond in alwaysTrue)
            {
                sqlSelectOp.RemoveCondition(cond);
            }

            if (sqlSelectOp.Conditions.OfType<AlwaysFalseCondition>().Any())
            {
                sqlSelectOp.ClearConditions();
                sqlSelectOp.AddCondition(new AlwaysFalseCondition());
            }

            if (vd.IsOnTheFly)
            {
                if ((sqlSelectOp.OriginalSource is NoRowSource)
                    || sqlSelectOp.JoinSources.Select(x => x.Source).OfType<NoRowSource>().Any()
                    || sqlSelectOp.JoinSources.Select(x => x.Condition).OfType<AlwaysFalseCondition>().Any()
                    || sqlSelectOp.Conditions.OfType<AlwaysFalseCondition>().Any())
                {
                    var noRowSource = new NoRowSource();

                    foreach (var valBinder in sqlSelectOp.ValueBinders)
                    {
                        noRowSource.AddValueBinder(new BlankValueBinder(valBinder.VariableName));
                    }

                    return noRowSource;
                }
            }

            return sqlSelectOp;
        }

        /// <summary>
        /// Visits the specified SQL union operator.
        /// </summary>
        /// <param name="sqlUnionOp">The SQL union operator.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        public object Visit(SqlUnionOp sqlUnionOp, object data)
        {
            var vd = (VisitData)data;

            if (!vd.IsOnTheFly)
            {
                foreach (var source in sqlUnionOp.Sources)
                {
                    source.Accept(this, data);
                }
            }

            return sqlUnionOp;
        }

        /// <summary>
        /// Visits the specified SQL statement.
        /// </summary>
        /// <param name="sqlStatement">The SQL statement.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        public object Visit(SqlStatement sqlStatement, object data)
        {
            return sqlStatement;
        }

        /// <summary>
        /// Visits the specified SQL table.
        /// </summary>
        /// <param name="sqlTable">The SQL table.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        public object Visit(SqlTable sqlTable, object data)
        {
            return sqlTable;
        }

        /// <summary>
        /// Visits the specified condition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        public object Visit(AlwaysFalseCondition condition, object data)
        {
            var visitData = (VisitData)data;

            if (visitData.SecondRun)
                return ProcessAlwaysFalseCondition(condition, visitData.Context);
            else
                return _ProcessAlwaysFalseCondition(condition, visitData).Accept(this, visitData.ForSecondRun());
        }

        /// <summary>
        /// Visits the specified condition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        public object Visit(AlwaysTrueCondition condition, object data)
        {
            var visitData = (VisitData)data;

            if (visitData.SecondRun)
                return ProcessAlwaysTrueCondition(condition, visitData.Context);
            else
                return _ProcessAlwaysTrueCondition(condition, visitData).Accept(this, visitData.ForSecondRun());
        }

        /// <summary>
        /// Visits the specified condition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        public object Visit(AndCondition condition, object data)
        {
            var visitData = (VisitData)data;

            if (visitData.SecondRun)
                return ProcessAndCondition(condition, visitData.Context);
            else
                return _ProcessAndCondition(condition, visitData).Accept(this, visitData.ForSecondRun());
        }

        /// <summary>
        /// Visits the specified condition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        public object Visit(EqualsCondition condition, object data)
        {
            var visitData = (VisitData)data;

            if (visitData.SecondRun)
                return ProcessEqualsCondition(condition, visitData.Context);
            else
                return _ProcessEqualsCondition(condition, visitData).Accept(this, visitData.ForSecondRun());
        }

        /// <summary>
        /// Visits the specified condition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        public object Visit(IsNullCondition condition, object data)
        {
            var visitData = (VisitData)data;

            if (visitData.SecondRun)
                return ProcessIsNullCondition(condition, visitData.Context);
            else
                return _ProcessIsNullCondition(condition, visitData).Accept(this, visitData.ForSecondRun());
        }

        /// <summary>
        /// Visits the specified condition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        public object Visit(NotCondition condition, object data)
        {
            var visitData = (VisitData)data;

            if (visitData.SecondRun)
                return ProcessNotCondition(condition, visitData.Context);
            else
                return _ProcessNotCondition(condition, visitData).Accept(this, visitData.ForSecondRun());
        }

        /// <summary>
        /// Visits the specified condition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        public object Visit(OrCondition condition, object data)
        {
            var visitData = (VisitData)data;

            if (visitData.SecondRun)
                return ProcessOrCondition(condition, visitData.Context);
            else
                return _ProcessOrCondition(condition, visitData).Accept(this, visitData.ForSecondRun());
        }

        /// <summary>
        /// Processes always false condition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="data">The data.</param>
        private ICondition _ProcessAlwaysFalseCondition(AlwaysFalseCondition condition, VisitData data)
        {
            return condition;
        }

        /// <summary>
        /// Processes always true condition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="data">The data.</param>
        private ICondition _ProcessAlwaysTrueCondition(AlwaysTrueCondition condition, VisitData data)
        {
            return condition;
        }

        /// <summary>
        /// Processes is null condition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="data">The data.</param>
        private ICondition _ProcessIsNullCondition(IsNullCondition condition, VisitData data)
        {
            return condition;
        }

        /// <summary>
        /// Processes not condition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="data">The data.</param>
        private ICondition _ProcessNotCondition(NotCondition condition, VisitData data)
        {
            var inner = (ICondition)condition.InnerCondition.Accept(this, data);

            if (inner is AlwaysTrueCondition)
                return new AlwaysFalseCondition();
            else if (inner is AlwaysFalseCondition)
                return new AlwaysTrueCondition();
            else
                return new NotCondition(inner);
        }

        /// <summary>
        /// Processes or condition.
        /// </summary>
        /// <param name="orCondition">The or condition.</param>
        /// <param name="data">The data.</param>
        private ICondition _ProcessOrCondition(OrCondition orCondition, VisitData data)
        {
            List<ICondition> conditions = new List<ICondition>();

            foreach (var cond in orCondition.Conditions)
            {
                var simpl = (ICondition)cond.Accept(this, data);

                if (simpl is AlwaysTrueCondition)
                    return new AlwaysTrueCondition();
                else if (!(simpl is AlwaysFalseCondition))
                    conditions.Add(simpl);
            }

            if (conditions.Count == 0)
                return new AlwaysFalseCondition();
            else if (conditions.Count == 1)
                return conditions[0];
            else
            {
                var orCond = new OrCondition();

                foreach (var cond in conditions)
                {
                    orCond.AddToCondition(cond);
                }

                return orCond;
            }
        }

        /// <summary>
        /// Processes and condition.
        /// </summary>
        /// <param name="andCondition">The and condition.</param>
        /// <param name="data">The data.</param>
        private ICondition _ProcessAndCondition(AndCondition andCondition, VisitData data)
        {
            List<ICondition> conditions = new List<ICondition>();

            foreach (var cond in andCondition.Conditions)
            {
                var simpl = (ICondition)cond.Accept(this, data);

                if (simpl is AlwaysFalseCondition)
                    return new AlwaysFalseCondition();
                else if (!(simpl is AlwaysTrueCondition))
                    conditions.Add(simpl);
            }

            if (conditions.Count == 0)
                return new AlwaysTrueCondition();
            else if (conditions.Count == 1)
                return conditions[0];
            else
            {
                var andCond = new AndCondition();

                foreach (var cond in conditions)
                {
                    andCond.AddToCondition(cond);
                }

                return andCond;
            }
        }

        /// <summary>
        ///Processes equals condition.
        /// </summary>
        /// <param name="equalsCondition">The equals condition.</param>
        /// <param name="data">The data.</param>
        private ICondition _ProcessEqualsCondition(EqualsCondition equalsCondition, VisitData data)
        {
            var leftOperand = (IExpression)equalsCondition.LeftOperand.Accept(this, data);
            var rightOperand = (IExpression)equalsCondition.RightOperand.Accept(this, data);

            equalsCondition.LeftOperand = leftOperand;
            equalsCondition.RightOperand = rightOperand;

            return equalsCondition;
        }

        /// <summary>
        /// Visits the specified expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        public object Visit(ColumnExpr expression, object data)
        {
            return expression;
        }

        /// <summary>
        /// Visits the specified expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        public object Visit(ConstantExpr expression, object data)
        {
            return expression;
        }

        /// <summary>
        /// Visits the specified expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        public object Visit(ConcatenationExpr expression, object data)
        {
            foreach (var part in expression.Parts.ToArray())
            {
                var newPart = (IExpression)part.Accept(this, data);

                if (newPart != part)
                    expression.ReplacePart(part, newPart);
            }

            return expression;
        }

        /// <summary>
        /// Visits the specified expression.
        /// </summary>
        /// <param name="nullExpr">The expression.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        public object Visit(NullExpr nullExpr, object data)
        {
            return nullExpr;
        }

        /// <summary>
        /// Visits the specified expression.
        /// </summary>
        /// <param name="coalesceExpr">The expression.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        public object Visit(CoalesceExpr coalesceExpr, object data)
        {
            foreach (var subExpr in coalesceExpr.Expressions.ToArray())
            {
                var newExpr = (IExpression)subExpr.Accept(this, data);

                if (newExpr is NullExpr)
                {
                    coalesceExpr.RemoveExpression(subExpr);
                }
                else if (newExpr != subExpr)
                    coalesceExpr.ReplaceExpression(subExpr, newExpr);
            }

            if(coalesceExpr.Expressions.Any())
            {
                if(coalesceExpr.Expressions.Count() == 1)
                {
                    return coalesceExpr.Expressions.First();
                }
                else
                {
                    return coalesceExpr;
                }
            }
            else
            {
                return new NullExpr(coalesceExpr.SqlType);
            }
        }

        /// <summary>
        /// Visits the specified expression.
        /// </summary>
        /// <param name="caseExpr">The expression.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        public object Visit(CaseExpr caseExpr, object data)
        {
            bool removeRest = false;

            foreach (var statement in caseExpr.Statements.ToArray())
            {
                if (removeRest)
                {
                    caseExpr.RemoveStatement(statement);
                }
                else
                {
                    var newCond = (ICondition)statement.Condition.Accept(this, data);

                    if (newCond is AlwaysFalseCondition)
                    {
                        caseExpr.RemoveStatement(statement);
                    }
                    else
                    {
                        var newExpr = (IExpression)statement.Expression.Accept(this, data);

                        statement.Condition = newCond;
                        statement.Expression = newExpr;

                        if (statement.Condition is AlwaysTrueCondition)
                        {
                            removeRest = true;
                        }
                    }
                }
            }

            if (caseExpr.Statements.Any())
                return caseExpr;
            else
                return new NullExpr(caseExpr.SqlType);
        }

        /// <summary>
        /// Processes the always false condition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="visitData">The visit data.</param>
        protected virtual ICondition ProcessAlwaysFalseCondition(AlwaysFalseCondition condition, QueryContext visitData)
        {
            return condition;
        }

        /// <summary>
        /// Processes the always true condition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="queryContext">The query context.</param>
        protected virtual ICondition ProcessAlwaysTrueCondition(AlwaysTrueCondition condition, QueryContext queryContext)
        {
            return condition;
        }

        /// <summary>
        /// Processes the and condition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="queryContext">The query context.</param>
        protected virtual ICondition ProcessAndCondition(AndCondition condition, QueryContext queryContext)
        {
            return condition;
        }

        /// <summary>
        /// Processes the equals condition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="queryContext">The query context.</param>
        protected virtual ICondition ProcessEqualsCondition(EqualsCondition condition, QueryContext queryContext)
        {
            return condition;
        }

        /// <summary>
        /// Processes the is null condition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="queryContext">The query context.</param>
        protected virtual ICondition ProcessIsNullCondition(IsNullCondition condition, QueryContext queryContext)
        {
            return condition;
        }

        /// <summary>
        /// Processes the not condition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="queryContext">The query context.</param>
        protected virtual ICondition ProcessNotCondition(NotCondition condition, QueryContext queryContext)
        {
            return condition;
        }

        /// <summary>
        /// Processes the or condition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="queryContext">The query context.</param>
        protected virtual ICondition ProcessOrCondition(OrCondition condition, QueryContext queryContext)
        {
            return condition;
        }

        /// <summary>
        /// Visit data for the visitor
        /// </summary>
        public class VisitData
        {
            /// <summary>
            /// Gets or sets the context.
            /// </summary>
            /// <value>The context.</value>
            public QueryContext Context { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether it is second run.
            /// </summary>
            /// <value><c>true</c> if it is second run; otherwise, <c>false</c>.</value>
            public bool SecondRun { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this instance is on the fly optimization.
            /// </summary>
            /// <value><c>true</c> if this instance is on the fly; otherwise, <c>false</c>.</value>
            public bool IsOnTheFly { get; set; }

            /// <summary>
            /// Data for the second run.
            /// </summary>
            /// <returns>VisitData.</returns>
            public VisitData ForSecondRun()
            {
                return new VisitData()
                {
                    Context = Context,
                    SecondRun = true,
                    IsOnTheFly = IsOnTheFly
                };
            }
        }
    }
}
