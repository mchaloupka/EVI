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
        /// Adds the information about replaced table
        /// </summary>
        /// <param name="sqlTable">The SQL table.</param>
        /// <param name="alreadyAddedSqlTable">The already added SQL table.</param>
        public void AddReplaceTableInformation(SqlTable sqlTable, SqlTable alreadyAddedSqlTable)
        {
            throw new NotImplementedException();
        }
    }
}
