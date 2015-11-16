using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Relational.Query;
using Slp.r2rml4net.Storage.Relational.Query.Sources;

namespace Slp.r2rml4net.Storage.Relational.Optimization.Optimizers.IsNullOptimizerHelpers
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
        /// Determines whether there is a stored value for the desired source.
        /// </summary>
        /// <param name="calculusSource">The calculus source.</param>
        public bool HasValueForSource(ICalculusSource calculusSource)
        {
            return _storedValues.ContainsKey(calculusSource);
        }
    }
}
