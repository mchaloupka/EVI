using System;
using System.Collections.Generic;
using Slp.Evi.Storage.Relational.Query;
using Slp.Evi.Storage.Relational.Query.Expressions;

namespace Slp.Evi.Storage.Relational.Builder.ConditionBuilderHelpers
{
    /// <summary>
    /// A class able to extract possible types from an expression set
    /// </summary>
    public class PossibleTypesExtractor
    {
        /// <summary>
        /// Collects the possible types of an expression set.
        /// </summary>
        public IEnumerable<int> CollectPossibleTypes(ExpressionsSet expressionSet)
        {
            var types = new List<int>();
            var extractorImpl = new PossibleTypesExtractorImplementation(types);
            expressionSet.TypeExpression.Accept(extractorImpl, null);
            return types;
        }

        /// <summary>
        /// Implementation of the types extraction
        /// </summary>
        /// <seealso cref="Slp.Evi.Storage.Relational.Query.Expressions.IExpressionVisitor" />
        private class PossibleTypesExtractorImplementation
            : IExpressionVisitor
        {
            private readonly List<int> _types;

            /// <summary>
            /// Initializes a new instance of the <see cref="PossibleTypesExtractorImplementation"/> class.
            /// </summary>
            /// <param name="types">The list of types where to store found types.</param>
            public PossibleTypesExtractorImplementation(List<int> types)
            {
                _types = types;
            }

            /// <inheritdoc />
            public object Visit(ColumnExpression columnExpression, object data)
            {
                throw new NotSupportedException();
            }

            /// <inheritdoc />
            public object Visit(ConcatenationExpression concatenationExpression, object data)
            {
                throw new NotSupportedException();
            }

            /// <inheritdoc />
            public object Visit(ConstantExpression constantExpression, object data)
            {
                if (!(constantExpression.Value is int typeIndex))
                {
                    throw new NotSupportedException();
                }

                _types.Add(typeIndex);
                return null;
            }

            /// <inheritdoc />
            public object Visit(CaseExpression caseExpression, object data)
            {
                foreach (var statement in caseExpression.Statements)
                {
                    statement.Expression.Accept(this, data);
                }

                return null;
            }

            /// <inheritdoc />
            public object Visit(CoalesceExpression coalesceExpression, object data)
            {
                foreach (var innerExpression in coalesceExpression.InnerExpressions)
                {
                    innerExpression.Accept(this, data);
                }

                return null;
            }

            /// <inheritdoc />
            public object Visit(NullExpression nullExpression, object data)
            {
                return null;
            }

            /// <inheritdoc />
            public object Visit(BinaryNumericExpression binaryNumericExpression, object data)
            {
                return null;
            }
        }
    }
}