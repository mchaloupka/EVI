using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseSchemaReader.DataSchema;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Relational.Query;
using Slp.r2rml4net.Storage.Relational.Query.Condition;
using Slp.r2rml4net.Storage.Relational.Query.Expression;
using Slp.r2rml4net.Storage.Relational.Query.Source;
using Slp.r2rml4net.Storage.Relational.Utils.CodeGeneration;

namespace Slp.r2rml4net.Storage.Database.Base
{
    /// <summary>
    /// The base sql query builder
    /// </summary>
    public class BaseSqlQueryBuilder
        : BaseExpressionTransformerG<BaseSqlQueryBuilder.VisitorContext, object, object, object>, ISqlQueryBuilder
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
        /// Fallback variant for the transformation.
        /// </summary>
        /// <param name="toTransform">Instance to be transformed.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The transformation result</returns>
        protected override object CommonFallbackTransform(ICalculusSource toTransform, VisitorContext data)
        {
            throw new Exception("This code should not be reached");
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
                var sourceInParent = data.Context.QueryNamingHelpers.GetTupleFromSourceCondtion(parentModel, this);
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
                    WriteCalculusVariable(neededVariables[i], data.StringBuilder, toTransform, data.Context);

                    data.StringBuilder.Append(" AS ");

                    if (parentModel == null)
                    {
                        data.StringBuilder.Append(data.Context.QueryNamingHelpers.GetVariableName(null, neededVariables[i]));
                    }
                    else
                    {
                        var sourceInParent = data.Context.QueryNamingHelpers.GetTupleFromSourceCondtion(parentModel, this);
                        data.StringBuilder.Append(data.Context.QueryNamingHelpers.GetVariableName(sourceInParent, neededVariables[i]));
                    }
                }
            }
            else
            {
                data.StringBuilder.Append(" NULL AS c");
            }

            data.EnterCalculusModel(toTransform);



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
        /// Fallback variant for the transformation.
        /// </summary>
        /// <param name="toTransform">Instance to be transformed.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The transformation result</returns>
        protected override object CommonFallbackTransform(ICondition toTransform, VisitorContext data)
        {
            throw new Exception("This code should not be reached");
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
                () => Transform(leftExpr, data),
                () => Transform(rightExpr, data),
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
                () => WriteCalculusVariable(leftExpr, data.StringBuilder, data.CurrentCalculusModel, data.Context),
                () => WriteCalculusVariable(rightExpr, data.StringBuilder, data.CurrentCalculusModel, data.Context),
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
            WriteCalculusVariable(toTransform.Variable, data.StringBuilder, data.CurrentCalculusModel, data.Context);
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
            Transform(toTransform.InnerCondition, data);
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
            throw new Exception("This code should not be reached");
        }

        /// <summary>
        /// Fallback variant for the transformation.
        /// </summary>
        /// <param name="toTransform">Instance to be transformed.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The transformation result</returns>
        protected override object CommonFallbackTransform(IExpression toTransform, VisitorContext data)
        {
            throw new Exception("This code should not be reached");
        }

        /// <summary>
        /// Process the <see cref="ColumnExpression"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override object Transform(ColumnExpression toTransform, VisitorContext data)
        {
            WriteCalculusVariable(toTransform.CalculusVariable, data.StringBuilder, data.CurrentCalculusModel, data.Context);
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
                    Transform(parts[i], data);
                }
                else
                {
                    data.StringBuilder.Append("CAST(");
                    Transform(parts[i], data);
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
        /// <param name="stringBuilder">The string builder.</param>
        /// <param name="currentModel">The current model.</param>
        /// <param name="context">The context.</param>
        private void WriteCalculusVariable(ICalculusVariable variable, StringBuilder stringBuilder, CalculusModel currentModel, QueryContext context)
        {
            var variableSource = context.QueryNamingHelpers.GetSourceOfVariable(variable, currentModel);

            if (variableSource is ISourceCondition)
            {
                var sourceCondition = (ISourceCondition) variableSource;

                stringBuilder.Append(context.QueryNamingHelpers.GetSourceConditionName(sourceCondition));
                stringBuilder.Append('.');
                stringBuilder.Append(context.QueryNamingHelpers.GetVariableName(sourceCondition, variable));
            }
            else if (variableSource is IAssignmentCondition)
            {
                throw new NotImplementedException();
            }
            else
            {
                throw new ArgumentException("Unexpected variable", "variable");
            }
        }
    }
}
