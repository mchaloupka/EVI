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
        private ISqlColumn dummyColumn1;
        private ISqlColumn dummyColumn2;


        [TestInitialize]
        public void TestInitialization()
        {
            this.optimizer = new ConcatenationInEqualConditionOptimizer();

            var dummyTable = new SqlTable("dummy");
            this.dummyColumn1 = dummyTable.GetColumn("col1");
            this.dummyColumn2 = dummyTable.GetColumn("col2");
        }

        [TestMethod]
        public void ConcatenationToConstEscaped()
        {
            var left = new ConcatenationExpr(new IExpression[] {
                new ConstantExpr("http://s.com/"),
                new ColumnExpr(dummyColumn1, true),
                new ConstantExpr("/"),
                new ColumnExpr(dummyColumn1, true)
            });

            var right = new ConstantExpr("http://s.com/12/45");

            var condition = new EqualsCondition(left, right);

            var visitData = new BaseConditionOptimizer.VisitData();
            visitData.IsOnTheFly = true;
            visitData.SecondRun = false;
            visitData.Context = null;

            var result = (ICondition)this.optimizer.Visit(condition, visitData);

            var expected = new AndCondition();
            expected.AddToCondition(new EqualsCondition(
                new ColumnExpr(dummyColumn1, true),
                new ConstantExpr("12")
                ));
            expected.AddToCondition(new EqualsCondition(
                new ColumnExpr(dummyColumn2, true),
                new ConstantExpr("45")
                ));

            AssertConditionsEqual(expected, result);
        }
    }
}
