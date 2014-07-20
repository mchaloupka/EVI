using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sql;
using Slp.r2rml4net.Storage.Sql.Algebra;
using Slp.r2rml4net.Storage.Sql.Algebra.Condition;
using Slp.r2rml4net.Storage.Sql.Algebra.Expression;
using Slp.r2rml4net.Storage.Sql.Algebra.Operator;
using Slp.r2rml4net.Storage.Sql.Algebra.Source;
using Slp.r2rml4net.Storage.Sql.Binders;
using Slp.r2rml4net.Storage.Sql.Binders.Utils;

namespace Slp.r2rml4net.Storage.Optimization.SqlAlgebra
{
    /// <summary>
    /// IS NULL optimizer
    /// </summary>
    public class IsNullOptimizer : ISqlAlgebraOptimizer, ISqlAlgebraOptimizerOnTheFly, ISqlSourceVisitor, IConditionVisitor, IValueBinderVisitor, IExpressionVisitor
    {
        /// <summary>
        /// The condition builder
        /// </summary>
        private ConditionBuilder conditionBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="IsNullOptimizer"/> class.
        /// </summary>
        public IsNullOptimizer()
        {
            this.conditionBuilder = new ConditionBuilder(new ExpressionBuilder());
        }

        /// <summary>
        /// Processes the algebra.
        /// </summary>
        /// <param name="algebra">The algebra.</param>
        /// <param name="context">The query context.</param>
        /// <returns>The processed algebra.</returns>
        public INotSqlOriginalDbSource ProcessAlgebra(INotSqlOriginalDbSource algebra, QueryContext context)
        {
            algebra.Accept(this, new VisitData(new GetIsNullList(), new GetIsNullList.GetIsNullListResult(), context, true));
            return algebra;
        }

        /// <summary>
        /// Processes the algebra.
        /// </summary>
        /// <param name="algebra">The algebra.</param>
        /// <param name="context">The query context.</param>
        /// <returns>The processed algebra.</returns>
        public INotSqlOriginalDbSource ProcessAlgebraOnTheFly(INotSqlOriginalDbSource algebra, QueryContext context)
        {
            algebra.Accept(this, new VisitData(new GetIsNullList(), new GetIsNullList.GetIsNullListResult(), context, false));
            return algebra;
        }

        /// <summary>
        /// Visits the specified no row source.
        /// </summary>
        /// <param name="noRowSource">The no row source.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        public object Visit(NoRowSource noRowSource, object data)
        {
            return null;
        }

        /// <summary>
        /// Visits the specified single empty row source.
        /// </summary>
        /// <param name="singleEmptyRowSource">The single empty row source.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        public object Visit(SingleEmptyRowSource singleEmptyRowSource, object data)
        {
            return null;
        }

        /// <summary>
        /// Visits the specified SQL select operator.
        /// </summary>
        /// <param name="sqlSelectOp">The SQL select operator.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        public object Visit(SqlSelectOp sqlSelectOp, object data)
        {
            var cvd = (VisitData)data;

            if (cvd.Recurse)
            {
                sqlSelectOp.OriginalSource.Accept(this, data);

                foreach (var join in sqlSelectOp.JoinSources)
                {
                    join.Source.Accept(this, data);
                }

                foreach (var join in sqlSelectOp.LeftOuterJoinSources)
                {
                    join.Source.Accept(this, data);
                }
            }

            var gres = cvd.GINL.Process(sqlSelectOp);

            List<ICondition> conditions = new List<ICondition>();
            foreach (var cond in sqlSelectOp.Conditions)
            {
                var resCond = (ICondition)cond.Accept(this, cvd.SetGlobalResult(gres));
                conditions.Add(resCond);
            }

            sqlSelectOp.ClearConditions();

            if (conditions.OfType<AlwaysFalseCondition>().Any())
            {
                sqlSelectOp.AddCondition(new AlwaysFalseCondition());
            }
            else
            {
                foreach (var cond in conditions.Where(x => !(x is AlwaysTrueCondition)))
                {
                    sqlSelectOp.AddCondition(cond);
                }
            }

            foreach (var join in sqlSelectOp.JoinSources)
            {
                var resCond = (ICondition)join.Condition.Accept(this, cvd.SetGlobalResult(gres));
                join.ReplaceCondition(resCond);
            }

            foreach (var join in sqlSelectOp.LeftOuterJoinSources)
            {
                var resCond = (ICondition)join.Condition.Accept(this, cvd.SetGlobalResult(gres));
                join.ReplaceCondition(resCond);
            }

            var bres = gres.GetForParentSource(sqlSelectOp);

            foreach (var binder in sqlSelectOp.ValueBinders.ToArray())
            {
                var newBinder = (IBaseValueBinder)binder.Accept(this, cvd.SetGlobalResult(bres));

                if (newBinder != binder)
                    sqlSelectOp.ReplaceValueBinder(binder, newBinder);
            }

            foreach (var exprColumn in sqlSelectOp.Columns.OfType<SqlExpressionColumn>())
            {
                var expression = (IExpression)exprColumn.Expression.Accept(this, cvd.SetGlobalResult(gres));

                if (exprColumn.Expression != expression)
                    exprColumn.Expression = expression;
            }

            return null;
        }

