using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Relational.Query;
using Slp.r2rml4net.Storage.Relational.Query.Source;
using Slp.r2rml4net.Storage.Relational.Utils.CodeGeneration;
using Slp.r2rml4net.Storage.Sparql.Algebra;
using Slp.r2rml4net.Storage.Utils;

namespace Slp.r2rml4net.Storage.Relational.Utils
{
    /// <summary>
    /// Base transformer for <see cref="ICalculusSource" />
    /// </summary>
    /// <typeparam name="T">Type of parameter passed to process</typeparam>
    public class BaseSourceTransformer<T>
        : BaseSourceTransformerG<T, ICalculusSource>
    {
        /// <summary>
        /// Fallback variant for the transformation.
        /// </summary>
        /// <param name="toTransform">Instance to be transformed.</param>
        /// <param name="data">The passed data.</param>
        /// <typeparam name="TI">Type of the instance</typeparam>
        /// <returns>The transformation result</returns>
        protected override ICalculusSource CommonFallbackTransform<TI>(TI toTransform, T data)
        {
            throw new Exception("This code should not be reached");
        }

        /// <summary>
        /// Process the <see cref="CalculusModel" />
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override ICalculusSource Transform(CalculusModel toTransform, T data)
        {
            return toTransform;
        }

        /// <summary>
        /// Process the <see cref="SqlTable" />
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        protected override ICalculusSource Transform(SqlTable toTransform, T data)
        {
            return toTransform;
        }
    }
}
