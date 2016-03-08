using System.Collections.Generic;
using System.Linq;
using Slp.Evi.Storage.Relational.Query;
using Slp.Evi.Storage.Relational.Query.Conditions.Filter;

namespace Slp.Evi.Storage.Relational.Optimization.Optimizers.IsNullOptimizerHelpers
{
    /// <summary>
    /// The aggregation result of the <see cref="IsNullCalculator"/>.
    /// </summary>
    public class IsNullOptimizerAggregatedValues
    {
        /// <summary>
        /// The is null columns
        /// </summary>
        private readonly Dictionary<ICalculusVariable, HashSet<IsNullCondition>> _isNullConditions;

        /// <summary>
        /// The is not null columns
        /// </summary>
        private readonly Dictionary<ICalculusVariable, HashSet<IsNullCondition>> _isNotNullConditions;

        /// <summary>
        /// Creates an instance of <see cref="IsNullOptimizerAggregatedValues"/>
        /// </summary>
        public IsNullOptimizerAggregatedValues()
        {
            _isNullConditions = new Dictionary<ICalculusVariable, HashSet<IsNullCondition>>();
            _isNotNullConditions = new Dictionary<ICalculusVariable, HashSet<IsNullCondition>>();
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
        private void MergeWith(Dictionary<ICalculusVariable, HashSet<IsNullCondition>> source, Dictionary<ICalculusVariable, HashSet<IsNullCondition>> with)
        {
            foreach (var item in with.Keys.ToArray())
            {
                if (source.ContainsKey(item))
                {
                    if (source[item].Count > with[item].Count)
                    {
                        source[item] = with[item];
                    }
                }
                else
                {
                    source.Add(item, with[item]);
                }
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
        private void IntersectsWith(Dictionary<ICalculusVariable, HashSet<IsNullCondition>> source, Dictionary<ICalculusVariable, HashSet<IsNullCondition>> with)
        {
            foreach (var item in source.Keys.ToArray())
            {
                if (with.ContainsKey(item))
                {
                    source[item].UnionWith(with[item]);
                }
                else
                {
                    source.Remove(item);
                }
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
            if (!_isNullConditions.ContainsKey(condition.Variable))
            {
                _isNullConditions.Add(condition.Variable, new HashSet<IsNullCondition>());
                _isNullConditions[condition.Variable].Add(condition);
            }
        }

        /// <summary>
        /// Adds the is not null information.
        /// </summary>
        /// <param name="calculusVariable">The calculus variable.</param>
        public void AddIsNotNull(ICalculusVariable calculusVariable)
        {
            if (!_isNotNullConditions.ContainsKey(calculusVariable))
            {
                _isNotNullConditions.Add(calculusVariable, new HashSet<IsNullCondition>());
            }
            else
            {
                _isNotNullConditions[calculusVariable].Clear();
            }
        }

        /// <summary>
        /// Determines whether the column is in "is not null list".
        /// </summary>
        /// <param name="calculusVariable">The calculus variable.</param>
        public bool IsInNotNullConditions(ICalculusVariable calculusVariable)
        {
            return _isNotNullConditions.ContainsKey(calculusVariable);
        }

        /// <summary>
        /// Determines whether the condition is in "is not null list" as the reason.
        /// </summary>
        /// <param name="condition">The condition.</param>
        public bool IsInNotNullConditions(IsNullCondition condition)
        {
            if (_isNotNullConditions.ContainsKey(condition.Variable))
            {
                return _isNotNullConditions[condition.Variable].Contains(condition);
            }

            return false;
        }

        /// <summary>
        /// Determines whether the column is in "is not null list".
        /// </summary>
        /// <param name="calculusVariable">The calculus variable.</param>
        public bool IsInNullConditions(ICalculusVariable calculusVariable)
        {
            return _isNullConditions.ContainsKey(calculusVariable);
        }

        /// <summary>
        /// Determines whether the condition is in "is null list" as the reason.
        /// </summary>
        /// <param name="condition">The condition.</param>
        public bool IsInNullConditions(IsNullCondition condition)
        {
            if (_isNullConditions.ContainsKey(condition.Variable))
            {
                return _isNullConditions[condition.Variable].Contains(condition);
            }

            return false;
        }
    }
}