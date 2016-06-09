using System.Collections.Generic;
using Slp.Evi.Storage.Relational.Query;
using Slp.Evi.Storage.Relational.Query.Sources;

namespace Slp.Evi.Storage.Relational.PostProcess.Optimizers.SelfJoinOptimizerHelpers
{
    /// <summary>
    /// The data for <see cref="SelfJoinOptimizer"/>
    /// </summary>
    public class SelfJoinOptimizerData
    {
        /// <summary>
        /// The variables map
        /// </summary>
        private readonly Dictionary<ICalculusVariable, ICalculusVariable> _variablesMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="SelfJoinOptimizerData"/> class.
        /// </summary>
        public SelfJoinOptimizerData()
        {
            _variablesMap = new Dictionary<ICalculusVariable, ICalculusVariable>();
        }

        /// <summary>
        /// Adds the information that the column was replaced.
        /// </summary>
        /// <param name="sqlColumn">The SQL column to be replaced.</param>
        /// <param name="targetColumn">The target column that will be used instead of <paramref name="sqlColumn"/>.</param>
        public void AddReplaceColumnInformation(SqlColumn sqlColumn, ICalculusVariable targetColumn)
        {
            _variablesMap.Add(sqlColumn, targetColumn);
        }

        /// <summary>
        /// Loads data from other context.
        /// </summary>
        /// <param name="data">The data.</param>
        public void LoadOtherData(SelfJoinOptimizerData data)
        {
            foreach (var calculusVariable in data._variablesMap.Keys)
            {
                _variablesMap.Add(calculusVariable, data._variablesMap[calculusVariable]);
            }
        }

        /// <summary>
        /// Determines whether the specified calculus variable is replaced.
        /// </summary>
        /// <param name="calculusVariable">The calculus variable.</param>
        /// <returns><c>true</c> if the specified calculus variable is replaced; otherwise, <c>false</c>.</returns>
        public bool IsReplaced(ICalculusVariable calculusVariable)
        {
            return _variablesMap.ContainsKey(calculusVariable);
        }

        /// <summary>
        /// Gets the variable that is replacing <paramref name="calculusVariable"/>.
        /// </summary>
        /// <param name="calculusVariable">The calculus variable.</param>
        public ICalculusVariable GetReplacingVariable(ICalculusVariable calculusVariable)
        {
            return _variablesMap[calculusVariable];
        }
    }
}
