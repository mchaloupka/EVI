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
        /// The list of unique constraints between tables
        /// </summary>
        private readonly List<UniqueConstraint> _uniqueConstraints;

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
        public SelfJoinConstraintsSatisfaction(SqlTable sqlTable, SqlTable replaceByTable, IEnumerable<UniqueConstraint> uniqueColumnConstraints)
        {
            SqlTable = sqlTable;
            ReplaceByTable = replaceByTable;

            _sqlTableColumnsEqualExpressions = new Dictionary<string, List<ConstantExpression>>();
            _replaceByTableColumnsEqualExpressions = new Dictionary<string, List<ConstantExpression>>();

            _uniqueConstraints = new List<UniqueConstraint>();

            foreach (var uniqueColumnConstraint in uniqueColumnConstraints)
            {
                _uniqueConstraints.Add(uniqueColumnConstraint);
            }
        }

        /// <summary>
        /// Determines whether the self join rules are satisfied and the <see cref="SqlTable"/> can be replaced with <see cref="ReplaceByTable"/>
        /// </summary>
        public bool IsSatisfied
        {
            get
            {
                if (_uniqueConstraints.Count > 0)
                {
                    return _uniqueConstraints.Any(x => x.Satisfied);
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
            foreach (var notSatisfiedUniqueConstraintPart in _uniqueConstraints)
            {
                if (notSatisfiedUniqueConstraintPart.HasNotEqualColumn(name))
                {
                    notSatisfiedUniqueConstraintPart.MarkAsEqual(name);
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
        private static bool AreEqual(ConstantExpression expression, ConstantExpression constantExpression)
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

        /// <summary>
        /// Intersects with the <paramref name="other"/> satisfaction.
        /// </summary>
        public void IntersectWith(SelfJoinConstraintsSatisfaction other)
        {
            foreach (var uniqueConstraint in _uniqueConstraints)
            {
                var otherUniqueConstraint =
                    other._uniqueConstraints.Single(x => ReferenceEquals(x.DatabaseConstraint, uniqueConstraint.DatabaseConstraint));

                uniqueConstraint.IntersectWith(otherUniqueConstraint);
            }

            IntersectColumnsEqualLists(_sqlTableColumnsEqualExpressions, other._sqlTableColumnsEqualExpressions);
            IntersectColumnsEqualLists(_replaceByTableColumnsEqualExpressions, other._replaceByTableColumnsEqualExpressions);
        }

        /// <summary>
        /// Intersects the columns equal lists.
        /// </summary>
        /// <param name="sqlTableColumnsEqualExpressions">The SQL table columns equal expressions.</param>
        /// <param name="otherSqlTableColumnsEqualExpressions">The other SQL table columns equal expressions.</param>
        private static void IntersectColumnsEqualLists(Dictionary<string, List<ConstantExpression>> sqlTableColumnsEqualExpressions, Dictionary<string, List<ConstantExpression>> otherSqlTableColumnsEqualExpressions)
        {
            foreach (var column in sqlTableColumnsEqualExpressions.Keys)
            {
                if (!otherSqlTableColumnsEqualExpressions.ContainsKey(column))
                {
                    sqlTableColumnsEqualExpressions[column].Clear();
                }
                else
                {
                    var unMatched = sqlTableColumnsEqualExpressions[column].ToArray();
                    var otherUnMatched = otherSqlTableColumnsEqualExpressions[column];

                    foreach (var constantExpression in unMatched)
                    {
                        if (!otherUnMatched.Any(x => AreEqual(x, constantExpression)))
                        {
                            sqlTableColumnsEqualExpressions[column].Remove(constantExpression);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Merges with the <paramref name="other"/> satisfaction.
        /// </summary>
        public void MergeWith(SelfJoinConstraintsSatisfaction other)
        {
            foreach (var uniqueConstraint in _uniqueConstraints)
            {
                var otherUniqueConstraint =
                    other._uniqueConstraints.Single(x => ReferenceEquals(x.DatabaseConstraint, uniqueConstraint.DatabaseConstraint));

                uniqueConstraint.MergeWith(otherUniqueConstraint);
            }

            MergeColumnsEqualLists(_sqlTableColumnsEqualExpressions, other._sqlTableColumnsEqualExpressions);
            MergeColumnsEqualLists(_replaceByTableColumnsEqualExpressions, other._replaceByTableColumnsEqualExpressions);
        }

        /// <summary>
        /// Merges the columns equal lists.
        /// </summary>
        /// <param name="sqlTableColumnsEqualExpressions">The SQL table columns equal expressions.</param>
        /// <param name="otherSqlTableColumnsEqualExpressions">The other SQL table columns equal expressions.</param>
        private void MergeColumnsEqualLists(Dictionary<string, List<ConstantExpression>> sqlTableColumnsEqualExpressions, Dictionary<string, List<ConstantExpression>> otherSqlTableColumnsEqualExpressions)
        {
            foreach (var column in sqlTableColumnsEqualExpressions.Keys)
            {
                if (otherSqlTableColumnsEqualExpressions.ContainsKey(column))
                {
                    var unMatched = sqlTableColumnsEqualExpressions[column].ToArray();
                    var otherUnMatched = otherSqlTableColumnsEqualExpressions[column];

                    foreach (var constantExpression in otherUnMatched)
                    {
                        if (!unMatched.Any(x => AreEqual(x, constantExpression)))
                        {
                            sqlTableColumnsEqualExpressions[column].Add(constantExpression);
                        }
                    }
                }
            }

            foreach (var column in otherSqlTableColumnsEqualExpressions.Keys)
            {
                if (!sqlTableColumnsEqualExpressions.ContainsKey(column))
                {
                    sqlTableColumnsEqualExpressions.Add(column, new List<ConstantExpression>(otherSqlTableColumnsEqualExpressions[column]));
                }
            }
        }
    }
}