        /// <summary>
        /// Visits the specified SQL union operator.
        /// </summary>
        /// <param name="sqlUnionOp">The SQL union operator.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        public object Visit(SqlUnionOp sqlUnionOp, object data)
        {
            var cvd = (VisitData)data;

            if (cvd.Recurse)
            {
                foreach (var inner in sqlUnionOp.Sources)
                {
                    inner.Accept(this, data);
                }
            }

            var gres = cvd.GINL.Process(sqlUnionOp);
            var bres = gres.GetForParentSource(sqlUnionOp);

            foreach (var binder in sqlUnionOp.ValueBinders.ToArray())
            {
                var newBinder = (IBaseValueBinder)binder.Accept(this, cvd.SetGlobalResult(bres));

                if (newBinder != binder)
                    sqlUnionOp.ReplaceValueBinder(binder, newBinder);
            }

            return null;
        }

        /// <summary>
        /// Visits the specified SQL statement.
        /// </summary>
        /// <param name="sqlStatement">The SQL statement.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        public object Visit(Sql.Algebra.Source.SqlStatement sqlStatement, object data)
        {
            return null;
        }

        /// <summary>
        /// Visits the specified SQL table.
        /// </summary>
        /// <param name="sqlTable">The SQL table.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        public object Visit(Sql.Algebra.Source.SqlTable sqlTable, object data)
        {
            return null;
        }

        /// <summary>
        /// Visits the specified condition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        public object Visit(AlwaysFalseCondition condition, object data)
        {
            return condition;
        }

        /// <summary>
        /// Visits the specified condition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        public object Visit(AlwaysTrueCondition condition, object data)
        {
            return condition;
        }

        /// <summary>
        /// Visits the specified condition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        public object Visit(AndCondition condition, object data)
        {
            var cvd = (VisitData)data;
            var lres = cvd.GINL.Process(condition);
            var mres = new GetIsNullList.GetIsNullListResult();
            mres.MergeWith(cvd.GlobalResult);
            mres.MergeWith(lres);

            var d = cvd.SetGlobalResult(mres);

            List<ICondition> conditions = new List<ICondition>();

            foreach (var cond in condition.Conditions)
            {
                conditions.Add((ICondition)cond.Accept(this, d));
            }

            var conds = conditions.Where(x => !(x is AlwaysTrueCondition)).ToArray();

            if (conds.OfType<AlwaysFalseCondition>().Any())
                return new AlwaysFalseCondition();
            else if (conds.Length == 1)
                return conds[0];
            else
            {
                var andCondition = new AndCondition();
                foreach (var cond in conds)
                {
                    andCondition.AddToCondition(cond);
                }
                return andCondition;
            }
        }

