using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Relational.Query;
using Slp.r2rml4net.Storage.Relational.Query.ValueBinder;
using VDS.RDF;

namespace Slp.r2rml4net.Storage.Relational.Builder
{
    /// <summary>
    /// The conditions builder
    /// </summary>
    public class ConditionBuilder
    {
        /// <summary>
        /// The expression builder
        /// </summary>
        private readonly ExpressionBuilder _expressionBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionBuilder"/> class.
        /// </summary>
        /// <param name="expressionBuilder">The expression builder.</param>
        public ConditionBuilder(ExpressionBuilder expressionBuilder)
        {
            _expressionBuilder = expressionBuilder;
        }

        public ICondition CreateEqualsCondition(INode node, BaseValueBinder valueBinder, QueryContext context)
        {
            throw new NotImplementedException();
        }

        public ICondition CreateIsNotNullCondition(BaseValueBinder valueBinder, QueryContext context)
        {
            throw new NotImplementedException();
        }

        public ICondition CreateEqualsCondition(BaseValueBinder valueBinder, IValueBinder sameVariableValueBinder, QueryContext context)
        {
            throw new NotImplementedException();
        }
    }
}
