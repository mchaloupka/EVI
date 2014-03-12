using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sql;
using Slp.r2rml4net.Storage.Sql.Algebra;
using Slp.r2rml4net.Storage.Sql.Algebra.Condition;
using Slp.r2rml4net.Storage.Sql.Algebra.Operator;
using Slp.r2rml4net.Storage.Sql.Binders;
using Slp.r2rml4net.Storage.Sql.Utils;

namespace Slp.r2rml4net.Storage.Optimization.SqlAlgebra
{
    public class IsNullOptimizer : ISqlAlgebraOptimizer, ISqlSourceVisitor, IConditionVisitor, IValueBinderVisitor
    {
        private ConditionBuilder conditionBuilder;

        public IsNullOptimizer()
        {
            this.conditionBuilder = new ConditionBuilder(new ExpressionBuilder());
        }

        public Sql.Algebra.INotSqlOriginalDbSource ProcessAlgebra(Sql.Algebra.INotSqlOriginalDbSource algebra, Query.QueryContext context)
        {
            algebra.Accept(this, new VisitData(new GetIsNullList(), new GetIsNullList.GetIsNullListResult(), context));
            return algebra;
        }

        public object Visit(NoRowSource noRowSource, object data)
        {
            return null;
        }

        public object Visit(SingleEmptyRowSource singleEmptyRowSource, object data)
        {
            return null;
        }

        public object Visit(SqlSelectOp sqlSelectOp, object data)
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

            var cvd = (VisitData)data;

            var gres = new GetIsNullList.GetIsNullListResult();

            foreach (var cond in sqlSelectOp.Conditions)
            {
                gres.MergeWith(cvd.GINL.Process(cond));
            }

            foreach (var join in sqlSelectOp.JoinSources)
            {
                gres.MergeWith(cvd.GINL.Process(join.Condition));
            }

            List<ICondition> conditions = new List<ICondition>();
            foreach (var cond in sqlSelectOp.Conditions)
            {
                var resCond = (ICondition)cond.Accept(this, new VisitData(cvd.GINL, gres, cvd.Context));
                conditions.Add(resCond);
            }

            sqlSelectOp.ClearConditions();

            if(conditions.OfType<AlwaysFalseCondition>().Any())
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
                var resCond = (ICondition)join.Condition.Accept(this, new VisitData(cvd.GINL, gres, cvd.Context));
                join.ReplaceCondition(resCond);
            }

            foreach (var join in sqlSelectOp.LeftOuterJoinSources)
            {
                var resCond = (ICondition)join.Condition.Accept(this, new VisitData(cvd.GINL, gres, cvd.Context));
                join.ReplaceCondition(resCond);
            }

            foreach (var binder in sqlSelectOp.ValueBinders.ToArray())
            {
                var newBinder = (IBaseValueBinder)binder.Accept(this, new VisitData(cvd.GINL, gres, cvd.Context));

                if (newBinder != binder)
                    sqlSelectOp.ReplaceValueBinder(binder, newBinder);
            }