        /// <summary>
        /// Visits the specified condition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        public object Visit(OrCondition condition, object data)
        {
            var cvd = (VisitData)data;
            var lres = cvd.GINL.Process(condition);
            var mres = new GetIsNullList.GetIsNullListResult();
            mres.MergeWith(cvd.GlobalResult);
            mres.MergeWith(lres);

            var d = cvd.SetGlobalResult(mres);

            List<ICondition> conditions = new List<ICondition>();

            foreach (var cond in condition.Conditions)
            {
                conditions.Add((ICondition)cond.Accept(this, d));
            }

            var conds = conditions.Where(x => !(x is AlwaysFalseCondition)).ToArray();

            if (conds.OfType<AlwaysTrueCondition>().Any())
                return new AlwaysTrueCondition();
            else if (conds.Length == 1)
                return conds[0];
            else
            {
                var orCondition = new OrCondition();
                foreach (var cond in conds)
                {
                    orCondition.AddToCondition(cond);
                }
                return orCondition;
            }
        }

        /// <summary>
        /// Visits the specified condition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        public object Visit(EqualsCondition condition, object data)
        {
            var leftExpr = (IExpression)condition.LeftOperand.Accept(this, data);
            var rightExpr = (IExpression)condition.RightOperand.Accept(this, data);

            if (condition.LeftOperand != leftExpr)
                condition.LeftOperand = leftExpr;
            if (condition.RightOperand != rightExpr)
                condition.RightOperand = rightExpr;

            return condition;
        }

        /// <summary>
        /// Visits the specified condition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        public object Visit(IsNullCondition condition, object data)
        {
            var cvd = (VisitData)data;

            if (cvd.GlobalResult.IsInNotNullColumns(condition.Column))
                return new AlwaysFalseCondition();
            else if (cvd.GlobalResult.IsInNullColumns(condition.Column) && !cvd.GlobalResult.IsInNullColumns(condition))
                return new AlwaysTrueCondition();
            else
                return condition;
        }

        /// <summary>
        /// Visits the specified condition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        public object Visit(NotCondition condition, object data)
        {
            var cvd = (VisitData)data;
            var gres = cvd.GlobalResult.GetInverse();

            var inner = (ICondition)condition.InnerCondition.Accept(this, cvd.SetGlobalResult(gres));

            if (inner is AlwaysFalseCondition)
                return new AlwaysTrueCondition();
            else if (inner is AlwaysTrueCondition)
                return new AlwaysFalseCondition();
            else
                return new NotCondition(inner);
        }

        /// <summary>
        /// Visits the specified case value binder.
        /// </summary>
        /// <param name="caseValueBinder">The case value binder.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        public object Visit(CaseValueBinder caseValueBinder, object data)
        {
            var cvd = (VisitData)data;

            foreach (var caseStatement in caseValueBinder.Statements.ToArray())
            {
                var lres = new GetIsNullList.GetIsNullListResult();
                lres.MergeWith(cvd.GlobalResult);
                lres.MergeWith(cvd.GINL.Process(caseStatement.Condition));
                var ncvd = cvd.SetGlobalResult(lres);

                var cond = (ICondition)caseStatement.Condition.Accept(this, ncvd);
                if (cond != caseStatement.Condition)
                    caseStatement.Condition = cond;

                if (caseStatement.Condition is AlwaysFalseCondition)
                {
                    caseValueBinder.RemoveStatement(caseStatement);
                }
                else
                {
                    var binder = (IBaseValueBinder)caseStatement.ValueBinder.Accept(this, ncvd);
                    if (binder != caseStatement.ValueBinder)
                        caseStatement.ValueBinder = binder;
                }
            }

            return caseValueBinder;
        }

        /// <summary>
        /// Visits the specified SQL side value binder.
        /// </summary>
        /// <param name="sqlSideValueBinder">The SQL side value binder.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        public object Visit(SqlSideValueBinder sqlSideValueBinder, object data)
        {
            var expressionCol = sqlSideValueBinder.Column as SqlExpressionColumn;

            if (expressionCol != null)
            {
                var expr = (IExpression)expressionCol.Expression.Accept(this, data);

                if (expr != expressionCol.Expression)
                    expressionCol.Expression = expr;
            }

            return sqlSideValueBinder;
        }

