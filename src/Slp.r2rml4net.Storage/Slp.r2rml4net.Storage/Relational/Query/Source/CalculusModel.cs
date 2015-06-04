using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Relational.Query.Source
{
    public class CalculusModel 
        : ICalculusSource
    {
        public CalculusModel(IEnumerable<ICalculusVariable> variables, IEnumerable<ICondition> conditions)
        {
            this.Variables = variables;
            this.Conditions = conditions;
        }

        public IEnumerable<ICalculusVariable> Variables { get; private set; }

        public IEnumerable<ICondition> Conditions { get; private set; }
    }
}
