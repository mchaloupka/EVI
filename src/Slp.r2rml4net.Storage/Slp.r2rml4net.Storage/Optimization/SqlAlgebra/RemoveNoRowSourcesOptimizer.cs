using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Sql.Algebra;
using Slp.r2rml4net.Storage.Sql.Algebra.Condition;
using Slp.r2rml4net.Storage.Sql.Algebra.Operator;
using Slp.r2rml4net.Storage.Sql.Binders;

namespace Slp.r2rml4net.Storage.Optimization.SqlAlgebra
{
    public class RemoveNoRowSourcesOptimizer : ISqlAlgebraOptimizer, ISqlSourceVisitor
    {
        public INotSqlOriginalDbSource ProcessAlgebra(INotSqlOriginalDbSource algebra, Query.QueryContext context)
        {
            return (INotSqlOriginalDbSource)algebra.Accept(this, context);
        }

        public object Visit(NoRowSource noRowSource, object data)
        {
            return noRowSource;
        }

        public object Visit(SingleEmptyRowSource singleEmptyRowSource, object data)
        {
            return singleEmptyRowSource;
        }

        public object Visit(SqlSelectOp sqlSelectOp, object data)
        {
            if (sqlSelectOp.Conditions.OfType<AlwaysFalseCondition>().Any())
                return CreateNoRowSource(sqlSelectOp);

            if (sqlSelectOp.JoinSources.Select(x => x.Condition).OfType<AlwaysFalseCondition>().Any())
                return CreateNoRowSource(sqlSelectOp);

            var originalSource = (ISqlSource)sqlSelectOp.OriginalSource.Accept(this, data);

            if (originalSource is NoRowSource)
                return CreateNoRowSource(sqlSelectOp);

            if (sqlSelectOp.JoinSources.Select(x => x.Source).Select(x => x.Accept(this, data)).OfType<NoRowSource>().Any())
                return CreateNoRowSource(sqlSelectOp);

            return sqlSelectOp;
        }

        public object Visit(SqlUnionOp sqlUnionOp, object data)
        {
            List<ISqlSource> toRemove = new List<ISqlSource>();

            foreach (var source in sqlUnionOp.Sources)
            {
                var accepted = (ISqlSource)source.Accept(this, data);

                if (accepted is NoRowSource)
                    toRemove.Add(source);
            }

            if (toRemove.Count == sqlUnionOp.Sources.Count())
                return CreateNoRowSource(sqlUnionOp);

            foreach (var source in toRemove)
            {
                sqlUnionOp.RemoveSource(source);
            }

            return sqlUnionOp;
        }

        public object Visit(Sql.Algebra.Source.SqlStatement sqlStatement, object data)
        {
            return sqlStatement;
        }

        public object Visit(Sql.Algebra.Source.SqlTable sqlTable, object data)
        {
            return sqlTable;
        }

        private NoRowSource CreateNoRowSource(INotSqlOriginalDbSource sqlSelectOp)
        {
            var noRowSource = new NoRowSource();

            foreach (var valBinder in sqlSelectOp.ValueBinders)
            {
                noRowSource.AddValueBinder(new BlankValueBinder(valBinder.VariableName));
            }

            return noRowSource;
        }
    }
}
