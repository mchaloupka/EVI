using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DatabaseSchemaReader.DataSchema;
using Slp.Evi.Storage.Common.Algebra;
using Slp.Evi.Storage.Query;
using Slp.Evi.Storage.Relational.Query;
using Slp.Evi.Storage.Relational.Query.Conditions.Assignment;
using Slp.Evi.Storage.Relational.Query.Conditions.Filter;
using Slp.Evi.Storage.Relational.Query.Conditions.Source;
using Slp.Evi.Storage.Relational.Query.Expressions;
using Slp.Evi.Storage.Relational.Query.Sources;
using Slp.Evi.Storage.Relational.Utils.CodeGeneration;

namespace Slp.Evi.Storage.Database.Base
{
    /// <summary>
    /// The base sql query builder
    /// </summary>
    public abstract class BaseSqlQueryBuilder
        : BaseExpressionTransformerG<BaseSqlQueryBuilder.VisitorContext, object, object, object, object, object>, ISqlQueryBuilder
    {
        /// <summary>
        /// Generates the query.
        /// </summary>
        /// <param name="relationalQuery">The relational query.</param>
        /// <param name="context">The context.</param>
        /// <returns>The query string.</returns>
        public string GenerateQuery(RelationalQuery relationalQuery, IQueryContext context)
        {
            var visitorContext = new VisitorContext(new StringBuilder(), context, relationalQuery);
            TransformCalculusSource(relationalQuery.Model, visitorContext);
            return visitorContext.StringBuilder.ToString();
        }

        /// <summary>
        /// The context for visitor
        /// </summary>
        public class VisitorContext
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="VisitorContext"/> class.
            /// </summary>
            /// <param name="stringBuilder">The string builder.</param>
            /// <param name="context">The context.</param>
            /// <param name="relationalQuery">The relational query</param>
            public VisitorContext(StringBuilder stringBuilder, IQueryContext context, RelationalQuery relationalQuery)
            {
                StringBuilder = stringBuilder;
                Context = context;
                RelationalQuery = relationalQuery;
                _modelsStack = new Stack<CalculusModel>();
            }

            /// <summary>
            /// Gets the string builder.
            /// </summary>
            /// <value>The string builder.</value>
            public StringBuilder StringBuilder { get; }

            /// <summary>
            /// Gets the context.
            /// </summary>
            /// <value>The context.</value>
            public IQueryContext Context { get; }

            /// <summary>
            /// Gets the relational query.
            /// </summary>
            /// <value>The relational query.</value>
            public RelationalQuery RelationalQuery { get; }

            /// <summary>
            /// Gets the current calculus model.
            /// </summary>
            /// <value>The current calculus model (<c>null</c> if you are topmost).</value>
            public CalculusModel CurrentCalculusModel => (_modelsStack.Count > 0) ? _modelsStack.Peek() : null;

            /// <summary>
            /// Enters the calculus model.
            /// </summary>
            /// <param name="model">The model.</param>
            public void EnterCalculusModel(CalculusModel model)
            {
                _modelsStack.Push(model);
            }

            /// <summary>
            /// Leaves the calculus model.
            /// </summary>
            public void LeaveCalculusModel()
            {
                _modelsStack.Pop();
            }

            /// <summary>
            /// The models stack
            /// </summary>
            private readonly Stack<CalculusModel> _modelsStack;
        }

        /// <summary>
        /// Process the <see cref="ModifiedCalculusModel"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override object Transform(ModifiedCalculusModel toTransform, VisitorContext data)
        {
            WriteCalculusModel(toTransform.InnerModel, data, toTransform.Ordering.ToList(), toTransform.Limit, toTransform.Offset, toTransform.IsDistinct);
            return null;
        }

        /// <summary>
        /// Process the <see cref="CalculusModel"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override object Transform(CalculusModel toTransform, VisitorContext data)
        {
            WriteCalculusModel(toTransform, data);
            return null;
        }

