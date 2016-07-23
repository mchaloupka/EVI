using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.Evi.Storage.Sparql.Algebra.Modifiers;

namespace Slp.Evi.Storage.Relational.Query.Sources
{
    /// <summary>
    /// Represents calculus model which is modified (by ORDER BY, LIMIT, OFFSET, DISTINCT or REDUCED modifier)
    /// </summary>
    public class ModifiedCalculusModel
        : ICalculusSource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModifiedCalculusModel"/> class.
        /// </summary>
        /// <param name="innerModel">The inner model.</param>
        /// <param name="ordering">The ordering.</param>
        /// <param name="limit">The limit.</param>
        /// <param name="offset">The offset.</param>
        public ModifiedCalculusModel(CalculusModel innerModel, IEnumerable<OrderingPart> ordering, int? limit, int? offset)
        {
            InnerModel = innerModel;
            Ordering = ordering;
            Limit = limit;
            Offset = offset;
        }

        /// <summary>
        /// Gets the ordering.
        /// </summary>
        public IEnumerable<OrderingPart> Ordering { get; }

        /// <summary>
        /// Gets or sets the limit.
        /// </summary>
        public int? Limit { get; set; }

        /// <summary>
        /// Gets or sets the offset.
        /// </summary>
        public int? Offset { get; set; }

        /// <summary>
        /// Gets the inner model.
        /// </summary>
        public CalculusModel InnerModel { get; }

        /// <summary>
        /// Accepts the specified visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value from visitor.</returns>
        public object Accept(ICalculusSourceVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }

        /// <summary>
        /// Gets the provided variables.
        /// </summary>
        /// <value>The variables.</value>
        public IEnumerable<ICalculusVariable> Variables => InnerModel.Variables;

        /// <summary>
        /// Representation of a part in ordering
        /// </summary>
        public class OrderingPart
        {
            /// <summary>
            /// Gets the expression.
            /// </summary>
            public IExpression Expression { get; }

            /// <summary>
            /// Gets a value indicating whether this variable has DESC ordering.
            /// </summary>
            public bool IsDescending { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="OrderByModifier.OrderingPart"/> class.
            /// </summary>
            /// <param name="expression">The variable.</param>
            /// <param name="isDescending">A value indicating whether the <paramref name="expression"/> has DESC ordering.</param>
            public OrderingPart(IExpression expression, bool isDescending)
            {
                Expression = expression;
                IsDescending = isDescending;
            }
        }
    }
}
