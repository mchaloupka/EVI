using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Sql.Algebra;
using Slp.r2rml4net.Storage.Sql.Algebra.Condition;
using Slp.r2rml4net.Storage.Sql.Algebra.Operator;

namespace Slp.r2rml4net.Storage.Optimization.SqlAlgebra
{
    public class IsNullOptimizer : ISqlAlgebraOptimizer, ISqlSourceVisitor, IConditionVisitor
    {
        public Sql.Algebra.INotSqlOriginalDbSource ProcessAlgebra(Sql.Algebra.INotSqlOriginalDbSource algebra, Query.QueryContext context)
        {
            algebra.Accept(this, new GetIsNullList());
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

            var ginl = (GetIsNullList)data;

            var gres = new GetIsNullList.GetIsNullListResult();

            foreach (var cond in sqlSelectOp.Conditions)
            {
                gres.MergeWith(ginl.Process(cond));
            }

            foreach (var join in sqlSelectOp.JoinSources)
            {
                gres.MergeWith(ginl.Process(join.Condition));
            }

            List<ICondition> conditions = new List<ICondition>();
            foreach (var cond in sqlSelectOp.Conditions)
            {
                var resCond = (ICondition)cond.Accept(this, new VisitData(ginl, gres));
                conditions.Add(cond);
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
                var resCond = (ICondition)join.Condition.Accept(this, new VisitData(ginl, gres));
                join.ReplaceCondition(resCond);
            }

            foreach (var join in sqlSelectOp.LeftOuterJoinSources)
            {
                var resCond = (ICondition)join.Condition.Accept(this, new VisitData(ginl, gres));
                join.ReplaceCondition(resCond);
            }

            return null;
        }

        public object Visit(SqlUnionOp sqlUnionOp, object data)
        {
            foreach (var inner in sqlUnionOp.Sources)
            {
                inner.Accept(this, data);
            }

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

            var d = new VisitData(cvd.GINL, mres);

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

            var d = new VisitData(cvd.GINL, mres);

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

            if (cvd.GlobalResult.IsNotNullColumns.Contains(condition.Column))
                return new AlwaysFalseCondition();
            else
                return condition;
        }

        public object Visit(NotCondition condition, object data)
        {
            var inner = (ICondition)condition.InnerCondition.Accept(this, data);

            if (inner is AlwaysFalseCondition)
                return new AlwaysTrueCondition();
            else if (inner is AlwaysTrueCondition)
                return new AlwaysFalseCondition();
            else
                return new NotCondition(inner);
        }

        private class VisitData
        {
            public VisitData(GetIsNullList ginl, GetIsNullList.GetIsNullListResult gres)
            {
                this.GINL = ginl;
                this.GlobalResult = gres;
            }

            public GetIsNullList GINL { get; private set; }

            public GetIsNullList.GetIsNullListResult GlobalResult { get; private set; }
        }

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
                    IsNullColumns = new List<ISqlColumn>();
                    IsNotNullColumns = new List<ISqlColumn>();
                }

                public List<ISqlColumn> IsNullColumns { get; private set; }
                public List<ISqlColumn> IsNotNullColumns { get; private set; }

                public void MergeWith(GetIsNullListResult other)
                {
                    this.IsNullColumns.AddRange(other.IsNullColumns);
                    this.IsNotNullColumns.AddRange(other.IsNotNullColumns);
                }

                public void IntersectWith(GetIsNullListResult other)
                {
                    IntersectWith(this.IsNullColumns, other.IsNullColumns);
                    IntersectWith(this.IsNotNullColumns, other.IsNotNullColumns);
                }

                private void IntersectWith(List<ISqlColumn> source, List<ISqlColumn> with)
                {
                    foreach (var item in source.ToArray())
                    {
                        if (!with.Contains(item))
                            source.Remove(item);
                    }
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
                res.IsNullColumns.Add(condition.Column);
                return res;
            }

            public object Visit(NotCondition condition, object data)
            {
                var res = new GetIsNullListResult();
                var innerRes = (GetIsNullListResult)condition.InnerCondition.Accept(this, data);
                res.IsNullColumns.AddRange(innerRes.IsNotNullColumns);
                res.IsNotNullColumns.AddRange(innerRes.IsNullColumns);
                return res;
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