        /// <summary>
        /// Writes the calculus model.
        /// </summary>
        /// <param name="toTransform">To transform.</param>
        /// <param name="data">The data.</param>
        /// <param name="ordering">The ordering.</param>
        /// <param name="limit">The limit.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="isDistinct">The flag whether the result should be distinct.</param>
        private void WriteCalculusModel(CalculusModel toTransform, VisitorContext data, List<ModifiedCalculusModel.OrderingPart> ordering, int? limit, int? offset, bool isDistinct)
        {
            foreach (var sourceCondition in toTransform.SourceConditions)
            {
                data.Context.QueryNamingHelpers.AddSourceCondition(toTransform, sourceCondition);
            }

            foreach (var assignmentCondition in toTransform.AssignmentConditions)
            {
                data.Context.QueryNamingHelpers.AddAssignmentCondition(toTransform, assignmentCondition);
            }

            List<ICalculusVariable> neededVariables = new List<ICalculusVariable>();

            var parentModel = data.CurrentCalculusModel;
            if (parentModel == null)
            {
                neededVariables.AddRange(data.RelationalQuery.ValueBinders.SelectMany(x => x.NeededCalculusVariables).Distinct());
            }
            else
            {
                var sourceInParent = data.Context.QueryNamingHelpers.GetSourceCondtion(parentModel, toTransform);
                neededVariables.AddRange(sourceInParent.CalculusVariables);
            }

            if (isDistinct && ordering.Count > 0)
            {
                // The ORDER BY and DISTINCT may cause issue if it is in the same query level
                data.StringBuilder.Append("SELECT * FROM (");
            }

            data.StringBuilder.Append("SELECT");

            if (isDistinct)
            {
                data.StringBuilder.Append(" DISTINCT ");
            }

            if (limit.HasValue && !offset.HasValue)
            {
                data.StringBuilder.Append(" TOP ");
                data.StringBuilder.Append(limit.Value);
            }

            if (neededVariables.Count > 0)
            {
                for (int i = 0; i < neededVariables.Count; i++)
                {
                    if (i > 0)
                    {
                        data.StringBuilder.Append(',');
                    }

                    data.StringBuilder.Append(' ');

                    data.EnterCalculusModel(toTransform);
                    WriteCalculusVariable(neededVariables[i], toTransform, data);
                    data.LeaveCalculusModel();

                    data.StringBuilder.Append(" AS ");

                    if (parentModel == null)
                    {
                        data.StringBuilder.Append(data.Context.QueryNamingHelpers.GetVariableName(null, neededVariables[i]));
                    }
                    else
                    {
                        var sourceInParent = data.Context.QueryNamingHelpers.GetSourceCondtion(parentModel, toTransform);
                        data.StringBuilder.Append(data.Context.QueryNamingHelpers.GetVariableName(sourceInParent, neededVariables[i]));
                    }
                }
            }
            else
            {
                data.StringBuilder.Append(" NULL AS c");
            }

            data.EnterCalculusModel(toTransform);

            var sourceConditions = toTransform.SourceConditions.Where(x => !(x is LeftJoinCondition)).ToList();

            if (sourceConditions.Count > 0)
            {
                data.StringBuilder.Append(" FROM ");

                for (int i = 0; i < sourceConditions.Count; i++)
                {
                    if (i > 0)
                    {
                        data.StringBuilder.Append(" INNER JOIN ");
                    }

                    var sourceCondition = sourceConditions[i];

                    TransformSourceCondition(sourceCondition, data);
                    data.StringBuilder.Append(" AS ");
                    data.StringBuilder.Append(data.Context.QueryNamingHelpers.GetSourceConditionName(sourceCondition));

                    if (i > 0)
                    {
                        data.StringBuilder.Append(" ON 1=1"); // TODO: Some conditions could be there instead of 1=1
                    }
                }
            }

            var leftJoinConditions = toTransform.SourceConditions.OfType<LeftJoinCondition>().ToList();
            foreach (var leftJoinCondition in leftJoinConditions)
            {
                TransformSourceCondition(leftJoinCondition, data);
            }

            var filterConditions = toTransform.FilterConditions.ToList();

            if (filterConditions.Count > 0)
            {
                data.StringBuilder.Append(" WHERE ");

                for (int i = 0; i < filterConditions.Count; i++)
                {
                    if (i > 0)
                    {
                        data.StringBuilder.Append(" AND ");
                    }

                    TransformFilterCondition(filterConditions[i], data);
                }
            }

            data.LeaveCalculusModel();

            if (isDistinct && ordering.Count > 0)
            {
                // The ORDER BY and DISTINCT may cause issue if it is in the same query lavel
                data.StringBuilder.Append(") AS o");
            }

            bool firstOrderBy = true;
            foreach (var orderingPart in ordering.Where(x => !x.Expression.HasAlwaysTheSameValue))
            {
                if (firstOrderBy)
                {
                    data.StringBuilder.Append(" ORDER BY ");
                    firstOrderBy = false;
                }
                else
                {
                    data.StringBuilder.Append(", ");
                }

                TransformExpression(orderingPart.Expression, data);

                if (orderingPart.IsDescending)
                {
                    data.StringBuilder.Append(" DESC");
                }
            }

            if (offset.HasValue)
            {
                if (firstOrderBy)
                {
                    throw new Exception("To enable offset, it is needed to use also order by clause");
                }

                data.StringBuilder.Append(" OFFSET ");
                data.StringBuilder.Append(offset.Value);
                data.StringBuilder.Append(" ROWS");

                if (limit.HasValue)
                {
                    data.StringBuilder.Append(" FETCH NEXT ");
                    data.StringBuilder.Append(limit.Value);
                    data.StringBuilder.Append(" ROWS ONLY");
                }
            }
        }

