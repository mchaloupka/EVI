using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Relational.Query;
using Slp.r2rml4net.Storage.Relational.Query.Conditions.Filter;
using Slp.r2rml4net.Storage.Relational.Query.Conditions.Source;
using Slp.r2rml4net.Storage.Relational.Utils.CodeGeneration;

namespace Slp.r2rml4net.Storage.Relational.Optimization.Optimizers
{
    /// <summary>
    /// The optimizer of <c>IS NULL</c> statements
    /// </summary>
    public class IsNullOptimizer
        : BaseRelationalOptimizer<IsNullOptimizerAggregatedValues>
    {
        /// <summary>
        /// Constructs an instance of <see cref="IsNullOptimizer"/>
        /// </summary>
        public IsNullOptimizer() 
            : base(new IsNullOptimizerImplementation())
        { }

        /// <summary>
        /// Creates the initial data.
        /// </summary>
        protected override IsNullOptimizerAggregatedValues CreateInitialData()
        {
            return new IsNullOptimizerAggregatedValues();
        }

        /// <summary>
        /// The implementation of <see cref="IsNullOptimizer"/>
        /// </summary>
        private class IsNullOptimizerImplementation
            : BaseRelationalOptimizerImplementation<IsNullOptimizerAggregatedValues>
        {
            
        }
    }

    /// <summary>
    /// The aggregation result of the <see cref="IsNullOptimizer"/>.
    /// </summary>
    public class IsNullOptimizerAggregatedValues
    {
        /// <summary>
        /// The is null columns
        /// </summary>
        private readonly Dictionary<ICalculusVariable, IsNullCondition> _isNullConditions;

        /// <summary>
        /// The is not null columns
        /// </summary>
        private readonly Dictionary<ICalculusVariable, IsNullCondition> _isNotNullConditions;

        /// <summary>
        /// Creates an instance of <see cref="IsNullOptimizerAggregatedValues"/>
        /// </summary>
        public IsNullOptimizerAggregatedValues()
        {
            _isNullConditions = new Dictionary<ICalculusVariable, IsNullCondition>();
            _isNotNullConditions = new Dictionary<ICalculusVariable, IsNullCondition>();
        }

        /// <summary>
        /// Merges with another aggregation result.
        /// </summary>
        /// <param name="other">The other.</param>
        public void MergeWith(IsNullOptimizerAggregatedValues other)
        {
            MergeWith(_isNullConditions, other._isNullConditions);
            MergeWith(_isNotNullConditions, other._isNotNullConditions);
        }

        /// <summary>
        /// Merges the <paramref name="source"/> dictionary with the <paramref name="with"/> dictionary
        /// </summary>
        private void MergeWith(Dictionary<ICalculusVariable, IsNullCondition> source, Dictionary<ICalculusVariable, IsNullCondition> with)
        {
            foreach (var item in with.Keys.ToArray())
            {
                if (!source.ContainsKey(item))
                    source.Add(item, with[item]);
            }
        }

        /// <summary>
        /// Intersects with another aggregation result.
        /// </summary>
        /// <param name="other">The other.</param>
        public void IntersectsWith(IsNullOptimizerAggregatedValues other)
        {
            IntersectsWith(_isNullConditions, other._isNullConditions);
            IntersectsWith(_isNotNullConditions, other._isNotNullConditions);
        }

        /// <summary>
        /// Intersects the <paramref name="source"/> dictionary with the <paramref name="with"/> dictionary
        /// </summary>
        private void IntersectsWith(Dictionary<ICalculusVariable, IsNullCondition> source, Dictionary<ICalculusVariable, IsNullCondition> with)
        {
            foreach (var item in source.Keys.ToArray())
            {
                if (!with.ContainsKey(item))
                    source.Remove(item);
            }
        }

        /// <summary>
        /// Gets the inverse.
        /// </summary>
        public IsNullOptimizerAggregatedValues GetInverse()
        {
            var res = new IsNullOptimizerAggregatedValues();
            MergeWith(res._isNotNullConditions, _isNullConditions);
            MergeWith(res._isNullConditions, _isNotNullConditions);
            return res;
        }

        /// <summary>
        /// Adds the is null condition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        public void AddIsNullCondition(IsNullCondition condition)
        {
            _isNullConditions.Add(condition.Variable, condition);
        }

        /// <summary>
        /// Determines whether the column is in "is not null list".
        /// </summary>
        /// <param name="calculusVariable">The calculus variable.</param>
        public bool IsInNotNullColumns(ICalculusVariable calculusVariable)
        {
            return _isNotNullConditions.ContainsKey(calculusVariable);
        }

        /// <summary>
        /// Determines whether the column is in "is not null list".
        /// </summary>
        /// <param name="calculusVariable">The calculus variable.</param>
        public bool IsInNullColumns(ICalculusVariable calculusVariable)
        {
            return _isNullConditions.ContainsKey(calculusVariable);
        }

        /// <summary>
        /// Determines whether the condition is in "is null list" as the reason.
        /// </summary>
        /// <param name="condition">The condition.</param>
        public bool IsInNullColumns(IsNullCondition condition)
        {
            if (_isNullConditions.ContainsKey(condition.Variable))
            {
                return condition == _isNullConditions[condition.Variable];
            }

            return false;
        }
    }
}
