using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Slp.Evi.Storage.Common.Algebra;
using Slp.Evi.Storage.Relational.PostProcess.Optimizers;
using Slp.Evi.Storage.Relational.Query.Conditions.Filter;
using Slp.Evi.Storage.Relational.Query.Expressions;

namespace Slp.Evi.Test.Unit.Relational.PostProcess.Optimizers
{
    [TestClass]
    public class ConstantExpressionEqualityTest
        : BaseOptimizerTest<object>
    {
        private ConstantExpressionEqualityOptimizer _optimizer;

        [TestInitialize]
        public void TestInitialization()
        {
            _optimizer = new ConstantExpressionEqualityOptimizer(NullLogger<ConstantExpressionEqualityOptimizer>.Instance);
        }

        [TestMethod]
        public void ConstantEquality_SameStrings_Prefix()
        {
            var queryContext = GenerateQueryContext();

            var left = new ConstantExpression("http://s.com/", queryContext);
            var right = new ConstantExpression("http://s.com/", queryContext);

            var condition = new ComparisonCondition(left, right, ComparisonTypes.EqualTo);

            var result = _optimizer.TransformFilterCondition(condition, GetContext(queryContext));

            var expected = new AlwaysTrueCondition();

            AssertFilterConditionsEqual(expected, result);
        }

        [TestMethod]
        public void ConstantEquality_StringDifferents_Prefix()
        {
            var queryContext = GenerateQueryContext();

            var left = new ConstantExpression("http://s.com/", queryContext);
            var right = new ConstantExpression("http://s.com/2", queryContext);

            var condition = new ComparisonCondition(left, right, ComparisonTypes.EqualTo);

            var result = _optimizer.TransformFilterCondition(condition, GetContext(queryContext));

            var expected = new AlwaysFalseCondition();

            AssertFilterConditionsEqual(expected, result);
        }

        [TestMethod]
        public void ConstantEquality_DifferentType_Prefix()
        {
            var queryContext = GenerateQueryContext();

            var left = new ConstantExpression("2", queryContext);
            var right = new ConstantExpression(2, queryContext);

            var condition = new ComparisonCondition(left, right, ComparisonTypes.EqualTo);

            var result = _optimizer.TransformFilterCondition(condition, GetContext(queryContext));

            var expected = new AlwaysFalseCondition();

            AssertFilterConditionsEqual(expected, result);
        }
    }
}
