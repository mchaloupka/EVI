using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Sparql.Algebra.Expression
{
    public class VariableExpression : ISparqlQueryExpression
    {
        public string Variable { get; private set; }

        public VariableExpression(string variable)
        {
            this.Variable = variable;
        }
    }
}
