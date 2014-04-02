using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sql.Algebra;
using Slp.r2rml4net.Storage.Sql.Algebra.Operator;

namespace Slp.r2rml4net.Storage.Optimization.SqlAlgebra
{
    public class ReducedSelectOptimization : ISqlAlgebraOptimizer, ISqlSourceVisitor
    {
        public INotSqlOriginalDbSource ProcessAlgebra(INotSqlOriginalDbSource algebra, QueryContext context)
        {
            var vData = new VisitData(context, false);

            foreach (var variable in algebra.ValueBinders.Select(x => x.VariableName))
            {
                vData.AddVariable(variable);
            }

            algebra.Accept(this, vData);
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
            var vData = (VisitData)data;
            bool isReduced = false;

            if(sqlSelectOp.IsReduced || sqlSelectOp.IsDistinct)
            {
                vData = vData.SetReduced(true);
                isReduced = true;
            }
            else if (vData.IsParentReduced)
            {
                sqlSelectOp.IsReduced = true;
                isReduced = true;
            }

            sqlSelectOp.OriginalSource.Accept(this, vData);

            foreach (var source in sqlSelectOp.JoinSources.Select(x => x.Source))
            {
                source.Accept(this, data);
            }

            foreach (var source in sqlSelectOp.LeftOuterJoinSources.Select(x => x.Source))
            {
                source.Accept(this, data);
            }

            if (isReduced)
            {
                var valueBindersNeedsColumns = sqlSelectOp.ValueBinders
                    .Where(x => vData.HasVariable(x.VariableName))
                    .SelectMany(x => x.AssignedColumns).Any();

                if (!valueBindersNeedsColumns && (!sqlSelectOp.Limit.HasValue || sqlSelectOp.Limit.Value > 1))
                {
                    sqlSelectOp.Limit = 1;
                }
            }

            return null;
        }

        public object Visit(SqlUnionOp sqlUnionOp, object data)
        {
            foreach (var source in sqlUnionOp.Sources)
            {
                source.Accept(this, data);
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

        private class VisitData
        {
            public QueryContext Context { get; private set; }

            public bool IsParentReduced { get; private set; }

            private HashSet<string> variables;

            public VisitData(QueryContext context, bool isParentReduced)
            {
                this.Context = context;
                this.IsParentReduced = isParentReduced;
                this.variables = new HashSet<string>();
            }

            public VisitData Clone()
            {
                var vd = new VisitData(Context, IsParentReduced);

                foreach (var variable in this.variables)
                {
                    vd.AddVariable(variable);
                }

                return vd;
            }

            public VisitData SetReduced(bool isReduced)
            {
                var vd = this.Clone();
                vd.IsParentReduced = isReduced;
                return vd;
            }

            public void AddVariable(string variable)
            {
                if (!variables.Contains(variable))
                    variables.Add(variable);
            }

            public bool HasVariable(string variable)
            {
                return variables.Contains(variable);
            }
        }
    }
}
