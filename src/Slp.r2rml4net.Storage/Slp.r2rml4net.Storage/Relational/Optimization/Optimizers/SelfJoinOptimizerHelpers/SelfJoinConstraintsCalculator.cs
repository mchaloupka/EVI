using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DatabaseSchemaReader.DataSchema;
using Slp.r2rml4net.Storage.Relational.Query;
using Slp.r2rml4net.Storage.Relational.Query.Conditions.Filter;
using Slp.r2rml4net.Storage.Relational.Query.Expressions;
using Slp.r2rml4net.Storage.Relational.Query.Sources;

namespace Slp.r2rml4net.Storage.Relational.Optimization.Optimizers.SelfJoinOptimizerHelpers
{
    /// <summary>
    /// Calculator of self join constraints, able to find self join of SqlTables according to filter conditions
    /// </summary>
    public class SelfJoinConstraintsCalculator
        : IFilterConditionVisitor
    {
        /// <summary>
        /// Processes the self join conditions.
        /// </summary>
        /// <param name="filterConditions">The filter conditions.</param>
        /// <param name="presentTables"></param>
        /// <param name="data">The data.</param>
        /// <returns>List of all tables that are self joined</returns>
        public Dictionary<SqlTable, SqlTable> ProcessSelfJoinConditions(IEnumerable<IFilterCondition> filterConditions, List<SqlTable> presentTables, BaseRelationalOptimizer<SelfJoinOptimizerData>.OptimizationContext data)
        {
            var result = new Dictionary<SqlTable, SqlTable>();

            var satisfactionMap = SatisfactionMap.CreateInitialSatisfactionMap(presentTables, data.Context);



            return result;
        }

        /// <summary>
        /// Visits <see cref="AlwaysFalseCondition"/>
        /// </summary>
        /// <param name="alwaysFalseCondition">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(AlwaysFalseCondition alwaysFalseCondition, object data)
        {
            return data;
        }

        /// <summary>
        /// Visits <see cref="AlwaysTrueCondition"/>
        /// </summary>
        /// <param name="alwaysTrueCondition">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(AlwaysTrueCondition alwaysTrueCondition, object data)
        {
            return data;
        }

        /// <summary>
        /// Visits <see cref="ConjunctionCondition"/>
        /// </summary>
        /// <param name="conjunctionCondition">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(ConjunctionCondition conjunctionCondition, object data)
        {
            return conjunctionCondition.InnerConditions.Aggregate(
                data, 
                (current1, filterCondition) => filterCondition.Accept(this, current1));
        }

        /// <summary>
        /// Visits <see cref="DisjunctionCondition"/>
        /// </summary>
        /// <param name="disjunctionCondition">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(DisjunctionCondition disjunctionCondition, object data)
        {
            var satisfactionMap = (SatisfactionMap)data;

            var current = satisfactionMap.CreateInitialSatisfactionMap();

            foreach (var filterCondition in disjunctionCondition.InnerConditions)
            {
                current.IntersectWith(
                    (SatisfactionMap) filterCondition.Accept(this, current.CreateInitialSatisfactionMap()));
            }

            satisfactionMap.MergeWith(current);
            return satisfactionMap;
        }

        /// <summary>
        /// Visits <see cref="EqualExpressionCondition"/>
        /// </summary>
        /// <param name="equalExpressionCondition">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(EqualExpressionCondition equalExpressionCondition, object data)
        {
            var satisfactionMap = (SatisfactionMap) data;

            var leftOperand = equalExpressionCondition.LeftOperand;
            var rightOperand = equalExpressionCondition.RightOperand;

            if (leftOperand is ColumnExpression)
            { }
            else if (rightOperand is ColumnExpression)
            {
                leftOperand = rightOperand;
                rightOperand = equalExpressionCondition.LeftOperand;
            }
            else
            {
                return satisfactionMap;
            }

            var leftVariable = ((ColumnExpression)leftOperand).CalculusVariable as SqlColumn;

            if (leftVariable != null)
            {
                foreach (var satisfaction in satisfactionMap.GetSatisfactionsFromMap(leftVariable.Table))
                {
                    if (satisfaction.IsSatisfied)
                    {
                        continue;
                    }

                    satisfaction.ProcessVariableEqualToVariablesCondition(leftVariable, rightOperand);

                    if (satisfaction.IsSatisfied)
                    {
                        satisfactionMap.MarkAsSatisfied(satisfaction);
                    }
                }
            }

            return satisfactionMap;
        }

        /// <summary>
        /// Visits <see cref="EqualVariablesCondition"/>
        /// </summary>
        /// <param name="equalVariablesCondition">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(EqualVariablesCondition equalVariablesCondition, object data)
        {
            var satisfactionMap = (SatisfactionMap)data;

            var leftVariable = equalVariablesCondition.LeftVariable as SqlColumn;
            var rightVariable = equalVariablesCondition.RightVariable as SqlColumn;

            if (leftVariable != null && rightVariable != null)
            {
                var satisfaction = satisfactionMap.GetSatisfactionFromMap(leftVariable.Table, rightVariable.Table);

                if (!satisfaction.IsSatisfied)
                {
                    satisfaction.ProcessEqualVariablesCondition(leftVariable, rightVariable);

                    if (satisfaction.IsSatisfied)
                    {
                        satisfactionMap.MarkAsSatisfied(satisfaction);
                    }
                }
            }

            return satisfactionMap;
        }

        /// <summary>
        /// Visits <see cref="IsNullCondition"/>
        /// </summary>
        /// <param name="isNullCondition">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(IsNullCondition isNullCondition, object data)
        {
            return data;
        }

        /// <summary>
        /// Visits <see cref="NegationCondition"/>
        /// </summary>
        /// <param name="negationCondition">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(NegationCondition negationCondition, object data)
        {
            return data;
        }
    }
}