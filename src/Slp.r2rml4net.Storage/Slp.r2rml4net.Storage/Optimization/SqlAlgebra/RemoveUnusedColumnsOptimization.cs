using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sql.Algebra;
using Slp.r2rml4net.Storage.Sql.Algebra.Operator;
using Slp.r2rml4net.Storage.Sql.Algebra.Utils;

namespace Slp.r2rml4net.Storage.Optimization.SqlAlgebra
{
    public class RemoveUnusedColumnsOptimization : ISqlAlgebraOptimizer, ISqlSourceVisitor
    {
        private class VisitorData
        {
            public VisitorData(IEnumerable<ISqlColumn> neededColumns)
            {
                this.NeededColumns = neededColumns.ToList();
            }

            public List<ISqlColumn> NeededColumns { get; private set; }
        }

        public INotSqlOriginalDbSource ProcessAlgebra(INotSqlOriginalDbSource algebra, QueryContext context)
        {
            algebra.Accept(this, new VisitorData(new List<ISqlColumn>()));
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
            var cvd = (VisitorData)data;

            var cols = sqlSelectOp.Columns.ToArray();

            var needed = new List<ISqlColumn>();
            needed.AddRange(cvd.NeededColumns);
            needed.AddRange(GetNeededColumnsOfValueVariables(sqlSelectOp));
            needed.AddRange(GetNeededColumnsOfOrderings(sqlSelectOp));

            foreach (var col in cols)
            {
                if (!needed.Contains(col))
                    sqlSelectOp.RemoveColumn(col);
            }

            var neededOrig = GetReferencedColumnsByColumns(needed);

            foreach (var cond in sqlSelectOp.Conditions)
            {
                neededOrig.AddRange(cond.GetAllReferencedColumns());
            }

            foreach (var join in sqlSelectOp.JoinSources)
            {
                neededOrig.AddRange(join.Condition.GetAllReferencedColumns());
            }

            foreach (var join in sqlSelectOp.LeftOuterJoinSources)
            {
                neededOrig.AddRange(join.Condition.GetAllReferencedColumns());
            }

            var ncvd = new VisitorData(neededOrig);

            sqlSelectOp.OriginalSource.Accept(this, ncvd);

            foreach (var join in sqlSelectOp.JoinSources)
            {
                join.Source.Accept(this, ncvd);
            }

            foreach (var join in sqlSelectOp.LeftOuterJoinSources)
            {
                join.Source.Accept(this, ncvd);
            }

            return null;
        }

        public object Visit(SqlUnionOp sqlUnionOp, object data)
        {
            var cvd = (VisitorData)data;

            var cols = sqlUnionOp.Columns.ToArray();

            var needed = new List<ISqlColumn>();
            needed.AddRange(cvd.NeededColumns);
            needed.AddRange(GetNeededColumnsOfValueVariables(sqlUnionOp));

            foreach (var col in cols)
            {
                if (!needed.Contains(col))
                    sqlUnionOp.RemoveColumn(col);
            }

            var neededOrig = GetReferencedColumnsByColumns(needed);

            foreach (var source in sqlUnionOp.Sources)
            {
                source.Accept(this, new VisitorData(neededOrig));
            }

            return null;
        }

        private static List<ISqlColumn> GetReferencedColumnsByColumns(IEnumerable<ISqlColumn> needed)
        {
            var neededOrig = new List<ISqlColumn>();
            neededOrig.AddRange(needed.OfType<SqlSelectColumn>().Select(x => x.OriginalColumn));
            neededOrig.AddRange(needed.OfType<SqlUnionColumn>().SelectMany(x => x.OriginalColumns));
            neededOrig.AddRange(needed.OfType<SqlExpressionColumn>().SelectMany(x => x.Expression.GetAllReferencedColumns()));
            return neededOrig;
        }

        private IEnumerable<ISqlColumn> GetNeededColumnsOfValueVariables(INotSqlOriginalDbSource source)
        {
            foreach (var binder in source.ValueBinders)
            {
                foreach (var col in binder.AssignedColumns)
                {
                    yield return col;
                }
            }
        }

        private IEnumerable<ISqlColumn> GetNeededColumnsOfOrderings(SqlSelectOp sqlSelectOp)
        {
            foreach (var ordering in sqlSelectOp.Orderings)
            {
                foreach (var col in ordering.Expression.GetAllReferencedColumns())
                {
                    yield return col;
                }
            }
        }

        public object Visit(Sql.Algebra.Source.SqlStatement sqlStatement, object data)
        {
            var cvd = (VisitorData)data;

            foreach (var col in sqlStatement.Columns.ToArray())
            {
                if (!cvd.NeededColumns.Contains(col))
                    sqlStatement.RemoveColumn(col);
            }

            return null;
        }

        public object Visit(Sql.Algebra.Source.SqlTable sqlTable, object data)
        {
            var cvd = (VisitorData)data;

            foreach (var col in sqlTable.Columns.ToArray())
            {
                if (!cvd.NeededColumns.Contains(col))
                    sqlTable.RemoveColumn(col);
            }

            return null;
        }
    }
}