        private void WriteCalculusModel(CalculusModel toTransform, VisitorContext data)
        {
            WriteCalculusModel(toTransform, data, new List<ModifiedCalculusModel.OrderingPart>(), null, null, false);
        }

        /// <summary>
        /// Process the <see cref="SqlTable"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override object Transform(SqlTable toTransform, VisitorContext data)
        {
            data.StringBuilder.Append(toTransform.TableName);
            return null;
        }

        /// <summary>
        /// Process the <see cref="AlwaysFalseCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override object Transform(AlwaysFalseCondition toTransform, VisitorContext data)
        {
            data.StringBuilder.Append("1=0");
            return null;
        }

        /// <summary>
        /// Process the <see cref="AlwaysTrueCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override object Transform(AlwaysTrueCondition toTransform, VisitorContext data)
        {
            data.StringBuilder.Append("1=1");
            return null;
        }

        /// <summary>
        /// Process the <see cref="ConjunctionCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override object Transform(ConjunctionCondition toTransform, VisitorContext data)
        {
            data.StringBuilder.Append("(");

            var innerConditions = toTransform.InnerConditions.ToList();

            for (int i = 0; i < innerConditions.Count; i++)
            {
                if (i > 0)
                {
                    data.StringBuilder.Append(" AND ");
                }

                TransformFilterCondition(innerConditions[i], data);
            }

            data.StringBuilder.Append(")");
            return null;
        }

        /// <summary>
        /// Process the <see cref="DisjunctionCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override object Transform(DisjunctionCondition toTransform, VisitorContext data)
        {
            data.StringBuilder.Append("(");

            var innerConditions = toTransform.InnerConditions.ToList();

            for (int i = 0; i < innerConditions.Count; i++)
            {
                if (i > 0)
                {
                    data.StringBuilder.Append(" OR ");
                }

                TransformFilterCondition(innerConditions[i], data);
            }

            data.StringBuilder.Append(")");
            return null;
        }

        /// <summary>
        /// Process the <see cref="ComparisonCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override object Transform(ComparisonCondition toTransform, VisitorContext data)
        {
            TransformComparisonCondition(() => TransformExpression(toTransform.LeftOperand, data),
                () => TransformExpression(toTransform.RightOperand, data), x => data.StringBuilder.Append(x),
                toTransform.LeftOperand.SqlType, toTransform.RightOperand.SqlType, toTransform.ComparisonType, data);

            return null;
        }

