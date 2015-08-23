using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseSchemaReader.DataSchema;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Relational.Query;
using Slp.r2rml4net.Storage.Relational.Query.Conditions;
using Slp.r2rml4net.Storage.Relational.Query.Conditions.Assignment;
using Slp.r2rml4net.Storage.Relational.Query.Conditions.Filter;
using Slp.r2rml4net.Storage.Relational.Query.Conditions.Source;
using Slp.r2rml4net.Storage.Relational.Query.Expressions;
using Slp.r2rml4net.Storage.Relational.Query.Sources;
using Slp.r2rml4net.Storage.Relational.Utils.CodeGeneration;

namespace Slp.r2rml4net.Storage.Database.Base
{
    /// <summary>
    /// The base sql query builder
    /// </summary>
    public class BaseSqlQueryBuilder
        : BaseExpressionTransformerG<BaseSqlQueryBuilder.VisitorContext, object, object, object, object, object>, ISqlQueryBuilder
    {
        /// <summary>
        /// Generates the query.
        /// </summary>
        /// <param name="relationalQuery">The relational query.</param>
        /// <param name="context">The context.</param>
        /// <returns>The query string.</returns>
        public string GenerateQuery(RelationalQuery relationalQuery, QueryContext context)
        {
            var visitorContext = new VisitorContext(new StringBuilder(), context, relationalQuery);
            Transform(relationalQuery.Model, visitorContext);
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
            public VisitorContext(StringBuilder stringBuilder, QueryContext context, RelationalQuery relationalQuery)
            {
                StringBuilder = stringBuilder;
                Context = context;
                RelationalQuery = relationalQuery;
                modelsStack = new Stack<CalculusModel>();
            }

            /// <summary>
            /// Gets the string builder.
            /// </summary>
            /// <value>The string builder.</value>
            public StringBuilder StringBuilder { get; private set; }

            /// <summary>
            /// Gets the context.
            /// </summary>
            /// <value>The context.</value>
            public QueryContext Context { get; private set; }

            /// <summary>
            /// Gets the relational query.
            /// </summary>
            /// <value>The relational query.</value>
            public RelationalQuery RelationalQuery { get; private set; }

            /// <summary>
            /// Gets the current calculus model.
            /// </summary>
            /// <value>The current calculus model (<c>null</c> if you are topmost).</value>
            public CalculusModel CurrentCalculusModel { get { return (modelsStack.Count > 0) ? modelsStack.Peek() : null; } }

            /// <summary>
            /// Enters the calculus model.
            /// </summary>
            /// <param name="model">The model.</param>
            public void EnterCalculusModel(CalculusModel model)
            {
                modelsStack.Push(model);
            }

            /// <summary>
            /// Leaves the calculus model.
            /// </summary>
            public void LeaveCalculusModel()
            {
                modelsStack.Pop();
            }

            /// <summary>
            /// The models stack
            /// </summary>
            private Stack<CalculusModel> modelsStack;
        }

        /// <summary>
        /// Process the <see cref="CalculusModel"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override object Transform(CalculusModel toTransform, VisitorContext data)
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

            data.StringBuilder.Append("SELECT");

            if (neededVariables.Count > 0)
            {
                for (int i = 0; i < neededVariables.Count; i++)
                {
                    if (i > 0)
                    {
                        data.StringBuilder.Append(',');
                    }

                    data.StringBuilder.Append(' ');
                    WriteCalculusVariable(neededVariables[i], toTransform, data);

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

            var sourceConditions = toTransform.SourceConditions.ToList();

            if (sourceConditions.Count > 0)
            {
                data.StringBuilder.Append(" FROM ");

                for (int i = 0; i < sourceConditions.Count; i++)
                {
                    if (i > 0)
                    {
                        data.StringBuilder.Append(',');
                    }

                    var sourceCondition = sourceConditions[i];

                    TransformSourceCondition(sourceCondition, data);
                    data.StringBuilder.Append(" AS ");
                    data.StringBuilder.Append(data.Context.QueryNamingHelpers.GetSourceConditionName(sourceCondition));
                }
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
            return null;
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
        /// Process the <see cref="EqualExpressionCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override object Transform(EqualExpressionCondition toTransform, VisitorContext data)
        {
            var leftExpr = toTransform.LeftOperand;
            var rightExpr = toTransform.RightOperand;

            TransformEqualCondition(
                () => TransformExpression(leftExpr, data),
                () => TransformExpression(rightExpr, data),
                x => data.StringBuilder.Append(x),
                leftExpr.SqlType,
                rightExpr.SqlType);

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

            TransformEqualCondition(
                () => WriteCalculusVariable(leftExpr, data.CurrentCalculusModel, data),
                () => WriteCalculusVariable(rightExpr, data.CurrentCalculusModel, data),
                x => data.StringBuilder.Append(x),
                leftExpr.SqlType,
                rightExpr.SqlType);

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
        protected virtual void TransformEqualCondition(Action writeLeft, Action writeRight, Action<string> writeText,
            DataType leftDataType, DataType rightDataType)
        {
            if (leftDataType.TypeName == rightDataType.TypeName)
            {
                writeLeft();
                writeText("=");
                writeRight();
            }
            else
            {
                writeText("CAST(");
                writeLeft();
                writeText(" AS nvarchar(MAX))");

                writeText("=");

                writeText("CAST(");
                writeRight();
                writeText(" AS nvarchar(MAX))");
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
            TransformCalculusSource(toTransform.Source, data);
            return null;
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
        /// Writes the calculus variable.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <param name="currentModel">The current model.</param>
        /// <param name="data"></param>
        /// 
        /// 
        private void WriteCalculusVariable(ICalculusVariable variable, CalculusModel currentModel, VisitorContext data)
        {
            var stringBuilder = data.StringBuilder;
            var context = data.Context;

            var variableSource = context.QueryNamingHelpers.GetSourceOfVariable(variable, currentModel);

            if (variableSource == null)
            {
                stringBuilder.Append("NULL");
            }
            else if (variableSource is ISourceCondition)
            {
                var sourceCondition = (ISourceCondition)variableSource;

                stringBuilder.Append(context.QueryNamingHelpers.GetSourceConditionName(sourceCondition));
                stringBuilder.Append('.');
                stringBuilder.Append(context.QueryNamingHelpers.GetVariableName(sourceCondition, variable));
            }
            else if (variableSource is IAssignmentCondition)
            {
                var assignmentCondition = (IAssignmentCondition) variableSource;
                assignmentCondition.Accept(new WriteCalculusVariable_Assignment_Visitor(this), data);
            }
            else
            {
                throw new ArgumentException("Unexpected variable source", "variable");
            }
        }

        /// <summary>
        /// Visitor for writing the calculus variable comming from assignment condition
        /// </summary>
        private class WriteCalculusVariable_Assignment_Visitor
            : IAssignmentConditionVisitor
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
            public BaseSqlQueryBuilder QueryBuilder { get; private set; }
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