            return null;
        }

        public object Visit(SqlUnionOp sqlUnionOp, object data)
        {
            foreach (var inner in sqlUnionOp.Sources)
            {
                inner.Accept(this, data);
            }

            // TODO: Rework that we take the is null info from sources and then modify the value binders

            return null;
        }

        public object Visit(Sql.Algebra.Source.SqlStatement sqlStatement, object data)
        {
            return null;
        }

        public object Visit(Sql.Algebra.Source.SqlTable sqlTable, object data)
        {
            return null;
        }

        public object Visit(AlwaysFalseCondition condition, object data)
        {
            return condition;
        }

        public object Visit(AlwaysTrueCondition condition, object data)
        {
            return condition;
        }

        public object Visit(AndCondition condition, object data)
        {
            var cvd = (VisitData)data;
            var lres = cvd.GINL.Process(condition);
            var mres = new GetIsNullList.GetIsNullListResult();
            mres.MergeWith(cvd.GlobalResult);
            mres.MergeWith(lres);

            var d = new VisitData(cvd.GINL, mres, cvd.Context);

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

        public object Visit(OrCondition condition, object data)
        {
            var cvd = (VisitData)data;
            var lres = cvd.GINL.Process(condition);
            var mres = new GetIsNullList.GetIsNullListResult();
            mres.MergeWith(cvd.GlobalResult);
            mres.MergeWith(lres);

            var d = new VisitData(cvd.GINL, mres, cvd.Context);

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

        public object Visit(EqualsCondition condition, object data)
        {
            return condition;
        }

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

        public object Visit(NotCondition condition, object data)
        {
            var cvd = (VisitData)data;
            var gres = cvd.GlobalResult.GetInverse();

            var inner = (ICondition)condition.InnerCondition.Accept(this, new VisitData(cvd.GINL, gres, cvd.Context));

            if (inner is AlwaysFalseCondition)
                return new AlwaysTrueCondition();
            else if (inner is AlwaysTrueCondition)
                return new AlwaysFalseCondition();
            else
                return new NotCondition(inner);
        }

        private class VisitData
        {
            public VisitData(GetIsNullList ginl, GetIsNullList.GetIsNullListResult gres, QueryContext context)
            {
                this.GINL = ginl;
                this.GlobalResult = gres;
                this.Context = context;
            }

            public GetIsNullList GINL { get; private set; }

            public GetIsNullList.GetIsNullListResult GlobalResult { get; private set; }

            public QueryContext Context { get; private set; }
        }

        public object Visit(CaseValueBinder caseValueBinder, object data)
        {
            var cvd = (VisitData)data;

            foreach (var caseStatement in caseValueBinder.Statements.ToArray())
            {
                var lres = new GetIsNullList.GetIsNullListResult();
                lres.MergeWith(cvd.GlobalResult);
                lres.MergeWith(cvd.GINL.Process(caseStatement.Condition));
                var ncvd = new VisitData(cvd.GINL, lres, cvd.Context);

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

        public object Visit(CollateValueBinder collateValueBinder, object data)
        {
            var cvd = (VisitData)data;

            List<IBaseValueBinder> bindersToRemove = new List<IBaseValueBinder>();
            var innerBinders = collateValueBinder.InnerBinders.ToArray();

            for (int i = 0; i < innerBinders.Length; i++)
            {
                var binder = innerBinders[i];
                var newBinder = (IBaseValueBinder)binder.Accept(this, cvd);

                if (binder != newBinder)
                    collateValueBinder.ReplaceValueBinder(binder, newBinder);

                var isNullCondition = conditionBuilder.CreateIsNullCondition(cvd.Context, binder.GetOriginalValueBinder(cvd.Context));
                var modified = isNullCondition.Accept(this, cvd);

                if (modified is AlwaysTrueCondition)
                    bindersToRemove.Add(newBinder);
                else if(modified is AlwaysFalseCondition)
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
                collateValueBinder.RemoveValueBinder(binder);
            }

            if (collateValueBinder.InnerBinders.Count() == 1)
                return collateValueBinder.InnerBinders.First();
            else
                return collateValueBinder;
        }

        public object Visit(ValueBinder valueBinder, object data)
        {
            return valueBinder;
        }

        // TODO: It can be modified to calculate null list even from subsources
        private class GetIsNullList : IConditionVisitor
        {
            private Dictionary<ICondition, GetIsNullListResult> cache = new Dictionary<ICondition, GetIsNullListResult>();

            public GetIsNullListResult Process(ICondition condition)
            {
                if(!cache.ContainsKey(condition))
                {
                    var res = (GetIsNullListResult)condition.Accept(this, null);
                    cache.Add(condition, res);
                }

                return cache[condition];
            }

            public class GetIsNullListResult
            {
                public GetIsNullListResult()
                {
                    isNullColumns = new Dictionary<ISqlColumn, IsNullCondition>();
                    isNotNullColumns = new Dictionary<ISqlColumn, IsNullCondition>();
                }

                private Dictionary<ISqlColumn, IsNullCondition> isNullColumns;
                private Dictionary<ISqlColumn, IsNullCondition> isNotNullColumns;

                public void MergeWith(GetIsNullListResult other)
                {
                    MergeWith(this.isNullColumns, other.isNullColumns);
                    MergeWith(this.isNotNullColumns, other.isNotNullColumns);
                }

                public void IntersectWith(GetIsNullListResult other)
                {
                    IntersectWith(this.isNullColumns, other.isNullColumns);
                    IntersectWith(this.isNotNullColumns, other.isNotNullColumns);
                }

                public void AddIsNullCondition(IsNullCondition condition)
                {
                    this.isNullColumns.Add(condition.Column, condition);
                }

                private void MergeWith(Dictionary<ISqlColumn, IsNullCondition> source, Dictionary<ISqlColumn, IsNullCondition> with)
                {
                    foreach (var item in with.Keys.ToArray())
                    {
                        if (!source.ContainsKey(item))
                            source.Add(item, with[item]);
                    }
                }

                private void IntersectWith(Dictionary<ISqlColumn, IsNullCondition> source, Dictionary<ISqlColumn, IsNullCondition> with)
                {
                    foreach (var item in source.Keys.ToArray())
                    {
                        if (!with.ContainsKey(item))
                            source.Remove(item);
                    }
                }

                public GetIsNullListResult GetInverse()
                {
                    var res = new GetIsNullListResult();
                    MergeWith(res.isNotNullColumns, this.isNullColumns);
                    MergeWith(res.isNullColumns, this.isNotNullColumns);
                    return res;
                }

                public bool IsInNotNullColumns(ISqlColumn sqlColumn)
                {
                    return this.isNotNullColumns.ContainsKey(sqlColumn);
                }

                public bool IsInNullColumns(ISqlColumn sqlColumn)
                {
                    return this.isNullColumns.ContainsKey(sqlColumn);
                }

                public bool IsInNullColumns(IsNullCondition condition)
                {
                    if(this.isNullColumns.ContainsKey(condition.Column))
                    {
                        var cond = this.isNullColumns[condition.Column];

                        return cond == condition;
                    }

                    return false;
                }
            }

            public object Visit(AlwaysFalseCondition condition, object data)
            {
                return new GetIsNullListResult();
            }

            public object Visit(AlwaysTrueCondition condition, object data)
            {
                return new GetIsNullListResult();
            }

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

            public object Visit(EqualsCondition condition, object data)
            {
                return new GetIsNullListResult();
            }

            public object Visit(IsNullCondition condition, object data)
            {
                var res = new GetIsNullListResult();
                res.AddIsNullCondition(condition);
                return res;
            }

            public object Visit(NotCondition condition, object data)
            {
                var innerRes = (GetIsNullListResult)condition.InnerCondition.Accept(this, data);
                return innerRes.GetInverse();
            }

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