        /// <summary>
        /// Process the <see cref="EqualVariablesCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override object Transform(EqualVariablesCondition toTransform, VisitorContext data)
        {
            var leftExpr = toTransform.LeftVariable;
            var rightExpr = toTransform.RightVariable;

            TransformComparisonCondition(() => WriteCalculusVariable(leftExpr, data.CurrentCalculusModel, data),
                () => WriteCalculusVariable(rightExpr, data.CurrentCalculusModel, data),
                x => data.StringBuilder.Append(x), leftExpr.SqlType, rightExpr.SqlType, ComparisonTypes.EqualTo, data);

            return null;
        }

        /// <summary>
        /// Transforms the equal condition.
        /// </summary>
        /// <param name="writeLeft">The write left action.</param>
        /// <param name="writeRight">The write right action.</param>
        /// <param name="writeText">The write text action.</param>
        /// <param name="leftDataType">Type of the left data.</param>
        /// <param name="rightDataType">Type of the right data.</param>
        /// <param name="comparisonType">Comparison type</param>
        /// <param name="context">The query context</param>
        protected virtual void TransformComparisonCondition(Action writeLeft, Action writeRight, Action<string> writeText, DataType leftDataType, DataType rightDataType, ComparisonTypes comparisonType, VisitorContext context)
        {
            var commonType = context.Context.Db.GetCommonTypeForComparison(leftDataType, rightDataType).TypeName;

            if (leftDataType.TypeName == commonType)
            {
                writeLeft();
            }
            else
            {
                writeText("CAST(");
                writeLeft();
                writeText($" AS {commonType})");
            }

            WriteComparisonOperator(writeText, comparisonType);

            if (rightDataType.TypeName == commonType)
            {
                writeRight();
            }
            else
            {
                writeText("CAST(");
                writeRight();
                writeText($" AS {commonType})");
            }
        }

        /// <inheritdoc />
        protected override object Transform(BinaryNumericExpression toTransform, VisitorContext data)
        {
            var commonType = data.Context.Db.GetCommonTypeForComparison(toTransform.LeftOperand.SqlType, toTransform.RightOperand.SqlType).TypeName;

            var leftCast = toTransform.LeftOperand.SqlType.TypeName != commonType;

            if (leftCast)
            {
                data.StringBuilder.Append("CAST((");
            }

            toTransform.LeftOperand.Accept(this, data);

            if (leftCast)
            {
                data.StringBuilder.Append($") AS {commonType})");
            }

            switch (toTransform.Operator)
            {
                case ArithmeticOperation.Add:
                    data.StringBuilder.Append("+");
                    break;
                case ArithmeticOperation.Subtract:
                    data.StringBuilder.Append("-");
                    break;
                case ArithmeticOperation.Divide:
                    data.StringBuilder.Append("/");
                    break;
                case ArithmeticOperation.Multiply:
                    data.StringBuilder.Append("*");
                    break;
                default:
                    throw new NotImplementedException();
            }

            var rightCast = toTransform.RightOperand.SqlType.TypeName != commonType;

            if (rightCast)
            {
                data.StringBuilder.Append("CAST((");
            }

            toTransform.RightOperand.Accept(this, data);

            if (rightCast)
            {
                data.StringBuilder.Append($") AS {commonType})");
            }

            return null;
        }

