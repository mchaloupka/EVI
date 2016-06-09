using Microsoft.VisualStudio.TestTools.UnitTesting;
using Slp.Evi.Storage.Common.Algebra;
using Slp.Evi.Storage.Relational.PostProcess.Optimizers;
using Slp.Evi.Storage.Relational.Query;
using Slp.Evi.Storage.Relational.Query.Conditions.Filter;
using Slp.Evi.Storage.Relational.Query.Expressions;

namespace Slp.Evi.Test.Unit.Relational.PostProcess.Optimizers
{
    [TestClass]
    public class CaseExpressionToConditionOptimizerTest
        : BaseOptimizerTest<object>
    {
        private CaseExpressionToConditionOptimizer _optimizer;

        [TestInitialize]
        public void TestInitialization()
        {
            this._optimizer = new CaseExpressionToConditionOptimizer();
        }

        [TestMethod]
        public void TestNoChange()
        {
            var queryContext = GenerateQueryContext();
            var left = new ConstantExpression(5, queryContext);
            var right = new ConstantExpression(6, queryContext);
            var condition = new ComparisonCondition(left, right, ComparisonTypes.GreaterThan);

            var result = _optimizer.TransformFilterCondition(condition, GetContext(queryContext));

            AssertFilterConditionsEqual(condition, result);
        }

        [TestMethod]
        public void TestLeftCase()
        {
            var queryContext = GenerateQueryContext();

            var dummyTable = GetDummyTable(queryContext);
            var dummyColumn1 = dummyTable.GetVariable("col1");
            var dummyColumn2 = dummyTable.GetVariable("col2");
            var dummyColumn3 = dummyTable.GetVariable("col3");

            var left1 = new ConstantExpression(5, queryContext);
            var left2 = new ConstantExpression(7, queryContext);
            var cond1 = new EqualVariablesCondition(dummyColumn1, dummyColumn2);
            var cond2 = new EqualVariablesCondition(dummyColumn1, dummyColumn3);
            var left = new CaseExpression(new CaseExpression.Statement[]
            {
                new CaseExpression.Statement(cond1, left1),
                new CaseExpression.Statement(cond2, left2)
            });

            var right = new ConstantExpression(6, queryContext);
            var condition = new ComparisonCondition(left, right, ComparisonTypes.GreaterThan);

            var result = _optimizer.TransformFilterCondition(condition, GetContext(queryContext));

            var expected = new DisjunctionCondition(new IFilterCondition[]
            {
                new ConjunctionCondition(new IFilterCondition[]
                {
                    cond1,
                    new ComparisonCondition(left1, right, condition.ComparisonType)
                }),
                new ConjunctionCondition(new IFilterCondition[]
                {
                    cond2,
                    new ComparisonCondition(left2, right, condition.ComparisonType)
                })
            });

            AssertFilterConditionsEqual(expected, result);
        }

        [TestMethod]
        public void TestRightCase()
        {
            var queryContext = GenerateQueryContext();

            var dummyTable = GetDummyTable(queryContext);
            var dummyColumn1 = dummyTable.GetVariable("col1");
            var dummyColumn2 = dummyTable.GetVariable("col2");
            var dummyColumn3 = dummyTable.GetVariable("col3");

            var right1 = new ConstantExpression(5, queryContext);
            var right2 = new ConstantExpression(7, queryContext);
            var cond1 = new EqualVariablesCondition(dummyColumn1, dummyColumn2);
            var cond2 = new EqualVariablesCondition(dummyColumn1, dummyColumn3);
            var right = new CaseExpression(new CaseExpression.Statement[]
            {
                new CaseExpression.Statement(cond1, right1),
                new CaseExpression.Statement(cond2, right2)
            });

            var left = new ConstantExpression(6, queryContext);
            var condition = new ComparisonCondition(left, right, ComparisonTypes.GreaterThan);

            var result = _optimizer.TransformFilterCondition(condition, GetContext(queryContext));

            var expected = new DisjunctionCondition(new IFilterCondition[]
            {
                new ConjunctionCondition(new IFilterCondition[]
                {
                    cond1,
                    new ComparisonCondition(left, right1, condition.ComparisonType)
                }),
                new ConjunctionCondition(new IFilterCondition[]
                {
                    cond2,
                    new ComparisonCondition(left, right2, condition.ComparisonType)
                })
            });

            AssertFilterConditionsEqual(expected, result);
        }

        [TestMethod]
        public void TestNestedLeftCase()
        {
            var queryContext = GenerateQueryContext();

            var dummyTable = GetDummyTable(queryContext);
            var dummyColumn1 = dummyTable.GetVariable("col1");
            var dummyColumn2 = dummyTable.GetVariable("col2");
            var dummyColumn3 = dummyTable.GetVariable("col3");
            var dummyColumn4 = dummyTable.GetVariable("col4");

            var left1_1 = new ConstantExpression(5, queryContext);
            var left1_2 = new ConstantExpression(7, queryContext);
            var cond1_1 = new EqualVariablesCondition(dummyColumn1, dummyColumn2);
            var cond1_2 = new EqualVariablesCondition(dummyColumn1, dummyColumn3);
            var cond1 = new EqualVariablesCondition(dummyColumn2, dummyColumn3);
            var left1 = new CaseExpression(new CaseExpression.Statement[]
            {
                new CaseExpression.Statement(cond1_1, left1_1),
                new CaseExpression.Statement(cond1_2, left1_2)
            });
            var left2 = new ConstantExpression(6, queryContext);
            var cond2 = new EqualVariablesCondition(dummyColumn2, dummyColumn4);
            var left = new CaseExpression(new CaseExpression.Statement[]
            {
                new CaseExpression.Statement(cond1, left1),
                new CaseExpression.Statement(cond2, left2)
            });

            var right = new ConstantExpression(6, queryContext);
            var condition = new ComparisonCondition(left, right, ComparisonTypes.GreaterThan);

            var result = _optimizer.TransformFilterCondition(condition, GetContext(queryContext));

            var expected = new DisjunctionCondition(new IFilterCondition[]
            {
                new ConjunctionCondition(new IFilterCondition[]
                {
                    cond1,
                    cond1_1,
                    new ComparisonCondition(left1_1, right, condition.ComparisonType)
                }),
                new ConjunctionCondition(new IFilterCondition[]
                {
                    cond1,
                    cond1_2,
                    new ComparisonCondition(left1_2, right, condition.ComparisonType)
                }),
                new ConjunctionCondition(new IFilterCondition[]
                {
                    cond2,
                    new ComparisonCondition(left2, right, condition.ComparisonType)
                })
            });

            AssertFilterConditionsEqual(expected, result);
        }
    }
}
