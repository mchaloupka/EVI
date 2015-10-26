using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Sparql.Utils.CodeGeneration;

namespace Slp.r2rml4net.Storage.Sparql.Optimization.Optimizers
{
    /// <summary>
    /// The union / join optimization
    /// </summary>
    public class UnionJoinOptimizer
        : BaseSparqlOptimizer<object>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnionJoinOptimizer"/> class.
        /// </summary>
        public UnionJoinOptimizer() 
            : base(new UnionJoinOptimizerImplementation())
        { }

        /// <summary>
        /// The implementation class for <see cref="UnionJoinOptimizer"/>
        /// </summary>
        public class UnionJoinOptimizerImplementation
            : BaseSparqlOptimizerImplementation<object>
        {
            
        }
    }
}
