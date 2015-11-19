using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Relational.Query.Sources;

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
        /// Constructs an instance of <see cref="SelfJoinConstraintsSatisfaction"/>
        /// </summary>
        /// <param name="sqlTable"></param>
        /// <param name="replaceByTable"></param>
        /// <param name="uniqueColumnConstraints"></param>
        public SelfJoinConstraintsSatisfaction(SqlTable sqlTable, SqlTable replaceByTable, IEnumerable<IEnumerable<string>> uniqueColumnConstraints)
        {
            SqlTable = sqlTable;
            ReplaceByTable = replaceByTable;

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
                foreach (var notSatisfiedUniqueConstraintPart in _notSatisfiedUniqueConstraintParts)
                {
                    if (notSatisfiedUniqueConstraintPart.Contains(leftVariable.Name))
                    {
                        notSatisfiedUniqueConstraintPart.Remove(leftVariable.Name);
                    }
                }
            }
        }
    }
}
