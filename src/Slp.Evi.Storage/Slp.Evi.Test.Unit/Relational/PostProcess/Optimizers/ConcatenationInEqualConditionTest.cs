using System.Collections.Generic;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Slp.Evi.Storage.Common.Algebra;
using Slp.Evi.Storage.Relational.PostProcess.Optimizers;
using Slp.Evi.Storage.Relational.Query;
using Slp.Evi.Storage.Relational.Query.Conditions.Filter;
using Slp.Evi.Storage.Relational.Query.Expressions;

namespace Slp.Evi.Test.Unit.Relational.PostProcess.Optimizers
{
    [TestClass]
    public class ConcatenationInEqualConditionTest
        : BaseOptimizerTest<object>
    {
        private ConcatenationInEqualConditionOptimizer _optimizer;

        [TestInitialize]
        public void TestInitialization()
        {
            _optimizer = new ConcatenationInEqualConditionOptimizer(NullLogger<ConcatenationInEqualConditionOptimizer>.Instance);
        }

        [TestMethod]
        public void ConcatenationToConstEscaped_Prefix()
        {
            var queryContext = GenerateQueryContext();
            var dummyTable = GetDummyTable(queryContext);
            var dummyColumn1 = dummyTable.GetVariable("col1");

            var left = new ConcatenationExpression(new List<IExpression> {
                new ConstantExpression("http://s.com/", queryContext),
                new ColumnExpression(dummyColumn1, true)
            }, queryContext.Db.SqlTypeForString);

            var right = new ConstantExpression("http://s.com/12", queryContext);

            var condition = new ComparisonCondition(left, right, ComparisonTypes.EqualTo);

            var result = _optimizer.TransformFilterCondition(condition, GetContext(queryContext));

            var expected = new ComparisonCondition(
                new ColumnExpression(dummyColumn1, true),
                new ConstantExpression("12", queryContext),
                ComparisonTypes.EqualTo);

            AssertFilterConditionsEqual(expected, result);
        }

        [TestMethod]
        public void ConcatenationToConstEscaped_Suffix()
        {
            var queryContext = GenerateQueryContext();
            var dummyTable = GetDummyTable(queryContext);
            var dummyColumn1 = dummyTable.GetVariable("col1");

            var left = new ConcatenationExpression(new List<IExpression> {
                new ColumnExpression(dummyColumn1, true),
                new ConstantExpression("/s", queryContext)
            }, queryContext.Db.SqlTypeForString);

            var right = new ConstantExpression("12/s", queryContext);

            var condition = new ComparisonCondition(left, right, ComparisonTypes.EqualTo);

            var result = _optimizer.TransformFilterCondition(condition, GetContext(queryContext));

            var expected = new ComparisonCondition(
                new ColumnExpression(dummyColumn1, true),
                new ConstantExpression("12", queryContext), ComparisonTypes.EqualTo);

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
                new ColumnExpression(dummyColumn1, true),
                new ConstantExpression("/s", queryContext)
            }, queryContext.Db.SqlTypeForString);

            var right = new ConstantExpression("http://s.com/12/s", queryContext);

            var condition = new ComparisonCondition(left, right, ComparisonTypes.EqualTo);

            var result = _optimizer.TransformFilterCondition(condition, GetContext(queryContext));

            var expected = new ComparisonCondition(
                new ColumnExpression(dummyColumn1, true),
                new ConstantExpression("12", queryContext), ComparisonTypes.EqualTo);

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
                new ColumnExpression(dummyColumn1, true),
                new ConstantExpression("/s/", queryContext),
                new ColumnExpression(dummyColumn2, true),
                new ConstantExpression("/e", queryContext),
            }, queryContext.Db.SqlTypeForString);

            var right = new ConstantExpression("http://s.com/12/s/14/e", queryContext);

            var condition = new ComparisonCondition(left, right, ComparisonTypes.EqualTo);

            var result = _optimizer.TransformFilterCondition(condition, GetContext(queryContext));

            var expected = new ConjunctionCondition(new List<IFilterCondition>()
            {
                new ComparisonCondition(
                    new ColumnExpression(dummyColumn1, true),
                    new ConstantExpression("12", queryContext), ComparisonTypes.EqualTo),
                new ComparisonCondition(
                    new ColumnExpression(dummyColumn2, true),
                    new ConstantExpression("14", queryContext), ComparisonTypes.EqualTo)
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
                new ColumnExpression(dummyColumn1, false)
            }, queryContext.Db.SqlTypeForString);

            var right = new ConstantExpression("http://s.com/12", queryContext);

            var condition = new ComparisonCondition(left, right, ComparisonTypes.EqualTo);

            var result = _optimizer.TransformFilterCondition(condition, GetContext(queryContext));

            var expected = new ComparisonCondition(
                new ColumnExpression(dummyColumn1, false),
                new ConstantExpression("12", queryContext), ComparisonTypes.EqualTo);

            AssertFilterConditionsEqual(expected, result);
        }

        [TestMethod]
        public void ConcatenationToConstNotEscaped_Suffix()
        {
            var queryContext = GenerateQueryContext();
            var dummyTable = GetDummyTable(queryContext);
            var dummyColumn1 = dummyTable.GetVariable("col1");

            var left = new ConcatenationExpression(new List<IExpression> {
                new ColumnExpression(dummyColumn1, false),
                new ConstantExpression("/s", queryContext)
            }, queryContext.Db.SqlTypeForString);

            var right = new ConstantExpression("12/s", queryContext);

            var condition = new ComparisonCondition(left, right, ComparisonTypes.EqualTo);

            var result = _optimizer.TransformFilterCondition(condition, GetContext(queryContext));

            var expected = new ComparisonCondition(
                new ColumnExpression(dummyColumn1, false),
                new ConstantExpression("12", queryContext), ComparisonTypes.EqualTo);

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
                new ColumnExpression(dummyColumn1, false),
                new ConstantExpression("/s", queryContext)
            }, queryContext.Db.SqlTypeForString);

            var right = new ConstantExpression("http://s.com/12/s", queryContext);

            var condition = new ComparisonCondition(left, right, ComparisonTypes.EqualTo);

            var result = _optimizer.TransformFilterCondition(condition, GetContext(queryContext));

            var expected = new ComparisonCondition(
                new ColumnExpression(dummyColumn1, false),
                new ConstantExpression("12", queryContext), ComparisonTypes.EqualTo);

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
                new ColumnExpression(dummyColumn1, false),
                new ConstantExpression("/s/", queryContext),
                new ColumnExpression(dummyColumn2, false),
                new ConstantExpression("/e", queryContext),
            }, queryContext.Db.SqlTypeForString);

            var right = new ConstantExpression("http://s.com/12/s/14/e", queryContext);

            var condition = new ComparisonCondition(left, right, ComparisonTypes.EqualTo);

            var result = _optimizer.TransformFilterCondition(condition, GetContext(queryContext));

            var expected = new ComparisonCondition(
                new ConcatenationExpression(new List<IExpression>()
                {
                    new ColumnExpression(dummyColumn1, false),
                    new ConstantExpression("/s/", queryContext),
                    new ColumnExpression(dummyColumn2, false)
                }, queryContext.Db.SqlTypeForString),
                new ConstantExpression("12/s/14", queryContext), ComparisonTypes.EqualTo);

            AssertFilterConditionsEqual(expected, result);
        }
    }
}
