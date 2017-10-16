using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.Evi.Storage.Relational.Query;

namespace Slp.Evi.Storage.Relational.Builder.ConditionBuilderHelpers
{
    /// <summary>
    /// Represents a set of expressions representing a single expression.
    /// </summary>
    /// <remarks>
    /// Its intention is to represent various expressions according to type
    /// they represent.
    /// </remarks>
    public class ExpressionsSet
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionsSet"/> class.
        /// </summary>
        /// <param name="typeExpression">The type expression.</param>
        /// <param name="typeCategoryExpression">The type category expression.</param>
        /// <param name="stringExpression">The string expression.</param>
        /// <param name="numericExpression">The numeric expression.</param>
        /// <param name="booleanExpression">The boolean expression.</param>
        /// <param name="dateTimeExpression">The date time expression.</param>
        public ExpressionsSet(IExpression typeExpression, IExpression typeCategoryExpression, IExpression stringExpression, IExpression numericExpression, IExpression booleanExpression, IExpression dateTimeExpression)
        {
            TypeExpression = typeExpression;
            TypeCategoryExpression = typeCategoryExpression;
            StringExpression = stringExpression;
            NumericExpression = numericExpression;
            BooleanExpression = booleanExpression;
            DateTimeExpression = dateTimeExpression;
        }

        /// <summary>
        /// Gets the expression which returns the type id.
        /// </summary>
        public IExpression TypeExpression { get; }

        /// <summary>
        /// Gets the expression which returns the type category (as int).
        /// </summary>
        public IExpression TypeCategoryExpression { get; }

        /// <summary>
        /// Gets the expression which contains the value (if it is not numeric, boolean or datetime literal)
        /// </summary>
        public IExpression StringExpression { get; }

        /// <summary>
        /// Gets the expression which contains the value if it is a numeric.
        /// </summary>
        public IExpression NumericExpression { get; }

        /// <summary>
        /// Gets the expression which contains the value if it is a boolean.
        /// </summary>
        public IExpression BooleanExpression { get; }

        /// <summary>
        /// Gets the expression which contains the value if it is datetime.
        /// </summary>
        public IExpression DateTimeExpression { get; }
    }
}
