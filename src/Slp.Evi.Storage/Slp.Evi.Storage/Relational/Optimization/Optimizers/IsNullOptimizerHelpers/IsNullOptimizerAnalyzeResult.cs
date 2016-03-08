using System;
using System.Collections.Generic;
using Slp.Evi.Storage.Relational.Query;

namespace Slp.Evi.Storage.Relational.Optimization.Optimizers.IsNullOptimizerHelpers
{
    /// <summary>
    /// The overall result of <see cref="IsNullCalculator"/>
    /// </summary>
    public class IsNullOptimizerAnalyzeResult
    {
        /// <summary>
        /// The stored values
        /// </summary>
        private readonly Dictionary<ICalculusSource, IsNullOptimizerAggregatedValues> _storedValues;

        /// <summary>
        /// The nested negations count
        /// </summary>
        private readonly Dictionary<ICalculusSource, int> _nestedNegationsCount; 

        /// <summary>
        /// The current source
        /// </summary>
        public ICalculusSource CurrentSource => _currentSources.Peek();

        /// <summary>
        /// The stack of current sources
        /// </summary>
        private readonly Stack<ICalculusSource> _currentSources; 

        /// <summary>
        /// Initializes a new instance of the <see cref="IsNullOptimizerAnalyzeResult"/> class.
        /// </summary>
        /// <param name="currentSource">The current source.</param>
        public IsNullOptimizerAnalyzeResult(ICalculusSource currentSource)
        {
            _nestedNegationsCount = new Dictionary<ICalculusSource, int>();
            _currentSources = new Stack<ICalculusSource>();
            _currentSources.Push(currentSource);

            _storedValues = new Dictionary<ICalculusSource, IsNullOptimizerAggregatedValues>();
        }

        /// <summary>
        /// Gets the result.
        /// </summary>
        /// <param name="source">The source.</param>
        public IsNullOptimizerAggregatedValues GetValue(ICalculusSource source)
        {
            if (!_storedValues.ContainsKey(source))
            {
                _storedValues.Add(source, new IsNullOptimizerAggregatedValues());
            }

            return _storedValues[source];
        }

        /// <summary>
        /// Gets the result for the current source.
        /// </summary>
        public IsNullOptimizerAggregatedValues GetCurrentValue() => GetValue(CurrentSource);

        /// <summary>
        /// Copies data to another instance.
        /// </summary>
        public void CopyTo(IsNullOptimizerAnalyzeResult data)
        {
            foreach (var source in _storedValues.Keys)
            {
                data.GetValue(source).MergeWith(GetValue(source));
            }
        }

        /// <summary>
        /// Pushes the source as the current source.
        /// </summary>
        /// <param name="source">The source.</param>
        public void PushCurrentSource(ICalculusSource source)
        {
            _currentSources.Push(source);
        }

        /// <summary>
        /// Removes the last current source and sets the previous one.
        /// </summary>
        public void PopCurrentSource()
        {
            _currentSources.Pop();
        }

        /// <summary>
        /// Increases the current negation count.
        /// </summary>
        public void EnterNegationCondition()
        {
            if (!_nestedNegationsCount.ContainsKey(CurrentSource))
            {
                _nestedNegationsCount.Add(CurrentSource, 1);
            }
            else
            {
                _nestedNegationsCount[CurrentSource]++;
            }
        }

        /// <summary>
        /// Decreases the current negation count.
        /// </summary>
        public void LeaveNegationCondition()
        {
            if (_nestedNegationsCount.ContainsKey(CurrentSource))
            {
                _nestedNegationsCount[CurrentSource]--;
            }
            else
            {
                throw new InvalidOperationException("Cannot decrease negation count without proper increment");
            }
        }

        /// <summary>
        /// Determines whether we are currently in a negated condition.
        /// </summary>
        public bool IsCurrentlyNegated
        {
            get
            {
                if (_nestedNegationsCount.ContainsKey(CurrentSource))
                {
                    return (_nestedNegationsCount[CurrentSource] % 2) == 1;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Determines whether there is a stored value for the desired source.
        /// </summary>
        /// <param name="calculusSource">The calculus source.</param>
        public bool HasValueForSource(ICalculusSource calculusSource)
        {
            return _storedValues.ContainsKey(calculusSource);
        }
    }
}
