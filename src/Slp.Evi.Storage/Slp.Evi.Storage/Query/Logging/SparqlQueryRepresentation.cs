using System;
using System.Linq;
using System.Text;
using Slp.Evi.Storage.Common.Algebra;
using Slp.Evi.Storage.Sparql.Algebra;
using Slp.Evi.Storage.Sparql.Algebra.Expressions;
using Slp.Evi.Storage.Sparql.Algebra.Modifiers;
using Slp.Evi.Storage.Sparql.Algebra.Patterns;
using Slp.Evi.Storage.Utils;
using TCode.r2rml4net.Mapping;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;
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
        private readonly Func<object, long> _getObjectIndex;
        private readonly StringBuilder _sb = new StringBuilder();

        /// <summary>
        /// Initializes a new instance of the <see cref="SparqlQueryRepresentation"/> class.
        /// </summary>
        /// <param name="getObjectIndex">Index of the get object function.</param>
        public SparqlQueryRepresentation(Func<object, long> getObjectIndex)
        {
            _getObjectIndex = getObjectIndex;
        }

        /// <inheritdoc />
        public object Visit(SelectModifier selectModifier, object data)
        {
            _sb.Append($"{_getObjectIndex(selectModifier)}:");
            _sb.Append("Select(");
            bool first = true;
            foreach (var variable in selectModifier.Variables)
            {
                if (!first)
                {
                    _sb.Append(", ");
                }
                first = false;

                _sb.Append("?");
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
            _sb.Append($"{_getObjectIndex(orderByModifier)}:");
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
            _sb.Append($"{_getObjectIndex(sliceModifier)}:");
            _sb.Append("Slice(");
            Process(sliceModifier.InnerQuery);
            _sb.Append($", Limit: {sliceModifier.Limit?.ToString() ?? "none"}, Offset: {sliceModifier.Offset?.ToString() ?? "none"})");
            return null;
        }

        /// <inheritdoc />
        public object Visit(DistinctModifier distinctModifier, object data)
        {
            _sb.Append($"{_getObjectIndex(distinctModifier)}:");
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
            _sb.Append($"{_getObjectIndex(filterPattern)}:");
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
            _sb.Append($"{_getObjectIndex(joinPattern)}:");
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
            _sb.Append($"{_getObjectIndex(leftJoinPattern)}:");
            _sb.Append("LeftJoin(");
            leftJoinPattern.LeftOperand.Accept(this, null);
            _sb.Append(", ");
            leftJoinPattern.RightOperand.Accept(this, null);
            _sb.Append(", ");
            leftJoinPattern.Condition.Accept(this, null);
            _sb.Append(")");
            return null;
        }

        /// <inheritdoc />
        public object Visit(MinusPattern minusPattern, object data)
        {
            _sb.Append($"{_getObjectIndex(minusPattern)}:");
            _sb.Append("Minus(");
            minusPattern.LeftOperand.Accept(this, null);
            _sb.Append(", ");
            minusPattern.RightOperand.Accept(this, null);
            _sb.Append(")");
            return null;
        }

        /// <inheritdoc />
        public object Visit(TriplePattern triplePattern, object data)
        {
            _sb.Append($"{_getObjectIndex(triplePattern)}:");
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
                _sb.Append("?");
                _sb.Append(variablePattern.VariableName);
            }
            else if (patternItem is NodeMatchPattern nodeMatchPattern)
            {
                _sb.Append(nodeMatchPattern.Node.ToString());
            }
            else if (patternItem is BlankNodePattern blankNodePattern)
            {
                _sb.Append(blankNodePattern.ID);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        /// <inheritdoc />
        public object Visit(UnionPattern unionPattern, object data)
        {
            _sb.Append($"{_getObjectIndex(unionPattern)}:");
            _sb.Append("Union(");

            bool first = true;
            foreach (var pattern in unionPattern.UnionedGraphPatterns)
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
        public object Visit(RestrictedTriplePattern restrictedTriplePattern, object data)
        {
            _sb.Append($"{_getObjectIndex(restrictedTriplePattern)}:");
            _sb.Append("RestrictedTriple((");
            Process(restrictedTriplePattern.SubjectPattern);
            _sb.Append(", ");
            Process(restrictedTriplePattern.PredicatePattern);
            _sb.Append(", ");
            Process(restrictedTriplePattern.ObjectPattern);
            _sb.Append(") From (");

            if (!string.IsNullOrEmpty(restrictedTriplePattern.TripleMap.TableName))
            {
                _sb.Append("Table: ");
                _sb.Append(restrictedTriplePattern.TripleMap.TableName);
            }
            else
            {
                throw new NotImplementedException();
            }

            _sb.Append(";");
            StringRepresentationOfMap(restrictedTriplePattern.SubjectMap);
            _sb.Append(";");
            StringRepresentationOfMap(restrictedTriplePattern.PredicateMap);
            _sb.Append(";");

            if (restrictedTriplePattern.RefObjectMap != null)
            {
                StringRepresentationOfMap(restrictedTriplePattern.RefObjectMap);
            }
            else
            {
                StringRepresentationOfMap(restrictedTriplePattern.ObjectMap);
            }

            _sb.Append(")");

            return null;
        }

        private void StringRepresentationOfMap(ITermMap termMap)
        {
            if (termMap.IsConstantValued)
            {
                if (termMap is IUriValuedTermMap uriTermMap)
                {
                    _sb.Append($"<{uriTermMap.URI}>");
                }
                else if (termMap is IObjectMap objectMap)
                {
                    _sb.Append(objectMap.Literal);
                }
                else
                {
                    throw new NotSupportedException("Unsupported constant");
                }
            }
            else if (termMap.IsColumnValued)
            {
                _sb.Append($"column:<{termMap.ColumnName}>");
            }
            else if (termMap.IsTemplateValued)
            {
                _sb.Append($"template:<{termMap.Template}>");
            }
            else
            {
                throw new NotSupportedException("Unsupported term type");
            }
        }

        private void StringRepresentationOfMap(IRefObjectMap refMap)
        {
            _sb.Append("ref:");
            _sb.Append(refMap.ParentTriplesMap.Node);
        }

        /// <inheritdoc />
        public object Visit(ExtendPattern extendPattern, object data)
        {
            _sb.Append($"{_getObjectIndex(extendPattern)}:");
            _sb.Append("Extend(");
            extendPattern.InnerPattern.Accept(this, null);
            _sb.Append(",");
            _sb.Append(extendPattern.VariableName);
            _sb.Append(": ");
            extendPattern.Expression.Accept(this, null);
            _sb.Append(")");

            return null;
        }

        /// <inheritdoc />
        public object Visit(IsBoundExpression isBoundExpression, object data)
        {
            _sb.Append($"{_getObjectIndex(isBoundExpression)}:");
            _sb.Append("bound(");
            _sb.Append(isBoundExpression.Variable);
            _sb.Append(")");

            return null;
        }

        /// <inheritdoc />
        public object Visit(BooleanTrueExpression booleanTrueExpression, object data)
        {
            _sb.Append("true");
            return null;
        }

        /// <inheritdoc />
        public object Visit(BooleanFalseExpression booleanFalseExpression, object data)
        {
            _sb.Append("false");
            return null;
        }

        /// <inheritdoc />
        public object Visit(NegationExpression negationExpression, object data)
        {
            _sb.Append($"{_getObjectIndex(negationExpression)}:");
            _sb.Append("not(");
            negationExpression.InnerCondition.Accept(this, null);
            _sb.Append(")");
            return null;
        }

        /// <inheritdoc />
        public object Visit(VariableExpression variableExpression, object data)
        {
            _sb.Append("?");
            _sb.Append(variableExpression.Variable);
            return null;
        }

        /// <inheritdoc />
        public object Visit(ConjunctionExpression conjunctionExpression, object data)
        {
            _sb.Append($"{_getObjectIndex(conjunctionExpression)}:");
            _sb.Append("and(");
            bool first = true;
            foreach (var operand in conjunctionExpression.Operands)
            {
                if (!first)
                {
                    _sb.Append(", ");
                }
                else
                {
                    first = false;
                }

                operand.Accept(this, null);
            }
            _sb.Append(")");
            return null;
        }

        /// <inheritdoc />
        public object Visit(ComparisonExpression comparisonExpression, object data)
        {
            _sb.Append($"{_getObjectIndex(comparisonExpression)}:(");
            comparisonExpression.LeftOperand.Accept(this, null);

            switch (comparisonExpression.ComparisonType)
            {
                case ComparisonTypes.GreaterThan:
                    _sb.Append(" > ");
                    break;
                case ComparisonTypes.GreaterOrEqualThan:
                    _sb.Append(" >= ");
                    break;
                case ComparisonTypes.LessThan:
                    _sb.Append(" < ");
                    break;
                case ComparisonTypes.LessOrEqualThan:
                    _sb.Append(" <= ");
                    break;
                case ComparisonTypes.EqualTo:
                    _sb.Append(" == ");
                    break;
                case ComparisonTypes.NotEqualTo:
                    _sb.Append(" != ");
                    break;
                default:
                    throw new Exception("Unsupported comparison type");
            }

            comparisonExpression.RightOperand.Accept(this, null);
            _sb.Append(")");

            return null;
        }

        /// <inheritdoc />
        public object Visit(NodeExpression nodeExpression, object data)
        {
            _sb.Append(nodeExpression.Node);
            return null;
        }

        /// <inheritdoc />
        public object Visit(DisjunctionExpression disjunctionExpression, object data)
        {
            _sb.Append($"{_getObjectIndex(disjunctionExpression)}:");
            _sb.Append("or(");
            bool first = true;
            foreach (var operand in disjunctionExpression.Operands)
            {
                if (!first)
                {
                    _sb.Append(", ");
                }
                else
                {
                    first = false;
                }

                operand.Accept(this, null);
            }
            _sb.Append(")");
            return null;
        }

        /// <inheritdoc />
        public object Visit(BinaryArithmeticExpression binaryArithmeticExpression, object data)
        {
            _sb.Append($"{_getObjectIndex(binaryArithmeticExpression)}:(");
            binaryArithmeticExpression.LeftOperand.Accept(this, null);

            switch (binaryArithmeticExpression.Operation)
            {
                case ArithmeticOperation.Add:
                    _sb.Append(" + ");
                    break;
                case ArithmeticOperation.Subtract:
                    _sb.Append(" - ");
                    break;
                case ArithmeticOperation.Divide:
                    _sb.Append(" / ");
                    break;
                case ArithmeticOperation.Multiply:
                    _sb.Append(" * ");
                    break;
                default:
                    throw new Exception("Unsupported arithmetic operation");
            }

            binaryArithmeticExpression.RightOperand.Accept(this, null);
            _sb.Append(")");

            return null;
        }

        /// <inheritdoc />
        public object Visit(RegexExpression regexExpression, object data)
        {
            _sb.Append("regex(");

            regexExpression.Text.Accept(this, null);
            _sb.Append(", ");
            regexExpression.Pattern.Accept(this, null);

            if (regexExpression.Flags != null)
            {
                _sb.Append(", ");
                regexExpression.Flags.Accept(this, null);
            }

            _sb.Append(")");
            return null;
        }

        /// <inheritdoc />
        public object Visit(LangMatchesExpression langMatchesExpression, object data)
        {
            _sb.Append("langMatches(");
            langMatchesExpression.LanguageExpression.Accept(this, null);
            _sb.Append(", ");
            langMatchesExpression.LanguageRangeExpression.Accept(this, null);
            _sb.Append(")");
            return null;
        }

        /// <inheritdoc />
        public object Visit(LangExpression langExpression, object data)
        {
            _sb.Append("lang(");
            langExpression.SparqlExpression.Accept(this, data);
            _sb.Append(")");
            return null;
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