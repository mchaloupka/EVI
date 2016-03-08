using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Slp.Evi.Storage.Relational.Optimization.Optimizers;
using Slp.Evi.Storage.Relational.Query;
using Slp.Evi.Storage.Relational.Query.Conditions.Filter;
using Slp.Evi.Storage.Relational.Query.Expressions;

namespace Slp.Evi.Test.Unit.Relational.Optimization.Optimizers
{
    [TestClass]
    public class ConcatenationInEqualConditionTest 
        : BaseOptimizerTest<object>
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
            var dummyColumn1 = dummyTable.GetVariable("col1");

            var left = new ConcatenationExpression(new List<IExpression> {
                new ConstantExpression("http://s.com/", queryContext),
                new ColumnExpression(queryContext, dummyColumn1, true)
            }, queryContext.Db.SqlTypeForString);

            var right = new ConstantExpression("http://s.com/12", queryContext);

            var condition = new EqualExpressionCondition(left, right);

            var result = optimizer.TransformFilterCondition(condition, GetContext(queryContext));

            var expected = (new EqualExpressionCondition(
                new ColumnExpression(queryContext, dummyColumn1, true),
                new ConstantExpression("12", queryContext)
                ));

            AssertFilterConditionsEqual(expected, result);
        }

        [TestMethod]
        public void ConcatenationToConstEscaped_Suffix()
        {
            var queryContext = GenerateQueryContext();
            var dummyTable = GetDummyTable(queryContext);
            var dummyColumn1 = dummyTable.GetVariable("col1");

            var left = new ConcatenationExpression(new List<IExpression> {
                new ColumnExpression(queryContext, dummyColumn1, true),
                new ConstantExpression("/s", queryContext)
            }, queryContext.Db.SqlTypeForString);

            var right = new ConstantExpression("12/s", queryContext);

            var condition = new EqualExpressionCondition(left, right);

            var result = optimizer.TransformFilterCondition(condition, GetContext(queryContext));

            var expected = (new EqualExpressionCondition(
                new ColumnExpression(queryContext, dummyColumn1, true),
                new ConstantExpression("12", queryContext)
                ));

            AssertFilterConditionsEqual(expected, result);
        }

        [TestMethod]
        public void ConcatenationToConstEscaped_Both()
        {
            var queryContext = GenerateQueryContext();
            var dummyTable = GetDummyTable(queryContext);
            var dummyColumn1 = dummyTable.GetVariable("col1");

            var left = new ConcatenationExpression(new List<IExpression> {
                new ConstantExpression("http://s.com/", queryContext),
                new ColumnExpression(queryContext, dummyColumn1, true),
                new ConstantExpression("/s", queryContext)
            }, queryContext.Db.SqlTypeForString);

            var right = new ConstantExpression("http://s.com/12/s", queryContext);

            var condition = new EqualExpressionCondition(left, right);

            var result = optimizer.TransformFilterCondition(condition, GetContext(queryContext));

            var expected = (new EqualExpressionCondition(
                new ColumnExpression(queryContext, dummyColumn1, true),
                new ConstantExpression("12", queryContext)
                ));

            AssertFilterConditionsEqual(expected, result);
        }

        [TestMethod]
        public void ConcatenationToConstEscaped_TwoColumns()
        {
            var queryContext = GenerateQueryContext();
            var dummyTable = GetDummyTable(queryContext);
            var dummyColumn1 = dummyTable.GetVariable("col1");
            var dummyColumn2 = dummyTable.GetVariable("col2");

            var left = new ConcatenationExpression(new List<IExpression> {
                new ConstantExpression("http://s.com/", queryContext),
                new ColumnExpression(queryContext, dummyColumn1, true),
                new ConstantExpression("/s/", queryContext),
                new ColumnExpression(queryContext, dummyColumn2, true),
                new ConstantExpression("/e", queryContext),
            }, queryContext.Db.SqlTypeForString);

            var right = new ConstantExpression("http://s.com/12/s/14/e", queryContext);

            var condition = new EqualExpressionCondition(left, right);

            var result = optimizer.TransformFilterCondition(condition, GetContext(queryContext));

            var expected = new ConjunctionCondition(new List<IFilterCondition>()
            {
                new EqualExpressionCondition(
                    new ColumnExpression(queryContext, dummyColumn1, true),
                    new ConstantExpression("12", queryContext)
                ),
                new EqualExpressionCondition(
                    new ColumnExpression(queryContext, dummyColumn2, true),
                    new ConstantExpression("14", queryContext)
                )
            });

            AssertFilterConditionsEqual(expected, result);
        }

        [TestMethod]
        public void ConcatenationToConstNotEscaped_Prefix()
        {
            var queryContext = GenerateQueryContext();
            var dummyTable = GetDummyTable(queryContext);
            var dummyColumn1 = dummyTable.GetVariable("col1");

            var left = new ConcatenationExpression(new List<IExpression> {
                new ConstantExpression("http://s.com/", queryContext),
                new ColumnExpression(queryContext, dummyColumn1, false)
            }, queryContext.Db.SqlTypeForString);

            var right = new ConstantExpression("http://s.com/12", queryContext);

            var condition = new EqualExpressionCondition(left, right);

            var result = optimizer.TransformFilterCondition(condition, GetContext(queryContext));

            var expected = (new EqualExpressionCondition(
                new ColumnExpression(queryContext, dummyColumn1, false),
                new ConstantExpression("12", queryContext)
                ));

            AssertFilterConditionsEqual(expected, result);
        }

        [TestMethod]
        public void ConcatenationToConstNotEscaped_Suffix()
        {
            var queryContext = GenerateQueryContext();
            var dummyTable = GetDummyTable(queryContext);
            var dummyColumn1 = dummyTable.GetVariable("col1");

            var left = new ConcatenationExpression(new List<IExpression> {
                new ColumnExpression(queryContext, dummyColumn1, false),
                new ConstantExpression("/s", queryContext)
            }, queryContext.Db.SqlTypeForString);

            var right = new ConstantExpression("12/s", queryContext);

            var condition = new EqualExpressionCondition(left, right);

            var result = optimizer.TransformFilterCondition(condition, GetContext(queryContext));

            var expected = (new EqualExpressionCondition(
                new ColumnExpression(queryContext, dummyColumn1, false),
                new ConstantExpression("12", queryContext)
                ));

            AssertFilterConditionsEqual(expected, result);
        }

        [TestMethod]
        public void ConcatenationToConstNotEscaped_Both()
        {
            var queryContext = GenerateQueryContext();
            var dummyTable = GetDummyTable(queryContext);
            var dummyColumn1 = dummyTable.GetVariable("col1");

            var left = new ConcatenationExpression(new List<IExpression> {
                new ConstantExpression("http://s.com/", queryContext),
                new ColumnExpression(queryContext, dummyColumn1, false),
                new ConstantExpression("/s", queryContext)
            }, queryContext.Db.SqlTypeForString);

            var right = new ConstantExpression("http://s.com/12/s", queryContext);

            var condition = new EqualExpressionCondition(left, right);

            var result = optimizer.TransformFilterCondition(condition, GetContext(queryContext));

            var expected = (new EqualExpressionCondition(
                new ColumnExpression(queryContext, dummyColumn1, false),
                new ConstantExpression("12", queryContext)
                ));

            AssertFilterConditionsEqual(expected, result);
        }

        [TestMethod]
        public void ConcatenationToConstNotEscaped_TwoColumns()
        {
            var queryContext = GenerateQueryContext();
            var dummyTable = GetDummyTable(queryContext);
            var dummyColumn1 = dummyTable.GetVariable("col1");
            var dummyColumn2 = dummyTable.GetVariable("col2");

            var left = new ConcatenationExpression(new List<IExpression> {
                new ConstantExpression("http://s.com/", queryContext),
                new ColumnExpression(queryContext, dummyColumn1, false),
                new ConstantExpression("/s/", queryContext),
                new ColumnExpression(queryContext, dummyColumn2, false),
                new ConstantExpression("/e", queryContext),
            }, queryContext.Db.SqlTypeForString);

            var right = new ConstantExpression("http://s.com/12/s/14/e", queryContext);

            var condition = new EqualExpressionCondition(left, right);

            var result = optimizer.TransformFilterCondition(condition, GetContext(queryContext));

            var expected = new EqualExpressionCondition(
                new ConcatenationExpression(new List<IExpression>()
                {
                    new ColumnExpression(queryContext, dummyColumn1, false),
                    new ConstantExpression("/s/", queryContext),
                    new ColumnExpression(queryContext, dummyColumn2, false)
                }, queryContext.Db.SqlTypeForString),
                new ConstantExpression("12/s/14", queryContext)
            );

            AssertFilterConditionsEqual(expected, result);
        }
    }
}