        /// <summary>
        /// Visits the specified expression value binder.
        /// </summary>
        /// <param name="expressionValueBinder">The expression value binder.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        public object Visit(ExpressionValueBinder expressionValueBinder, object data)
        {
            var expr = (IExpression)expressionValueBinder.Expression.Accept(this, data);

            if (expr != expressionValueBinder.Expression)
                expressionValueBinder.Expression = expr;

            return expressionValueBinder;
        }

        /// <summary>
        /// Visits the specified collate value binder.
        /// </summary>
        /// <param name="coalesceValueBinder">The coalesce value binder.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        public object Visit(CoalesceValueBinder coalesceValueBinder, object data)
        {
            var cvd = (VisitData)data;

            List<IBaseValueBinder> bindersToRemove = new List<IBaseValueBinder>();
            var innerBinders = coalesceValueBinder.InnerBinders.ToArray();

            for (int i = 0; i < innerBinders.Length; i++)
            {
                var binder = innerBinders[i];
                var newBinder = (IBaseValueBinder)binder.Accept(this, cvd);

                if (binder != newBinder)
                    coalesceValueBinder.ReplaceValueBinder(binder, newBinder);

                var isNullCondition = conditionBuilder.CreateIsNullCondition(cvd.Context, binder);
                var modified = isNullCondition.Accept(this, cvd);

                if (modified is AlwaysTrueCondition)
                    bindersToRemove.Add(newBinder);
                else if (modified is AlwaysFalseCondition)
                {
                    for (int y = i + 1; y < innerBinders.Length; y++)
                    {
                        bindersToRemove.Add(innerBinders[y]);
                    }
                    break;
                }
            }

            foreach (var binder in bindersToRemove)
            {
                coalesceValueBinder.RemoveValueBinder(binder);
            }

            if (coalesceValueBinder.InnerBinders.Count() == 1)
                return coalesceValueBinder.InnerBinders.First();
            else
                return coalesceValueBinder;
        }

        /// <summary>
        /// Visits the specified blank value binder.
        /// </summary>
        /// <param name="blankValueBinder">The blank value binder.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        public object Visit(BlankValueBinder blankValueBinder, object data)
        {
            return blankValueBinder;
        }

        /// <summary>
        /// Visits the specified value binder.
        /// </summary>
        /// <param name="valueBinder">The value binder.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        public object Visit(ValueBinder valueBinder, object data)
        {
            return valueBinder;
        }