        /// <summary>
        /// Writes the comparison operator.
        /// </summary>
        /// <param name="writeText">The write text action.</param>
        /// <param name="comparisonType">Type of the comparison.</param>
        protected virtual void WriteComparisonOperator(Action<string> writeText, ComparisonTypes comparisonType)
        {
            switch (comparisonType)
            {
                case ComparisonTypes.GreaterThan:
                    writeText(">");
                    break;
                case ComparisonTypes.GreaterOrEqualThan:
                    writeText(">=");
                    break;
                case ComparisonTypes.LessThan:
                    writeText("<");
                    break;
                case ComparisonTypes.LessOrEqualThan:
                    writeText("<=");
                    break;
                case ComparisonTypes.EqualTo:
                    writeText("=");
                    break;
                case ComparisonTypes.NotEqualTo:
                    writeText("<>");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Process the <see cref="IsNullCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override object Transform(IsNullCondition toTransform, VisitorContext data)
        {
            WriteCalculusVariable(toTransform.Variable, data.CurrentCalculusModel, data);
            data.StringBuilder.Append(" IS NULL");
            return null;
        }

        /// <summary>
        /// Process the <see cref="NegationCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override object Transform(NegationCondition toTransform, VisitorContext data)
        {
            data.StringBuilder.Append("NOT ");
            TransformFilterCondition(toTransform.InnerCondition, data);
            return null;
        }

        /// <summary>
        /// Process the <see cref="TupleFromSourceCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override object Transform(TupleFromSourceCondition toTransform, VisitorContext data)
        {
            TransformNestedCalculusSource(toTransform.Source, data);
            return null;
        }

        /// <summary>
        /// Transforms the nested calculus source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="data">The data.</param>
        private void TransformNestedCalculusSource(ICalculusSource source, VisitorContext data)
        {
            bool needsEscaping = !(source is SqlTable);

            if (needsEscaping)
            {
                data.StringBuilder.Append("(");
            }

            TransformCalculusSource(source, data);

            if (needsEscaping)
            {
                data.StringBuilder.Append(")");
            }
        }

        /// <summary>
        /// Process the <see cref="UnionedSourcesCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override object Transform(UnionedSourcesCondition toTransform, VisitorContext data)
        {
            data.StringBuilder.Append("(");

            bool first = true;

            foreach (var calculusSource in toTransform.Sources)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    data.StringBuilder.Append(" UNION ALL ");
                }

                TransformCalculusSource(calculusSource, data);
            }

            data.StringBuilder.Append(")");
            return null;
        }

        /// <summary>
        /// Process the <see cref="LeftJoinCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override object Transform(LeftJoinCondition toTransform, VisitorContext data)
        {
            data.StringBuilder.Append(" LEFT JOIN ");
            TransformNestedCalculusSource(toTransform.RightOperand, data);

            data.StringBuilder.Append(" AS ");
            data.StringBuilder.Append(data.Context.QueryNamingHelpers.GetSourceConditionName(toTransform));

            data.StringBuilder.Append(" ON ");

            if (toTransform.JoinConditions.Length > 0)
            {
                for (int i = 0; i < toTransform.JoinConditions.Length; i++)
                {
                    if (i > 0)
                    {
                        data.StringBuilder.Append(" AND ");
                    }

                    var condition = toTransform.JoinConditions[i];
                    TransformFilterCondition(condition, data);
                }
            }
            else
            {
                data.StringBuilder.Append("1=1");
            }

            data.StringBuilder.Append(" ");
            return null;
        }

        /// <summary>
        /// Process the <see cref="ColumnExpression"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override object Transform(ColumnExpression toTransform, VisitorContext data)
        {
            WriteCalculusVariable(toTransform.CalculusVariable, data.CurrentCalculusModel, data);
            return null;
        }

        /// <summary>
        /// Process the <see cref="ConcatenationExpression"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override object Transform(ConcatenationExpression toTransform, VisitorContext data)
        {
            data.StringBuilder.Append("CONCAT(");

            var parts = toTransform.InnerExpressions.ToList();

            for (int i = 0; i < parts.Count; i++)
            {
                if (i > 0)
                {
                    data.StringBuilder.Append(",");
                }

                if (parts[i].SqlType.IsString)
                {
                    TransformExpression(parts[i], data);
                }
                else
                {
                    data.StringBuilder.Append("CAST(");
                    TransformExpression(parts[i], data);
                    data.StringBuilder.Append(" AS nvarchar(MAX))");
                }
            }

            data.StringBuilder.Append(")");
            return null;
        }

        /// <summary>
        /// Process the <see cref="ConstantExpression"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override object Transform(ConstantExpression toTransform, VisitorContext data)
        {
            data.StringBuilder.Append(toTransform.SqlString);
            return null;
        }

