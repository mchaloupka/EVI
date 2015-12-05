using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Relational.Query;
using Slp.r2rml4net.Storage.Relational.Query.Sources;

namespace Slp.r2rml4net.Storage.Relational.Optimization.Optimizers.SelfJoinOptimizerHelpers
{
    /// <summary>
    /// The data for <see cref="SelfJoinOptimizer"/>
    /// </summary>
    public class SelfJoinOptimizerData
    {
        /// <summary>
        /// The variables map
        /// </summary>
        private Dictionary<ICalculusVariable, ICalculusVariable> _variablesMap;

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
    }
}