        /// <summary>
        /// Visits the specified expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        public object Visit(ColumnExpr expression, object data)
        {
            var cvd = (VisitData)data;

            if (cvd.GlobalResult.IsInNullColumns(expression.Column))
                return new NullExpr();
            else
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
            foreach (var item in expression.Parts.ToArray())
            {
                var nItem = (IExpression)item.Accept(this, data);

                if(nItem is NullExpr)
                {
                    return new NullExpr();
                }
                if (item != nItem)
                    expression.ReplacePart(item, nItem);
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
        /// <param name="collateExpr">The expression.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>Returned value.</returns>
        public object Visit(CoalesceExpr collateExpr, object data)
        {
            foreach (var expr in collateExpr.Expressions.ToArray())
            {
                var nExpr = (IExpression)expr.Accept(this, data);

                if (nExpr is NullExpr)
                {
                    collateExpr.RemoveExpression(expr);
                }
                if (expr != nExpr)
                {
                    collateExpr.ReplaceExpression(expr, nExpr);
                }
            }

            if (!collateExpr.Expressions.Any())
            {
                return new NullExpr();
            }
            else if (collateExpr.Expressions.Count() == 1)
            {
                return collateExpr.Expressions.First();
            }
            else
            {
                return collateExpr;
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
            foreach (var statement in caseExpr.Statements.ToArray())
            {
                var condition = (ICondition)statement.Condition.Accept(this, data);

                if (condition is AlwaysFalseCondition)
                {
                    caseExpr.RemoveStatement(statement);
                }
                else
                {
                    if (condition != statement.Condition)
                        statement.Condition = condition;

                    var expr = (IExpression)statement.Expression.Accept(this, data);

                    if (expr != statement.Expression)
                        statement.Expression = expr;
                }
            }

            if (!caseExpr.Statements.Any())
            {
                return new NullExpr();
            }
            else
            {
                return caseExpr;
            }
        }

        /// <summary>
        /// Visit data for the optimizer
        /// </summary>
        private class VisitData
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="VisitData"/> class.
            /// </summary>
            /// <param name="ginl">The get is null list.</param>
            /// <param name="gres">The global result.</param>
            /// <param name="context">The context.</param>
            /// <param name="recurse">if set to <c>true</c> it should do recurse.</param>
            public VisitData(GetIsNullList ginl, GetIsNullList.GetIsNullListResult gres, QueryContext context, bool recurse)
            {
                this.GINL = ginl;
                this.GlobalResult = gres;
                this.Context = context;
                this.Recurse = recurse;
            }

            /// <summary>
            /// Sets the global result.
            /// </summary>
            /// <param name="gres">The global result.</param>
            /// <returns>VisitData.</returns>
            public VisitData SetGlobalResult(GetIsNullList.GetIsNullListResult gres)
            {
                return new VisitData(this.GINL, gres, this.Context, this.Recurse);
            }

            /// <summary>
            /// Gets or sets the get is null list.
            /// </summary>
            /// <value>The get is null list.</value>
            public GetIsNullList GINL { get; private set; }

            /// <summary>
            /// Gets or sets the global result.
            /// </summary>
            /// <value>The global result.</value>
            public GetIsNullList.GetIsNullListResult GlobalResult { get; private set; }

            /// <summary>
            /// Gets or sets the context.
            /// </summary>
            /// <value>The context.</value>
            public QueryContext Context { get; private set; }

            /// <summary>
            /// Gets or sets a value indicating whether this <see cref="VisitData"/> is recurse.
            /// </summary>
            /// <value><c>true</c> if recurse; otherwise, <c>false</c>.</value>
            public bool Recurse { get; private set; }
        }

        /// <summary>
        /// Visitor that retrieves the is null list
        /// </summary>
        private class GetIsNullList : ISqlSourceVisitor, IConditionVisitor
        {
            /// <summary>
            /// The condition cache
            /// </summary>
            private Dictionary<ICondition, GetIsNullListResult> conditionCache = new Dictionary<ICondition, GetIsNullListResult>();

            /// <summary>
            /// The SQL cache
            /// </summary>
            private Dictionary<ISqlSource, GetIsNullListResult> sqlCache = new Dictionary<ISqlSource, GetIsNullListResult>();

            /// <summary>
            /// Processes the specified condition.
            /// </summary>
            /// <param name="condition">The condition.</param>
            public GetIsNullListResult Process(ICondition condition)
            {
                if (!conditionCache.ContainsKey(condition))
                {
                    var res = (GetIsNullListResult)condition.Accept(this, null);
                    conditionCache.Add(condition, res);
                }

                return conditionCache[condition];
            }

            /// <summary>
            /// Processes the specified source.
            /// </summary>
            /// <param name="source">The source.</param>
            public GetIsNullListResult Process(ISqlSource source)
            {
                if (!sqlCache.ContainsKey(source))
                {
                    var res = (GetIsNullListResult)source.Accept(this, null);
                    sqlCache.Add(source, res);
                }

                return sqlCache[source];
            }

            /// <summary>
            /// Visits the specified no row source.
            /// </summary>
            /// <param name="noRowSource">The no row source.</param>
            /// <param name="data">The passed data.</param>
            /// <returns>Returned value.</returns>
            public object Visit(NoRowSource noRowSource, object data)
            {
                return new GetIsNullListResult();
            }

            /// <summary>
            /// Visits the specified single empty row source.
            /// </summary>
            /// <param name="singleEmptyRowSource">The single empty row source.</param>
            /// <param name="data">The passed data.</param>
            /// <returns>Returned value.</returns>
            public object Visit(SingleEmptyRowSource singleEmptyRowSource, object data)
            {
                return new GetIsNullListResult();
            }

            /// <summary>
            /// Visits the specified SQL select operator.
            /// </summary>
            /// <param name="sqlSelectOp">The SQL select operator.</param>
            /// <param name="data">The passed data.</param>
            /// <returns>Returned value.</returns>
            public object Visit(SqlSelectOp sqlSelectOp, object data)
            {
                var gres = new GetIsNullListResult();
                var sres = new GetIsNullListResult();

                foreach (var cond in sqlSelectOp.Conditions)
                {
                    gres.MergeWith(this.Process(cond));
                }

                sres.MergeWith(this.Process(sqlSelectOp.OriginalSource).GetForParentSource(sqlSelectOp.OriginalSource));

                foreach (var join in sqlSelectOp.JoinSources)
                {
                    gres.MergeWith(this.Process(join.Condition));
                    sres.MergeWith(this.Process(join.Source).GetForParentSource(join.Source));
                }

                sres.MergeWith(gres);

                return sres;
            }

            /// <summary>
            /// Visits the specified SQL union operator.
            /// </summary>
            /// <param name="sqlUnionOp">The SQL union operator.</param>
            /// <param name="data">The passed data.</param>
            /// <returns>Returned value.</returns>
            public object Visit(SqlUnionOp sqlUnionOp, object data)
            {
                var gres = new GetIsNullListResult();

                foreach (var source in sqlUnionOp.Sources)
                {
                    gres.MergeWith(this.Process(source).GetForParentSource(source));
                }

                return gres;
            }

            /// <summary>
            /// Visits the specified SQL statement.
            /// </summary>
            /// <param name="sqlStatement">The SQL statement.</param>
            /// <param name="data">The passed data.</param>
            /// <returns>Returned value.</returns>
            public object Visit(Sql.Algebra.Source.SqlStatement sqlStatement, object data)
            {
                return new GetIsNullListResult();
            }

            /// <summary>
            /// Visits the specified SQL table.
            /// </summary>
            /// <param name="sqlTable">The SQL table.</param>
            /// <param name="data">The passed data.</param>
            /// <returns>Returned value.</returns>
            public object Visit(Sql.Algebra.Source.SqlTable sqlTable, object data)
            {
                // TODO: Get info from db schema
                return new GetIsNullListResult();
            }

            /// <summary>
            /// The result of get is null list
            /// </summary>
            public class GetIsNullListResult
            {
                /// <summary>
                /// Initializes a new instance of the <see cref="GetIsNullListResult"/> class.
                /// </summary>
                public GetIsNullListResult()
                {
                    isNullColumns = new Dictionary<ISqlColumn, IsNullCondition>();
                    isNotNullColumns = new Dictionary<ISqlColumn, IsNullCondition>();
                }

                /// <summary>
                /// The is null columns
                /// </summary>
                private Dictionary<ISqlColumn, IsNullCondition> isNullColumns;

                /// <summary>
                /// The is not null columns
                /// </summary>
                private Dictionary<ISqlColumn, IsNullCondition> isNotNullColumns;

                /// <summary>
                /// Merges with.
                /// </summary>
                /// <param name="other">The other.</param>
                public void MergeWith(GetIsNullListResult other)
                {
                    MergeWith(this.isNullColumns, other.isNullColumns);
                    MergeWith(this.isNotNullColumns, other.isNotNullColumns);
                }

                /// <summary>
                /// Intersects with.
                /// </summary>
                /// <param name="other">The other.</param>
                public void IntersectWith(GetIsNullListResult other)
                {
                    IntersectWith(this.isNullColumns, other.isNullColumns);
                    IntersectWith(this.isNotNullColumns, other.isNotNullColumns);
                }

                /// <summary>
                /// Adds the is null condition.
                /// </summary>
                /// <param name="condition">The condition.</param>
                public void AddIsNullCondition(IsNullCondition condition)
                {
                    this.isNullColumns.Add(condition.Column, condition);
                }

                /// <summary>
                /// Adds the is not null column.
                /// </summary>
                /// <param name="col">The col.</param>
                private void AddIsNotNullColumn(ISqlColumn col)
                {
                    if (this.isNotNullColumns.ContainsKey(col))
                        this.isNotNullColumns[col] = null;
                    else
                        this.isNotNullColumns.Add(col, null);
                }

                /// <summary>
                /// Adds the is null column.
                /// </summary>
                /// <param name="col">The col.</param>
                private void AddIsNullColumn(ISqlColumn col)
                {
                    if (this.isNullColumns.ContainsKey(col))
                        this.isNullColumns[col] = null;
                    else
                        this.isNullColumns.Add(col, null);
                }

                /// <summary>
                /// Merges with.
                /// </summary>
                /// <param name="source">The source.</param>
                /// <param name="with">The with.</param>
                private void MergeWith(Dictionary<ISqlColumn, IsNullCondition> source, Dictionary<ISqlColumn, IsNullCondition> with)
                {
                    foreach (var item in with.Keys.ToArray())
                    {
                        if (!source.ContainsKey(item))
                            source.Add(item, with[item]);
                    }
                }

                /// <summary>
                /// Intersects with.
                /// </summary>
                /// <param name="source">The source.</param>
                /// <param name="with">The with.</param>
                private void IntersectWith(Dictionary<ISqlColumn, IsNullCondition> source, Dictionary<ISqlColumn, IsNullCondition> with)
                {
                    foreach (var item in source.Keys.ToArray())
                    {
                        if (!with.ContainsKey(item))
                            source.Remove(item);
                    }
                }

                /// <summary>
                /// Gets the inverse.
                /// </summary>
                public GetIsNullListResult GetInverse()
                {
                    var res = new GetIsNullListResult();
                    MergeWith(res.isNotNullColumns, this.isNullColumns);
                    MergeWith(res.isNullColumns, this.isNotNullColumns);
                    return res;
                }

                /// <summary>
                /// Determines whether the column is in "is not null list".
                /// </summary>
                /// <param name="sqlColumn">The SQL column.</param>
                public bool IsInNotNullColumns(ISqlColumn sqlColumn)
                {
                    return this.isNotNullColumns.ContainsKey(sqlColumn);
                }

                /// <summary>
                /// Determines whether the column is in "is not null list".
                /// </summary>
                /// <param name="sqlColumn">The SQL column.</param>
                public bool IsInNullColumns(ISqlColumn sqlColumn)
                {
                    return this.isNullColumns.ContainsKey(sqlColumn);
                }

                /// <summary>
                /// Determines whether the column is in "is null list".
                /// </summary>
                /// <param name="condition">The condition.</param>
                public bool IsInNullColumns(IsNullCondition condition)
                {
                    if (this.isNullColumns.ContainsKey(condition.Column))
                    {
                        var cond = this.isNullColumns[condition.Column];

                        return cond == condition;
                    }

                    return false;
                }

                /// <summary>
                /// Gets the columns for parent source.
                /// </summary>
                /// <param name="source">The source.</param>
                /// <exception cref="System.Exception">
                /// SqlUnionColumn should be only in SqlUnionOp
                /// or
                /// Other column type than expected
                /// </exception>
                public GetIsNullListResult GetForParentSource(ISqlSource source)
                {
                    var res = new GetIsNullListResult();

                    foreach (var col in source.Columns)
                    {
                        if (col is SqlExpressionColumn)
                        {
                            // TODO: now expression cannot be null, it can change in the future
                            res.AddIsNotNullColumn(col);
                        }
                        else if (col is SqlUnionColumn)
                        {
                            if (source is SqlUnionOp)
                            {
                                var unCol = (SqlUnionColumn)col;
                                var unOp = (SqlUnionOp)source;

                                if (unCol.OriginalColumns.Count() == unOp.Sources.Count()) // Can decide only when it contains columns from all sources
                                {
                                    if (unCol.OriginalColumns.All(x => this.IsInNullColumns(x)))
                                    {
                                        res.AddIsNullColumn(col);
                                    }
                                    if (unCol.OriginalColumns.All(x => this.IsInNotNullColumns(x)))
                                    {
                                        res.AddIsNotNullColumn(col);
                                    }
                                }
                            }
                            else
                            {
                                throw new Exception("SqlUnionColumn should be only in SqlUnionOp");
                            }
                        }
                        else if (col is SqlSelectColumn)
                        {
                            var selCol = (SqlSelectColumn)col;

                            if (this.IsInNullColumns(selCol.OriginalColumn))
                                res.AddIsNullColumn(col);
                            if (this.IsInNotNullColumns(selCol.OriginalColumn))
                                res.AddIsNotNullColumn(col);
                        }
                        else if (col is SqlTableColumn)
                        {
                            if (this.IsInNullColumns(col))
                                res.AddIsNullColumn(col);
                            if (this.IsInNotNullColumns(col))
                                res.AddIsNotNullColumn(col);
                        }
                        else
                        {
                            throw new Exception("Other column type than expected");
                        }
                    }

                    return res;
                }
            }

            /// <summary>
            /// Visits the specified condition.
            /// </summary>
            /// <param name="condition">The condition.</param>
            /// <param name="data">The passed data.</param>
            /// <returns>Returned value.</returns>
            public object Visit(AlwaysFalseCondition condition, object data)
            {
                return new GetIsNullListResult();
            }

            /// <summary>
            /// Visits the specified condition.
            /// </summary>
            /// <param name="condition">The condition.</param>
            /// <param name="data">The passed data.</param>
            /// <returns>Returned value.</returns>
            public object Visit(AlwaysTrueCondition condition, object data)
            {
                return new GetIsNullListResult();
            }

            /// <summary>
            /// Visits the specified condition.
            /// </summary>
            /// <param name="condition">The condition.</param>
            /// <param name="data">The passed data.</param>
            /// <returns>Returned value.</returns>
            public object Visit(AndCondition condition, object data)
            {
                var res = new GetIsNullListResult();

                foreach (var inner in condition.Conditions)
                {
                    var innerRes = (GetIsNullListResult)inner.Accept(this, data);
                    res.MergeWith(innerRes);
                }

                return res;
            }

            /// <summary>
            /// Visits the specified condition.
            /// </summary>
            /// <param name="condition">The condition.</param>
            /// <param name="data">The passed data.</param>
            /// <returns>Returned value.</returns>
            public object Visit(EqualsCondition condition, object data)
            {
                return new GetIsNullListResult();
            }

            /// <summary>
            /// Visits the specified condition.
            /// </summary>
            /// <param name="condition">The condition.</param>
            /// <param name="data">The passed data.</param>
            /// <returns>Returned value.</returns>
            public object Visit(IsNullCondition condition, object data)
            {
                var res = new GetIsNullListResult();
                res.AddIsNullCondition(condition);
                return res;
            }

            /// <summary>
            /// Visits the specified condition.
            /// </summary>
            /// <param name="condition">The condition.</param>
            /// <param name="data">The passed data.</param>
            /// <returns>Returned value.</returns>
            public object Visit(NotCondition condition, object data)
            {
                var innerRes = (GetIsNullListResult)condition.InnerCondition.Accept(this, data);
                return innerRes.GetInverse();
            }

            /// <summary>
            /// Visits the specified condition.
            /// </summary>
            /// <param name="condition">The condition.</param>
            /// <param name="data">The passed data.</param>
            /// <returns>Returned value.</returns>
            public object Visit(OrCondition condition, object data)
            {
                var res = new GetIsNullListResult();

                foreach (var inner in condition.Conditions)
                {
                    var innerRes = (GetIsNullListResult)inner.Accept(this, data);
                    res.IntersectWith(innerRes);
                }

                return res;
            }
        }
    }
}
