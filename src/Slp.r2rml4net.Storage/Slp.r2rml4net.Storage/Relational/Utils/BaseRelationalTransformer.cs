using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Relational.Query;
using Slp.r2rml4net.Storage.Relational.Query.Conditions;
using Slp.r2rml4net.Storage.Relational.Query.Conditions.Filter;
using Slp.r2rml4net.Storage.Relational.Query.Conditions.Source;
using Slp.r2rml4net.Storage.Relational.Query.Expressions;
using Slp.r2rml4net.Storage.Relational.Query.Sources;
using Slp.r2rml4net.Storage.Relational.Utils.CodeGeneration;
using Slp.r2rml4net.Storage.Sparql.Algebra;
using Slp.r2rml4net.Storage.Utils;

namespace Slp.r2rml4net.Storage.Relational.Utils
{
    /// <summary>
    /// Base transformer for <see cref="ICalculusSource" />
    /// </summary>
    /// <typeparam name="T">Type of parameter passed to process</typeparam>
    public class BaseRelationalTransformer<T>
        : BaseExpressionTransformerG<T, IExpression,IAssignmentCondition, ISourceCondition, IFilterCondition, ICalculusSource>
    {
        /// <summary>
        /// Process the <see cref="CalculusModel" />
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override ICalculusSource Transform(CalculusModel toTransform, T data)
        {
            var newConditions = new List<ICondition>();
            bool changed = false;

            foreach (var sourceCondition in toTransform.SourceConditions)
            {
                var newCondition = Transform(sourceCondition, data);
                newConditions.Add(newCondition);

                if (newCondition != sourceCondition)
                {
                    changed = true;
                }
            }

            foreach (var assignmentCondition in toTransform.AssignmentConditions)
            {
                var newCondition = Transform(assignmentCondition, data);
                newConditions.Add(newCondition);

                if (newCondition != assignmentCondition)
                {
                    changed = true;
                }
            }

            var newFilterConditions = new List<IFilterCondition>();

            foreach (var filterCondition in toTransform.FilterConditions)
            {
                var newCondition = Transform(filterCondition, data);
                if (newCondition is AlwaysTrueCondition)
                {
                    changed = true;
                }
                else if (newCondition is AlwaysFalseCondition)
                {
                    changed = true;
                    newFilterConditions.Clear();
                    newFilterConditions.Add(new AlwaysFalseCondition());
                    break;
                }
                else if (newCondition is ConjunctionCondition)
                {
                    changed = true;
                    newFilterConditions.AddRange(((ConjunctionCondition)newCondition).InnerConditions);
                }
                else
                {
                    if (newCondition != filterCondition)
                    {
                        changed = true;
                    }

                    newFilterConditions.Add(newCondition);
                }
            }

            if (changed)
            {
                newConditions.AddRange(newFilterConditions);

                return new CalculusModel(toTransform.Variables.ToArray(), newConditions);
            }
            else
            {
                return toTransform;
            }
        }

        /// <summary>
        /// Process the <see cref="SqlTable" />
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        protected override ICalculusSource Transform(SqlTable toTransform, T data)
        {
            return toTransform;
        }

        /// <summary>
        /// Process the <see cref="AlwaysFalseCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IFilterCondition Transform(AlwaysFalseCondition toTransform, T data)
        {
            return toTransform;
        }

        /// <summary>
        /// Process the <see cref="AlwaysTrueCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IFilterCondition Transform(AlwaysTrueCondition toTransform, T data)
        {
            return toTransform;
        }

        /// <summary>
        /// Process the <see cref="ConjunctionCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IFilterCondition Transform(ConjunctionCondition toTransform, T data)
        {
            bool changed = false;
            List<IFilterCondition> innerConditions = new List<IFilterCondition>();

            foreach (var condition in toTransform.InnerConditions)
            {
                var newCondition = Transform(condition, data);

                if (newCondition is AlwaysFalseCondition)
                {
                    return newCondition;
                }
                else if (newCondition is AlwaysTrueCondition)
                {
                    changed = true;
                }
                else if (newCondition is ConjunctionCondition)
                {
                    innerConditions.AddRange(((ConjunctionCondition) newCondition).InnerConditions);
                    changed = true;
                }
                else
                {
                    innerConditions.Add(newCondition);

                    if (newCondition != condition)
                    {
                        changed = true;
                    }
                }
            }

            if (innerConditions.Count == 0)
            {
                return new AlwaysTrueCondition();
            }
            else if (innerConditions.Count == 1)
            {
                return innerConditions[0];
            }
            else if (changed)
            {
                return new ConjunctionCondition(innerConditions);
            }
            else
            {
                return toTransform;
            }
        }

        /// <summary>
        /// Process the <see cref="DisjunctionCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IFilterCondition Transform(DisjunctionCondition toTransform, T data)
        {
            bool changed = false;
            List<IFilterCondition> innerConditions = new List<IFilterCondition>();

            foreach (var condition in toTransform.InnerConditions)
            {
                var newCondition = Transform(condition, data);

                if (newCondition is AlwaysTrueCondition)
                {
                    return newCondition;
                }
                else if (newCondition is AlwaysFalseCondition)
                {
                    changed = true;
                }
                else if (newCondition is DisjunctionCondition)
                {
                    innerConditions.AddRange(((DisjunctionCondition)newCondition).InnerConditions);
                    changed = true;
                }
                else
                {
                    innerConditions.Add(newCondition);

                    if (newCondition != condition)
                    {
                        changed = true;
                    }
                }
            }

            if (innerConditions.Count == 0)
            {
                return new AlwaysFalseCondition();
            }
            else if (innerConditions.Count == 1)
            {
                return innerConditions[0];
            }
            else if (changed)
            {
                return new DisjunctionCondition(innerConditions);
            }
            else
            {
                return toTransform;
            }
        }

        /// <summary>
        /// Process the <see cref="EqualExpressionCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IFilterCondition Transform(EqualExpressionCondition toTransform, T data)
        {
            var newLeftOperand = Transform(toTransform.LeftOperand, data);
            var newRightOperand = Transform(toTransform.RightOperand, data);

            if (newLeftOperand != toTransform.LeftOperand || newRightOperand != toTransform.RightOperand)
            {
                return new EqualExpressionCondition(newLeftOperand, newRightOperand);
            }
            else
            {
                return toTransform;
            }
        }

        /// <summary>
        /// Process the <see cref="EqualVariablesCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IFilterCondition Transform(EqualVariablesCondition toTransform, T data)
        {
            return toTransform;
        }

        /// <summary>
        /// Process the <see cref="IsNullCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IFilterCondition Transform(IsNullCondition toTransform, T data)
        {
            return toTransform;
        }

        /// <summary>
        /// Process the <see cref="NegationCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IFilterCondition Transform(NegationCondition toTransform, T data)
        {
            var newInner = Transform(toTransform.InnerCondition, data);

            if (newInner is NegationCondition)
            {
                return ((NegationCondition) newInner).InnerCondition;
            }
            else if (newInner is AlwaysFalseCondition)
            {
                return new AlwaysTrueCondition();
            }
            else if (newInner is AlwaysTrueCondition)
            {
                return new AlwaysFalseCondition();
            }
            else if (newInner != toTransform.InnerCondition)
            {
                return new NegationCondition(newInner);
            }
            else
            {
                return toTransform;
            }
        }

        /// <summary>
        /// Process the <see cref="TupleFromSourceCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override ISourceCondition Transform(TupleFromSourceCondition toTransform, T data)
        {
            var newSource = Transform(toTransform.Source, data);

            if (newSource != toTransform.Source)
            {
                return new TupleFromSourceCondition(toTransform.CalculusVariables, newSource);
            }
            else
            {
                return toTransform;
            }
        }

        /// <summary>
        /// Process the <see cref="ColumnExpression"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IExpression Transform(ColumnExpression toTransform, T data)
        {
            return toTransform;
        }

        /// <summary>
        /// Process the <see cref="ConcatenationExpression"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IExpression Transform(ConcatenationExpression toTransform, T data)
        {
            List<IExpression> innerExpressions = new List<IExpression>();
            bool changed = false;

            foreach (var innerExpression in toTransform.InnerExpressions)
            {
                var newExpression = Transform(innerExpression, data);

                innerExpressions.Add(newExpression);

                if (newExpression != innerExpression)
                {
                    changed = true;
                }
            }

            if (changed)
            {
                return new ConcatenationExpression(innerExpressions, toTransform.SqlType);
            }
            else
            {
                return toTransform;
            }
        }

        /// <summary>
        /// Process the <see cref="ConstantExpression"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IExpression Transform(ConstantExpression toTransform, T data)
        {
            return toTransform;
        }
    }
}
