using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Relational.Query;
using Slp.r2rml4net.Storage.Relational.Query.Source;
using Slp.r2rml4net.Storage.Sparql.Algebra;
using Slp.r2rml4net.Storage.Utils;

namespace Slp.r2rml4net.Storage.Relational.Utils
{
    /// <summary>
    /// Base transformer for <see cref="ICalculusSource" />
    /// </summary>
    /// <typeparam name="T">Type of parameter passed to process</typeparam>
    public class BaseSourceTransformer<T>
        : ICalculusSourceVisitor
    {
        /// <summary>
        /// Transforms the calculus source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="data">The data.</param>
        /// <returns>The transformed calculus source.</returns>
        public ICalculusSource TransformCalculusSource(ICalculusSource source, T data)
        {
            return (ICalculusSource) source.Accept(this, data);
        }

        /// <summary>
        /// Process the <see cref="CalculusModel"/>
        /// </summary>
        /// <param name="calculusModel">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns></returns>
        protected virtual ICalculusSource Process(CalculusModel calculusModel, T data)
        {
            return calculusModel;
        }

        /// <summary>
        /// Process the <see cref="SqlTable"/>
        /// </summary>
        /// <param name="sqlTable">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns></returns>
        protected virtual ICalculusSource Process(SqlTable sqlTable, T data)
        {
            return sqlTable;
        }

        #region ICalculusSourceVisitor
        /// <summary>
        /// Visits <see cref="CalculusModel" />
        /// </summary>
        /// <param name="calculusModel">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(CalculusModel calculusModel, object data)
        {
            // TODO: Process child sources
            
            return Process(calculusModel, (T)data);
        }

        /// <summary>
        /// Visits <see cref="SqlTable" />
        /// </summary>
        /// <param name="sqlTable">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(SqlTable sqlTable, object data)
        {
            return Process(sqlTable, (T)data);
        } 
        #endregion
    }
}