        /// <summary>
        /// Process the <see cref="CaseExpression"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override object Transform(CaseExpression toTransform, VisitorContext data)
        {
            data.StringBuilder.Append("CASE");

            foreach (var statement in toTransform.Statements)
            {
                data.StringBuilder.Append(" WHEN ");
                TransformFilterCondition(statement.Condition, data);
                data.StringBuilder.Append(" THEN ");
                TransformExpression(statement.Expression, data);
            }

            data.StringBuilder.Append(" END");
            return null;
        }

        /// <summary>
        /// Process the <see cref="CoalesceExpression"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override object Transform(CoalesceExpression toTransform, VisitorContext data)
        {
            data.StringBuilder.Append("COALESCE(");

            for (int i = 0; i < toTransform.InnerExpressions.Length; i++)
            {
                if (i > 0)
                {
                    data.StringBuilder.Append(", ");
                }

                TransformExpression(toTransform.InnerExpressions[i], data);
            }

            data.StringBuilder.Append(")");
            return null;
        }

        /// <inheritdoc />
        protected override object Transform(NullExpression toTransform, VisitorContext data)
        {
            data.StringBuilder.Append("NULL");
            return null;
        }

        /// <summary>
        /// Writes the calculus variable.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <param name="currentModel">The current model.</param>
        /// <param name="data"></param>
        private void WriteCalculusVariable(ICalculusVariable variable, CalculusModel currentModel, VisitorContext data)
        {
            var stringBuilder = data.StringBuilder;
            var context = data.Context;

            if (currentModel == null)
            {
                stringBuilder.Append(context.QueryNamingHelpers.GetVariableName(null, variable));
            }
            else
            {
                var variableSource = context.QueryNamingHelpers.GetSourceOfVariable(variable, currentModel);

                if (variableSource == null)
                {
                    stringBuilder.Append("NULL");
                }
                else if (variableSource is ISourceCondition sourceCondition)
                {
                    stringBuilder.Append(context.QueryNamingHelpers.GetSourceConditionName(sourceCondition));
                    stringBuilder.Append('.');
                    stringBuilder.Append(context.QueryNamingHelpers.GetVariableName(sourceCondition, variable));
                }
                else if (variableSource is IAssignmentCondition assignmentCondition)
                {
                    assignmentCondition.Accept(new WriteCalculusVariable_Assignment_Visitor(this), data);
                }
                else
                {
                    throw new ArgumentException("Unexpected variable source", nameof(variable));
                }
            }
        }

        /// <summary>
        /// Visitor for writing the calculus variable comming from assignment condition
        /// </summary>
        private class WriteCalculusVariable_Assignment_Visitor : IAssignmentConditionVisitor
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="WriteCalculusVariable_Assignment_Visitor"/> class.
            /// </summary>
            /// <param name="queryBuilder">The query builder.</param>
            public WriteCalculusVariable_Assignment_Visitor(BaseSqlQueryBuilder queryBuilder)
            {
                QueryBuilder = queryBuilder;
            }

            /// <summary>
            /// Visits <see cref="AssignmentFromExpressionCondition"/>
            /// </summary>
            /// <param name="assignmentFromExpressionCondition">The visited instance</param>
            /// <param name="data">The passed data</param>
            /// <returns>The returned data</returns>
            public object Visit(AssignmentFromExpressionCondition assignmentFromExpressionCondition, object data)
            {
                QueryBuilder.TransformExpression(assignmentFromExpressionCondition.Expression, (VisitorContext) data);
                return null;
            }

            /// <summary>
            /// Gets or sets the query builder.
            /// </summary>
            /// <value>The query builder.</value>
            public BaseSqlQueryBuilder QueryBuilder { get; }
        }

        /// <summary>
        /// Process the <see cref="AssignmentFromExpressionCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override object Transform(AssignmentFromExpressionCondition toTransform, VisitorContext data)
        {
            throw new NotImplementedException();
        }
    }
}
