using System;
using System.Linq;
using System.Text;
using Slp.Evi.Storage.Sparql.Algebra;
using Slp.Evi.Storage.Sparql.Algebra.Expressions;
using Slp.Evi.Storage.Sparql.Algebra.Modifiers;
using Slp.Evi.Storage.Sparql.Algebra.Patterns;
using VDS.RDF.Query.Patterns;
using FilterPattern = Slp.Evi.Storage.Sparql.Algebra.Patterns.FilterPattern;
using GraphPattern = Slp.Evi.Storage.Sparql.Algebra.Patterns.GraphPattern;
using TriplePattern = Slp.Evi.Storage.Sparql.Algebra.Patterns.TriplePattern;

namespace Slp.Evi.Storage.Query.Logging
{
    /// <summary>
    /// Writes a representation of a SPARQL query
    /// </summary>
    /// <seealso cref="Slp.Evi.Storage.Sparql.Algebra.Modifiers.IModifierVisitor" />
    /// <seealso cref="Slp.Evi.Storage.Sparql.Algebra.Patterns.IGraphPatternVisitor" />
    /// <seealso cref="Slp.Evi.Storage.Sparql.Algebra.Expressions.ISparqlExpressionVisitor" />
    public class SparqlQueryRepresentation
        : IModifierVisitor, IGraphPatternVisitor, ISparqlExpressionVisitor
    {
        private StringBuilder _sb = new StringBuilder();

        /// <inheritdoc />
        public object Visit(SelectModifier selectModifier, object data)
        {
            _sb.Append("Select(");
            bool first = true;
            foreach (var variable in selectModifier.Variables)
            {
                if (!first)
                {
                    _sb.Append(", ");
                }
                first = false;

                _sb.Append(variable);
            }
            _sb.Append(" | ");
            Process(selectModifier.InnerQuery);
            _sb.Append(")");
            return null;
        }

        /// <inheritdoc />
        public object Visit(OrderByModifier orderByModifier, object data)
        {
            _sb.Append("OrderBy(");
            Process(orderByModifier.InnerQuery);
            _sb.Append(", By: {");

            bool first = true;
            foreach (var orderingPart in orderByModifier.Ordering)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    _sb.Append(", ");
                }

                _sb.Append(orderingPart.Variable);
                _sb.Append(" ");
                if (orderingPart.IsDescending)
                {
                    _sb.Append("desc");
                }
            }

            _sb.Append("});");
            return null;
        }

        /// <inheritdoc />
        public object Visit(SliceModifier sliceModifier, object data)
        {
            _sb.Append("Slice(");
            Process(sliceModifier.InnerQuery);
            _sb.Append($", Limit: {sliceModifier.Limit?.ToString() ?? "none"}, Offset: {sliceModifier.Offset?.ToString() ?? "none"})");
            return null;
        }

        /// <inheritdoc />
        public object Visit(DistinctModifier distinctModifier, object data)
        {
            _sb.Append("Distinct(");
            Process(distinctModifier.InnerQuery);
            _sb.Append($")");
            return null;
        }

        /// <inheritdoc />
        public object Visit(EmptyPattern emptyPattern, object data)
        {
            _sb.Append("EmptyPattern");
            return null;
        }

        /// <inheritdoc />
        public object Visit(FilterPattern filterPattern, object data)
        {
            _sb.Append("Filter(");
            filterPattern.InnerPattern.Accept(this, null);
            _sb.Append(", ");
            filterPattern.Condition.Accept(this, null);
            _sb.Append(")");
            return null;
        }

        /// <inheritdoc />
        public object Visit(NotMatchingPattern notMatchingPattern, object data)
        {
            _sb.Append("NotMatchingPattern");
            return null;
        }

        /// <inheritdoc />
        public object Visit(GraphPattern graphPattern, object data)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public object Visit(JoinPattern joinPattern, object data)
        {
            _sb.Append("Join(");

            bool first = true;
            foreach (var pattern in joinPattern.JoinedGraphPatterns)
            {
                if (!first)
                {
                    _sb.Append(", ");
                }
                first = false;

                pattern.Accept(this, null);
            }

            _sb.Append(")");
            return null;
        }

        /// <inheritdoc />
        public object Visit(LeftJoinPattern leftJoinPattern, object data)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public object Visit(MinusPattern minusPattern, object data)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public object Visit(TriplePattern triplePattern, object data)
        {
            _sb.Append("Triple(");
            Process(triplePattern.SubjectPattern);
            _sb.Append(", ");
            Process(triplePattern.PredicatePattern);
            _sb.Append(", ");
            Process(triplePattern.ObjectPattern);
            _sb.Append(")");
            return null;
        }

        private void Process(PatternItem patternItem)
        {
            if (patternItem is VariablePattern variablePattern)
            {
                _sb.Append(variablePattern.VariableName);
            }
            else if (patternItem is NodeMatchPattern nodeMatchPattern)
            {
                _sb.Append(nodeMatchPattern.Node.ToString());
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        /// <inheritdoc />
        public object Visit(UnionPattern unionPattern, object data)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public object Visit(RestrictedTriplePattern restrictedTriplePattern, object data)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public object Visit(ExtendPattern extendPattern, object data)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public object Visit(IsBoundExpression isBoundExpression, object data)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public object Visit(BooleanTrueExpression booleanTrueExpression, object data)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public object Visit(BooleanFalseExpression booleanFalseExpression, object data)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public object Visit(NegationExpression negationExpression, object data)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public object Visit(VariableExpression variableExpression, object data)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public object Visit(ConjunctionExpression conjunctionExpression, object data)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public object Visit(ComparisonExpression comparisonExpression, object data)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public object Visit(NodeExpression nodeExpression, object data)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public object Visit(DisjunctionExpression disjunctionExpression, object data)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public object Visit(BinaryArithmeticExpression binaryArithmeticExpression, object data)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Gets the representation.
        /// </summary>
        /// <param name="sparqlQuery">The sparql query.</param>
        public string GetRepresentation(ISparqlQuery sparqlQuery)
        {
            _sb.Clear();
            Process(sparqlQuery);
            return _sb.ToString();
        }

        private void Process(ISparqlQuery sparqlQuery)
        {
            if (sparqlQuery is IModifier modifier)
            {
                modifier.Accept(this, null);
            }
            else if (sparqlQuery is IGraphPattern graphPattern)
            {
                graphPattern.Accept(this, null);
            }
            else
            {
                throw new ArgumentException("Argument is not of a correct type", nameof(sparqlQuery));
            }
        }
    }
}