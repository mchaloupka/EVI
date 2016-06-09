using System.Collections.Generic;
using System.Linq;
using DatabaseSchemaReader.DataSchema;

namespace Slp.Evi.Storage.Relational.PostProcess.Optimizers.SelfJoinOptimizerHelpers
{
    /// <summary>
    /// Class representing a unique constraint.
    /// </summary>
    public class UniqueConstraint
    {
        /// <summary>
        /// The database constraint
        /// </summary>
        private readonly DatabaseConstraint _databaseConstraint;

        /// <summary>
        /// The not equal columns
        /// </summary>
        private readonly HashSet<string> _notEqualColumns;

        /// <summary>
        /// Initializes a new instance of the <see cref="UniqueConstraint"/> class.
        /// </summary>
        /// <param name="databaseConstraint">The database constraint.</param>
        public UniqueConstraint(DatabaseConstraint databaseConstraint)
        {
            _databaseConstraint = databaseConstraint;
            _notEqualColumns = new HashSet<string>(_databaseConstraint.Columns);
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="UniqueConstraint"/> is satisfied.
        /// </summary>
        public bool Satisfied => _notEqualColumns.Count == 0;

        /// <summary>
        /// Gets the database constraint.
        /// </summary>
        public DatabaseConstraint DatabaseConstraint => _databaseConstraint;

        /// <summary>
        /// Determines whether this constraint has a column with name specified by <paramref name="name"/>
        /// which is not marked as equal
        /// </summary>
        public bool HasNotEqualColumn(string name)
        {
            return _notEqualColumns.Contains(name);
        }

        /// <summary>
        /// Marks a column with name specified by <paramref name="name"/> as equal.
        /// </summary>
        public void MarkAsEqual(string name)
        {
            _notEqualColumns.Remove(name);
        }

        /// <summary>
        /// Intersects with the other <see cref="UniqueConstraint" />
        /// </summary>
        public void IntersectWith(UniqueConstraint other)
        {
            foreach (var notEqualColumn in other._notEqualColumns)
            {
                if (!_notEqualColumns.Contains(notEqualColumn))
                {
                    _notEqualColumns.Add(notEqualColumn);
                }
            }
        }

        /// <summary>
        /// Merges with the other <see cref="UniqueConstraint" />
        /// </summary>
        public void MergeWith(UniqueConstraint other)
        {
            var notEqualColumns = _notEqualColumns.ToArray();

            foreach (var notEqualColumn in notEqualColumns)
            {
                if (!other._notEqualColumns.Contains(notEqualColumn))
                {
                    _notEqualColumns.Remove(notEqualColumn);
                }
            }
        }
    }
}