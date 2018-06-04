using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Slp.Evi.Storage.Common.Algebra;
using Slp.Evi.Storage.Mapping.Representation;
using Slp.Evi.Storage.Relational.Query;
using Slp.Evi.Storage.Relational.Query.Conditions.Assignment;
using Slp.Evi.Storage.Relational.Query.Conditions.Filter;
using Slp.Evi.Storage.Relational.Query.Conditions.Source;
using Slp.Evi.Storage.Relational.Query.Expressions;
using Slp.Evi.Storage.Relational.Query.Sources;
using Slp.Evi.Storage.Relational.Query.ValueBinders;

namespace Slp.Evi.Storage.Query.Logging
{
    /// <summary>
    /// Textual representation for relational queries.
    /// </summary>
    /// <seealso cref="Slp.Evi.Storage.Relational.Query.ValueBinders.IValueBinderVisitor" />
    /// <seealso cref="Slp.Evi.Storage.Relational.Query.Sources.ICalculusSourceVisitor" />
    /// <seealso cref="Slp.Evi.Storage.Relational.Query.Expressions.IExpressionVisitor" />
    /// <seealso cref="Slp.Evi.Storage.Relational.Query.Conditions.Source.ISourceConditionVisitor" />
    /// <seealso cref="Slp.Evi.Storage.Relational.Query.Conditions.Assignment.IAssignmentConditionVisitor" />
    /// <seealso cref="Slp.Evi.Storage.Relational.Query.Conditions.Filter.IFilterConditionVisitor" />
    public class RelationalQueryRepresentation
        : IValueBinderVisitor,
            ICalculusSourceVisitor,
            IExpressionVisitor,
            ISourceConditionVisitor,
            IAssignmentConditionVisitor,
            IFilterConditionVisitor
    {
        private readonly Func<object, long> _getObjectIndex;
        private readonly StringBuilder _sb = new StringBuilder();

        /// <summary>
        /// Initializes a new instance of the <see cref="RelationalQueryRepresentation"/> class.
        /// </summary>
        /// <param name="getObjectIndex">A function to get an index of an object.</param>
        public RelationalQueryRepresentation(Func<object, long> getObjectIndex)
        {
            _getObjectIndex = getObjectIndex;
        }

        /// <summary>
        /// Gets the representation.
        /// </summary>
        /// <param name="query">The query.</param>
        public string GetRepresentation(RelationalQuery query)
        {
            _sb.Clear();
            _sb.Append($"{_getObjectIndex(query)}:(");
            _sb.Append("binders: ");

            Iterate(query.ValueBinders, GetRepresentation, ", ");

            _sb.Append(" | query: ");

            query.Model.Accept(this, null);

            _sb.Append(")");
            return _sb.ToString();
        }

        private void GetRepresentation(IValueBinder valueBinder)
        {
            _sb.Append(valueBinder.VariableName);
            _sb.Append("<-");
            valueBinder.Accept(this, null);
        }

        /// <inheritdoc />
        public object Visit(BaseValueBinder baseValueBinder, object data)
        {
            _sb.Append($"{_getObjectIndex(baseValueBinder)}:");
            _sb.Append("base(");
            StringRepresentationOfMap(baseValueBinder.TermMap);
            _sb.Append("; ");
            Iterate(baseValueBinder.NeededCalculusVariables, GetRepresentation, ", ");
            _sb.Append(")");
            return null;
        }

        private void Iterate<T>(IEnumerable<T> collection, Action<T> writeItem, string separator)
        {
            bool first = true;
            foreach (var item in collection)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    _sb.Append(separator);
                }

                writeItem(item);
            }
        }

        private void StringRepresentationOfMap(ITermMapping termMap)
        {
            if (termMap.IsConstantValued)
            {
                if(termMap.Iri != null)
                {
                    _sb.Append($"<{termMap.Iri}>");
                }
                else if (termMap is IObjectMapping objectMap)
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

        /// <inheritdoc />
        public object Visit(EmptyValueBinder emptyValueBinder, object data)
        {
            _sb.Append("empty");
            return null;
        }

        /// <inheritdoc />
        public object Visit(CoalesceValueBinder coalesceValueBinder, object data)
        {
            _sb.Append($"{_getObjectIndex(coalesceValueBinder)}:");
            _sb.Append("coalesce(");
            Iterate(coalesceValueBinder.ValueBinders, x => x.Accept(this, null), ", ");
            _sb.Append(")");
            return null;
        }

        /// <inheritdoc />
        public object Visit(SwitchValueBinder switchValueBinder, object data)
        {
            _sb.Append($"{_getObjectIndex(switchValueBinder)}:");
            _sb.Append("switch(");

            GetRepresentation(switchValueBinder.CaseVariable);

            _sb.Append(" cases: ");
            Iterate(switchValueBinder.Cases, x => {
                _sb.Append(x.CaseValue);
                _sb.Append(": ");
                x.ValueBinder.Accept(this, null);
            }, ", ");

            _sb.Append(")");
            return null;
        }

        private void GetRepresentation(ICalculusVariable variable)
        {
            _sb.Append($"var:{_getObjectIndex(variable)}");
            if (variable is SqlColumn col)
            {
                _sb.Append(":");
                _sb.Append(col.Name);
            }
        }

        /// <inheritdoc />
        public object Visit(ExpressionSetValueBinder expressionSetValueBinder, object data)
        {
            _sb.Append($"{_getObjectIndex(expressionSetValueBinder)}:");
            _sb.Append("expression(");
            GetRepresentation(expressionSetValueBinder.ExpressionSet);
            _sb.Append(")");
            return null;
        }

        private void GetRepresentation(ExpressionsSet expressionSet)
        {
            _sb.Append("<");
            _sb.Append(expressionSet.IsNotErrorCondition);
            _sb.Append(";");
            _sb.Append(expressionSet.TypeCategoryExpression);
            _sb.Append(";");
            _sb.Append(expressionSet.TypeExpression);
            _sb.Append(";");
            _sb.Append(expressionSet.StringExpression);
            _sb.Append(";");
            _sb.Append(expressionSet.BooleanExpression);
            _sb.Append(";");
            _sb.Append(expressionSet.NumericExpression);
            _sb.Append(";");
            _sb.Append(expressionSet.DateTimeExpression);
            _sb.Append(">");
        }

        /// <inheritdoc />
        public object Visit(CalculusModel calculusModel, object data)
        {
            _sb.Append($"{_getObjectIndex(calculusModel)}:");
            _sb.Append("{");
            Iterate(calculusModel.Variables, GetRepresentation, ", ");
            _sb.Append(" | ");
            _sb.Append("sources: ");
            Iterate(calculusModel.SourceConditions, x => x.Accept(this, null), ", ");
            _sb.Append(" assignments: ");
            Iterate(calculusModel.AssignmentConditions, x => x.Accept(this, null), ", ");
            _sb.Append(" conditions: ");
            Iterate(calculusModel.FilterConditions, x => x.Accept(this, null), ", ");
            _sb.Append("}");
            return null;
        }

        /// <inheritdoc />
        public object Visit(SqlTable sqlTable, object data)
        {
            _sb.Append("table:");
            _sb.Append(sqlTable.TableName);
            return null;
        }

        /// <inheritdoc />
        public object Visit(ModifiedCalculusModel calculusModel, object data)
        {
            _sb.Append($"{_getObjectIndex(calculusModel)}:");
            _sb.Append("modified(");
            if (calculusModel.IsDistinct)
            {
                _sb.Append("distinct ");
            }

            if (calculusModel.Limit.HasValue)
            {
                _sb.Append("limit:");
                _sb.Append(calculusModel.Limit.Value);
                _sb.Append(" ");
            }

            if (calculusModel.Offset.HasValue)
            {
                _sb.Append("offset:");
                _sb.Append(calculusModel.Offset.Value);
                _sb.Append(" ");
            }

            if (calculusModel.Ordering.Any())
            {
                _sb.Append("order by ");

                Iterate(calculusModel.Ordering, x =>
                {
                    x.Expression.Accept(this, null);
                    if (x.IsDescending)
                    {
                        _sb.Append(" desc");
                    }
                    _sb.Append(" ");
                }, " then by ");
                _sb.Append(" ");
            }

            calculusModel.InnerModel.Accept(this, null);

            _sb.Append(")");
            return null;
        }

        /// <inheritdoc />
        public object Visit(ColumnExpression columnExpression, object data)
        {
            _sb.Append($"{_getObjectIndex(columnExpression)}:");
            _sb.Append("col(");
            GetRepresentation(columnExpression.CalculusVariable);
            _sb.Append(")");
            return null;
        }

        /// <inheritdoc />
        public object Visit(ConcatenationExpression concatenationExpression, object data)
        {
            _sb.Append($"{_getObjectIndex(concatenationExpression)}:");
            _sb.Append("concat(");
            Iterate(concatenationExpression.InnerExpressions, x => x.Accept(this, null), ", ");
            _sb.Append(")");
            return null;
        }

        /// <inheritdoc />
        public object Visit(ConstantExpression constantExpression, object data)
        {
            _sb.Append($"{_getObjectIndex(constantExpression)}:");
            _sb.Append("const(");
            _sb.Append(constantExpression.Value);
            _sb.Append(")");
            return null;
        }

        /// <inheritdoc />
        public object Visit(CaseExpression caseExpression, object data)
        {
            _sb.Append($"{_getObjectIndex(caseExpression)}:");
            _sb.Append("case(");
            Iterate(caseExpression.Statements, x =>
            {
                x.Condition.Accept(this, null);
                _sb.Append(" -> ");
                x.Expression.Accept(this, null);
            }, "; ");
            _sb.Append(")");
            return null;
        }

        /// <inheritdoc />
        public object Visit(CoalesceExpression coalesceExpression, object data)
        {
            _sb.Append($"{_getObjectIndex(coalesceExpression)}:");
            _sb.Append("coalesce(");
            Iterate(coalesceExpression.InnerExpressions, x => x.Accept(this, null), ", ");
            _sb.Append(")");
            return null;
        }

        /// <inheritdoc />
        public object Visit(NullExpression nullExpression, object data)
        {
            _sb.Append("null()");
            return null;
        }

        /// <inheritdoc />
        public object Visit(BinaryNumericExpression binaryNumericExpression, object data)
        {
            _sb.Append($"{_getObjectIndex(binaryNumericExpression)}:(");
            binaryNumericExpression.LeftOperand.Accept(this, null);

            switch (binaryNumericExpression.Operator)
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
                    throw new NotSupportedException("Unsupported arithmetic operation");
            }

            binaryNumericExpression.RightOperand.Accept(this, null);

            _sb.Append(")");
            return null;
        }

        /// <inheritdoc />
        public object Visit(TupleFromSourceCondition tupleFromSourceCondition, object data)
        {
            _sb.Append($"{_getObjectIndex(tupleFromSourceCondition)}:");
            _sb.Append("(");
            Iterate(tupleFromSourceCondition.CalculusVariables, GetRepresentation, ", ");
            _sb.Append(") from ");
            tupleFromSourceCondition.Source.Accept(this, null);
            return null;
        }

        /// <inheritdoc />
        public object Visit(UnionedSourcesCondition unionedSourcesCondition, object data)
        {
            _sb.Append($"{_getObjectIndex(unionedSourcesCondition)}:");
            _sb.Append("(");
            Iterate(unionedSourcesCondition.CalculusVariables, GetRepresentation, ", ");
            _sb.Append(") with case ");
            GetRepresentation(unionedSourcesCondition.CaseVariable);
            _sb.Append(" from ");
            Iterate(unionedSourcesCondition.Sources, x => x.Accept(this, null), " union ");
            return null;
        }

        /// <inheritdoc />
        public object Visit(LeftJoinCondition leftJoinCondition, object data)
        {
            _sb.Append($"{_getObjectIndex(leftJoinCondition)}:");
            _sb.Append("leftjoin(");
            leftJoinCondition.RightOperand.Accept(this, null);
            _sb.Append("; ");
            Iterate(leftJoinCondition.JoinConditions, x => x.Accept(this, null), ", ");
            _sb.Append(")");
            return null;
        }

        /// <inheritdoc />
        public object Visit(AssignmentFromExpressionCondition assignmentFromExpressionCondition, object data)
        {
            _sb.Append("assignment(");
            GetRepresentation(assignmentFromExpressionCondition.Variable);
            _sb.Append(" <- ");
            assignmentFromExpressionCondition.Expression.Accept(this, null);
            _sb.Append(")");
            return null;
        }

        /// <inheritdoc />
        public object Visit(AlwaysFalseCondition alwaysFalseCondition, object data)
        {
            _sb.Append("false()");
            return null;
        }

        /// <inheritdoc />
        public object Visit(AlwaysTrueCondition alwaysTrueCondition, object data)
        {
            _sb.Append("true()");
            return null;
        }

        /// <inheritdoc />
        public object Visit(ConjunctionCondition conjunctionCondition, object data)
        {
            _sb.Append($"{_getObjectIndex(conjunctionCondition)}:");
            _sb.Append("conjunction(");
            Iterate(conjunctionCondition.InnerConditions, x => x.Accept(this, null), ", ");
            _sb.Append(")");
            return null;
        }

        /// <inheritdoc />
        public object Visit(DisjunctionCondition disjunctionCondition, object data)
        {
            _sb.Append($"{_getObjectIndex(disjunctionCondition)}:");
            _sb.Append("disjunction(");
            Iterate(disjunctionCondition.InnerConditions, x => x.Accept(this, null), ", ");
            _sb.Append(")");
            return null;
        }

        /// <inheritdoc />
        public object Visit(ComparisonCondition comparisonCondition, object data)
        {
            _sb.Append($"{_getObjectIndex(comparisonCondition)}:");
            _sb.Append("(");
            comparisonCondition.LeftOperand.Accept(this, null);

            switch (comparisonCondition.ComparisonType)
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
                    _sb.Append(" <> ");
                    break;
                default:
                    throw new NotSupportedException("Unsupported comparison type");
            }

            comparisonCondition.RightOperand.Accept(this, null);
            _sb.Append(")");
            return null;
        }

        /// <inheritdoc />
        public object Visit(EqualVariablesCondition equalVariablesCondition, object data)
        {
            _sb.Append($"{_getObjectIndex(equalVariablesCondition)}:");
            _sb.Append("(");
            GetRepresentation(equalVariablesCondition.LeftVariable);
            _sb.Append("==");
            GetRepresentation(equalVariablesCondition.RightVariable);
            _sb.Append(")");
            return null;
        }

        /// <inheritdoc />
        public object Visit(IsNullCondition isNullCondition, object data)
        {
            _sb.Append($"{_getObjectIndex(isNullCondition)}:");
            _sb.Append("isnull(");
            GetRepresentation(isNullCondition.Variable);
            _sb.Append(")");
            return null;
        }

        /// <inheritdoc />
        public object Visit(NegationCondition negationCondition, object data)
        {
            _sb.Append($"{_getObjectIndex(negationCondition)}:");
            _sb.Append("not(");
            negationCondition.InnerCondition.Accept(this, null);
            _sb.Append(")");
            return null;
        }

        /// <inheritdoc />
        public object Visit(LikeCondition likeCondition, object data)
        {
            _sb.Append($"{_getObjectIndex(likeCondition)}:");
            _sb.Append("like(");
            likeCondition.Expression.Accept(this, null);
            _sb.Append(",");
            _sb.Append(likeCondition.Pattern);
            _sb.Append(")");
            return null;
        }

        /// <inheritdoc />
        public object Visit(LangMatchesCondition langMatchesCondition, object data)
        {
            _sb.Append($"{_getObjectIndex(langMatchesCondition)}:langMatches(");
            langMatchesCondition.LanguageExpression.Accept(this, null);
            _sb.Append(",");
            langMatchesCondition.LanguageRangeExpression.Accept(this, null);
            _sb.Append(")");
            return null;
        }
    }
}
