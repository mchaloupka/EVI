using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Slp.r2rml4net.Storage.Optimization.SqlAlgebra;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sql.Algebra;
using Slp.r2rml4net.Storage.Sql.Algebra.Condition;
using Slp.r2rml4net.Storage.Sql.Algebra.Expression;
using Slp.r2rml4net.Storage.Sql.Algebra.Source;

namespace Slp.r2rml4net.Test.Unit.Optimization.SqlAlgebra
{
    [TestClass]
    public class ConcatenationInEqualConditionTest : BaseConditionTest
    {
        private ConcatenationInEqualConditionOptimizer optimizer;

        [TestInitialize]
        public void TestInitialization()
        {
            this.optimizer = new ConcatenationInEqualConditionOptimizer();
        }

        [TestMethod]
        public void ConcatenationToConstEscaped_Prefix()
        {
            var queryContext = GenerateQueryContext();
            var dummyTable = GetDummyTable(queryContext);
            var dummyColumn1 = dummyTable.GetColumn("col1");
            var dummyColumn2 = dummyTable.GetColumn("col2");

            var left = new ConcatenationExpr(new IExpression[] {
                new ConstantExpr("http://s.com/", queryContext),
                new ColumnExpr(dummyColumn1, true)
            }, queryContext);

            var right = new ConstantExpr("http://s.com/12", queryContext);

            var condition = new EqualsCondition(left, right);

            var result = (ICondition)this.optimizer.Visit(condition, GenerateInitialVisitData(queryContext));

            var expected = (new EqualsCondition(
                new ColumnExpr(dummyColumn1, true),
                new ConstantExpr("12", queryContext)
                ));

            AssertConditionsEqual(expected, result);
        }

        [Ignore]
        [TestMethod]
        public void ConcatenationToConstEscaped_Suffix()
        {
            var queryContext = GenerateQueryContext();
            var dummyTable = GetDummyTable(queryContext);
            var dummyColumn1 = dummyTable.GetColumn("col1");
            var dummyColumn2 = dummyTable.GetColumn("col2");

            var left = new ConcatenationExpr(new IExpression[] {
                new ColumnExpr(dummyColumn1, true),
                new ConstantExpr("/s", queryContext)
            }, queryContext);

            var right = new ConstantExpr("12/s", queryContext);

            var condition = new EqualsCondition(left, right);

            var result = (ICondition)this.optimizer.Visit(condition, GenerateInitialVisitData(queryContext));

            var expected = (new EqualsCondition(
                new ColumnExpr(dummyColumn1, true),
                new ConstantExpr("12", queryContext)
                ));

            AssertConditionsEqual(expected, result);
        }

        [Ignore]
        [TestMethod]
        public void ConcatenationToConstEscaped_Both()
        {
            var queryContext = GenerateQueryContext();
            var dummyTable = GetDummyTable(queryContext);
            var dummyColumn1 = dummyTable.GetColumn("col1");
            var dummyColumn2 = dummyTable.GetColumn("col2");

            var left = new ConcatenationExpr(new IExpression[] {
                new ConstantExpr("http://s.com/", queryContext),
                new ColumnExpr(dummyColumn1, true),
                new ConstantExpr("/s", queryContext)
            }, queryContext);

            var right = new ConstantExpr("http://s.com/12/s", queryContext);

            var condition = new EqualsCondition(left, right);

            var result = (ICondition)this.optimizer.Visit(condition, GenerateInitialVisitData(queryContext));

            var expected = (new EqualsCondition(
                new ColumnExpr(dummyColumn1, true),
                new ConstantExpr("12", queryContext)
                ));

            AssertConditionsEqual(expected, result);
        }

        [Ignore]
        [TestMethod]
        public void ConcatenationToConstEscaped_TwoColumns()
        {
            var queryContext = GenerateQueryContext();
            var dummyTable = GetDummyTable(queryContext);
            var dummyColumn1 = dummyTable.GetColumn("col1");
            var dummyColumn2 = dummyTable.GetColumn("col2");

            var left = new ConcatenationExpr(new IExpression[] {
                new ConstantExpr("http://s.com/", queryContext),
                new ColumnExpr(dummyColumn1, true),
                new ConstantExpr("/", queryContext),
                new ColumnExpr(dummyColumn1, true)
            }, queryContext);

            var right = new ConstantExpr("http://s.com/12/45", queryContext);

            var condition = new EqualsCondition(left, right);

            var result = (ICondition)this.optimizer.Visit(condition, GenerateInitialVisitData(queryContext));

            var expected = new AndCondition();
            expected.AddToCondition(new EqualsCondition(
                new ColumnExpr(dummyColumn1, true),
                new ConstantExpr("12", queryContext)
                ));
            expected.AddToCondition(new EqualsCondition(
                new ColumnExpr(dummyColumn2, true),
                new ConstantExpr("45", queryContext)
                ));

            AssertConditionsEqual(expected, result);
        }
    }
}
