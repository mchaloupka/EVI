using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Relational.Query;
using Slp.r2rml4net.Storage.Relational.Query.Expressions;
using Slp.r2rml4net.Storage.Relational.Query.Sources;
using VDS.RDF.Query.Expressions.Arithmetic;

namespace Slp.r2rml4net.Storage.Relational.Optimization.Optimizers.SelfJoinOptimizerHelpers
{
    /// <summary>
    /// Class representing unsatisfied self-join constraints between tables
    /// </summary>
    public class SelfJoinConstraintsSatisfaction
    {
        /// <summary>
        /// The SQL table
        /// </summary>
        public SqlTable SqlTable { get; }

        /// <summary>
        /// The SQL table that will be used to replace <see cref="SqlTable"/>
        /// </summary>
        public SqlTable ReplaceByTable { get; }

        /// <summary>
        /// The list of not satisfied constraint parts
        /// </summary>
        private readonly List<HashSet<string>> _notSatisfiedUniqueConstraintParts;

        /// <summary>
        /// The <see cref="SqlTable"/> map from columns to their equal expressions
        /// </summary>
        private readonly Dictionary<string, List<ConstantExpression>> _sqlTableColumnsEqualExpressions;

        /// <summary>
        /// The <see cref="ReplaceByTable"/> map from columns to their equal expressions
        /// </summary>
        private readonly Dictionary<string, List<ConstantExpression>> _replaceByTableColumnsEqualExpressions;

        /// <summary>
        /// Constructs an instance of <see cref="SelfJoinConstraintsSatisfaction"/>
        /// </summary>
        public SelfJoinConstraintsSatisfaction(SqlTable sqlTable, SqlTable replaceByTable, IEnumerable<IEnumerable<string>> uniqueColumnConstraints)
        {
            SqlTable = sqlTable;
            ReplaceByTable = replaceByTable;

            _sqlTableColumnsEqualExpressions = new Dictionary<string, List<ConstantExpression>>();
            _replaceByTableColumnsEqualExpressions = new Dictionary<string, List<ConstantExpression>>();

            _notSatisfiedUniqueConstraintParts = new List<HashSet<string>>();

            foreach (var uniqueColumnConstraint in uniqueColumnConstraints)
            {
                var columnSet = new HashSet<string>();

                foreach (var column in uniqueColumnConstraint)
                {
                    columnSet.Add(column);
                }

                _notSatisfiedUniqueConstraintParts.Add(columnSet);
            }
        }

        /// <summary>
        /// Determines whether the self join rules are satisfied and the <see cref="SqlTable"/> can be replaced with <see cref="ReplaceByTable"/>
        /// </summary>
        public bool IsSatisfied
        {
            get
            {
                if (_notSatisfiedUniqueConstraintParts.Count > 0)
                {
                    return _notSatisfiedUniqueConstraintParts.Any(x => x.Count == 0);
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Processes the condition that two <see cref="SqlColumn"/> variables are equal
        /// </summary>
        /// <param name="leftVariable">The left variable</param>
        /// <param name="rightVariable">The right variable</param>
        public void ProcessEqualVariablesCondition(SqlColumn leftVariable, SqlColumn rightVariable)
        {
            if (leftVariable.Name == rightVariable.Name)
            {
                SatisfyConditionsWithVariable(leftVariable.Name);
            }
        }

        /// <summary>
        /// Satisfies the conditions with variable specified by <paramref name="name"/>
        /// </summary>
        private void SatisfyConditionsWithVariable(string name)
        {
            foreach (var notSatisfiedUniqueConstraintPart in _notSatisfiedUniqueConstraintParts)
            {
                if (notSatisfiedUniqueConstraintPart.Contains(name))
                {
                    notSatisfiedUniqueConstraintPart.Remove(name);
                }
            }
        }

        /// <summary>
        /// Process the variable equal to variables condition.
        /// </summary>
        /// <param name="leftVariable">The left variable.</param>
        /// <param name="rightOperand">The right operand.</param>
        public void ProcessVariableEqualToVariablesCondition(SqlColumn leftVariable, IExpression rightOperand)
        {
            if (!(rightOperand is ConstantExpression))
            {
                // Currently we support only constant expressions
                return;
            }

            var constantExpression = (ConstantExpression)rightOperand;

            if (leftVariable.Table == SqlTable)
            {
                AddExpression(_sqlTableColumnsEqualExpressions, leftVariable.Name, constantExpression);
                if (CheckExpression(_replaceByTableColumnsEqualExpressions, leftVariable.Name, constantExpression))
                {
                    SatisfyConditionsWithVariable(leftVariable.Name);
                }
            }
            else if (leftVariable.Table == ReplaceByTable)
            {
                AddExpression(_replaceByTableColumnsEqualExpressions, leftVariable.Name, constantExpression);
                if (CheckExpression(_sqlTableColumnsEqualExpressions, leftVariable.Name, constantExpression))
                {
                    SatisfyConditionsWithVariable(leftVariable.Name);
                }
            }
            else
            {
                return;
            }
        }

        /// <summary>
        /// Checks the expression, whether the same is present also in the map <paramref name="tableColumnsEqualExpressions"/> for the variable
        /// specified by <paramref name="variableName"/>
        /// </summary>
        private bool CheckExpression(Dictionary<string, List<ConstantExpression>> tableColumnsEqualExpressions, string variableName, ConstantExpression expression)
        {
            if (tableColumnsEqualExpressions.ContainsKey(variableName))
            {
                return tableColumnsEqualExpressions[variableName].Any(constantExpression => AreEqual(expression, constantExpression));
            }

            return false;
        }

        /// <summary>
        /// Determines whether the expression are equal.
        /// </summary>
        private bool AreEqual(ConstantExpression expression, ConstantExpression constantExpression)
        {
            return expression.Value.Equals(constantExpression.Value);
        }

        /// <summary>
        /// Adds the expression to the map <paramref name="tableColumnsEqualExpressions"/>
        /// </summary>
        /// <param name="tableColumnsEqualExpressions">The table columns equal expressions map.</param>
        /// <param name="variableName">Name of the variable.</param>
        /// <param name="expression">The expression.</param>
        private void AddExpression(Dictionary<string, List<ConstantExpression>> tableColumnsEqualExpressions, string variableName, ConstantExpression expression)
        {
            if (!tableColumnsEqualExpressions.ContainsKey(variableName))
            {
                tableColumnsEqualExpressions.Add(variableName, new List<ConstantExpression>());
            }

            tableColumnsEqualExpressions[variableName].Add(expression);
        }
    }
}
