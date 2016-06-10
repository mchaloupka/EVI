using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.Evi.Storage.Sparql.Algebra.Patterns
{
    /// <summary>
    /// Class representing EXTEND (BIND) pattern
    /// </summary>
    /// <seealso cref="Slp.Evi.Storage.Sparql.Algebra.IGraphPattern" />
    public class ExtendPattern
        : IGraphPattern
    {
        private readonly string[] _variables;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendPattern"/> class.
        /// </summary>
        /// <param name="innerAlgebra">The inner algebra.</param>
        /// <param name="variableName">Name of the variable.</param>
        /// <param name="expression">The expression.</param>
        public ExtendPattern(IGraphPattern innerAlgebra, string variableName, ISparqlExpression expression)
        {
            InnerAlgebra = innerAlgebra;
            VariableName = variableName;
            Expression = expression;

            List<string> variables = new List<string>(innerAlgebra.Variables);

            if (!variables.Contains(variableName))
            {
                variables.Add(variableName);
            }

            _variables = variables.ToArray();
        }

        /// <summary>
        /// Gets the SPARQL variables.
        /// </summary>
        /// <value>The variables.</value>
        public IEnumerable<string> Variables => _variables;

        /// <summary>
        /// Gets the inner algebra.
        /// </summary>
        /// <value>The inner algebra.</value>
        public IGraphPattern InnerAlgebra { get; }

        /// <summary>
        /// Gets the name of the variable.
        /// </summary>
        /// <value>The name of the variable.</value>
        public string VariableName { get; }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <value>The expression.</value>
        public ISparqlExpression Expression { get; }

        /// <summary>
        /// Accepts the specified visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value from visitor.</returns>
        public object Accept(IGraphPatternVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }

        /// <summary>
        /// Gets the set of always bound variables.
        /// </summary>
        public IEnumerable<string> AlwaysBoundVariables => InnerAlgebra.AlwaysBoundVariables;
    }
}
